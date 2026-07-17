using Replay.Models.Descriptors;
using Replay.Valorant.Descriptors;

namespace Replay.Valorant.GameState;

internal sealed class BombPlayerStateDescriptor : ExportGroupDescriptor<BombPlayerStateDescriptor>
{
    public override string Path => "/Game/GameModes/Bomb/BombPlayerState.BombPlayerState_C";
    public override ExportCategory Categories => ExportCategory.GameState;
    public override ExportGroupKind Kind => ExportGroupKind.Actor;

    public int PlayerId { get; set; }
    public ValorantRawPayload? UniqueId { get; set; }
    public int CompetitiveTier { get; set; }
    public string? Subject { get; set; }
    public uint SpectatedPlayer { get; set; }
    public uint PlayerInfo { get; set; }
    public uint SpawnedCharacter { get; set; }
    public uint PossessedCharacter { get; set; }
    public bool UltimateActive { get; set; }
    public int NumUltimatePoints { get; set; }
    public int TotalAcquiredUltimatePoints { get; set; }

    protected override void Configure()
    {
        AddProperty("PlayerId", x => x.PlayerId).Int32OrRaw();
        AddProperty("PlayerID", x => x.PlayerId).Int32OrRaw();
        AddProperty(x => x.UniqueId).Decode(ValorantPayloadDecoders.RawPayload("FUniqueNetIdRepl"));
        AddProperty(x => x.CompetitiveTier).Int32OrRaw();
        AddProperty(x => x.Subject).FStringOrRaw();
        AddProperty(x => x.SpectatedPlayer).ObjectNetGuidOrRaw();
        AddProperty(x => x.PlayerInfo).ObjectNetGuidOrRaw();
        AddProperty(x => x.SpawnedCharacter).ObjectNetGuidOrRaw();
        AddProperty(x => x.PossessedCharacter).ObjectNetGuidOrRaw();
        AddProperty("bUltimateActive", x => x.UltimateActive).BoolOrRaw();
        AddProperty(x => x.NumUltimatePoints).Int32OrRaw();
        AddProperty(x => x.TotalAcquiredUltimatePoints).Int32OrRaw();
    }
}

internal sealed class BombGameStateDescriptor : ExportGroupDescriptor<BombGameStateDescriptor>
{
    public override string Path => "/Game/GameModes/Bomb/BombGameState.BombGameState_C";
    public override ExportCategory Categories => ExportCategory.GameState;
    public override ExportGroupKind Kind => ExportGroupKind.Actor;

    public double ReplicatedWorldTimeSecondsDouble { get; set; }
    public string? MatchState { get; set; }
    public uint WinningTeam { get; set; }
    public byte CompletionState { get; set; }
    public object? TeamEconomy { get; set; }
    public float DisplayRemainingTime { get; set; }
    public float StateRemainingTime { get; set; }
    public float GamePhaseElapsedTime { get; set; }
    public float AuthGameplayStartTimestamp { get; set; }
    public float AuthGameplayEndTimestamp { get; set; }
    public int NetServerMaxTickRate { get; set; }
    public string? MatchID { get; set; }
    public object? RoundResults { get; set; }
    public byte Phase { get; set; }
    public ValorantRawPayload? RoundParticipantsInfos { get; set; }
    public int RoundNumber { get; set; }
    public byte BombState { get; set; }

    protected override void Configure()
    {
        AddProperty(x => x.ReplicatedWorldTimeSecondsDouble).DoubleOrRaw();
        AddProperty(x => x.MatchState).FNameOrRaw();
        AddProperty(x => x.WinningTeam).ObjectNetGuidOrRaw();
        AddProperty(x => x.CompletionState).EnumByteOrRaw();
        AddProperty(x => x.TeamEconomy).Decode(new CompatibleAresTeamEconomyDecoder());
        AddProperty(x => x.DisplayRemainingTime).FloatOrRaw();
        AddProperty(x => x.StateRemainingTime).FloatOrRaw();
        AddProperty(x => x.GamePhaseElapsedTime).FloatOrRaw();
        AddProperty(x => x.AuthGameplayStartTimestamp).FloatOrRaw();
        AddProperty(x => x.AuthGameplayEndTimestamp).FloatOrRaw();
        AddProperty(x => x.NetServerMaxTickRate).Int32OrRaw();
        AddProperty(x => x.MatchID).FStringOrRaw();
        AddProperty(x => x.RoundResults).Decode(new CompatibleAresRoundResultsDecoder());
        AddProperty(x => x.Phase).EnumByteOrRaw();
        AddProperty(x => x.RoundParticipantsInfos)
            .Decode(ValorantPayloadDecoders.RawPayload("TArray<FRoundParticipantsInfo>"));
        AddProperty(x => x.RoundNumber).Int32OrRaw();
        AddProperty(x => x.BombState).EnumByteOrRaw();
    }
}

