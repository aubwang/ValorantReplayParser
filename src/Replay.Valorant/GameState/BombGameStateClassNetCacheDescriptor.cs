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

        AddFunction<MulticastEndRoundParameters>(
                "MulticastEndRound",
                "/Script/ShooterGame.ShooterGameState:MulticastEndRound",
                ExportCategory.GameState)
            .Decode(ValorantPayloadDecoders.RawRpc("MulticastEndRound"));

        AddFunction<MulticastEnterPlayspaceParameters>(
                "MulticastEnterPlayspace",
                "/Script/ShooterGame.ShooterGameState:MulticastEnterPlayspace",
                ExportCategory.GameState)
            .Decode(ValorantPayloadDecoders.RawRpc("MulticastEnterPlayspace"));

        AddFunction<MulticastReceivePlayerResurrectEventParameters>(
            "MulticastReceivePlayerResurrectEvent",
            "/Script/ShooterGame.ShooterGameState:MulticastReceivePlayerResurrectEvent",
            ExportCategory.GameState | ExportCategory.Gunplay);

        AddTemporaryDeathBase();
        AddTemporaryDeathPoint();

        AddFunction<MulticastSetPhaseParameters>(
                "MulticastSetPhase",
                "/Script/ShooterGame.ShooterGameState:MulticastSetPhase",
                ExportCategory.GameState)
            .Decode(ValorantPayloadDecoders.RawRpc("MulticastSetPhase"));

        AddFunction<MulticastResetForRespawnParameters>(
            "MulticastResetForRespawn",
            "/Script/ShooterGame.AresGameStateBase:MulticastResetForRespawn",
            ExportCategory.GameState);
    }

    private void AddTemporaryDeathBase()
    {
        AddFunction<MulticastReceivePlayerTemporaryDeathEventBaseParameters>(
            "MulticastReceivePlayerTemporaryDeathEvent_Base",
            "/Script/ShooterGame.ShooterGameState:MulticastReceivePlayerTemporaryDeathEvent_Base",
            ExportCategory.GameState | ExportCategory.Gunplay);
    }

    private void AddTemporaryDeathPoint()
    {
        AddFunction<MulticastReceivePlayerTemporaryDeathEventPointParameters>(
            "MulticastReceivePlayerTemporaryDeathEvent_Point",
            "/Script/ShooterGame.ShooterGameState:MulticastReceivePlayerTemporaryDeathEvent_Point",
            ExportCategory.GameState | ExportCategory.Gunplay);
    }
}

public sealed class MulticastEndRoundParameters : ExportGroupDescriptor<MulticastEndRoundParameters>
{
    public override string Path => "/Script/ShooterGame.ShooterGameState:MulticastEndRound";
    public override ExportCategory Categories => ExportCategory.GameState;
    public override ExportGroupKind Kind => ExportGroupKind.ClassNetCache;
    public override FieldStreamGrammar Grammar => FieldStreamGrammar.FunctionParameters;

    public int NewRoundNumber { get; set; }

    protected override void Configure()
    {
        AddProperty(x => x.NewRoundNumber, ExportCategory.GameState).Int32OrRaw();
    }
}

public sealed class MulticastEnterPlayspaceParameters : ExportGroupDescriptor<MulticastEnterPlayspaceParameters>
{
    public override string Path => "/Script/ShooterGame.ShooterGameState:MulticastEnterPlayspace";
    public override ExportCategory Categories => ExportCategory.GameState;
    public override ExportGroupKind Kind => ExportGroupKind.ClassNetCache;
    public override FieldStreamGrammar Grammar => FieldStreamGrammar.FunctionParameters;

    public uint PlayspaceComponent { get; set; }
    public uint NewPlayspace { get; set; }
    public bool LeaveCurrentPlayspaces { get; set; }
    public bool ExecuteOnOwner { get; set; }

    protected override void Configure()
    {
        AddProperty(x => x.PlayspaceComponent, ExportCategory.GameState).ObjectNetGuidOrRaw();
        AddProperty(x => x.NewPlayspace, ExportCategory.GameState).ObjectNetGuidOrRaw();
        AddProperty("bLeaveCurrentPlayspaces", x => x.LeaveCurrentPlayspaces, ExportCategory.GameState).BoolOrRaw();
        AddProperty("bExecuteOnOwner", x => x.ExecuteOnOwner, ExportCategory.GameState).BoolOrRaw();
    }
}

public sealed class MulticastReceivePlayerResurrectEventParameters
    : ExportGroupDescriptor<MulticastReceivePlayerResurrectEventParameters>
{
    public override string Path => "/Script/ShooterGame.ShooterGameState:MulticastReceivePlayerResurrectEvent";
    public override ExportCategory Categories => ExportCategory.GameState | ExportCategory.Gunplay;
    public override ExportGroupKind Kind => ExportGroupKind.ClassNetCache;
    public override FieldStreamGrammar Grammar => FieldStreamGrammar.FunctionParameters;

    public uint ResurrectorPlayer { get; set; }
    public uint ResurrectedPlayer { get; set; }
    public int KillNumberInRoundForResurrector { get; set; }
    public int KillNumberInRoundForResurrected { get; set; }

    protected override void Configure()
    {
        AddProperty(x => x.ResurrectorPlayer, ExportCategory.GameState).ObjectNetGuidOrRaw();
        AddProperty(x => x.ResurrectedPlayer, ExportCategory.GameState).ObjectNetGuidOrRaw();
        AddProperty(x => x.KillNumberInRoundForResurrector, ExportCategory.Gunplay).Int32OrRaw();
        AddProperty(x => x.KillNumberInRoundForResurrected, ExportCategory.Gunplay).Int32OrRaw();
    }
}

