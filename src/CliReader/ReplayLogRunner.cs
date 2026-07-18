using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Replay.Encoding.Archives;
using Replay.Models.Descriptors;
using Replay.Valorant;

namespace CliReader;

internal sealed class ReplayLogRunner
{
    private readonly ILoggerFactory _loggerFactory;
    private readonly ILogger _logger;

    public ReplayLogRunner(ILoggerFactory loggerFactory, ILogger logger)
    {
        _loggerFactory = loggerFactory;
        _logger = logger;
    }

    public void Run(LogOptions options)
    {
        _logger.LogInformation("Reading replay {ReplayPath}", options.ReplayPath);

        var stopwatch = Stopwatch.StartNew();
        using var file = File.OpenRead(options.ReplayPath);
        using var archive = new FBinaryArchive(file);

        var actorEventLogger = new ActorEventLogger(_loggerFactory.CreateLogger<ActorEventLogger>());
        var context = ValorantReplayReader.CreateDefault(
            _loggerFactory,
            actorEventLogger,
            ParseProfile.Default).Read(archive);

        _logger.LogInformation("Took: {ElapsedMilliseconds}ms", stopwatch.ElapsedMilliseconds);
        _logger.LogInformation("Read replay {ReplayName}", context.ReplayInfo.FriendlyName);
        _logger.LogInformation("Version {ReplayVersion}", context.ReplayVersion.Branch);
        _logger.LogInformation("Chunks {ChunkCount}", context.ReplayInfo.Chunks.Count);
        _logger.LogInformation("Timestamp {Timestamp}", context.ReplayInfo.Timestamp);
        _logger.LogInformation("Duration {Duration}", TimeSpan.FromMilliseconds(context.ReplayInfo.LengthInMs));
        _logger.LogInformation("File Size {FileSize} MB", file.Length / 1_000_000);
        _logger.LogInformation(
            "Packet Stats: Bunch Count={BunchCount}\tPacket Count={PacketCount}\tMalformedPacketCount={MalformedPacketCount}\tPartialErrorCount={PartialErrorCount}\tTTL Bytes={TotalBytes} MB",
            context.PacketStats.BunchCount,
            context.PacketStats.PacketCount,
            context.PacketStats.MalformedPacketCount,
            context.PacketStats.PartialErrorCount,
            context.PacketStats.TotalPacketBytes / 1_000_000);
        actorEventLogger.LogSummary();
    }
}
