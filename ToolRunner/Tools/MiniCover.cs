namespace ToolRunner.Tools;

/// <summary>
/// MiniCover. MiniCover 3.10.0 supports the .NET 8 SDK this branch uses
/// (its own TargetFrameworks are net10.0;net9.0;net8.0), so the target framework does
/// not exclude it. It instruments a BUILD and then runs the test suite, so it runs
/// only when OrderKit.sln builds (see <see cref="DotnetSolution"/>), which it does not
/// on this branch: duplicate AssemblyInfo attributes (CS0579) and broken project
/// references (OrderKit.Tests, OrderKit.Contracts). That is exit 3.
/// </summary>
public sealed class MiniCover : ITool
{
    public string Slug => "minicover";

    public int Run(ToolContext ctx)
    {
        DotnetSolution.RequireBuild(ctx);
        var minicover = DotnetTool.Require(ctx, "minicover", "3.10.0", "minicover");
        var dotnet = ctx.RequireCmd("dotnet");
        string[] workdir = ["--workdir", ctx.RepoRoot];

        if (ctx.Run(minicover, ["instrument", .. workdir, "--sources", "src/**/*.cs", "--tests", "tests/**/*.cs"]) != 0)
        {
            return ExitCodes.Fail;
        }

        try
        {
            ctx.Run(minicover, ["reset", .. workdir]);
            if (ctx.Run(dotnet, ["test", DotnetSolution.TestProjectPath(ctx), "--no-build"]) != 0)
            {
                return ExitCodes.Fail;
            }
        }
        finally
        {
            ctx.Run(minicover, ["uninstrument", .. workdir]);
        }

        // --threshold 0: the report is the measurement, not a pass/fail budget.
        var rc = ctx.Run(minicover,
            ["coberturareport", .. workdir, "--output", ctx.Report("minicover.cobertura.xml"), "--threshold", "0"]);
        return rc == 0 ? ExitCodes.Ok : ExitCodes.Fail;
    }
}
