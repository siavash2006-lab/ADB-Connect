using System.Text.RegularExpressions;

namespace ADB_Connect;

internal static partial class CommandRules
{
    public static bool IsPackageName(string value) => PackageRegex().IsMatch(value);
    public static bool Succeeded(string operation, int exitCode, string stdout, string stderr)
    {
        if (exitCode != 0) return false;
        string output = stdout + "\n" + stderr;
        if (FailureRegex().IsMatch(output)) return false;
        return operation switch
        {
            "install" or "uninstall" => stdout.Split('\n').Any(x => x.Trim() == "Success"),
            "start" => output.Contains("Events injected: 1", StringComparison.Ordinal),
            "stop" => true, // am force-stop normally succeeds without stdout.
            _ => false
        };
    }

    public static List<string> OnlineSerials(string output) => output.Split('\n')
        .Select(x => x.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries))
        .Where(x => x.Length >= 2 && x[1] == "device")
        .Select(x => x[0]).Distinct(StringComparer.Ordinal).ToList();

    [GeneratedRegex(@"^[A-Za-z][A-Za-z0-9_]*(\.[A-Za-z0-9_]+)*$")]
    private static partial Regex PackageRegex();
    [GeneratedRegex(@"(^|\n)\s*(Failure\b|Error\b|Exception\b)|monkey aborted|No activities found|SecurityException", RegexOptions.IgnoreCase)]
    private static partial Regex FailureRegex();
}
