using Replay.Encoding.Archives;
using Replay.Encoding.Net;
using Replay.Models.Events;
using Replay.Models.Net;
using Replay.Models.Unreal;
using Replay.Unreal.Parsing;
using Replay.Valorant.Combat;
using Replay.Valorant.Descriptors;

namespace Replay.Valorant.Tests.Combat;

public class ValorantShotTests
{
    [Test]
    public void CreateShot_MapsOnlyNamedFiringStateValues()
    {
        var attackVector1 = new FVector(1, 0, 0);
        var attackVector2 = new FVector(0, 1, 0);
        var attackVector10 = new FVector(0, 0, 1);
        var parameters = new ReplayPlayContinuousEffectAtLocationParameters
        {
            EffectId = 4,
            StartMovementTime = 7.3303437f,
            FloatValues =
            [
                FloatValue("FiringState.NumProjectiles", 1),
                FloatValue("FiringState.AmmoRemaining", 11),
                FloatValue("FiringState.TracerOption", 1),
                FloatValue("FiringState.RandomSeed", -894045900),
                FloatValue("FXC.Duration", 99),
                new EffectDataFloat { Name = new FGameplayTag(65535, null), Float = 123 },
            ],
            VectorValues =
            [
                VectorValue("FiringState.AttackVector.10", attackVector10),
                VectorValue("FiringState.AttackVector.2", attackVector2),
                VectorValue("FiringState.AttackVector.1", attackVector1),
                VectorValue("FiringState.AttackVector.16", new FVector(16, 0, 0)),
            ],
            ObjectValues =
            [
                ObjectValue("FiringState.FiringState", 368),
                ObjectValue("FiringState.FiringPlayerState", 92),
                ObjectValue("FXC.EffectContext", 146),
                ObjectValue("FXC.Equippable", 500),
            ],
        };

        var shot = parameters.CreateShot();

        Assert.Multiple(() =>
        {
            Assert.That(shot.EffectId, Is.EqualTo(4));
            Assert.That(shot.StartMovementTime, Is.EqualTo(7.3303437f));
            Assert.That(shot.NumProjectiles, Is.EqualTo(1));
            Assert.That(shot.AmmoRemaining, Is.EqualTo(11));
            Assert.That(shot.TracerOption, Is.EqualTo(1));
            Assert.That(shot.RandomSeed, Is.EqualTo(-894045900));
            Assert.That(shot.FiringState, Is.EqualTo(368));
            Assert.That(shot.FiringPlayerState, Is.EqualTo(92));
            Assert.That(shot.EffectEquippable, Is.EqualTo(500));
            Assert.That(shot.AttackVectors, Is.EqualTo(new[] { attackVector1, attackVector2, attackVector10 }));
        });
    }

