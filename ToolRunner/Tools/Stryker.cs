namespace ToolRunner.Tools;

/// <summary>
/// Stryker.NET. This branch is an SDK-style net8.0 solution, which Stryker.NET
/// supports. It mutates the code under test, REBUILDS it and runs the test project for
/// every mutant, so it runs only when OrderKit.sln builds (see
/// <see cref="DotnetSolution"/>), which it does not on this branch: duplicate
/// AssemblyInfo attributes (CS0579) and broken project references (OrderKit.Tests, OrderKit.Contracts). That
/// is exit 3. Also: the sheet makes it primary for 16 Data Flow metrics, which it cannot
/// compute -- see roslyn-dataflow.
/// </summary>
public sealed class Stryker : ITool
{
    public string Slug => "stryker";

    public int Run(ToolContext ctx)
    {
        DotnetSolution.RequireBuild(ctx);
        var stryker = DotnetTool.Require(ctx, "dotnet-stryker", "4.6.0", "dotnet-stryker");

        var testProject = DotnetSolution.TestProjectPath(ctx);
        var output = Path.Combine(ctx.ReportDir(), "stryker");

        // --break-at 0: a low mutation score is the measurement, not a failure.
        var rc = ctx.Run(stryker,
            ["--reporter", "json", "--reporter", "progress", "--output", output, "--break-at", "0"],
            workDir: Path.GetDirectoryName(testProject));
        return rc == 0 ? ExitCodes.Ok : ExitCodes.Fail;
    }
}
