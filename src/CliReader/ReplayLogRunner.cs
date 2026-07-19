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

        Console.WriteLine($"Took: {stopwatch.ElapsedMilliseconds}ms");
        Console.WriteLine($"Read replay {context.ReplayInfo.FriendlyName}");
        Console.WriteLine($"Version {context.ReplayVersion.Branch}");
        Console.WriteLine($"Chunks {context.ReplayInfo.Chunks.Count}");
        Console.WriteLine($"Timestamp {context.ReplayInfo.Timestamp}");
        Console.WriteLine($"Duration {TimeSpan.FromMilliseconds(context.ReplayInfo.LengthInMs)}");
        Console.WriteLine($"File Size {file.Length / 1_000_000} MB");
        Console.WriteLine(
            $"Packet Stats: Bunch Count={context.PacketStats.BunchCount}\tPacket Count={context.PacketStats.PacketCount}\tMalformedPacketCount={context.PacketStats.MalformedPacketCount}\tPartialErrorCount={context.PacketStats.PartialErrorCount}\tTTL Bytes={context.PacketStats.TotalPacketBytes / 1_000_000} MB");
        actorEventLogger.LogSummary();
    }
}