    [Test]
    public void DescriptorPipeline_DecodesContinuousEffectAndEmitsShotEvent()
    {
        var descriptor = new ReplayPlayContinuousEffectAtLocationParameters();
        var boundGroup = CreateBoundGroup(descriptor);
        var eventSink = new CapturingReplayEventSink();
        var context = new FieldDecodeContext
        {
            NetGuidCache = CreateTagCache(),
            EventSink = eventSink,
            CurrentTimeSeconds = 12.5f,
            CurrentPacketId = 99,
            ChannelIndex = 7,
            ActorNetGuid = new NetworkGuid(100),
            ObjectNetGuid = new NetworkGuid(101),
        };
        using var archive = CreateArchive(writer =>
        {
            WriteField(writer, 1, field => field.WriteUInt64(4));
            WriteField(writer, 3, field => field.WriteBit(false));
            WriteField(writer, 4, field => field.WriteBit(true));
            WriteField(writer, 5, field => field.WriteIntPacked(17));
            WriteField(writer, 6, field => WriteFloatValues(field,
            [
                (252u, 1f),
                (231u, 11f),
                (254u, 1f),
                (253u, -894045900f),
                (273u, 99f),
            ]));
            WriteField(writer, 14, field => WriteObjectValues(field,
            [
                (251u, 368u),
                (250u, 92u),
                (274u, 146u),
                (275u, 500u),
            ]));
            WriteField(writer, 26, field =>
            {
                field.WriteDouble(1521.38);
                field.WriteDouble(-10219.86);
                field.WriteDouble(476.3);
            });
            WriteField(writer, 28, field => field.WriteBits((int)EAresAlliance.AllianceAny, 3));
            WriteField(writer, 29, field => field.WriteSingle(7.3303437f));
            writer.WriteIntPacked(0);
        });

        var result = new FieldPayloadParser().ParseRepLayoutProperties(
            archive,
            boundGroup,
            ref context,
            readPropertyChecksum: false);

        var parameters = (ReplayPlayContinuousEffectAtLocationParameters)result.Payload!;
        var shotEvents = eventSink.Events.OfType<ValorantShotReceived>().ToArray();
        var shotEvent = shotEvents.Single();

        Assert.Multiple(() =>
        {
            Assert.That(shotEvents, Has.Length.EqualTo(1));
            Assert.That(result.DecodedFieldCount, Is.EqualTo(9));
            Assert.That(parameters.FloatValues, Has.Length.EqualTo(5));
            Assert.That(parameters.ObjectValues, Has.Length.EqualTo(4));
            Assert.That(shotEvent.ActorNetGuid, Is.EqualTo(100));
            Assert.That(shotEvent.ObjectNetGuid, Is.EqualTo(101));
            Assert.That(shotEvent.ChannelIndex, Is.EqualTo(7));
            Assert.That(shotEvent.Shot.EffectId, Is.EqualTo(4));
            Assert.That(shotEvent.Shot.IsLocalEffect, Is.False);
            Assert.That(shotEvent.Shot.IsTransient, Is.True);
            Assert.That(shotEvent.Shot.WaitOnReplicationActor, Is.EqualTo(17));
            Assert.That(shotEvent.Shot.AllianceFilter, Is.EqualTo(EAresAlliance.AllianceAny));
            Assert.That(shotEvent.Shot.NumProjectiles, Is.EqualTo(1));
            Assert.That(shotEvent.Shot.AmmoRemaining, Is.EqualTo(11));
            Assert.That(shotEvent.Shot.FiringState, Is.EqualTo(368));
            Assert.That(shotEvent.Shot.FiringPlayerState, Is.EqualTo(92));
            Assert.That(shotEvent.Shot.EffectEquippable, Is.EqualTo(500));
            Assert.That(archive.AtEnd, Is.True);
        });
    }

    [Test]
    public void BindingRegistry_BindsContinuousEffectParameterDescriptorFromClassNetCacheOnly()
    {
        var registry = new ExportBindingRegistry(ValorantDescriptors.CreateCatalog());
        var exports = new NetFieldExport?[3];
        exports[0] = new NetFieldExport
        {
            Handle = 0,
            CompatibleChecksum = 0,
            Name = "ReplayPlayContinuousEffectAtLocation",
        };
        exports[1] = new NetFieldExport
        {
            Handle = 1,
            CompatibleChecksum = 0,
            Name = "ReplayPlayOneShotEffectAtLocation",
        };
        exports[2] = new NetFieldExport
        {
            Handle = 2,
            CompatibleChecksum = 0,
            Name = "ReplayStopContinuousEffectAtLocation",
        };
        registry.OnExportGroupChanged(new NetFieldExportGroup
        {
            PathName = "/Script/ShooterGame.ReplayEffectComponent_ClassNetCache",
            PathNameIndex = 1,
            NetFieldExports = exports,
        });

        var boundCache = registry.GetBoundCache("/Script/ShooterGame.ReplayEffectComponent_ClassNetCache")!;
        var function = boundCache.FunctionsByHandle[0];

        Assert.Multiple(() =>
        {
            Assert.That(boundCache.FunctionsByHandle, Has.Length.EqualTo(3));
            Assert.That(function.Enabled, Is.True);
            Assert.That(function.Decoder, Is.Null);
            Assert.That(function.FunctionGroup, Is.Not.Null);
            Assert.That(function.FunctionGroup!.SourceDescriptor,
                Is.TypeOf<ReplayPlayContinuousEffectAtLocationParameters>());
            Assert.That(function.FunctionGroup.FieldsByHandle[6].Enabled, Is.True);
            Assert.That(function.FunctionGroup.FieldsByHandle[14].Enabled, Is.True);
            Assert.That(function.FunctionGroup.FieldsByHandle[29].Enabled, Is.True);
        });
    }

