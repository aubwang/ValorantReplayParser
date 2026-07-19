using Replay.Models.Events;

namespace Replay.Valorant.Movement;

public interface IRemoteCharacterMovementSink
{
    void EmitRemoteCharacterMovement(
        float timeSeconds,
        int packetId,
        uint actorNetGuid,
        uint objectNetGuid,
        uint channelIndex,
        int updateIndex,
        uint shooterCharacterNetGuidValue,
        int moveIndex,
        in MovementMove move);
}

public sealed record RemoteCharacterMovementReceived(
    float TimeSeconds,
    int PacketId,
    uint ActorNetGuid,
    uint ObjectNetGuid,
    uint ChannelIndex,
    int UpdateIndex,
    uint ShooterCharacterNetGuidValue,
    int MoveIndex,
    MovementMove Move)
    : ReplayEvent(TimeSeconds, PacketId);
