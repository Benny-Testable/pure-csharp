# Data for the 47 retired taxonomy metrics

The platform retired these 47 metrics (migration `wb0113_retire_unvalidated`, TP-6263),
so they do not appear in taxonomy reports. This branch carries data for every one of
them, so they produce evidence as soon as they are re-activated (taxonomy admin, or by
reverting wb0113). Path Coverage also needs altcover to run on C# (it currently reports
not_applicable).

Fixtures live in `src/OrderKit.Analysis/RetiredMetrics/` (compiled into OrderKit, inside
the SAST/lint and def-use scan roots) and are driven by `tests/OrderKit.Tests/RetiredMetricsTests.cs`.
Test gaps are deliberate: RiskBand "ban", CreditCheck "over", Grade "D"/"F",
CanShip with paid=false and inStock=false, and ExportInvoice are never executed.

| # | Testing type | Technique | Classification | Tool | Data |
|---|---|---|---|---|---|
| 1 | Code Quality Auditing | Code Duplication | Refactoring Opportunity Detection | jscpd-cs | src/OrderKit.Analysis/RetiredMetrics/DuplicationScenarios.cs |
| 2 | Code Quality Auditing | Code Duplication | Risk-Based Testing Prioritization | jscpd-cs | src/OrderKit.Analysis/RetiredMetrics/DuplicationScenarios.cs |
| 3 | Code Quality Auditing | Code Duplication | Maintainability Testing | jscpd-cs | src/OrderKit.Analysis/RetiredMetrics/DuplicationScenarios.cs |
| 4 | Control Flow Testing | Branch Coverage | Logic Error Detection | coverlet | src/OrderKit.Analysis/RetiredMetrics/LogicScenarios.cs (QualifiesForFreeShipping) |
| 5 | Control Flow Testing | Path Coverage | Path Execution Tracking | altcover, coverlet | src/OrderKit.Analysis/RetiredMetrics/PathScenarios.cs |
| 6 | Control Flow Testing | Path Coverage | Partial Path Coverage Detection | altcover, coverlet | src/OrderKit.Analysis/RetiredMetrics/PathScenarios.cs |
| 7 | Control Flow Testing | Path Coverage | Nested Condition Path Testing | altcover, coverlet | src/OrderKit.Analysis/RetiredMetrics/PathScenarios.cs |
| 8 | Control Flow Testing | Path Coverage | Loop Path Detection | altcover, coverlet | src/OrderKit.Analysis/RetiredMetrics/PathScenarios.cs |
| 9 | Control Flow Testing | Path Coverage | Unreachable Path Detection | altcover, coverlet | src/OrderKit.Analysis/RetiredMetrics/PathScenarios.cs |
| 10 | Control Flow Testing | Path Coverage | Exception Path Handling | altcover, coverlet | src/OrderKit.Analysis/RetiredMetrics/PathScenarios.cs |
| 11 | Control Flow Testing | Path Coverage | Multi-Function Path Tracking | altcover, coverlet | src/OrderKit.Analysis/RetiredMetrics/PathScenarios.cs |
| 12 | Control Flow Testing | Path Coverage | CI/CD Integration Test | altcover, coverlet | .github/workflows/ci.yml runs tests with coverage |
| 13 | Control Flow Testing | Statement Coverage | Test Completeness Evaluation | coverlet | src/OrderKit.Analysis/RetiredMetrics/LogicScenarios.cs (Grade) |
| 14 | Data Flow Testing | All Definition Coverage | Definition-Use Mapping | cs_all_defs_uses | src/OrderKit.Analysis/RetiredMetrics/DataFlowScenarios.cs |
| 15 | Data Flow Testing | All Definition Coverage | Edge Case Handling | cs_all_defs_uses | src/OrderKit.Analysis/RetiredMetrics/DataFlowScenarios.cs |
| 16 | Data Flow Testing | All Definition Coverage | Reporting Validation | cs_all_defs_uses | src/OrderKit.Analysis/RetiredMetrics/DataFlowScenarios.cs |
| 17 | Data Flow Testing | All Uses Coverage | Definition-Use Pair Identification | cs_all_defs_uses | src/OrderKit.Analysis/RetiredMetrics/DataFlowScenarios.cs |
| 18 | Data Flow Testing | All Uses Coverage | Partial Uses Coverage Detection | cs_all_defs_uses | src/OrderKit.Analysis/RetiredMetrics/DataFlowScenarios.cs |
| 19 | Data Flow Testing | All Uses Coverage | Multiple Definitions Handling | cs_all_defs_uses | src/OrderKit.Analysis/RetiredMetrics/DataFlowScenarios.cs |
| 20 | Data Flow Testing | All Uses Coverage | Unreachable Use Detection | cs_all_defs_uses | src/OrderKit.Analysis/RetiredMetrics/DataFlowScenarios.cs |
| 21 | Data Flow Testing | All Uses Coverage | Coverage Reporting Validation | cs_all_defs_uses | src/OrderKit.Analysis/RetiredMetrics/DataFlowScenarios.cs |
| 22 | Mutation Testing | Mutation Score | Code Logic Validation | stryker-net | src/OrderKit.Analysis/RetiredMetrics/LogicScenarios.cs (LoyaltyPoints) |
| 23 | Readability / Maintainability | Cognitive Complexity | Risk Detection | sonar-cs (S3776), lizard | src/OrderKit.Analysis/RetiredMetrics/CognitiveScenarios.cs |
| 24 | Readability / Maintainability | Cognitive Complexity | Code Review Support | sonar-cs (S3776), lizard | src/OrderKit.Analysis/RetiredMetrics/CognitiveScenarios.cs |
| 25 | Readability / Maintainability | Cognitive Complexity | Testing Effort Prioritization | sonar-cs (S3776), lizard | src/OrderKit.Analysis/RetiredMetrics/CognitiveScenarios.cs |
| 26 | Readability / Maintainability | Cognitive Complexity | Code Understandability Analysis | sonar-cs (S3776), lizard | src/OrderKit.Analysis/RetiredMetrics/CognitiveScenarios.cs |
| 27 | Security White-box Testing | Dependency Risk (SCA) | Transitive Dependency Analysis | dotnet-sca | Directory.Packages.props + PLANTED-CVES.md (5 vulnerable pins, transitive graph via Microsoft.AspNet.Mvc, deprecated BouncyCastle 1.8.1) |
| 28 | Security White-box Testing | Dependency Risk (SCA) | License Compliance Testing | dotnet-sca | Directory.Packages.props + PLANTED-CVES.md (5 vulnerable pins, transitive graph via Microsoft.AspNet.Mvc, deprecated BouncyCastle 1.8.1) |
| 29 | Security White-box Testing | Dependency Risk (SCA) | Supply Chain Security Analysis | dotnet-sca | Directory.Packages.props + PLANTED-CVES.md (5 vulnerable pins, transitive graph via Microsoft.AspNet.Mvc, deprecated BouncyCastle 1.8.1) |
| 30 | Security White-box Testing | Dependency Risk (SCA) | Dependency Health Monitoring | dotnet-sca | Directory.Packages.props + PLANTED-CVES.md (5 vulnerable pins, transitive graph via Microsoft.AspNet.Mvc, deprecated BouncyCastle 1.8.1) |
| 31 | Security White-box Testing | Dependency Risk (SCA) | Risk Prioritization | dotnet-sca | Directory.Packages.props + PLANTED-CVES.md (5 vulnerable pins, transitive graph via Microsoft.AspNet.Mvc, deprecated BouncyCastle 1.8.1) |
| 32 | Security White-box Testing | Dependency Risk (SCA) | Vulnerability Dependency Detection | dotnet-sca | Directory.Packages.props + PLANTED-CVES.md (5 vulnerable pins, transitive graph via Microsoft.AspNet.Mvc, deprecated BouncyCastle 1.8.1) |
| 33 | Static Code Analysis | Lint / Rule Violations | Unused Variable Detection | roslyn-analyzers, sonar-cs | src/OrderKit.Analysis/RetiredMetrics/LintScenarios.cs + src/OrderKit.Analysis/RetiredMetrics/.editorconfig |
| 34 | Static Code Analysis | Lint / Rule Violations | Naming Convention Validation | roslyn-analyzers, sonar-cs | src/OrderKit.Analysis/RetiredMetrics/LintScenarios.cs + src/OrderKit.Analysis/RetiredMetrics/.editorconfig |
| 35 | Static Code Analysis | Lint / Rule Violations | Code Style Rule Validation | roslyn-analyzers, sonar-cs | src/OrderKit.Analysis/RetiredMetrics/LintScenarios.cs + src/OrderKit.Analysis/RetiredMetrics/.editorconfig |
| 36 | Static Code Analysis | Lint / Rule Violations | Complexity Rule Detection | roslyn-analyzers, sonar-cs | src/OrderKit.Analysis/RetiredMetrics/LintScenarios.cs + src/OrderKit.Analysis/RetiredMetrics/.editorconfig |
| 37 | Static Code Analysis | Lint / Rule Violations | Multiple Violations Detection | roslyn-analyzers, sonar-cs | src/OrderKit.Analysis/RetiredMetrics/LintScenarios.cs + src/OrderKit.Analysis/RetiredMetrics/.editorconfig |
| 38 | Static Code Analysis | Lint / Rule Violations | False Positive Prevention | roslyn-analyzers, sonar-cs | src/OrderKit.Analysis/RetiredMetrics/LintScenarios.cs + src/OrderKit.Analysis/RetiredMetrics/.editorconfig |
| 39 | Static Code Analysis | Lint / Rule Violations | Custom Rule Validation | roslyn-analyzers, sonar-cs | src/OrderKit.Analysis/RetiredMetrics/.editorconfig (custom naming rule) |
| 40 | Static Code Analysis | Lint / Rule Violations | Configuration File Handling | roslyn-analyzers, sonar-cs | src/OrderKit.Analysis/RetiredMetrics/.editorconfig |
| 41 | Static Code Analysis | Lint / Rule Violations | CI/CD Integration Validation | roslyn-analyzers, sonar-cs | .github/workflows/ci.yml runs the build with analyzers |
| 42 | Static Code Analysis | Lint / Rule Violations | Violation Reporting Validation | roslyn-analyzers, sonar-cs | src/OrderKit.Analysis/RetiredMetrics/LintScenarios.cs + src/OrderKit.Analysis/RetiredMetrics/.editorconfig |
| 43 | Structural Analysis | Cyclomatic Complexity | Decision Coverage | lizard, coverlet | src/OrderKit.Analysis/RetiredMetrics/LogicScenarios.cs (CanShip, ShippingCost) |
| 44 | Structural Analysis | Cyclomatic Complexity | Condition Coverage | lizard, coverlet | src/OrderKit.Analysis/RetiredMetrics/LogicScenarios.cs (CanShip, ShippingCost) |
| 45 | Structural Analysis | Cyclomatic Complexity | Logic Coverage Metric | lizard, coverlet | src/OrderKit.Analysis/RetiredMetrics/LogicScenarios.cs (CanShip, ShippingCost) |
| 46 | Structural Analysis | Cyclomatic Complexity | Maintainability Analysis | lizard, coverlet | src/OrderKit.Analysis/RetiredMetrics/LogicScenarios.cs (CanShip, ShippingCost) |
| 47 | Test Regression/Coverage Analysis | Coverage Delta | Test Suite Effectiveness Tracking | cs_coverage_delta | Second run on a new commit (baseline = previous run) |
