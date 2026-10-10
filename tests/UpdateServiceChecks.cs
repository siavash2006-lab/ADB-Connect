using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using ADB_Connect;

internal static class UpdateServiceChecks
{
    internal static async Task RunAsync()
    {
        int checks = 0;
        void Check(bool value, string name) { if (!value) throw new Exception("Update check failed: " + name); checks++; }
        byte[] payload = Encoding.UTF8.GetBytes("test update binary");
        string digest = Convert.ToHexString(SHA256.HashData(payload));
        string Json(string tag = "v1.6.8", string arch = "x64", bool installed = true, bool preview = false, string? hash = null, bool foreign = false)
        {
            string name = $"ADB-Connect-{tag.TrimStart('v')}-{arch}-{(installed ? "Setup.exe" : "Portable.zip")}";
            return JsonSerializer.Serialize(new { draft = false, prerelease = preview, tag_name = tag, assets = new[] {
                new { name, size = payload.Length, digest = hash ?? "sha256:" + digest,
                    browser_download_url = foreign ? "https://example.org/payload.exe" :
                    $"https://github.com/{UpdateService.Repository}/releases/download/{tag}/{name}" } } });
        }
        var current = new Version(1,6,7,0);
        AvailableUpdate? Parse(string json, string arch = "x64", bool installed = true) => UpdateService.ParseRelease(json,current,arch,installed);
        Check(Parse(Json("v1.6.7")) == null, "three/four component equal version");
        Check(Parse(Json("v1.6.6")) == null, "older release ignored");
        Check(Parse(Json("v1.6.8-rc1")) == null && Parse(Json(preview:true)) == null, "previews ignored");
        Check(Parse(Json())?.Asset?.Name.EndsWith("x64-Setup.exe") == true, "installer selected");
        Check(Parse(Json(arch:"x86",installed:false),"x86",false)?.Asset?.Name.EndsWith("x86-Portable.zip") == true, "x86 portable selected");
        Check(Parse(Json(arch:"x86"))?.Asset == null, "wrong architecture fallback");
        Check(Parse(Json(hash:""))?.Asset == null, "missing hash disables auto download");
        Check(Parse(Json(foreign:true))?.Asset == null, "foreign download refused");
        string root = Path.Combine(Path.GetTempPath(),"adb-update-tests-"+Guid.NewGuid().ToString("N"));
        try
        {
            using var service = new UpdateService(new StubHandler(payload));
            var asset = Parse(Json())!.Asset!;
            string path = await service.DownloadAsync(asset,Path.Combine(root,"valid"),null,default);
            Check(File.ReadAllBytes(path).SequenceEqual(payload), "verified binary retained");
            bool rejected = false;
            try { await service.DownloadAsync(asset with { Sha256 = new string('0',64) },Path.Combine(root,"bad"),null,default); }
            catch (InvalidDataException) { rejected = true; }
            Check(rejected && Directory.GetFiles(Path.Combine(root,"bad")).Length == 0, "bad digest leaves no executable");
            using var cancelled = new CancellationTokenSource(); cancelled.Cancel();
            bool stopped = false;
            try { await service.DownloadAsync(asset,Path.Combine(root,"cancel"),null,cancelled.Token); }
            catch (OperationCanceledException) { stopped = true; }
            Check(stopped && Directory.GetFiles(Path.Combine(root,"cancel")).Length == 0, "cancellation cleanup");
        }
        finally { if(Directory.Exists(root)) Directory.Delete(root,true); }
        Console.WriteLine($"All {checks} update checks passed using simulated HTTP responses.");
    }
    private sealed class StubHandler(byte[] bytes) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK){Content=new ByteArrayContent(bytes)});
        }
    }
}
