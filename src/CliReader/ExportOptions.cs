using Replay.Models.Descriptors;

namespace CliReader;

internal sealed record ExportOptions(
    string ReplayPath,
    string OutputDirectory,
    string ProfileName,
    ParseProfile ParseProfile)
{
    public static bool TryParse(string[] args, out ExportOptions? options, out string? error)
    {
        options = null;
        error = null;
        if (args.Length < 4 || args[0] != "export")
        {
            error = Usage;
            return false;
        }

        return TryParseArguments(args, out options, out error);
    }

    public const string Usage =
        "Usage: CliReader export <replay-path> --output <directory> [--profile viewer]";

    private static bool TryParseArguments(
        string[] args,
        out ExportOptions? options,
        out string? error)
    {
        string? outputDirectory = null;
        var profileName = "default";
        for (var index = 2; index < args.Length; index += 2)
        {
            if (index + 1 >= args.Length)
            {
                return Fail($"Missing value for {args[index]}.", out options, out error);
            }

            if (!TrySetOption(args[index], args[index + 1], ref outputDirectory, ref profileName, out error))
            {
                options = null;
                return false;
            }
        }

        if (outputDirectory is null)
        {
            return Fail("--output is required.", out options, out error);
        }

        var profile = profileName == "viewer"
            ? new ParseProfile
            {
                EnabledCategories = ExportCategory.All,
                CaptureDiagnosticFields = true,
            }
            : ParseProfile.Default;
        options = new ExportOptions(args[1], outputDirectory, profileName, profile);
        error = null;
        return true;
    }

    private static bool TrySetOption(
        string name,
        string value,
        ref string? outputDirectory,
        ref string profileName,
        out string? error)
    {
        error = null;
        switch (name)
        {
            case "--output" when outputDirectory is null:
                outputDirectory = value;
                return true;
            case "--profile" when profileName == "default" && value == "viewer":
                profileName = value;
                return true;
            case "--profile":
                error = "Only the 'viewer' export profile is supported.";
                return false;
            default:
                error = $"Unknown or duplicate option '{name}'.";
                return false;
        }
    }

    private static bool Fail(
        string message,
        out ExportOptions? options,
        out string? error)
    {
        options = null;
        error = message;
        return false;
    }
}
