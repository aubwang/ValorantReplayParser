using System.Reflection;
using System.Text.Json;
using Replay.Unreal.Readers;
using Replay.Valorant;

namespace CliReader;

internal sealed class ReplayExportManifestWriter
{
    private const int SchemaVersion = 1;

    public void Write(
        string outputDirectory,
        string sourcePath,
        string sourceSha256,
        long sourceSize,
        string profileName,
        ReplayReaderContext context,
        ReplayExportSink sink)
    {
        var path = Path.Combine(outputDirectory, "manifest.json");
        var temporaryPath = path + ".tmp";
        try
        {
            using (var stream = new FileStream(
                       temporaryPath,
                       FileMode.Create,
                       FileAccess.Write,
                       FileShare.None))
            using (var writer = new Utf8JsonWriter(stream, new JsonWriterOptions { Indented = true }))
            {
                WriteManifest(
                    writer,
                    sourcePath,
                    sourceSha256,
                    sourceSize,
                    profileName,
                    context,
                    sink);
            }

            File.Move(temporaryPath, path, overwrite: true);
        }
        catch
        {
            File.Delete(temporaryPath);
            throw;
        }
    }

    private static void WriteManifest(
        Utf8JsonWriter writer,
        string sourcePath,
        string sourceSha256,
        long sourceSize,
        string profileName,
        ReplayReaderContext context,
        ReplayExportSink sink)
    {
        var version = context.ReplayVersion;
        var parserAssembly = typeof(ValorantReplayReader).Assembly;

        writer.WriteStartObject();
        writer.WriteNumber("schema_version", SchemaVersion);
        writer.WriteString("source_file", Path.GetFileName(sourcePath));
        writer.WriteString("source_sha256", sourceSha256);
        writer.WriteNumber("source_size_bytes", sourceSize);
        writer.WriteString("replay_build", version.Branch);
        writer.WriteString("replay_version", $"{version.Major}.{version.Minor}.{version.Patch}");
        writer.WriteNumber("replay_changelist", version.Changelist);
        writer.WriteNumber("duration_ms", context.ReplayInfo.LengthInMs);
        writer.WriteString("parse_profile", profileName);
        writer.WriteString("parser_assembly", parserAssembly.GetName().Name);
        writer.WriteString("parser_version", ParserVersion(parserAssembly));
        WriteStats(writer, context);
        WriteCounts(writer, sink);
        WriteLimitations(writer);
        writer.WriteEndObject();
    }

    private static void WriteStats(Utf8JsonWriter writer, ReplayReaderContext context)
    {
        var stats = context.PacketStats;
        writer.WriteStartObject("stats");
        writer.WriteNumber("packet_count", stats.PacketCount);
        writer.WriteNumber("packets_with_bunches", stats.PacketsWithBunches);
        writer.WriteNumber("bunch_count", stats.BunchCount);
        writer.WriteNumber("malformed_packet_count", stats.MalformedPacketCount);
        writer.WriteNumber("partial_error_count", stats.PartialErrorCount);
        writer.WriteNumber("total_packet_bytes", stats.TotalPacketBytes);
        writer.WriteEndObject();
    }

    private static void WriteCounts(Utf8JsonWriter writer, ReplayExportSink sink)
    {
        writer.WriteStartObject("counts");
        writer.WriteNumber("movement", sink.MovementCount);
        writer.WriteNumber("events", sink.EventCount);
        writer.WriteNumber("actor_spawned", sink.ActorSpawnedCount);
        writer.WriteNumber("actor_closed", sink.ActorClosedCount);
        writer.WriteNumber("export_group_received", sink.ExportGroupCount);
        writer.WriteNumber("rpc_received", sink.RpcCount);
        writer.WriteNumber("filtered_export_groups", sink.FilteredExportGroupCount);
        writer.WriteEndObject();
    }

    private static void WriteLimitations(Utf8JsonWriter writer)
    {
        writer.WriteStartArray("limitations");
        writer.WriteStringValue(
            "Only the latest decoded move in each remote-character update is currently emitted.");
        writer.WriteStringValue(
            "Undecoded export groups and decoded export shells without payloads are omitted.");
        writer.WriteStringValue(
            "Unknown payload object graphs are bounded to eight levels and 4096 collection items.");
        writer.WriteEndArray();
    }

    private static string ParserVersion(Assembly assembly)
    {
        return assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                   ?.InformationalVersion
               ?? assembly.GetName().Version?.ToString()
               ?? "unknown";
    }
}
