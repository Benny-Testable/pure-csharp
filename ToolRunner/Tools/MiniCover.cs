namespace ToolRunner.Tools;

/// <summary>
/// MiniCover. MiniCover 3.10.0 declares .NET SDK 8.0/9.0/10.0 only -- verified against
/// its own MiniCover.csproj
/// (&lt;TargetFrameworks&gt;net10.0;net9.0;net8.0&lt;/TargetFrameworks&gt;). It is the
/// alternative for 22 metrics and cannot reach any .NET Framework target.
/// </summary>
public sealed class MiniCover : ITool
{
    public string Slug => "minicover";

    public int Run(ToolContext ctx)
    {
        ctx.Skip("MiniCover 3.10.0 declares .NET SDK 8.0/9.0/10.0 only -- verified against its own MiniCover.csproj (<TargetFrameworks>net10.0;net9.0;net8.0</TargetFrameworks>)");
        return ExitCodes.Ok;
    }
}
