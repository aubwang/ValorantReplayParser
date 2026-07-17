using Replay.Models.Descriptors;
using Replay.Valorant.Descriptors;

namespace Replay.Valorant.GameState;

internal sealed class AbilityRechargeComponentDescriptor
    : ExportGroupDescriptor<AbilityRechargeComponentDescriptor>
{
    public override string Path => "/Script/ShooterGame.AbilityRechargeComponent";
    public override ExportCategory Categories => ExportCategory.Ability;
    public override ExportGroupKind Kind => ExportGroupKind.Component;

    public int MaxCharges { get; set; }
    public int CurrentCharges { get; set; }

    protected override void Configure()
    {
        AddProperty(x => x.MaxCharges).Int32OrRaw();
        AddProperty(x => x.CurrentCharges).Int32OrRaw();
    }
}

internal sealed class AbilityRechargeCooldownComponentDescriptor
    : ExportGroupDescriptor<AbilityRechargeCooldownComponentDescriptor>
{
    public override string Path => "/Script/ShooterGame.AbilityRechargeCooldownComponent";
    public override ExportCategory Categories => ExportCategory.Ability;
    public override ExportGroupKind Kind => ExportGroupKind.Component;

    public float CooldownSeconds { get; set; }
    public float TempChargeCooldownSeconds { get; set; }
    public float CooldownFinishTimestamp { get; set; }
    public float TempChargeCooldownFinishTimestamp { get; set; }
    public int ChargesInUse { get; set; }
    public bool CooldownPaused { get; set; }

    protected override void Configure()
    {
        AddProperty(x => x.CooldownSeconds).FloatOrRaw();
        AddProperty(x => x.TempChargeCooldownSeconds).FloatOrRaw();
        AddProperty(x => x.CooldownFinishTimestamp).FloatOrRaw();
        AddProperty(x => x.TempChargeCooldownFinishTimestamp).FloatOrRaw();
        AddProperty(x => x.ChargesInUse).Int32OrRaw();
        AddProperty("bCooldownPaused", x => x.CooldownPaused).BoolOrRaw();
    }
}

internal class ResourceComponentDescriptor<TDescriptor> : ExportGroupDescriptor<TDescriptor>
    where TDescriptor : ResourceComponentDescriptor<TDescriptor>
{
    public override ExportCategory Categories => ExportCategory.Ability | ExportCategory.Inventory;
    public override ExportGroupKind Kind => ExportGroupKind.Component;

    public int AuthResourceAmount { get; set; }
    public int PredictedResourceAmount { get; set; }

    protected override void Configure()
    {
        AddProperty(x => x.AuthResourceAmount).Int32OrRaw();
        AddProperty(x => x.PredictedResourceAmount).Int32OrRaw();
    }
}

internal sealed class ResourceComponentDescriptor
    : ResourceComponentDescriptor<ResourceComponentDescriptor>
{
    public override string Path => "/Script/ShooterGame.ResourceComponent";
}

internal sealed class AbilityResourceComponentDescriptor
    : ResourceComponentDescriptor<AbilityResourceComponentDescriptor>
{
    public override string Path => "/Script/ShooterGame.AbilityResourceComponent";
}

internal sealed class EquipmentChargeComponentDescriptor
    : ResourceComponentDescriptor<EquipmentChargeComponentDescriptor>
{
    public override string Path => "/Script/ShooterGame.EquipmentChargeComponent";

    public int MaxCharges { get; set; }
    public int ChargesBoughtThisRound { get; set; }
    public int CurrentTemporaryCharges { get; set; }
    public int TotalChargesAllowedToPurchaseThisRound { get; set; }

    protected override void Configure()
    {
        base.Configure();
        AddProperty(x => x.MaxCharges).Int32OrRaw();
        AddProperty(x => x.ChargesBoughtThisRound).Int32OrRaw();
        AddProperty(x => x.CurrentTemporaryCharges).Int32OrRaw();
        AddProperty(x => x.TotalChargesAllowedToPurchaseThisRound).Int32OrRaw();
    }
}

