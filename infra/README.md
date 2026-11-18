# infra/ -- IaC test data

Infrastructure-as-code for the **IaC Security** and **Access Control** checks.
These templates are scanner fixtures, **not deployable infrastructure**: every
resource carries a deliberate misconfiguration. They contain no credentials;
the database password is a `NoEcho` parameter supplied at deploy time.

None of these files is part of `OrderKit.sln`, so the build and tests are unaffected.

| File | Format | Planted findings |
|---|---|---|
| `cloudformation/orderkit-aws.yaml` | AWS CloudFormation | S3 bucket public-read with no public-access block, no encryption, no versioning or logging; security group open to `0.0.0.0/0` on 22 and 3389 with all egress; RDS publicly accessible, unencrypted, no backups or deletion protection; unencrypted EBS volume; IAM policy `Action: "*"` on `Resource: "*"` attached to a role any AWS principal may assume |
| `kubernetes/orderkit-api.yaml` | Kubernetes | privileged container running as root with privilege escalation, added capabilities, host network and host PID, `:latest` image, no resource limits; LoadBalancer service open to `0.0.0.0/0`; default service account bound to `cluster-admin`; ClusterRole with `*` on every API group, resource and verb |
| `arm/orderkit-azure.json` | Azure ARM | storage account allowing HTTP, public blob access, TLS 1.0 and default-allow network rules; blob container with public access; NSG allowing RDP and SSH from the internet; SQL server with public network access, TLS 1.0 and a firewall rule for every IP |

Verified with checkov (CloudFormation, Kubernetes and ARM frameworks) and
`trivy config`: each file is detected and fails the checks above.

| Report classification | Evidence here |
|---|---|
| Open Security Group Rule Detection / Open Network Access | security group, NSG and SQL firewall rules open to the internet |
| Unencrypted Storage Definition / Storage Encryption Gap | unencrypted RDS and EBS; storage account allowing HTTP |
| Publicly Exposed Resource Detection / Public Exposure Risk | public S3 bucket and blob container, public RDS, LoadBalancer open to all |
| CIS Benchmark Compliance / CIS Compliance Gap | the CIS-mapped checks above across AWS, Azure and Kubernetes |
| Privileged Access Audit (Access Control) | wildcard IAM policy, `cluster-admin` binding, wildcard ClusterRole, privileged container |