public sealed class MulticastSetPhaseParameters : ExportGroupDescriptor<MulticastSetPhaseParameters>
{
    public override string Path => "/Script/ShooterGame.ShooterGameState:MulticastSetPhase";
    public override ExportCategory Categories => ExportCategory.GameState;
    public override ExportGroupKind Kind => ExportGroupKind.ClassNetCache;
    public override FieldStreamGrammar Grammar => FieldStreamGrammar.FunctionParameters;

    public byte NewPhase { get; set; }

    protected override void Configure()
    {
        AddProperty(x => x.NewPhase, ExportCategory.GameState).EnumByte();
    }
}

public sealed class MulticastResetForRespawnParameters : ExportGroupDescriptor<MulticastResetForRespawnParameters>
{
    public override string Path => "/Script/ShooterGame.AresGameStateBase:MulticastResetForRespawn";
    public override ExportCategory Categories => ExportCategory.GameState;
    public override ExportGroupKind Kind => ExportGroupKind.ClassNetCache;
    public override FieldStreamGrammar Grammar => FieldStreamGrammar.FunctionParameters;

    public uint ShooterCharacter { get; set; }
    public ValorantRawPayload? SpawnTransform { get; set; }

    protected override void Configure()
    {
        AddProperty(x => x.ShooterCharacter, ExportCategory.GameState).ObjectNetGuidOrRaw();
        AddProperty(x => x.SpawnTransform, ExportCategory.GameState)
            .Decode(ValorantPayloadDecoders.RawPayload("FTransform"));
    }
}

public sealed class MulticastReceivePlayerTemporaryDeathEventBaseParameters
    : ExportGroupDescriptor<MulticastReceivePlayerTemporaryDeathEventBaseParameters>
{
    public override string Path => "/Script/ShooterGame.ShooterGameState:MulticastReceivePlayerTemporaryDeathEvent_Base";
    public override ExportCategory Categories => ExportCategory.GameState | ExportCategory.Gunplay;
    public override ExportGroupKind Kind => ExportGroupKind.ClassNetCache;
    public override FieldStreamGrammar Grammar => FieldStreamGrammar.FunctionParameters;

    public uint DamagerPlayer { get; set; }
    public uint DownedPlayer { get; set; }
    public ValorantRawPayload? DamageResponseData { get; set; }
    public uint EquippableUsed { get; set; }
    public bool RecoversInstantly { get; set; }

    protected override void Configure()
    {
        AddProperty(x => x.DamagerPlayer, ExportCategory.Gunplay).ObjectNetGuidOrRaw();
        AddProperty(x => x.DownedPlayer, ExportCategory.Gunplay).ObjectNetGuidOrRaw();
        AddProperty(x => x.DamageResponseData, ExportCategory.Gunplay)
            .Decode(ValorantPayloadDecoders.RawPayload("FNetworkedDamageResponseData"));
        AddProperty(x => x.EquippableUsed, ExportCategory.Inventory | ExportCategory.Gunplay)
            .ObjectNetGuidOrRaw();
        AddProperty("bRecoversInstantly", x => x.RecoversInstantly, ExportCategory.GameState).BoolOrRaw();
    }
}

public sealed class MulticastReceivePlayerTemporaryDeathEventPointParameters
    : ExportGroupDescriptor<MulticastReceivePlayerTemporaryDeathEventPointParameters>
{
    public override string Path => "/Script/ShooterGame.ShooterGameState:MulticastReceivePlayerTemporaryDeathEvent_Point";
    public override ExportCategory Categories => ExportCategory.GameState | ExportCategory.Gunplay;
    public override ExportGroupKind Kind => ExportGroupKind.ClassNetCache;
    public override FieldStreamGrammar Grammar => FieldStreamGrammar.FunctionParameters;

    public uint DamagerPlayer { get; set; }
    public uint DownedPlayer { get; set; }
    public ValorantRawPayload? PointDamageResponseData { get; set; }
    public uint EquippableUsed { get; set; }
    public bool RecoversInstantly { get; set; }

    protected override void Configure()
    {
        AddProperty(x => x.DamagerPlayer, ExportCategory.Gunplay).ObjectNetGuidOrRaw();
        AddProperty(x => x.DownedPlayer, ExportCategory.Gunplay).ObjectNetGuidOrRaw();
        AddProperty(x => x.PointDamageResponseData, ExportCategory.Gunplay)
            .Decode(ValorantPayloadDecoders.RawPayload("FNetworkedPointDamageResponseData"));
        AddProperty(x => x.EquippableUsed, ExportCategory.Inventory | ExportCategory.Gunplay)
            .ObjectNetGuidOrRaw();
        AddProperty("bRecoversInstantly", x => x.RecoversInstantly, ExportCategory.GameState).BoolOrRaw();
    }
}
