namespace ToolRunner.Tools;

/// <summary>
/// OpenTelemetry (.NET) + OTLP exporter. netstandard2.0 minimum, below this family. Also:
/// tracing is not path coverage. The TypeScript corpus shipped an OTel bootstrap that
/// registered a no-op processor, exported nothing and exited 0 -- a structurally zero
/// result.
/// </summary>
public sealed class OpenTelemetry : ITool
{
    public string Slug => "opentelemetry";

    public int Run(ToolContext ctx)
    {
        ctx.Skip("netstandard2.0 minimum, below this family");
        return ExitCodes.Ok;
    }
}
