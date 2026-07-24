namespace Replay.Models.Events;

public interface IReplayEventSink
{
    void Emit(ReplayEvent replayEvent);
}

public interface IRepLayoutFieldOccurrenceSink
{
    bool ShouldCapture(string exportGroupPath);

    void RecordBlock(RepLayoutFieldOccurrenceBlock block);
}

public enum RepLayoutFieldStreamStatus
{
    Complete,
    GroupNotParsed,
    TransformNotApplied,
    UnsupportedGrammar,
    MalformedFieldHeader,
    InvalidFieldLength,
    MissingTerminator,
    TrailingBits,
}

public enum RepLayoutFieldBindingStatus
{
    Unavailable,
    Disabled,
    Enabled,
}

public readonly record struct RepLayoutFieldOccurrence(
    uint Handle,
    bool WireExported,
    string? ExportName,
    uint? CompatibleChecksum,
    bool HasPayload,
    RepLayoutFieldBindingStatus BindingStatus);

public sealed record RepLayoutFieldOccurrenceBlock(
    string ExportGroupPath,
    RepLayoutFieldStreamStatus Status,
    IReadOnlyList<RepLayoutFieldOccurrence> Fields);