    [Test]
    public void BindingRegistry_BindsContinuousEffectFromSuffixlessCatalogPathBeforeExportArrives()
    {
        var registry = new ExportBindingRegistry(ValorantDescriptors.CreateCatalog());

        var boundCache = registry.GetBoundCache("/Script/ShooterGame.ReplayEffectComponent")!;
        var function = boundCache.FunctionsByHandle[0];

        Assert.Multiple(() =>
        {
            Assert.That(boundCache.FunctionsByHandle, Has.Length.EqualTo(3));
            Assert.That(function.Name, Is.EqualTo("ReplayPlayContinuousEffectAtLocation"));
            Assert.That(function.Enabled, Is.True);
            Assert.That(function.Decoder, Is.Null);
            Assert.That(function.FunctionGroup, Is.Not.Null);
            Assert.That(function.FunctionGroup!.SourceDescriptor,
                Is.TypeOf<ReplayPlayContinuousEffectAtLocationParameters>());
        });
    }

    private static BoundExportGroup CreateBoundGroup(ReplayPlayContinuousEffectAtLocationParameters descriptor)
    {
        var fieldsByHandle = new FieldBinding[30];
        foreach (var field in descriptor.Fields)
        {
            fieldsByHandle[field.Handle!.Value] = new FieldBinding
            {
                Enabled = true,
                Categories = field.Categories,
                Decoder = (IFieldDecoder)field.Decoder!,
                Name = field.PropertyName,
                ExportName = field.ExportName,
                TargetProperty = field.TargetProperty,
            };
        }

        return new BoundExportGroup
        {
            SourceDescriptor = descriptor,
            Categories = descriptor.Categories,
            Grammar = descriptor.Grammar,
            Enabled = true,
            CaptureDiagnosticFields = false,
            FieldsByHandle = fieldsByHandle,
        };
    }

    private static EffectDataFloat FloatValue(string tagName, float value) =>
        new() { Name = new FGameplayTag(0, tagName), Float = value };

    private static EffectDataVector VectorValue(string tagName, FVector value) =>
        new() { Name = new FGameplayTag(0, tagName), Vector = value };

    private static EffectDataObject ObjectValue(string tagName, uint value) =>
        new() { Name = new FGameplayTag(0, tagName), Object = value };

    private static void WriteFloatValues(BitWriter writer, IReadOnlyList<(uint TagIndex, float Value)> values)
    {
        writer.WriteIntPacked((uint)values.Count);
        for (var i = 0; i < values.Count; i++)
        {
            var i1 = i;
            writer.WriteIntPacked((uint)i + 1);
            WriteField(writer, 7, field => field.WriteIntPacked(values[i1].TagIndex));
            WriteField(writer, 8, field => field.WriteSingle(values[i1].Value));
            writer.WriteIntPacked(0);
        }

        writer.WriteIntPacked(0);
    }

    private static void WriteObjectValues(BitWriter writer, IReadOnlyList<(uint TagIndex, uint Value)> values)
    {
        writer.WriteIntPacked((uint)values.Count);
        for (var i = 0; i < values.Count; i++)
        {
            writer.WriteIntPacked((uint)i + 1);
            var i1 = i;
            WriteField(writer, 15, field => field.WriteIntPacked(values[i1].TagIndex));
            WriteField(writer, 16, field => field.WriteIntPacked(values[i1].Value));
            writer.WriteIntPacked(0);
        }

        writer.WriteIntPacked(0);
    }

