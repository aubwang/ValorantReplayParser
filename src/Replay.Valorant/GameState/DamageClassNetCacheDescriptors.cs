using Replay.Models.Descriptors;
using Replay.Valorant.Descriptors;

namespace Replay.Valorant.GameState;

internal sealed class ChildDamageSectionClassNetCacheDescriptor
    : ClassNetCacheDescriptor<ChildDamageSectionClassNetCacheDescriptor>
{
    public override string Path => "/Script/ShooterGame.ChildDamageSectionComponent_ClassNetCache";

    protected override void Configure()
    {
        AddFunction<MulticastNotifySetLifeParameters>(
            "MulticastNotifySetLife",
            "/Script/ShooterGame.DamageSectionComponent:MulticastNotifySetLife",
            ExportCategory.Gunplay);
    }
}

internal sealed class AttachedDamageSectionClassNetCacheDescriptor
    : ClassNetCacheDescriptor<AttachedDamageSectionClassNetCacheDescriptor>
{
    public override string Path => "/Script/ShooterGame.AttachedDamageSectionComponent_ClassNetCache";

    protected override void Configure()
    {
        AddFunction<MulticastNotifySetLifeParameters>(
            "MulticastNotifySetLife",
            "/Script/ShooterGame.DamageSectionComponent:MulticastNotifySetLife",
            ExportCategory.Gunplay);
    }
}

internal sealed class ArmorDamageSectionClassNetCacheDescriptor
    : ClassNetCacheDescriptor<ArmorDamageSectionClassNetCacheDescriptor>
{
    public override string Path =>
        "/Game/Gear/BasicArmorAttachedDamageSection.BasicArmorAttachedDamageSection_C_ClassNetCache";

    protected override void Configure()
    {
        AddFunction<MulticastNotifySetLifeParameters>(
            "MulticastNotifySetLife",
            "/Script/ShooterGame.DamageSectionComponent:MulticastNotifySetLife",
            ExportCategory.Gunplay);
    }
}

internal sealed class DamageableComponentClassNetCacheDescriptor
    : ClassNetCacheDescriptor<DamageableComponentClassNetCacheDescriptor>
{
    public override string Path => "/Script/ShooterGame.DamageableComponent_ClassNetCache";

    protected override void Configure()
    {
        AddFunction<MulticastNotifyDamageBaseParameters>(
            "MulticastNotifyDamage_Base",
            "/Script/ShooterGame.DamageableComponent:MulticastNotifyDamage_Base",
            ExportCategory.Gunplay);
        AddFunction<MulticastNotifyDamagePointParameters>(
            "MulticastNotifyDamage_Point",
            "/Script/ShooterGame.DamageableComponent:MulticastNotifyDamage_Point",
            ExportCategory.Gunplay);
    }
}

internal sealed class MulticastNotifySetLifeParameters
    : ExportGroupDescriptor<MulticastNotifySetLifeParameters>
{
    public override string Path =>
        "/Script/ShooterGame.DamageSectionComponent:MulticastNotifySetLife";
    public override ExportCategory Categories => ExportCategory.Gunplay;
    public override ExportGroupKind Kind => ExportGroupKind.ClassNetCache;
    public override FieldStreamGrammar Grammar => FieldStreamGrammar.FunctionParameters;

    public float NewLife { get; set; }
    public bool NewAlive { get; set; }

    protected override void Configure()
    {
        AddProperty(x => x.NewLife).FloatOrRaw();
        AddProperty("bNewAlive", x => x.NewAlive).BoolOrRaw();
    }
}

internal abstract class DamageNotificationParameters<TDescriptor>
    : ExportGroupDescriptor<TDescriptor>
    where TDescriptor : DamageNotificationParameters<TDescriptor>
{
    public override ExportCategory Categories => ExportCategory.Gunplay;
    public override ExportGroupKind Kind => ExportGroupKind.ClassNetCache;
    public override FieldStreamGrammar Grammar => FieldStreamGrammar.FunctionParameters;

    public float DamageTaken { get; set; }
    public float DamageDealt { get; set; }
    public bool DamageKilledTarget { get; set; }
    public bool AliveAfterDamage { get; set; }
    public uint EventInstigator { get; set; }
    public uint DamageCauser { get; set; }
    public object? DamageOrigin { get; set; }
    public uint EquippableUsed { get; set; }
    public uint DamageType { get; set; }
    public object? LifeChangeEvents { get; set; }
    public uint ChangedComponent { get; set; }
    public float LifeResult { get; set; }
    public float DeltaLife { get; set; }
    public bool AliveAfterChange { get; set; }
    public uint EventInstigatorPawn { get; set; }
    public uint DamagerPlayerState { get; set; }
    public uint KillCreditPlayerState { get; set; }
    public byte RegionalDamage { get; set; }
    public uint Character { get; set; }
    public float NetTimestamp { get; set; }
    public int RespawnNumber { get; set; }
    public int VictimRespawnNumber { get; set; }

    protected override void Configure()
    {
        AddProperty(x => x.DamageTaken).FloatOrRaw();
        AddProperty(x => x.DamageDealt).FloatOrRaw();
        AddProperty("bDamageKilledTarget", x => x.DamageKilledTarget).BoolOrRaw();
        AddProperty("bAliveAfterDamage", x => x.AliveAfterDamage).BoolOrRaw();
        AddProperty(x => x.EventInstigator).ObjectNetGuidOrRaw();
        AddProperty(x => x.DamageCauser).ObjectNetGuidOrRaw();
        AddProperty(x => x.DamageOrigin)
            .Decode(ValorantPayloadDecoders.RawPayload("FVector"));
        AddProperty(x => x.EquippableUsed).ObjectNetGuidOrRaw();
        AddProperty(x => x.DamageType).ObjectNetGuidOrRaw();
        AddProperty(x => x.LifeChangeEvents)
            .Decode(ValorantPayloadDecoders.RawPayload("TArray<FLifeChangeEvent>"));
        AddProperty(x => x.ChangedComponent).ObjectNetGuidOrRaw();
        AddProperty(x => x.LifeResult).FloatOrRaw();
        AddProperty(x => x.DeltaLife).FloatOrRaw();
        AddProperty("bAliveAfterChange", x => x.AliveAfterChange).BoolOrRaw();
        AddProperty(x => x.EventInstigatorPawn).ObjectNetGuidOrRaw();
        AddProperty(x => x.DamagerPlayerState).ObjectNetGuidOrRaw();
        AddProperty(x => x.KillCreditPlayerState).ObjectNetGuidOrRaw();
        AddProperty(x => x.RegionalDamage).EnumByteOrRaw();
        AddProperty(x => x.Character).ObjectNetGuidOrRaw();
        AddProperty(x => x.NetTimestamp).FloatOrRaw();
        AddProperty(x => x.RespawnNumber).Int32OrRaw();
        AddProperty(x => x.VictimRespawnNumber).Int32OrRaw();
    }
}

internal sealed class MulticastNotifyDamageBaseParameters
    : DamageNotificationParameters<MulticastNotifyDamageBaseParameters>
{
    public override string Path =>
        "/Script/ShooterGame.DamageableComponent:MulticastNotifyDamage_Base";
}

internal sealed class MulticastNotifyDamagePointParameters
    : DamageNotificationParameters<MulticastNotifyDamagePointParameters>
{
    public override string Path =>
        "/Script/ShooterGame.DamageableComponent:MulticastNotifyDamage_Point";
}
