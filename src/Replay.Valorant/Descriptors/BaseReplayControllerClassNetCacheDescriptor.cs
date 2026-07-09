using Replay.Models.Descriptors;
using Replay.Valorant.Movement;

namespace Replay.Valorant.Descriptors;

internal sealed class BaseReplayControllerClassNetCacheDescriptor : ClassNetCacheDescriptor<BaseReplayControllerClassNetCacheDescriptor>
{
    public override string Path => "/Game/Characters/_Core/BaseReplayController.BaseReplayController_C_ClassNetCache";

    protected override void Configure()
    {
        AddFunction<ClientReplayReceiveInputEventProcessingCaptureParameters>(
                "ClientReplayReceiveInputEventProcessingCapture",
                "/Script/ShooterGame.ReplayPlayerController:ClientReplayReceiveInputEventProcessingCapture",
                ExportCategory.Debug)
            .Decode(ValorantPayloadDecoders.RawRpc("ClientReplayReceiveInputEventProcessingCapture"));

        AddFunction(
                "ReplaysClientReceiveRemoteCharacterUpdatesSingleArrayNoAutonomous",
                "/Script/ShooterGame.ReplayPlayerController:ReplaysClientReceiveRemoteCharacterUpdatesSingleArrayNoAutonomous",
                ExportCategory.Movement)
            .Decode(RemoteCharacterUpdatesRpcDecoder.Instance);

        AddFunction<ClientGamePhaseBeginParameters>(
                "ClientGamePhaseBegin",
                "/Script/ShooterGame.AresPlayerController:ClientGamePhaseBegin",
                ExportCategory.GameState)
            .Decode(ValorantPayloadDecoders.RawRpc("ClientGamePhaseBegin"));

        AddFunction<ClientGamePhaseEndedParameters>(
                "ClientGamePhaseEnded",
                "/Script/ShooterGame.AresPlayerController:ClientGamePhaseEnded",
                ExportCategory.GameState)
            .Decode(ValorantPayloadDecoders.RawRpc("ClientGamePhaseEnded"));
    }
}

public sealed class ClientReplayReceiveInputEventProcessingCaptureParameters
    : ExportGroupDescriptor<ClientReplayReceiveInputEventProcessingCaptureParameters>
{
    public override string Path =>
        "/Script/ShooterGame.ReplayPlayerController:ClientReplayReceiveInputEventProcessingCapture";
    public override ExportCategory Categories => ExportCategory.Debug;
    public override ExportGroupKind Kind => ExportGroupKind.ClassNetCache;
    public override FieldStreamGrammar Grammar => FieldStreamGrammar.FunctionParameters;

    public int PlayerId { get; set; }
    public byte[]? InputEventData { get; set; }

    protected override void Configure()
    {
        AddProperty("PlayerID", x => x.PlayerId, ExportCategory.Debug).Int32OrRaw();
        AddProperty(x => x.InputEventData, ExportCategory.Debug)
            .ByteArrayOrRaw(ValorantPayloadDecoders.MaxInputEventBytes);
    }
}

public sealed class ClientGamePhaseBeginParameters : ExportGroupDescriptor<ClientGamePhaseBeginParameters>
{
    public override string Path => "/Script/ShooterGame.AresPlayerController:ClientGamePhaseBegin";
    public override ExportCategory Categories => ExportCategory.GameState;
    public override ExportGroupKind Kind => ExportGroupKind.ClassNetCache;
    public override FieldStreamGrammar Grammar => FieldStreamGrammar.FunctionParameters;

    public byte NewPhase { get; set; }

    protected override void Configure()
    {
        AddProperty(x => x.NewPhase, ExportCategory.GameState).EnumByteOrRaw();
    }
}

public sealed class ClientGamePhaseEndedParameters : ExportGroupDescriptor<ClientGamePhaseEndedParameters>
{
    public override string Path => "/Script/ShooterGame.AresPlayerController:ClientGamePhaseEnded";
    public override ExportCategory Categories => ExportCategory.GameState;
    public override ExportGroupKind Kind => ExportGroupKind.ClassNetCache;
    public override FieldStreamGrammar Grammar => FieldStreamGrammar.FunctionParameters;

    public byte OldPhase { get; set; }

    protected override void Configure()
    {
        AddProperty(x => x.OldPhase, ExportCategory.GameState).EnumByteOrRaw();
    }
}