internal sealed class SignatureAbilityResourceComponentDescriptor
    : ResourceComponentDescriptor<SignatureAbilityResourceComponentDescriptor>
{
    public override string Path => "/Script/ShooterGame.SignatureAbilityResourceComponent";

    public int ChargesBoughtThisRound { get; set; }
    public int CurrentTemporaryCharges { get; set; }
    public int TotalChargesAllowedToPurchaseThisRound { get; set; }
    public int AuthSignatureChargeAmount { get; set; }

    protected override void Configure()
    {
        base.Configure();
        AddProperty(x => x.ChargesBoughtThisRound).Int32OrRaw();
        AddProperty(x => x.CurrentTemporaryCharges).Int32OrRaw();
        AddProperty(x => x.TotalChargesAllowedToPurchaseThisRound).Int32OrRaw();
        AddProperty(x => x.AuthSignatureChargeAmount).Int32OrRaw();
    }
}

internal sealed class AbilityCooldownComponentDescriptor
    : ExportGroupDescriptor<AbilityCooldownComponentDescriptor>
{
    public override string Path =>
        "/Game/Characters/Components/Comp_Ability_CooldownComponent.Comp_Ability_CooldownComponent_C";
    public override ExportCategory Categories => ExportCategory.Ability;
    public override ExportGroupKind Kind => ExportGroupKind.Component;

    public float CooldownSeconds { get; set; }
    public float StartTimeStamp { get; set; }
    public bool CooldownActive { get; set; }

    protected override void Configure()
    {
        AddProperty(x => x.CooldownSeconds).FloatOrRaw();
        AddProperty(x => x.StartTimeStamp).FloatOrRaw();
        AddProperty(x => x.CooldownActive).BoolOrRaw();
    }
}

internal abstract class ArmorItemDescriptor<TDescriptor> : ExportGroupDescriptor<TDescriptor>
    where TDescriptor : ArmorItemDescriptor<TDescriptor>
{
    public override ExportCategory Categories => ExportCategory.Inventory | ExportCategory.Gunplay;
    public override ExportGroupKind Kind => ExportGroupKind.Actor;

    public int MaximumAmount { get; set; }
    public uint Owner { get; set; }
    public uint MyPawn { get; set; }
    public byte InInventory { get; set; }
    public uint AttachedDamageSection { get; set; }

    protected override void Configure()
    {
        AddProperty(x => x.MaximumAmount).Int32OrRaw();
        AddProperty(x => x.Owner).ObjectNetGuidOrRaw();
        AddProperty(x => x.MyPawn).ObjectNetGuidOrRaw();
        AddProperty(x => x.InInventory).EnumByteOrRaw();
        AddProperty(x => x.AttachedDamageSection).ObjectNetGuidOrRaw();
    }
}

internal sealed class LightArmorItemDescriptor
    : ArmorItemDescriptor<LightArmorItemDescriptor>
{
    public override string Path => "/Game/Gear/LightArmorItem.LightArmorItem_C";
}

internal sealed class HeavyArmorItemDescriptor
    : ArmorItemDescriptor<HeavyArmorItemDescriptor>
{
    public override string Path => "/Game/Gear/HeavyArmorItem.HeavyArmorItem_C";
}

internal sealed class PlasmaArmorItemDescriptor
    : ArmorItemDescriptor<PlasmaArmorItemDescriptor>
{
    public override string Path => "/Game/Gear/PlasmaArmor/PlasmaArmorItem.PlasmaArmorItem_C";

    public bool RegenActive { get; set; }
    public double MaxRegenPool { get; set; }
    public double CurrentRegenPool { get; set; }

    protected override void Configure()
    {
        base.Configure();
        AddProperty(x => x.RegenActive).BoolOrRaw();
        AddProperty(x => x.MaxRegenPool).DoubleOrRaw();
        AddProperty(x => x.CurrentRegenPool).DoubleOrRaw();
    }
}
