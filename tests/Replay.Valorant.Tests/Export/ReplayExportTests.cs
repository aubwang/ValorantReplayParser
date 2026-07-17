using System.Text.Json;
using CliReader;
using Replay.Encoding.Archives;
using Replay.Models.Descriptors;
using Replay.Models.Events;
using Replay.Models.Net;
using Replay.Models.Replay;
using Replay.Models.Unreal;
using Replay.Unreal.Readers;
using Replay.Valorant.GameState;
using Replay.Valorant.Movement;

namespace Replay.Valorant.Tests.Export;

public class ReplayExportTests
{
    [Test]
    public void ExportOptions_ParsesViewerProfile()
    {
        var parsed = ExportOptions.TryParse(
            ["export", "match.replay", "--output", "bundle", "--profile", "viewer"],
            out var options,
            out var error);

        Assert.Multiple(() =>
        {
            Assert.That(parsed, Is.True);
            Assert.That(error, Is.Null);
            Assert.That(options!.ReplayPath, Is.EqualTo("match.replay"));
            Assert.That(options.OutputDirectory, Is.EqualTo("bundle"));
            Assert.That(options.ProfileName, Is.EqualTo("viewer"));
            Assert.That(options.ParseProfile.CaptureDiagnosticFields, Is.True);
        });
    }

    [Test]
    public void EventSink_WritesSupportedEventsAndFiltersExportShells()
    {
        using var events = new MemoryStream();
        using var movement = new MemoryStream();
        using (var sink = CreateSink(events, movement))
        {
            sink.Emit(Spawned());
            sink.Emit(new ActorClosed(2, 12, 100, 7, ChannelCloseReason.Destroyed));
            sink.Emit(Export(wasDecoded: false, payload: new TestPayload()));
            sink.Emit(Export(wasDecoded: true, payload: null));
            sink.Emit(Export(wasDecoded: true, payload: new TestPayload()));
            sink.Emit(Rpc());

            Assert.Multiple(() =>
            {
                Assert.That(sink.EventCount, Is.EqualTo(4));
                Assert.That(sink.FilteredExportGroupCount, Is.EqualTo(2));
                Assert.That(sink.UndecodedExportGroupCount, Is.EqualTo(1));
                Assert.That(sink.EmptyDecodedExportGroupCount, Is.EqualTo(1));
                Assert.That(sink.FilteredExportGroups.Count, Is.EqualTo(2));
            });
        }

        var documents = ParseLines(events);
        Assert.That(
            documents.Select(document => document.RootElement.GetProperty("type").GetString()),
            Is.EqualTo(new[]
            {
                "actor_spawned",
                "actor_closed",
                "export_group_received",
                "rpc_received",
            }));

        var exported = documents[2].RootElement;
        Assert.Multiple(() =>
        {
            Assert.That(exported.GetProperty("time_ms").GetInt64(), Is.EqualTo(2500));
            Assert.That(exported.GetProperty("actor_net_guid").GetUInt32(), Is.EqualTo(100));
            Assert.That(exported.GetProperty("object_net_guid").GetUInt32(), Is.EqualTo(101));
            Assert.That(exported.GetProperty("channel").GetUInt32(), Is.EqualTo(7));
            Assert.That(exported.GetProperty("was_decoded").GetBoolean(), Is.True);
            Assert.That(exported.GetProperty("categories")[0].GetString(), Is.EqualTo("movement"));
            Assert.That(exported.GetProperty("payload").GetProperty("Health").GetInt32(), Is.EqualTo(87));
            Assert.That(exported.GetProperty("payload").TryGetProperty("Ignored", out _), Is.False);
            Assert.That(documents[3].RootElement.GetProperty("was_decoded").GetBoolean(), Is.False);
        });

        Dispose(documents);
    }

