using System.Text.Json;
using Replay.Models.Descriptors;
using Replay.Models.Events;
using Replay.Models.Unreal;
using Replay.Valorant.Movement;

namespace CliReader;

internal sealed class ReplayExportSink :
    IReplayEventSink,
    IRemoteCharacterMovementSink,
    IDisposable
{
    private readonly NdjsonWriter _events;
    private readonly NdjsonWriter _movement;
    private readonly ReplayJsonNormalizer _normalizer;
    private readonly Dictionary<(string Path, ExportGroupKind Kind, bool WasDecoded), FilteredExportGroupSummary>
        _filteredExportGroups = [];

    internal ReplayExportSink(
        NdjsonWriter events,
        NdjsonWriter movement,
        ReplayJsonNormalizer normalizer)
    {
        _events = events;
        _movement = movement;
        _normalizer = normalizer;
    }

    public int ActorSpawnedCount { get; private set; }
    public int ActorClosedCount { get; private set; }
    public int ExportGroupCount { get; private set; }
    public int FilteredExportGroupCount { get; private set; }
    public int UndecodedExportGroupCount { get; private set; }
    public int EmptyDecodedExportGroupCount { get; private set; }
    public int RpcCount { get; private set; }
    public int MovementCount { get; private set; }
    public int EventCount => ActorSpawnedCount + ActorClosedCount + ExportGroupCount + RpcCount;
    public IReadOnlyCollection<FilteredExportGroupSummary> FilteredExportGroups =>
        _filteredExportGroups.Values;

    public static ReplayExportSink Create(string outputDirectory)
    {
        Directory.CreateDirectory(outputDirectory);
        var events = NdjsonWriter.Create(Path.Combine(outputDirectory, "events.ndjson"));
        try
        {
            var movement = NdjsonWriter.Create(Path.Combine(outputDirectory, "movement.ndjson"));
            return new ReplayExportSink(events, movement, new ReplayJsonNormalizer());
        }
        catch
        {
            events.Dispose();
            throw;
        }
    }

    public void Emit(ReplayEvent replayEvent)
    {
        switch (replayEvent)
        {
            case ActorSpawned spawned:
                WriteActorSpawned(spawned);
                break;
            case ActorClosed closed:
                WriteActorClosed(closed);
                break;
            case ExportGroupReceived exportGroup:
                WriteExportGroup(exportGroup);
                break;
            case RpcReceived rpc:
                WriteRpc(rpc);
                break;
            case RemoteCharacterMovementReceived movement:
                var move = movement.Move;
                EmitRemoteCharacterMovement(
                    movement.TimeSeconds,
                    movement.PacketId,
                    movement.ActorNetGuid,
                    movement.ObjectNetGuid,
                    movement.ChannelIndex,
                    movement.UpdateIndex,
                    movement.ShooterCharacterNetGuidValue,
                    movement.MoveIndex,
                    in move);
                break;
        }
    }

    public void EmitRemoteCharacterMovement(
        float timeSeconds,
        int packetId,
        uint actorNetGuid,
        uint objectNetGuid,
        uint channelIndex,
        int updateIndex,
        uint shooterCharacterNetGuidValue,
        int moveIndex,
        in MovementMove move)
    {
        var value = move;
        _movement.Write(writer => WriteMovement(
            writer,
            timeSeconds,
            packetId,
            actorNetGuid,
            objectNetGuid,
            channelIndex,
            updateIndex,
            shooterCharacterNetGuidValue,
            moveIndex,
            value));
        MovementCount++;
    }

    public void Dispose()
    {
        try
        {
            _events.Dispose();
        }
        finally
        {
            _movement.Dispose();
        }
    }

    private void WriteActorSpawned(ActorSpawned spawned)
    {
        _events.Write(writer =>
        {
            WriteEventStart(writer, "actor_spawned", spawned);
            writer.WriteNumber("actor_net_guid", spawned.ActorNetGuid);
            writer.WriteNumber("channel", spawned.ChannelIndex);
            writer.WriteBoolean("is_dynamic", spawned.IsDynamic);
            WriteNullableString(writer, "actor_path", spawned.ActorPath);
            writer.WriteNumber("archetype_net_guid", spawned.ArchetypeNetGuid);
            WriteNullableString(writer, "archetype_path", spawned.ArchetypePath);
            WriteNullableString(writer, "replication_class_path", spawned.ReplicationClassPath);
            writer.WriteNumber("level_net_guid", spawned.LevelNetGuid);
            WriteNullableValue(writer, "location", spawned.Location);
            WriteNullableValue(writer, "rotation", spawned.Rotation);
            WriteNullableValue(writer, "scale", spawned.Scale);
            WriteNullableValue(writer, "velocity", spawned.Velocity);
            writer.WriteEndObject();
        });
        ActorSpawnedCount++;
    }

    private void WriteActorClosed(ActorClosed closed)
    {
        _events.Write(writer =>
        {
            WriteEventStart(writer, "actor_closed", closed);
            writer.WriteNumber("actor_net_guid", closed.ActorNetGuid);
            writer.WriteNumber("channel", closed.ChannelIndex);
            writer.WriteString("reason", ReplayJsonNormalizer.ToSnakeCase(closed.Reason.ToString()));
            writer.WriteEndObject();
        });
        ActorClosedCount++;
    }

    private void WriteExportGroup(ExportGroupReceived exportGroup)
    {
        if (!exportGroup.WasDecoded || exportGroup.Payload is null)
        {
            RecordFilteredExportGroup(exportGroup);
            FilteredExportGroupCount++;
            return;
        }

        _events.Write(writer =>
        {
            WriteEventStart(writer, "export_group_received", exportGroup);
            WriteObjectIdentity(writer, exportGroup.ActorNetGuid, exportGroup.ObjectNetGuid, exportGroup.ChannelIndex);
            writer.WriteBoolean("is_actor", exportGroup.IsActor);
            writer.WriteBoolean("is_deleted", exportGroup.IsDeleted);
            writer.WriteNumber("delete_flags", exportGroup.DeleteFlags);
            WriteNullableString(writer, "export_group_path", exportGroup.ExportGroupPath);
            writer.WriteString("kind", ReplayJsonNormalizer.ToSnakeCase(exportGroup.Kind.ToString()));
            WriteCategories(writer, exportGroup.Categories);
            writer.WriteNumber("class_net_guid", exportGroup.ClassNetGuid);
            writer.WriteNumber("outer_net_guid", exportGroup.OuterNetGuid);
            WriteNullableString(writer, "object_path", exportGroup.ObjectPath);
            WriteNullableString(writer, "class_path", exportGroup.ClassPath);
            WriteNullableString(writer, "outer_path", exportGroup.OuterPath);
            WriteDecodeMetadata(writer, exportGroup.PayloadBits, exportGroup.ParsedBits, true, exportGroup.DecodedFieldCount);
            WritePayload(writer, exportGroup.Payload);
            WriteDiagnosticFields(writer, exportGroup.DiagnosticFields);
            writer.WriteEndObject();
        });
        ExportGroupCount++;
    }

    private void RecordFilteredExportGroup(ExportGroupReceived exportGroup)
    {
        var path = exportGroup.ExportGroupPath ?? exportGroup.ClassPath ?? "<unresolved>";
        var key = (path, exportGroup.Kind, exportGroup.WasDecoded);
        if (!_filteredExportGroups.TryGetValue(key, out var summary))
        {
            summary = new FilteredExportGroupSummary(path, exportGroup.Kind, exportGroup.WasDecoded);
            _filteredExportGroups.Add(key, summary);
        }

        summary.Add(exportGroup);
        if (exportGroup.WasDecoded)
        {
            EmptyDecodedExportGroupCount++;
        }
        else
        {
            UndecodedExportGroupCount++;
        }
    }

    private void WriteRpc(RpcReceived rpc)
    {
        _events.Write(writer =>
        {
            WriteEventStart(writer, "rpc_received", rpc);
            WriteObjectIdentity(writer, rpc.ActorNetGuid, rpc.ObjectNetGuid, rpc.ChannelIndex);
            writer.WriteString("class_path", rpc.ClassPath);
            writer.WriteString("function_name", rpc.FunctionName);
            writer.WriteString("function_export_path", rpc.FunctionExportPath);
            writer.WriteNumber("function_handle", rpc.FunctionHandle);
            WriteCategories(writer, rpc.Categories);
            WriteDecodeMetadata(writer, rpc.PayloadBits, rpc.ParsedBits, rpc.WasDecoded, rpc.DecodedFieldCount);
            WritePayload(writer, rpc.Payload);
            WriteDiagnosticFields(writer, rpc.DiagnosticFields);
            writer.WriteEndObject();
        });
        RpcCount++;
    }

    private void WriteMovement(
        Utf8JsonWriter writer,
        float timeSeconds,
        int packetId,
        uint actorNetGuid,
        uint objectNetGuid,
        uint channelIndex,
        int updateIndex,
        uint shooterCharacterNetGuid,
        int moveIndex,
        MovementMove move)
    {
        writer.WriteStartObject();
        WriteEventDiscriminator(writer, "remote_character_movement", timeSeconds, packetId);
        WriteObjectIdentity(writer, actorNetGuid, objectNetGuid, channelIndex);
        writer.WriteNumber("shooter_character_net_guid", shooterCharacterNetGuid);
        writer.WriteNumber("update_index", updateIndex);
        writer.WriteNumber("move_index", moveIndex);
        writer.WritePropertyName("position");
        ReplayJsonNormalizer.WriteVector(writer, move.Position);
        writer.WriteNumber("yaw", move.Yaw);
        writer.WriteNumber("pitch", move.Pitch);
        WriteNullableValue(writer, "velocity", move.Velocity);
        writer.WriteNumber("timestamp", move.Timestamp);
        writer.WriteNumber("movement_state", move.MovementState);
        writer.WriteNumber("mode_flags", move.ModeFlags);
        writer.WriteNumber("marker", move.Marker);
        writer.WriteNumber("move_type", move.MoveType);
        writer.WritePropertyName("rotation_input");
        ReplayJsonNormalizer.WriteVector(writer, move.RotationInput);
        WriteNullableValue(writer, "variant1_vector", move.Variant1Vector);
        writer.WriteNumber("rotation_yaw_multiplier", move.RotationYawMultiplier);
        writer.WriteBoolean("has_optional_movement_value", move.HasOptionalMovementValue);
        WriteNullableNumber(writer, "optional_movement_raw_byte", move.OptionalMovementRawByte);
        WriteNullableNumber(writer, "optional_movement_value", move.OptionalMovementValue);
        writer.WriteBoolean("flag48", move.Flag48);
        writer.WriteNumber("packed_angles", move.PackedAngles);
        writer.WriteNumber("raw_yaw", move.RawYaw);
        writer.WriteNumber("raw_pitch", move.RawPitch);
        WriteNullableBoolean(writer, "variant0_has_external_character_ref", move.Variant0HasExternalCharacterRef);
        WriteNullableNumber(writer, "variant0_packed_angles", move.Variant0PackedAngles);
        WriteNullableBoolean(writer, "variant1_flag", move.Variant1Flag);
        writer.WriteBoolean("error_sentinel", move.ErrorSentinel);
        writer.WriteEndObject();
    }

    private static void WriteEventStart(Utf8JsonWriter writer, string type, ReplayEvent replayEvent)
    {
        writer.WriteStartObject();
        WriteEventDiscriminator(writer, type, replayEvent.TimeSeconds, replayEvent.PacketId);
    }

    private static void WriteEventDiscriminator(
        Utf8JsonWriter writer,
        string type,
        float timeSeconds,
        int packetId)
    {
        writer.WriteString("type", type);
        writer.WriteNumber("time_ms", ToMilliseconds(timeSeconds));
        writer.WriteNumber("packet_id", packetId);
    }

    private static long ToMilliseconds(float seconds) =>
        float.IsFinite(seconds)
            ? (long)Math.Round(seconds * 1000d, MidpointRounding.AwayFromZero)
            : 0;

    private static void WriteObjectIdentity(
        Utf8JsonWriter writer,
        uint actorNetGuid,
        uint objectNetGuid,
        uint channelIndex)
    {
        writer.WriteNumber("actor_net_guid", actorNetGuid);
        writer.WriteNumber("object_net_guid", objectNetGuid);
        writer.WriteNumber("channel", channelIndex);
    }

    private static void WriteDecodeMetadata(
        Utf8JsonWriter writer,
        int payloadBits,
        int parsedBits,
        bool wasDecoded,
        int decodedFieldCount)
    {
        writer.WriteNumber("payload_bits", payloadBits);
        writer.WriteNumber("parsed_bits", parsedBits);
        writer.WriteBoolean("was_decoded", wasDecoded);
        writer.WriteNumber("decoded_field_count", decodedFieldCount);
    }

    private void WritePayload(Utf8JsonWriter writer, object? payload)
    {
        writer.WritePropertyName("payload");
        _normalizer.WriteValue(writer, payload);
    }

    private void WriteDiagnosticFields(
        Utf8JsonWriter writer,
        IReadOnlyList<DecodedReplayField> fields)
    {
        writer.WriteStartArray("diagnostic_fields");
        foreach (var field in fields)
        {
            writer.WriteStartObject();
            writer.WriteNumber("handle", field.Handle);
            WriteNullableString(writer, "name", field.Name);
            WriteNullableString(writer, "export_name", field.ExportName);
            WriteCategories(writer, field.Categories);
            writer.WritePropertyName("value");
            _normalizer.WriteValue(writer, field.Value);
            writer.WriteEndObject();
        }

        writer.WriteEndArray();
    }

    private static void WriteCategories(Utf8JsonWriter writer, ExportCategory categories)
    {
        writer.WriteStartArray("categories");
        foreach (var category in Enum.GetValues<ExportCategory>())
        {
            if (category is ExportCategory.None or ExportCategory.All ||
                !categories.HasFlag(category))
            {
                continue;
            }

            writer.WriteStringValue(ReplayJsonNormalizer.ToSnakeCase(category.ToString()));
        }

        writer.WriteEndArray();
    }

    private void WriteNullableValue(Utf8JsonWriter writer, string name, object? value)
    {
        writer.WritePropertyName(name);
        _normalizer.WriteValue(writer, value);
    }

    private static void WriteNullableString(Utf8JsonWriter writer, string name, string? value)
    {
        if (value is null) writer.WriteNull(name);
        else writer.WriteString(name, value);
    }

    private static void WriteNullableNumber(Utf8JsonWriter writer, string name, uint? value)
    {
        if (value is null) writer.WriteNull(name);
        else writer.WriteNumber(name, value.Value);
    }

    private static void WriteNullableNumber(Utf8JsonWriter writer, string name, byte? value)
    {
        if (value is null) writer.WriteNull(name);
        else writer.WriteNumber(name, value.Value);
    }

    private static void WriteNullableNumber(Utf8JsonWriter writer, string name, double? value)
    {
        if (value is null || !double.IsFinite(value.Value)) writer.WriteNull(name);
        else writer.WriteNumber(name, value.Value);
    }

    private static void WriteNullableBoolean(Utf8JsonWriter writer, string name, bool? value)
    {
        if (value is null) writer.WriteNull(name);
        else writer.WriteBoolean(name, value.Value);
    }
}