internal sealed class BombCombatReportComponentDescriptor : ExportGroupDescriptor<BombCombatReportComponentDescriptor>
{
    public override string Path => "/Game/GameModes/Bomb/Bomb_CombatReportComponent.Bomb_CombatReportComponent_C";
    public override ExportCategory Categories => ExportCategory.GameState | ExportCategory.Gunplay;
    public override ExportGroupKind Kind => ExportGroupKind.Component;

    public object? Rounds { get; set; }
    public int RoundNum { get; set; }
    public ValorantRawPayload? Reports { get; set; }
    public int RoundNumber { get; set; }
    public float StateRemainingTime { get; set; }
    public float GameTime { get; set; }
    public byte GamePhase { get; set; }
    public ValorantRawPayload? Interactions { get; set; }
    public string? ParticipantSubject { get; set; }
    public string? ParticipantTeamName { get; set; }
    public uint ParticipantCharacterIcon { get; set; }
    public float DamageDealt { get; set; }
    public int HitsDealt { get; set; }
    public float DamageRecieved { get; set; }
    public int HitsRecieved { get; set; }
    public bool DidKill { get; set; }
    public byte AssistType { get; set; }
    public uint ParticipantsKillerState { get; set; }
    public bool WasKiller { get; set; }
    public ValorantRawPayload? DealtIteractions { get; set; }
    public uint DamageType { get; set; }
    public ValorantRawPayload? RegionalDamageInteractions { get; set; }
    public byte Region { get; set; }
    public int Hits { get; set; }
    public float Damage { get; set; }
    public bool IsWallPen { get; set; }
    public bool IsKill { get; set; }
    public uint DestroyedArmor { get; set; }
    public ValorantRawPayload? ReceivedInteractions { get; set; }
    public int CombatReportIndex { get; set; }
    public uint ResurrectorPlayerState { get; set; }
    public bool Died { get; set; }
    public ValorantRawPayload? DeathLocation { get; set; }

    protected override void Configure()
    {
        AddProperty(x => x.Rounds).Decode(new CompatibleCombatRoundReportsDecoder());
        AddProperty(x => x.RoundNum).Int32OrRaw();
        AddProperty(x => x.Reports).Decode(ValorantPayloadDecoders.RawPayload("TArray<FCharacterCombatReport>"));
        AddProperty(x => x.RoundNumber).Int32OrRaw();
        AddProperty(x => x.StateRemainingTime).FloatOrRaw();
        AddProperty(x => x.GameTime).FloatOrRaw();
        AddProperty(x => x.GamePhase).EnumByteOrRaw();
        AddProperty(x => x.Interactions).Decode(ValorantPayloadDecoders.RawPayload("TArray<FParticipantInteractions>"));
        AddProperty(x => x.ParticipantSubject).FStringOrRaw();
        AddProperty(x => x.ParticipantTeamName).FNameOrRaw();
        AddProperty(x => x.ParticipantCharacterIcon).ObjectNetGuidOrRaw();
        AddProperty(x => x.DamageDealt).FloatOrRaw();
        AddProperty(x => x.HitsDealt).Int32OrRaw();
        AddProperty(x => x.DamageRecieved).FloatOrRaw();
        AddProperty(x => x.HitsRecieved).Int32OrRaw();
        AddProperty("bDidKill", x => x.DidKill).BoolOrRaw();
        AddProperty(x => x.AssistType).EnumByteOrRaw();
        AddProperty(x => x.ParticipantsKillerState).ObjectNetGuidOrRaw();
        AddProperty("bWasKiller", x => x.WasKiller).BoolOrRaw();
        AddProperty(x => x.DealtIteractions).Decode(ValorantPayloadDecoders.RawPayload("TArray<FCombatInteraction>"));
        AddProperty(x => x.DamageType).ObjectNetGuidOrRaw();
        AddProperty(x => x.RegionalDamageInteractions)
            .Decode(ValorantPayloadDecoders.RawPayload("TArray<FRegionalDamageInteraction>"));
        AddProperty(x => x.Region).EnumByteOrRaw();
        AddProperty(x => x.Hits).Int32OrRaw();
        AddProperty(x => x.Damage).FloatOrRaw();
        AddProperty("bIsWallPen", x => x.IsWallPen).BoolOrRaw();
        AddProperty("bIsKill", x => x.IsKill).BoolOrRaw();
        AddProperty(x => x.DestroyedArmor).ObjectNetGuidOrRaw();
        AddProperty(x => x.ReceivedInteractions).Decode(ValorantPayloadDecoders.RawPayload("TArray<FCombatInteraction>"));
        AddProperty(x => x.CombatReportIndex).Int32OrRaw();
        AddProperty(x => x.ResurrectorPlayerState).ObjectNetGuidOrRaw();
        AddProperty("bDied", x => x.Died).BoolOrRaw();
        AddProperty(x => x.DeathLocation).FVectorOrRaw();
    }
}

