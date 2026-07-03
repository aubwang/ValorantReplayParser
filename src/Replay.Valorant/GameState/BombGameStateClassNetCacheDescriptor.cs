using Replay.Models.Descriptors;
using Replay.Unreal.Parsing;
using Replay.Valorant.Descriptors;

namespace Replay.Valorant.GameState;

internal sealed class BombGameStateClassNetCacheDescriptor : ClassNetCacheDescriptor<BombGameStateClassNetCacheDescriptor>
{
    public override string Path => "/Game/GameModes/Bomb/BombGameState.BombGameState_C_ClassNetCache";

    protected override void Configure()
    {
        AddFunction("ClientBuyPhaseEnd", "/Game/GameModes/Bomb/BombGameState.BombGameState_C:ClientBuyPhaseEnd",
                ExportCategory.GameState)
            .Decode(ValorantPayloadDecoders.NoParametersRpc);
        AddFunction("ClientRoundStart", "/Game/GameModes/Bomb/BombGameState.BombGameState_C:ClientRoundStart",
                ExportCategory.GameState)
            .Decode(ValorantPayloadDecoders.NoParametersRpc);
        AddFunction("Multicast Side Switch Event",
                "/Game/GameModes/Bomb/BombGameState.BombGameState_C:Multicast Side Switch Event",
                ExportCategory.GameState)
            .Decode(ValorantPayloadDecoders.NoParametersRpc);
        AddFunction("ClientResetRound", "/Script/ShooterGame.ShooterGameState:ClientResetRound", ExportCategory.GameState)
            .Decode(ValorantPayloadDecoders.NoParametersRpc);

        var endRound = AddFunction(
            "MulticastEndRound",
            "/Script/ShooterGame.ShooterGameState:MulticastEndRound",
            ExportCategory.GameState);
        endRound.AddField("NewRoundNumber", "NewRoundNumber", ExportCategory.GameState).Int32OrRaw();
        endRound.Decode(ValorantPayloadDecoders.RawRpc("MulticastEndRound"));

        var enterPlayspace = AddFunction(
            "MulticastEnterPlayspace",
            "/Script/ShooterGame.ShooterGameState:MulticastEnterPlayspace",
            ExportCategory.GameState);
        enterPlayspace.AddField("PlayspaceComponent", "PlayspaceComponent", ExportCategory.GameState).ObjectNetGuidOrRaw();
        enterPlayspace.AddField("NewPlayspace", "NewPlayspace", ExportCategory.GameState).ObjectNetGuidOrRaw();
        enterPlayspace.AddField("bLeaveCurrentPlayspaces", "LeaveCurrentPlayspaces", ExportCategory.GameState).BoolOrRaw();
        enterPlayspace.AddField("bExecuteOnOwner", "ExecuteOnOwner", ExportCategory.GameState).BoolOrRaw();
        enterPlayspace.Decode(ValorantPayloadDecoders.RawRpc("MulticastEnterPlayspace"));

        var resurrect = AddFunction(
            "MulticastReceivePlayerResurrectEvent",
            "/Script/ShooterGame.ShooterGameState:MulticastReceivePlayerResurrectEvent",
            ExportCategory.GameState | ExportCategory.Gunplay);
        resurrect.AddField("ResurrectorPlayer", "ResurrectorPlayer", ExportCategory.GameState).ObjectNetGuidOrRaw();
        resurrect.AddField("ResurrectedPlayer", "ResurrectedPlayer", ExportCategory.GameState).ObjectNetGuidOrRaw();
        resurrect.AddField("KillNumberInRoundForResurrector", "KillNumberInRoundForResurrector",
            ExportCategory.Gunplay).Int32OrRaw();
        resurrect.AddField("KillNumberInRoundForResurrected", "KillNumberInRoundForResurrected",
            ExportCategory.Gunplay).Int32OrRaw();

        AddTemporaryDeathBase();
        AddTemporaryDeathPoint();

        var setPhase = AddFunction(
            "MulticastSetPhase",
            "/Script/ShooterGame.ShooterGameState:MulticastSetPhase",
            ExportCategory.GameState);
        setPhase.AddField("NewPhase", "NewPhase", ExportCategory.GameState).EnumByte();
        setPhase.Decode(ValorantPayloadDecoders.RawRpc("MulticastSetPhase"));

        var resetForRespawn = AddFunction(
            "MulticastResetForRespawn",
            "/Script/ShooterGame.AresGameStateBase:MulticastResetForRespawn",
            ExportCategory.GameState);
        resetForRespawn.AddField("ShooterCharacter", "ShooterCharacter", ExportCategory.GameState).ObjectNetGuidOrRaw();
        resetForRespawn.AddField("SpawnTransform", "SpawnTransform", ExportCategory.GameState)
            .Decode(ValorantPayloadDecoders.RawPayload("FTransform"));
    }

    private void AddTemporaryDeathBase()
    {
        var rpc = AddFunction(
            "MulticastReceivePlayerTemporaryDeathEvent_Base",
            "/Script/ShooterGame.ShooterGameState:MulticastReceivePlayerTemporaryDeathEvent_Base",
            ExportCategory.GameState | ExportCategory.Gunplay);
        rpc.AddField("DamagerPlayer", "DamagerPlayer", ExportCategory.Gunplay).ObjectNetGuidOrRaw();
        rpc.AddField("DownedPlayer", "DownedPlayer", ExportCategory.Gunplay).ObjectNetGuidOrRaw();
        rpc.AddField("DamageResponseData", "DamageResponseData", ExportCategory.Gunplay)
            .Decode(ValorantPayloadDecoders.RawPayload("FNetworkedDamageResponseData"));
        rpc.AddField("EquippableUsed", "EquippableUsed", ExportCategory.Inventory | ExportCategory.Gunplay)
            .ObjectNetGuidOrRaw();
        rpc.AddField("bRecoversInstantly", "RecoversInstantly", ExportCategory.GameState).BoolOrRaw();
    }

    private void AddTemporaryDeathPoint()
    {
        var rpc = AddFunction(
            "MulticastReceivePlayerTemporaryDeathEvent_Point",
            "/Script/ShooterGame.ShooterGameState:MulticastReceivePlayerTemporaryDeathEvent_Point",
            ExportCategory.GameState | ExportCategory.Gunplay);
        rpc.AddField("DamagerPlayer", "DamagerPlayer", ExportCategory.Gunplay).ObjectNetGuidOrRaw();
        rpc.AddField("DownedPlayer", "DownedPlayer", ExportCategory.Gunplay).ObjectNetGuidOrRaw();
        rpc.AddField("PointDamageResponseData", "PointDamageResponseData", ExportCategory.Gunplay)
            .Decode(ValorantPayloadDecoders.RawPayload("FNetworkedPointDamageResponseData"));
        rpc.AddField("EquippableUsed", "EquippableUsed", ExportCategory.Inventory | ExportCategory.Gunplay)
            .ObjectNetGuidOrRaw();
        rpc.AddField("bRecoversInstantly", "RecoversInstantly", ExportCategory.GameState).BoolOrRaw();
    }
}
