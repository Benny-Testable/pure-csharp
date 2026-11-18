# Planted analysis fixtures

Each file here exists so that one class of tool has a known, non-empty answer.
If a tool reports nothing against this project, the tool did not run --
whatever its exit code said. Nothing in the OrderKit domain references this
project; only the tests do.

| File | Planted for | What a working tool should report |
|---|---|---|
| `ComplexitySample.cs` | Lizard, unilyze, Roslyn metrics | `ClassifyShipment` well above a cyclomatic limit of 10 and a cognitive limit of 15 |
| `LintViolations.cs` | Roslyn analyzers / `dotnet format` | unused usings, unused variable, naming violations, mutable public static field, empty catch, redundant boolean comparison |
| `DeadCode.cs` | dead-code analyzers, coverage tools | an unused constant, an unused private method, an unconstructed class, and one unreachable statement |
| `CallGraphSample.cs` | call-graph and data-flow tools | a five-deep call chain and a two-node recursive cycle |
| `NestedPathsSample.cs` | coverage tools' nested-path measure | decisions nested four and five deep, only some taken by `NestedPathsSampleTests` |

`.testable/defs-uses-scan-roots.txt` points the hosted data-flow workers at this
folder.
