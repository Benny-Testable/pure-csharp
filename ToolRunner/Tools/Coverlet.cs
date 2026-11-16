namespace ToolRunner.Tools;

/// <summary>
/// Coverlet. Coverlet ships netstandard2.0; its .NET Framework floor is net461. This
/// family is below that floor.
/// </summary>
public sealed class Coverlet : ITool
{
    public string Slug => "coverlet";

    public int Run(ToolContext ctx)
    {
        ctx.Skip("Coverlet ships netstandard2.0; its .NET Framework floor is net461");
        return ExitCodes.Ok;
    }
}
