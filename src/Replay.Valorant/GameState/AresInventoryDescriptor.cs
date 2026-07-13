using Replay.Models.Descriptors;
using Replay.Unreal.Parsing;
using Replay.Valorant.Descriptors;

namespace Replay.Valorant.GameState;

public sealed class AresInventoryDescriptor : ExportGroupDescriptor<AresInventoryDescriptor>
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
        AddProperty("bIsActive", x => x.IsActive).Bool();
        AddProperty(x => x.ItemSlots).Decode(ValorantPayloadDecoders.RawPayload("UItemSlot*[16]"));
        AddProperty(x => x.NewCurrentEquippable).ObjectNetGuid();
        AddProperty(x => x.Character).ObjectNetGuid();
        AddProperty(x => x.NetTimestamp).Float();
        AddProperty(x => x.RespawnNumber).Int32();
        AddProperty(x => x.CurrentEquippable).ObjectNetGuid();
    }
}