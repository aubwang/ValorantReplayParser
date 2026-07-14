using Replay.Encoding.Net;
using Replay.Models.Descriptors;
using Replay.Models.Events;
using Replay.Valorant.Combat;
using Replay.Valorant.GameState;

namespace Replay.Valorant.Tests.Combat;

public class ValorantShotEventEnricherTests
{
    [Test]
    public void Emit_PlayerInventorySelectedGun_EnrichesShotImmediately()
    {
        var sink = new CapturingReplayEventSink();
        var enricher = new ValorantShotEventEnricher(sink, new NetGuidCache());

        enricher.Emit(new ActorSpawned(
            0, 0, 500, 0, false, null, 0, null,
            "/Game/Equippables/Guns/Rifles/AK/AssaultRifle_AK.AssaultRifle_AK_C",
            0, null, null, null, null));

        var playerState = new BombPlayerStateDescriptor { PossessedCharacter = 20 };
        playerState.MarkDecoded(nameof(BombPlayerStateDescriptor.PossessedCharacter));
        enricher.Emit(ExportGroup(10, playerState));

        var inventory = new AresInventoryDescriptor { Character = 20, CurrentEquippable = 500 };
        inventory.MarkDecoded(nameof(AresInventoryDescriptor.Character));
        inventory.MarkDecoded(nameof(AresInventoryDescriptor.CurrentEquippable));
        enricher.Emit(ExportGroup(20, inventory));

        enricher.Emit(new ValorantShotReceived(1, 2, 30, 0, 4, new ValorantShot(
            null, null, null, null, null, null, null, null, null, null, null, null, null, null, null,
            10, null, [], 500, null)));

        var shot = sink.Events.OfType<ValorantShotReceived>().Single().Shot;

        Assert.Multiple(() =>
        {
            Assert.That(shot.Equippable, Is.Not.Null);
            Assert.That(shot.Equippable!.NetGuid, Is.EqualTo(500));
            Assert.That(shot.Equippable.Name, Is.EqualTo("Vandal"));
            Assert.That(shot.Equippable.Category, Is.EqualTo(ValorantEquippableCategory.Rifle));
        });
    }

    private static ExportGroupReceived ExportGroup(uint actorNetGuid, ExportGroupDescriptor payload) => new(
        0, 0, actorNetGuid, 0, 0, true, false, 0, payload.Path, payload.Kind, payload.Categories,
        0, 0, null, null, null, 0, 0, true, payload, 0, []);

    private sealed class CapturingReplayEventSink : IReplayEventSink
    {
        public List<ReplayEvent> Events { get; } = [];

        public void Emit(ReplayEvent replayEvent) => Events.Add(replayEvent);
    }
}
