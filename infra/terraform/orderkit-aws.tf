# OrderKit test data for IaC scanners (tfsec, checkov, KICS). NOT FOR DEPLOYMENT:
# every resource below carries a deliberate misconfiguration, listed in
# infra/README.md. No credentials: the database password is a variable with no
# default, supplied at plan time.

terraform {
  required_providers {
    aws = {
      source  = "hashicorp/aws"
      version = "~> 5.0"
    }
  }
}

provider "aws" {
  region = "us-east-1"
}

variable "db_password" {
  type      = string
  sensitive = true
}

# ---------------------------------------------------------------------------
# Open firewall rules: SSH, RDP and every port open to the internet.
# ---------------------------------------------------------------------------
resource "aws_security_group" "orderkit_open" {
  name        = "orderkit-open"
  description = "OrderKit app tier"

  ingress {
    description = "SSH from anywhere"
    from_port   = 22
    to_port     = 22
    protocol    = "tcp"
    cidr_blocks = ["0.0.0.0/0"]
  }

  ingress {
    description = "RDP from anywhere"
    from_port   = 3389
    to_port     = 3389
    protocol    = "tcp"
    cidr_blocks = ["0.0.0.0/0"]
  }

  ingress {
    description      = "All traffic from anywhere"
    from_port        = 0
    to_port          = 65535
    protocol         = "-1"
    cidr_blocks      = ["0.0.0.0/0"]
    ipv6_cidr_blocks = ["::/0"]
  }

  egress {
    from_port   = 0
    to_port     = 0
    protocol    = "-1"
    cidr_blocks = ["0.0.0.0/0"]
  }
}

resource "aws_network_acl_rule" "orderkit_allow_all" {
  network_acl_id = "acl-00000000"
  rule_number    = 100
  egress         = false
  protocol       = "-1"
  rule_action    = "allow"
  cidr_block     = "0.0.0.0/0"
}

# ---------------------------------------------------------------------------
# Public storage access: public-read bucket, public access block disabled.
# Unencrypted storage: no server-side encryption, no versioning, no logging.
# ---------------------------------------------------------------------------
resource "aws_s3_bucket" "orderkit_reports" {
  bucket = "orderkit-reports-public-tf"
}

resource "aws_s3_bucket_acl" "orderkit_reports" {
  bucket = aws_s3_bucket.orderkit_reports.id
  acl    = "public-read-write"
}

resource "aws_s3_bucket_public_access_block" "orderkit_reports" {
  bucket                  = aws_s3_bucket.orderkit_reports.id
  block_public_acls       = false
  block_public_policy     = false
  ignore_public_acls      = false
  restrict_public_buckets = false
}

resource "aws_s3_bucket_policy" "orderkit_reports" {
  bucket = aws_s3_bucket.orderkit_reports.id
  policy = jsonencode({
    Version = "2012-10-17"
    Statement = [{
      Effect    = "Allow"
      Principal = "*"
      Action    = "s3:*"
      Resource  = "${aws_s3_bucket.orderkit_reports.arn}/*"
    }]
  })
}

# Unencrypted EBS volume and snapshot copy.
resource "aws_ebs_volume" "orderkit_data" {
  availability_zone = "us-east-1a"
  size              = 20
  encrypted         = false
}

# Publicly accessible, unencrypted RDS with no backups or deletion protection.
resource "aws_db_instance" "orderkit_db" {
  identifier              = "orderkit-db"
  engine                  = "mysql"
  engine_version          = "5.7"
  instance_class          = "db.t3.micro"
  allocated_storage       = 20
  username                = "orderkit"
  password                = var.db_password
  publicly_accessible     = true
  storage_encrypted       = false
  backup_retention_period = 0
  deletion_protection     = false
  skip_final_snapshot     = true
  vpc_security_group_ids  = [aws_security_group.orderkit_open.id]
}

# Unencrypted SQS queue and SNS topic.
resource "aws_sqs_queue" "orderkit_events" {
  name                    = "orderkit-events"
  sqs_managed_sse_enabled = false
}

resource "aws_sns_topic" "orderkit_alerts" {
  name = "orderkit-alerts"
}

# EC2 instance with a public IP, IMDSv1 allowed and an unencrypted root volume.
resource "aws_instance" "orderkit_api" {
  ami                         = "ami-00000000000000000"
  instance_type               = "t3.micro"
  associate_public_ip_address = true
  vpc_security_group_ids      = [aws_security_group.orderkit_open.id]

  metadata_options {
    http_tokens = "optional"
  }

  root_block_device {
    encrypted = false
  }
}

# Internet-facing load balancer listening on plain HTTP.
resource "aws_lb" "orderkit_public" {
  name               = "orderkit-public"
  internal           = false
  load_balancer_type = "application"
  security_groups    = [aws_security_group.orderkit_open.id]
  subnets            = ["subnet-00000000", "subnet-11111111"]
}

resource "aws_lb_listener" "orderkit_http" {
  load_balancer_arn = aws_lb.orderkit_public.arn
  port              = 80
  protocol          = "HTTP"

  default_action {
    type             = "forward"
    target_group_arn = "arn:aws:elasticloadbalancing:us-east-1:000000000000:targetgroup/orderkit/0000000000000000"
  }
}

# ---------------------------------------------------------------------------
# CIS benchmark gaps: wildcard IAM policy, no CloudTrail validation/encryption,
# KMS key without rotation, weak account password policy.
# ---------------------------------------------------------------------------
resource "aws_iam_policy" "orderkit_admin" {
  name = "orderkit-admin"
  policy = jsonencode({
    Version = "2012-10-17"
    Statement = [{
      Effect   = "Allow"
      Action   = "*"
      Resource = "*"
    }]
  })
}

resource "aws_cloudtrail" "orderkit" {
  name                          = "orderkit-trail"
  s3_bucket_name                = aws_s3_bucket.orderkit_reports.id
  is_multi_region_trail         = false
  enable_log_file_validation    = false
  include_global_service_events = false
}

resource "aws_kms_key" "orderkit" {
  description         = "OrderKit data key"
  enable_key_rotation = false
}

resource "aws_iam_account_password_policy" "orderkit" {
  minimum_password_length        = 6
  require_lowercase_characters   = false
  require_numbers                = false
  require_uppercase_characters   = false
  require_symbols                = false
  allow_users_to_change_password = true
  max_password_age               = 0
  password_reuse_prevention      = 0
}
