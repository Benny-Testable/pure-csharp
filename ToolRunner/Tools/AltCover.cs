namespace ToolRunner.Tools;

/// <summary>
/// AltCover. Declares .NET Framework 2.0+ via a net20 recorder, so it is the one coverage
/// tool in the roster that could reach this family. Blocked only by NuGet.
/// </summary>
public sealed class AltCover : ITool
{
    public string Slug => "altcover";

    public int Run(ToolContext ctx)
    {
        ctx.Missing("Declares .NET Framework 2.0+ via a net20 recorder, so it is the one coverage tool in the roster that could reach this family");
        return ExitCodes.Ok;
    }
}