internal sealed class FilteredExportGroupSummary(
    string path,
    ExportGroupKind kind,
    bool wasDecoded)
{
    public string Path { get; } = path;
    public ExportGroupKind Kind { get; } = kind;
    public bool WasDecoded { get; } = wasDecoded;
    public int Count { get; private set; }
    public long PayloadBits { get; private set; }
    public uint ActorNetGuid { get; private set; }
    public uint ObjectNetGuid { get; private set; }
    public uint ClassNetGuid { get; private set; }
    public uint OuterNetGuid { get; private set; }
    public string? ObjectPath { get; private set; }
    public string? ClassPath { get; private set; }
    public string? OuterPath { get; private set; }

    public void Add(ExportGroupReceived exportGroup)
    {
        Count++;
        PayloadBits += exportGroup.PayloadBits;
        if (Count != 1)
        {
            return;
        }

        ActorNetGuid = exportGroup.ActorNetGuid;
        ObjectNetGuid = exportGroup.ObjectNetGuid;
        ClassNetGuid = exportGroup.ClassNetGuid;
        OuterNetGuid = exportGroup.OuterNetGuid;
        ObjectPath = exportGroup.ObjectPath;
        ClassPath = exportGroup.ClassPath;
        OuterPath = exportGroup.OuterPath;
    }
}
