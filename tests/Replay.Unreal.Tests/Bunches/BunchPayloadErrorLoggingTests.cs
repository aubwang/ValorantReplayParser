using Microsoft.Extensions.Logging;
using Replay.Encoding.Archives;
using Replay.Models.Net;
using Replay.Unreal.Bunches;
using Replay.Unreal.Bunches.Payload;
using Replay.Unreal.Bunches.Payload.Stages;
using Replay.Unreal.Channels;
using Replay.Unreal.PackageMap;
using Replay.Unreal.Readers;

namespace Replay.Unreal.Tests.Bunches;

public class BunchPayloadErrorLoggingTests
{
    [Test]
    public void MustBeMappedGuidsFailure_LogsArchiveException()
    {
        var loggerFactory = new CapturingLoggerFactory();
        var readerContext = CreateReaderContext(loggerFactory);
        var payload = new BitArchiveReader(ReadOnlyMemory<byte>.Empty, bitCount: 0);
        var context = new BunchPayloadContext(
            readerContext,
            new RawBunchHeader { PacketId = 12, ChIndex = 3, bHasMustBeMappedGUIDs = true },
            payload);

        var result = new MustBeMappedGuidsBunchStage().Process(ref context);

        AssertErrorLog(loggerFactory, result, "must-be-mapped GUIDs", packetId: 12, channelIndex: 3);
        Assert.That(readerContext.BunchPayloadStats.MalformedMustBeMappedGuidCount, Is.EqualTo(1));
    }

    [Test]
    public void ActorChannelOpenFailure_LogsArchiveException()
    {
        var loggerFactory = new CapturingLoggerFactory();
        var readerContext = CreateReaderContext(loggerFactory);
        var payload = new BitArchiveReader(ReadOnlyMemory<byte>.Empty, bitCount: 0);
        var context = new BunchPayloadContext(
            readerContext,
            new RawBunchHeader { PacketId = 21, ChIndex = 5, bOpen = true },
            payload);
        var stage = new ActorChannelOpenBunchStage(
            new ThrowingNewActorSerializer(),
            new NoOpActorChannelLifecycleService());

        var result = stage.Process(ref context);

        AssertErrorLog(loggerFactory, result, "open actor channel", packetId: 21, channelIndex: 5);
        Assert.That(readerContext.BunchPayloadStats.MalformedActorOpenCount, Is.EqualTo(1));
    }

    [Test]
    public void ContentBlocksFailure_LogsArchiveException()
    {
        var loggerFactory = new CapturingLoggerFactory();
        var readerContext = CreateReaderContext(loggerFactory);
        var payload = new BitArchiveReader(new byte[] { 0 }, bitCount: 1);
        var context = new BunchPayloadContext(
            readerContext,
            new RawBunchHeader { PacketId = 34, ChIndex = 8 },
            payload)
        {
            Channel = new ActorChannelState { ChannelIndex = 8 },
        };
        var framer = new ContentBlockFramer(new PackageMapReader(readerContext.NetGuidCache), readerContext);

        var result = new ContentBlocksBunchStage(framer).Process(ref context);

        AssertErrorLog(loggerFactory, result, "parse content blocks", packetId: 34, channelIndex: 8);
        Assert.That(readerContext.BunchPayloadStats.MalformedPayloadExceptionCount, Is.EqualTo(1));
    }

    private static ReplayReaderContext CreateReaderContext(ILoggerFactory loggerFactory) =>
        new(new FBinaryArchive(ReadOnlyMemory<byte>.Empty), loggerFactory: loggerFactory);

    private static void AssertErrorLog(
        CapturingLoggerFactory loggerFactory,
        BunchStageResult result,
        string operation,
        int packetId,
        uint channelIndex)
    {
        Assert.Multiple(() =>
        {
            Assert.That(result.ShouldContinue, Is.False);
            Assert.That(loggerFactory.Entries, Has.Count.EqualTo(1));
        });

        var entry = loggerFactory.Entries.Single();
        Assert.Multiple(() =>
        {
            Assert.That(entry.Level, Is.EqualTo(LogLevel.Error));
            Assert.That(entry.Exception, Is.TypeOf<ArchiveReadException>());
            Assert.That(entry.Message, Does.Contain(operation));
            Assert.That(entry.Message, Does.Contain($"packet {packetId}"));
            Assert.That(entry.Message, Does.Contain($"channel {channelIndex}"));
            Assert.That(entry.Message, Does.Contain("payload position"));
        });
    }

    private sealed class ThrowingNewActorSerializer : INewActorSerializer
    {
        public void Serialize(FBitArchive payload, ActorChannelState channelState, bool isClosingChannel) =>
            throw CreateArchiveException(nameof(ThrowingNewActorSerializer));
    }

    private sealed class NoOpActorChannelLifecycleService : IActorChannelLifecycleService
    {
        public void OpenActor(ActorChannelState channel, BunchPayloadStats stats)
        {
        }

        public void CloseActorChannel(ActorChannelState channel, RawBunchHeader header, BunchPayloadStats stats)
        {
        }
    }

    private sealed class CapturingLoggerFactory : ILoggerFactory
    {
        public List<LogEntry> Entries { get; } = [];

        public void AddProvider(ILoggerProvider provider)
        {
        }

        public ILogger CreateLogger(string categoryName) => new CapturingLogger(Entries);

        public void Dispose()
        {
        }
    }

    private sealed class CapturingLogger(List<LogEntry> entries) : ILogger
    {
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter) =>
            entries.Add(new LogEntry(logLevel, exception, formatter(state, exception)));
    }

    private sealed record LogEntry(LogLevel Level, Exception? Exception, string Message);

    private static ArchiveReadException CreateArchiveException(string operation) =>
        new(ArchiveErrorCode.EndOfArchive, operation, position: 0, length: 0, requested: 1);
}