    private static void WriteField(BitWriter writer, uint handle, Action<BitWriter> writePayload)
    {
        var payload = new BitWriter();
        writePayload(payload);

        writer.WriteIntPacked(handle + 1);
        writer.WriteIntPacked((uint)payload.BitCount);
        writer.WriteBits(payload.ToArray(), payload.BitCount);
    }

    private static BitArchiveReader CreateArchive(Action<BitWriter> write)
    {
        var writer = new BitWriter();
        write(writer);
        return new BitArchiveReader(writer.ToArray(), writer.BitCount);
    }

    private static NetGuidCache CreateTagCache()
    {
        var tags = new Dictionary<uint, string>
        {
            [231] = "FiringState.AmmoRemaining",
            [250] = "FiringState.FiringPlayerState",
            [251] = "FiringState.FiringState",
            [252] = "FiringState.NumProjectiles",
            [253] = "FiringState.RandomSeed",
            [254] = "FiringState.TracerOption",
            [273] = "FXC.Duration",
            [274] = "FXC.EffectContext",
            [275] = "FXC.Equippable",
        };
        var exports = new NetFieldExport?[276];
        foreach (var (tagIndex, tagName) in tags)
        {
            exports[tagIndex] = new NetFieldExport
            {
                Handle = tagIndex,
                CompatibleChecksum = 0,
                Name = tagName,
            };
        }

        var cache = new NetGuidCache();
        cache.AddExportGroup(new NetFieldExportGroup
        {
            PathName = "NetworkGameplayTagNodeIndex",
            PathNameIndex = 1,
            NetFieldExports = exports,
        });
        return cache;
    }

    private sealed class CapturingReplayEventSink : IReplayEventSink
    {
        public List<ReplayEvent> Events { get; } = [];

        public void Emit(ReplayEvent replayEvent) => Events.Add(replayEvent);
    }

    private sealed class BitWriter
    {
        private readonly List<bool> _bits = [];

        public int BitCount => _bits.Count;

        public void WriteBit(bool value) => _bits.Add(value);

        public void WriteBits(int value, int bitCount)
        {
            for (var i = 0; i < bitCount; i++)
            {
                _bits.Add((value & (1 << i)) != 0);
            }
        }

        public void WriteBits(byte[] bytes, int bitCount)
        {
            for (var i = 0; i < bitCount; i++)
            {
                _bits.Add((bytes[i >> 3] & (1 << (i & 7))) != 0);
            }
        }

        public void WriteByte(byte value)
        {
            for (var i = 0; i < 8; i++)
            {
                _bits.Add((value & (1 << i)) != 0);
            }
        }

        public void WriteIntPacked(uint value)
        {
            do
            {
                var byteVal = (byte)((value & 0x7F) << 1);
                value >>= 7;
                if (value != 0)
                {
                    byteVal |= 1;
                }

                WriteByte(byteVal);
            } while (value != 0);
        }

        public void WriteUInt64(ulong value)
        {
            foreach (var b in BitConverter.GetBytes(value))
            {
                WriteByte(b);
            }
        }

        public void WriteSingle(float value)
        {
            foreach (var b in BitConverter.GetBytes(value))
            {
                WriteByte(b);
            }
        }

        public void WriteDouble(double value)
        {
            foreach (var b in BitConverter.GetBytes(value))
            {
                WriteByte(b);
            }
        }

        public byte[] ToArray()
        {
            var bytes = new byte[(_bits.Count + 7) / 8];
            for (var i = 0; i < _bits.Count; i++)
            {
                if (_bits[i])
                {
                    bytes[i >> 3] |= (byte)(1 << (i & 7));
                }
            }

            return bytes;
        }
    }
}
