using Replay.Models.Descriptors;
using Replay.Unreal.Parsing;
using Replay.Valorant.Movement;

namespace Replay.Valorant.Descriptors;

internal sealed class BaseReplayControllerClassNetCacheDescriptor : ClassNetCacheDescriptor<BaseReplayControllerClassNetCacheDescriptor>
{
    public override string Path => "/Game/Characters/_Core/BaseReplayController.BaseReplayController_C_ClassNetCache";

    protected override void Configure()
    {
        var inputCapture = AddFunction(
            "ClientReplayReceiveInputEventProcessingCapture",
            "/Script/ShooterGame.ReplayPlayerController:ClientReplayReceiveInputEventProcessingCapture",
            ExportCategory.Debug);
        inputCapture.AddField("PlayerID", "PlayerId", ExportCategory.Debug).Int32OrRaw();
        inputCapture.AddField("InputEventData", "InputEventData", ExportCategory.Debug)
            .ByteArrayOrRaw(ValorantPayloadDecoders.MaxInputEventBytes);
        inputCapture.Decode(ValorantPayloadDecoders.RawRpc("ClientReplayReceiveInputEventProcessingCapture"));

        AddFunction(
                "ReplaysClientReceiveRemoteCharacterUpdatesSingleArrayNoAutonomous",
                "/Script/ShooterGame.ReplayPlayerController:ReplaysClientReceiveRemoteCharacterUpdatesSingleArrayNoAutonomous",
                ExportCategory.Movement)
            .Decode(RemoteCharacterUpdatesRpcDecoder.Instance);

        var gamePhaseBegin = AddFunction(
            "ClientGamePhaseBegin",
            "/Script/ShooterGame.AresPlayerController:ClientGamePhaseBegin",
            ExportCategory.GameState);
        gamePhaseBegin.AddField("NewPhase", "NewPhase", ExportCategory.GameState).EnumByteOrRaw();
        gamePhaseBegin.Decode(ValorantPayloadDecoders.RawRpc("ClientGamePhaseBegin"));

        var gamePhaseEnded = AddFunction(
            "ClientGamePhaseEnded",
            "/Script/ShooterGame.AresPlayerController:ClientGamePhaseEnded",
            ExportCategory.GameState);
        gamePhaseEnded.AddField("OldPhase", "OldPhase", ExportCategory.GameState).EnumByteOrRaw();
        gamePhaseEnded.Decode(ValorantPayloadDecoders.RawRpc("ClientGamePhaseEnded"));
    }
}
