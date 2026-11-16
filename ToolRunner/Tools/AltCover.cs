namespace ToolRunner.Tools;

/// <summary>
/// AltCover. net8.0 is supported. AltCover rewrites COMPILED assemblies to record
/// visits, so it runs only when OrderKit.sln builds (see <see cref="DotnetSolution"/>),
/// which it does not on this branch: duplicate AssemblyInfo attributes (CS0579) and
/// broken project references (OrderKit.Tests, OrderKit.Contracts). That is exit 3. AltCover itself also
/// comes from NuGet.
/// </summary>
public sealed class AltCover : ITool
{
    public string Slug => "altcover";

    public int Run(ToolContext ctx)
    {
        DotnetSolution.RequireBuild(ctx);
        var altcover = DotnetTool.Require(ctx, "altcover.global", "9.0.1", "altcover");

        var testProject = DotnetSolution.TestProjectPath(ctx);
        var bin = Path.Combine(Path.GetDirectoryName(testProject)!, "bin", "Debug", "net8.0");
        var instrumented = Path.Combine(ctx.ReportDir(), "altcover", "instrumented");
        var report = ctx.Report("altcover.xml");

        // 1. instrument the built output into a copy; 2. run the tests against the
        //    copy and collect the visit counts into the report.
        var rc = ctx.Run(altcover, ["--inputDirectory", bin, "--outputDirectory", instrumented, "--report", report]);
        if (rc != 0)
        {
            return ExitCodes.Fail;
        }

        var dotnet = ctx.RequireCmd("dotnet");
        rc = ctx.Run(altcover,
            ["Runner", "--recorderDirectory", instrumented, "--executable", dotnet, "--collect", "--",
             "test", Path.Combine(instrumented, "OrderKit.Tests.dll")]);
        return rc == 0 ? ExitCodes.Ok : ExitCodes.Fail;
    }
}
