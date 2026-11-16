namespace ToolRunner.Tools;

/// <summary>
/// Stryker.NET. Requires an SDK-style project and a modern SDK to run. Also: the sheet
/// makes it primary for 16 Data Flow metrics, which it cannot compute -- see roslyn-
/// dataflow.
/// </summary>
public sealed class Stryker : ITool
{
    public string Slug => "stryker";

    public int Run(ToolContext ctx)
    {
        ctx.Skip("Requires an SDK-style project and a modern SDK to run");
        return ExitCodes.Ok;
    }
}
