namespace Replay.Models.Events;

public sealed record DecodedPayloadResult(
    object? Payload,
    int DecodedFieldCount,
    IReadOnlyList<DecodedReplayField> DiagnosticFields)
{
    public static DecodedPayloadResult Empty { get; } = new(null, 0, []);

    public RepLayoutFieldStreamStatus FieldStreamStatus { get; init; } =
        RepLayoutFieldStreamStatus.Complete;

    public IReadOnlyList<RepLayoutFieldOccurrence> FieldOccurrences { get; init; } = [];
}
