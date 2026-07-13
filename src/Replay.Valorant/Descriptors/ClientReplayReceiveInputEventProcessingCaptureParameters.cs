using Replay.Models.Descriptors;
using Replay.Unreal.Parsing;

namespace Replay.Valorant.Descriptors;

public sealed class ClientReplayReceiveInputEventProcessingCaptureParameters
    : ExportGroupDescriptor<ClientReplayReceiveInputEventProcessingCaptureParameters>
{
    public override string Path =>
        "/Script/ShooterGame.ReplayPlayerController:ClientReplayReceiveInputEventProcessingCapture";
    public override ExportCategory Categories => ExportCategory.Debug;
    public override ExportGroupKind Kind => ExportGroupKind.ClassNetCache;
    public override FieldStreamGrammar Grammar => FieldStreamGrammar.FunctionParameters;

    public int PlayerId { get; set; }
    public ValorantRawPayload? InputEventData { get; set; }

    protected override void Configure()
    {
        AddProperty("PlayerID", x => x.PlayerId, ExportCategory.Debug).Int32();
        AddProperty(x => x.InputEventData, ExportCategory.Debug)
            .Decode(ValorantPayloadDecoders.RawPayload("TArray<uint8>")); // TODO: Implement the array encoding.
    }
}