using Replay.Models.Descriptors;
using Replay.Unreal.Parsing;
using Replay.Valorant.Descriptors;

namespace Replay.Valorant.GameState;

public sealed class BombPlayerStateDescriptor : ExportGroupDescriptor<BombPlayerStateDescriptor>
{
    public override string Path => "/Game/GameModes/Bomb/BombPlayerState.BombPlayerState_C";
    public override ExportCategory Categories => ExportCategory.GameState;
    public override ExportGroupKind Kind => ExportGroupKind.Actor;

    public int PlayerId { get; set; }
    public ValorantRawPayload? UniqueId { get; set; } // TODO: Implement FUniqueNetIdRepl.
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
        AddProperty("PlayerId", x => x.PlayerId).Int32();
        AddProperty("PlayerID", x => x.PlayerId).Int32();
        AddProperty(x => x.UniqueId).Decode(ValorantPayloadDecoders.RawPayload("FUniqueNetIdRepl"));
        AddProperty(x => x.CompetitiveTier).Int32();
        AddProperty(x => x.Subject).FString();
        AddProperty(x => x.SpectatedPlayer).ObjectNetGuid();
        AddProperty(x => x.PlayerInfo).ObjectNetGuid();
        AddProperty(x => x.SpawnedCharacter).ObjectNetGuid();
        AddProperty(x => x.PossessedCharacter).ObjectNetGuid();
        AddProperty("bUltimateActive", x => x.UltimateActive).Bool();
        AddProperty(x => x.NumUltimatePoints).Int32();
        AddProperty(x => x.TotalAcquiredUltimatePoints).Int32();
    }
}