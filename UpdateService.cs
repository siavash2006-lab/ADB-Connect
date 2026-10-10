using System.Net.Http;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace ADB_Connect;

internal sealed record AvailableUpdate(Version Version, string Tag, string PageUrl, UpdateAsset? Asset);
internal sealed record UpdateAsset(string Name, string Url, long Size, string Sha256);

internal sealed class UpdateService : IDisposable
{
    internal const string Repository = "siavash2006-lab/ADB-Connect";
    internal const long MaxAssetSize = 500L * 1024 * 1024;
    private readonly HttpClient _http;
    public UpdateService(HttpMessageHandler? handler = null)
    {
        _http = handler == null ? new HttpClient() : new HttpClient(handler);
        _http.Timeout = Timeout.InfiniteTimeSpan;
        _http.DefaultRequestHeaders.UserAgent.ParseAdd("ADB-Connect-Updater/1.0");
        _http.DefaultRequestHeaders.Accept.ParseAdd("application/vnd.github+json");
    }
    public async Task<AvailableUpdate?> CheckAsync(Version current, string architecture, bool installed, CancellationToken token)
    {
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(token);
        timeout.CancelAfter(TimeSpan.FromSeconds(15));
        using var response = await _http.GetAsync($"https://api.github.com/repos/{Repository}/releases/latest", timeout.Token);
        response.EnsureSuccessStatusCode();
        string json = await response.Content.ReadAsStringAsync(timeout.Token);
        return ParseRelease(json, current, architecture, installed);
    }
    internal static AvailableUpdate? ParseRelease(string json, Version current, string architecture, bool installed)
    {
        if (architecture is not ("x64" or "x86")) throw new ArgumentException("Unsupported architecture.");
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;
        if (root.GetProperty("draft").GetBoolean() || root.GetProperty("prerelease").GetBoolean()) return null;
        string tag = root.GetProperty("tag_name").GetString() ?? "";
        if (!Regex.IsMatch(tag, @"^v?\d+\.\d+\.\d+(\.\d+)?$")) return null;
        if (!Version.TryParse(tag.TrimStart('v'), out var version)) return null;
        static Version Normalize(Version v) => new(v.Major, v.Minor, Math.Max(0,v.Build), Math.Max(0,v.Revision));
        if (Normalize(version) <= Normalize(current)) return null;
        string page = $"https://github.com/{Repository}/releases/tag/{tag}";
        string name = $"ADB-Connect-{tag.TrimStart('v')}-{architecture}-{(installed ? "Setup.exe" : "Portable.zip")}";
        UpdateAsset? asset = null;
        foreach (var item in root.GetProperty("assets").EnumerateArray())
        {
            if (item.GetProperty("name").GetString() != name) continue;
            string url = item.GetProperty("browser_download_url").GetString() ?? "";
            string expected = $"https://github.com/{Repository}/releases/download/{tag}/{name}";
            string digest = item.TryGetProperty("digest", out var d) ? d.GetString() ?? "" : "";
            long size = item.GetProperty("size").GetInt64();
            if (url == expected && Regex.IsMatch(digest, "^sha256:[a-fA-F0-9]{64}$") && size is > 0 and <= MaxAssetSize)
                asset = new(name, url, size, digest[7..]);
            break;
        }
        return new(version, tag, page, asset);
    }
    public async Task<string> DownloadAsync(UpdateAsset asset, string directory, IProgress<int>? progress, CancellationToken token)
    {
        if (Path.GetFileName(asset.Name) != asset.Name || asset.Size is <= 0 or > MaxAssetSize)
            throw new InvalidDataException("Invalid update asset.");
        Directory.CreateDirectory(directory);
        string final = Path.Combine(directory, asset.Name);
        string partial = final + ".partial";
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(token);
        timeout.CancelAfter(TimeSpan.FromMinutes(15));
        try
        {
            using var response = await _http.GetAsync(asset.Url, HttpCompletionOption.ResponseHeadersRead, timeout.Token);
            response.EnsureSuccessStatusCode();
            if (response.Content.Headers.ContentLength is long length && length != asset.Size)
                throw new InvalidDataException("The update size does not match GitHub.");
            using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
            long received = 0; int lastPercent = -1;
            await using (var output = new FileStream(partial, FileMode.CreateNew, FileAccess.Write, FileShare.None, 81920, true))
            await using (var input = await response.Content.ReadAsStreamAsync(timeout.Token))
            {
                byte[] buffer = new byte[81920];
                int count;
                while ((count = await input.ReadAsync(buffer, timeout.Token)) > 0)
                {
                    received += count;
                    if (received > asset.Size) throw new InvalidDataException("The update exceeds its declared size.");
                    hash.AppendData(buffer, 0, count);
                    await output.WriteAsync(buffer.AsMemory(0, count), timeout.Token);
                    int percent = (int)(received * 100 / asset.Size);
                    if (percent != lastPercent) { lastPercent = percent; progress?.Report(percent); }
                }
            }
            string actual = Convert.ToHexString(hash.GetHashAndReset());
            if (received != asset.Size || !actual.Equals(asset.Sha256, StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("Update verification failed. Nothing was installed.");
            File.Move(partial, final);
            return final;
        }
        catch { if (File.Exists(partial)) File.Delete(partial); throw; }
    }
    public void Dispose() => _http.Dispose();
}