internal sealed class AresInventoryDescriptor : ExportGroupDescriptor<AresInventoryDescriptor>
{
    public override string Path => "/Script/ShooterGame.AresInventory";
    public override ExportCategory Categories => ExportCategory.Inventory;
    public override ExportGroupKind Kind => ExportGroupKind.Component;

    public bool IsActive { get; set; }
    public ValorantRawPayload? ItemSlots { get; set; }
    public uint NewCurrentEquippable { get; set; }
    public uint Character { get; set; }
    public float NetTimestamp { get; set; }
    public int RespawnNumber { get; set; }
    public uint CurrentEquippable { get; set; }

    protected override void Configure()
    {
        AddProperty("bIsActive", x => x.IsActive).BoolOrRaw();
        AddProperty(x => x.ItemSlots).Decode(ValorantPayloadDecoders.RawPayload("UItemSlot*[16]"));
        AddProperty(x => x.NewCurrentEquippable).ObjectNetGuidOrRaw();
        AddProperty(x => x.Character).ObjectNetGuidOrRaw();
        AddProperty(x => x.NetTimestamp).FloatOrRaw();
        AddProperty(x => x.RespawnNumber).Int32OrRaw();
        AddProperty(x => x.CurrentEquippable).ObjectNetGuidOrRaw();
    }
}

internal sealed class EquippableStateMachineComponentDescriptor
    : ExportGroupDescriptor<EquippableStateMachineComponentDescriptor>
{
    public override string Path => "/Script/ShooterGame.EquippableStateMachineComponent";
    public override ExportCategory Categories => ExportCategory.Inventory | ExportCategory.Gunplay;
    public override ExportGroupKind Kind => ExportGroupKind.Component;

    public uint CurrentState { get; set; }
    public ValorantRawPayload? TransitionContext { get; set; }
    public float AuthStartWorldTime { get; set; }

    protected override void Configure()
    {
        AddProperty(x => x.CurrentState).ObjectNetGuidOrRaw();
        AddProperty(x => x.TransitionContext).Decode(ValorantPayloadDecoders.RawPayload("UTransitionContext"));
        AddProperty(x => x.AuthStartWorldTime).FloatOrRaw();
    }
}

internal sealed class AmmoComponentDescriptor : ExportGroupDescriptor<AmmoComponentDescriptor>
{
    public override string Path => "/Script/ShooterGame.AmmoComponent";
    public override ExportCategory Categories => ExportCategory.Inventory | ExportCategory.Gunplay;
    public override ExportGroupKind Kind => ExportGroupKind.Component;

    public int AuthResourceAmount { get; set; }

    protected override void Configure()
    {
        AddProperty(x => x.AuthResourceAmount).Int32OrRaw();
    }
}

