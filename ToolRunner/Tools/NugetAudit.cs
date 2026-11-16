namespace ToolRunner.Tools;

/// <summary>
/// dotnet list package --vulnerable. Requires PackageReference and an SDK-style project.
/// Structurally impossible on the packages.config cells, and requires a restore this host
/// cannot perform.
/// </summary>
public sealed class NugetAudit : ITool
{
    public string Slug => "nuget-audit";

    public int Run(ToolContext ctx)
    {
        ctx.Skip("Requires PackageReference and an SDK-style project");
        return ExitCodes.Ok;
    }
}
