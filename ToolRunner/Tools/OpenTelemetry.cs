namespace ToolRunner.Tools;

/// <summary>
/// OpenTelemetry (.NET) + OTLP exporter. STATUS CHANGED AT THIS FAMILY, for the same
/// reason as Coverlet: netstandard2.0 clears at net461, so the target framework no longer
/// excludes it and only NuGet does. Note separately that tracing is not path coverage.
/// The TypeScript corpus shipped an OTel bootstrap that registered a no-op processor,
/// exported nothing and exited 0 -- a structurally zero result.
/// </summary>
public sealed class OpenTelemetry : ITool
{
    public string Slug => "opentelemetry";

    public int Run(ToolContext ctx)
    {
        ctx.Missing("STATUS CHANGED AT THIS FAMILY, for the same reason as Coverlet: netstandard2.0 clears at net461, so the target framework no longer excludes it and only NuGet does");
        return ExitCodes.Ok;
    }
}