    [Test]
    public void EventSink_WritesStructuredRoundResultPayload()
    {
        using var events = new MemoryStream();
        using var movement = new MemoryStream();
        using (var sink = CreateSink(events, movement))
        {
            sink.Emit(Export(wasDecoded: true, payload: new RoundResultPayload()));
        }

        using var document = ParseLines(events).Single();
        var result = document.RootElement
            .GetProperty("payload")
            .GetProperty("RoundResults")[0];

        Assert.Multiple(() =>
        {
            Assert.That(result.GetProperty("RoundNumber").GetInt32(), Is.Zero);
            Assert.That(result.GetProperty("WinningTeam").GetString(), Is.EqualTo("Blue"));
            Assert.That(result.GetProperty("WinningTeamRole").GetString(), Is.EqualTo("defender"));
            Assert.That(result.GetProperty("RoundResult").GetString(), Is.EqualTo("defuse"));
        });
    }

    [Test]
    public void MovementSink_WritesMachineReadableMovementShape()
    {
        using var events = new MemoryStream();
        using var movement = new MemoryStream();
        using (var sink = CreateSink(events, movement))
        {
            var move = Movement();
            sink.EmitRemoteCharacterMovement(1.25f, 99, 100, 101, 7, 3, 1234, 4, in move);
        }

        var documents = ParseLines(movement);
        var exported = documents.Single().RootElement;
        Assert.Multiple(() =>
        {
            Assert.That(exported.GetProperty("type").GetString(), Is.EqualTo("remote_character_movement"));
            Assert.That(exported.GetProperty("time_ms").GetInt64(), Is.EqualTo(1250));
            Assert.That(exported.GetProperty("packet_id").GetInt32(), Is.EqualTo(99));
            Assert.That(exported.GetProperty("shooter_character_net_guid").GetUInt32(), Is.EqualTo(1234));
            Assert.That(exported.GetProperty("update_index").GetInt32(), Is.EqualTo(3));
            Assert.That(exported.GetProperty("move_index").GetInt32(), Is.EqualTo(4));
            Assert.That(exported.GetProperty("position").GetProperty("x").GetDouble(), Is.EqualTo(1.5));
            Assert.That(exported.GetProperty("velocity").GetProperty("z").GetDouble(), Is.EqualTo(6));
            Assert.That(exported.GetProperty("yaw").GetDouble(), Is.EqualTo(90));
            Assert.That(exported.GetProperty("pitch").GetDouble(), Is.EqualTo(45));
            Assert.That(exported.GetProperty("timestamp").GetUInt32(), Is.EqualTo(42));
            Assert.That(exported.GetProperty("movement_state").GetByte(), Is.EqualTo(3));
        });

        Dispose(documents);
    }