internal sealed class ChildDamageSectionComponentDescriptor
    : ExportGroupDescriptor<ChildDamageSectionComponentDescriptor>
{
    public override string Path => "/Script/ShooterGame.ChildDamageSectionComponent";
    public override ExportCategory Categories => ExportCategory.Gunplay;
    public override ExportGroupKind Kind => ExportGroupKind.Component;

    public bool Alive { get; set; }
    public float Life { get; set; }
    public float MaximumLife { get; set; }

    protected override void Configure()
    {
        AddProperty("bAlive", x => x.Alive).BoolOrRaw();
        AddProperty(x => x.Life).FloatOrRaw();
        AddProperty(x => x.MaximumLife).FloatOrRaw();
    }
}

internal sealed class ChildRegionDamageSectionComponentDescriptor
    : ExportGroupDescriptor<ChildRegionDamageSectionComponentDescriptor>
{
    public override string Path => "/Script/ShooterGame.ChildRegionDamageSectionComponent";
    public override ExportCategory Categories => ExportCategory.Gunplay;
    public override ExportGroupKind Kind => ExportGroupKind.Component;

    public bool Alive { get; set; }
    public float Life { get; set; }
    public float MaximumLife { get; set; }

    protected override void Configure()
    {
        AddProperty("bAlive", x => x.Alive).BoolOrRaw();
        AddProperty(x => x.Life).FloatOrRaw();
        AddProperty(x => x.MaximumLife).FloatOrRaw();
    }
}

internal sealed class AttachedDamageSectionComponentDescriptor
    : ExportGroupDescriptor<AttachedDamageSectionComponentDescriptor>
{
    public override string Path => "/Script/ShooterGame.AttachedDamageSectionComponent";
    public override ExportCategory Categories => ExportCategory.Gunplay;
    public override ExportGroupKind Kind => ExportGroupKind.Component;

    public bool Alive { get; set; }
    public float Life { get; set; }
    public float MaximumLife { get; set; }

    protected override void Configure()
    {
        AddProperty("bAlive", x => x.Alive).BoolOrRaw();
        AddProperty(x => x.Life).FloatOrRaw();
        AddProperty(x => x.MaximumLife).FloatOrRaw();
    }
}

internal sealed class FiringStateComponentDescriptor : ExportGroupDescriptor<FiringStateComponentDescriptor>
{
    public override string Path => "/Script/ShooterGame.FiringStateComponent";
    public override ExportCategory Categories => ExportCategory.Gunplay;
    public override ExportGroupKind Kind => ExportGroupKind.Component;

    public ValorantRawPayload? NetworkGameplayTagNodeIndex { get; set; }
    public int AmmoRemaining { get; set; }
    public ValorantRawPayload? AttackVector { get; set; }
    public int BurstShotNumber { get; set; }
    public uint FiringPlayerState { get; set; }
    public byte FiringState { get; set; }
    public int NumProjectiles { get; set; }
    public int RandomSeed { get; set; }
    public byte TracerOption { get; set; }
    public byte YawSwitch { get; set; }

    protected override void Configure()
    {
        AddProperty(x => x.NetworkGameplayTagNodeIndex)
            .Decode(ValorantPayloadDecoders.RawPayload("FGameplayTagNetIndex"));
        AddProperty("FiringState.AmmoRemaining", x => x.AmmoRemaining).Int32OrRaw();
        for (var i = 1; i <= 16; i++)
        {
            AddProperty($"FiringState.AttackVector.{i}", x => x.AttackVector)
                .Decode(ValorantPayloadDecoders.RawPayload("FiringState.AttackVector"));
        }

        AddProperty("FiringState.BurstShotNumber", x => x.BurstShotNumber).Int32OrRaw();
        AddProperty("FiringState.FiringPlayerState", x => x.FiringPlayerState).ObjectNetGuidOrRaw();
        AddProperty("FiringState.FiringState", x => x.FiringState).EnumByteOrRaw();
        AddProperty("FiringState.NumProjectiles", x => x.NumProjectiles).Int32OrRaw();
        AddProperty("FiringState.RandomSeed", x => x.RandomSeed).Int32OrRaw();
        AddProperty("FiringState.TracerOption", x => x.TracerOption).EnumByteOrRaw();
        AddProperty("FiringState.YawSwitch", x => x.YawSwitch).EnumByteOrRaw();
    }
}