    [Test]
    public void ManifestWriter_WritesSchemaIdentityAndCounts()
    {
        var directory = Path.Combine(Path.GetTempPath(), $"valorant-export-{Guid.NewGuid():N}");
        Directory.CreateDirectory(directory);
        try
        {
            using var archive = new FBinaryArchive(ReadOnlyMemory<byte>.Empty);
            var context = new ReplayReaderContext(archive)
            {
                ReplayInfo = new ReplayInfo { LengthInMs = 60000 },
                ReplayVersion = new ReplayVersion
                {
                    Major = 13,
                    Minor = 1,
                    Patch = 0,
                    Changelist = 123,
                    Branch = "++Ares-Core+release-13.01",
                },
            };
            using var events = new MemoryStream();
            using var movement = new MemoryStream();
            using var sink = CreateSink(events, movement);
            sink.Emit(Spawned());
            sink.Emit(Export(wasDecoded: false, payload: null));

            new ReplayExportManifestWriter().Write(
                directory,
                "match.replay",
                new string('a', 64),
                42,
                "viewer",
                context,
                sink);

            using var document = JsonDocument.Parse(File.ReadAllText(Path.Combine(directory, "manifest.json")));
            var manifest = document.RootElement;
            Assert.Multiple(() =>
            {
                Assert.That(manifest.GetProperty("schema_version").GetInt32(), Is.EqualTo(3));
                Assert.That(manifest.GetProperty("source_sha256").GetString(), Has.Length.EqualTo(64));
                Assert.That(manifest.GetProperty("replay_build").GetString(), Does.EndWith("release-13.01"));
                Assert.That(manifest.GetProperty("duration_ms").GetInt32(), Is.EqualTo(60000));
                Assert.That(manifest.GetProperty("parse_profile").GetString(), Is.EqualTo("viewer"));
                Assert.That(manifest.GetProperty("parser_version").GetString(), Is.Not.Empty);
                Assert.That(manifest.GetProperty("counts").GetProperty("actor_spawned").GetInt32(), Is.EqualTo(1));
                Assert.That(manifest.GetProperty("net_field_export_groups").GetArrayLength(), Is.Zero);
                Assert.That(
                    manifest.GetProperty("counts").GetProperty("undecoded_export_groups").GetInt32(),
                    Is.EqualTo(1));
                Assert.That(
                    manifest.GetProperty("filtered_export_group_summary")[0]
                        .GetProperty("path").GetString(),
                    Is.EqualTo("/Game/Export"));
                Assert.That(
                    manifest.GetProperty("filtered_export_group_summary")[0]
                        .GetProperty("sample_class_path").GetString(),
                    Is.EqualTo("/Game/Class"));
                Assert.That(manifest.GetProperty("limitations").GetArrayLength(), Is.GreaterThan(0));
            });
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    private static ReplayExportSink CreateSink(Stream events, Stream movement) =>
        new(
            new NdjsonWriter(events),
            new NdjsonWriter(movement),
            new ReplayJsonNormalizer());

    private static ActorSpawned Spawned() =>
        new(
            1,
            10,
            100,
            7,
            true,
            "/Game/Actor",
            200,
            "/Game/Archetype",
            "/Game/Class",
            300,
            new FVector(1, 2, 3),
            new FRotator(4, 5, 6),
            new FVector(1, 1, 1),
            new FVector(7, 8, 9));

    private static ExportGroupReceived Export(bool wasDecoded, object? payload) =>
        new(
            2.5f,
            20,
            100,
            101,
            7,
            false,
            false,
            0,
            "/Game/Export",
            ExportGroupKind.Component,
            ExportCategory.Movement | ExportCategory.Agent,
            200,
            100,
            "/Game/Object",
            "/Game/Class",
            "/Game/Outer",
            64,
            64,
            wasDecoded,
            payload,
            wasDecoded ? 1 : 0,
            []);

    private static RpcReceived Rpc() =>
        new(
            3,
            30,
            100,
            101,
            7,
            "/Game/Class",
            "DoThing",
            "/Game/Class.DoThing",
            9,
            ExportCategory.Ability,
            8,
            0,
            false,
            null,
            0,
            []);

    private static MovementMove Movement() =>
        new(
            Marker: 1,
            MoveType: 1,
            Position: new FVector(1.5, 2.5, 3.5),
            Velocity: new FVector(4, 5, 6),
            RotationInput: new FVector(0.1, 0.2, 0.3),
            Variant1Vector: new FVector(4, 5, 6),
            Timestamp: 42,
            ModeFlags: 3,
            MovementState: 3,
            RotationYawMultiplier: 2,
            UnusedByte: 0,
            HasOptionalMovementValue: true,
            OptionalMovementRawByte: 8,
            OptionalMovementValue: 8,
            Flag48: false,
            PackedAngles: 12,
            RawYaw: 100,
            RawPitch: 200,
            Yaw: 90,
            Pitch: 45,
            Variant0HasExternalCharacterRef: null,
            Variant0PackedAngles: null,
            Variant1Flag: true,
            ErrorSentinel: false);

    private static List<JsonDocument> ParseLines(MemoryStream stream) =>
        System.Text.Encoding.UTF8.GetString(stream.ToArray())
            .Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .Select(line => JsonDocument.Parse(line))
            .ToList();

    private static void Dispose(IEnumerable<JsonDocument> documents)
    {
        foreach (var document in documents)
        {
            document.Dispose();
        }
    }

    private sealed class TestPayload : IDecodedPayload
    {
        public IReadOnlySet<string> DecodedProperties { get; } = new HashSet<string>
        {
            nameof(Health),
        };

        public int Health { get; } = 87;
        public string Ignored { get; } = "not decoded";

        public bool HasDecoded(string propertyName) => DecodedProperties.Contains(propertyName);
    }

    private sealed class RoundResultPayload
    {
        public AresRoundResult[] RoundResults { get; } =
        [
            new(0, "Blue", AresTeamRole.Defender, AresRoundOutcome.Defuse),
        ];
    }
}
