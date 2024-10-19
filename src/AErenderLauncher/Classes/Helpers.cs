using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml;
using Newtonsoft.Json;
using Semver;

namespace AErenderLauncher.Classes;

public enum OS {
    Windows,
    macOS
}

public static class Helpers {
    public static OS Platform = OperatingSystem.IsWindows() ? OS.Windows : OS.macOS;//AvaloniaLocator.Current.GetService<IRuntimePlatform>()?.GetRuntimeInfo();

    public static string? GetPackageVersionStringDarwin(string path) {
        string plistPath = "";
        // macos app versions are stored in the plist file
        if (path.EndsWith(".app"))
            plistPath = Path.Combine(path, "Contents", "Info.plist");


        if (path.EndsWith(".plist"))
            plistPath = path;


        if (plistPath != "" && File.Exists(plistPath)) {
            XmlDocument plist = new XmlDocument();
            plist.Load(plistPath);
            return plist
                .SelectSingleNode("/plist/dict/key[text() = 'CFBundleShortVersionString']/following-sibling::string[1]")
                ?.InnerText;
        }

        return null;
    }

    public static bool IsFolder(string path) => Directory.Exists(path) && !File.Exists(path);
    public static bool IsFile(string path) => File.Exists(path) && !Directory.Exists(path);


    public static string GetCurrentDirectory(string path) {
        DirectoryInfo dir = new(path);

        return IsFolder(path)
            ? dir.FullName
            : dir.Parent!.FullName;
    }

    public static string GetCurrentDirectoryName(string path) {
        return new DirectoryInfo(GetCurrentDirectory(path)).Name;
    }

    public static long GetPlatformMemory() {
        return GC.GetGCMemoryInfo().TotalAvailableMemoryBytes / 1024 / 1024;
    }

    public static int GetAvailableCores() => Environment.ProcessorCount;

    public class Release {
        [JsonProperty("url")]
        public required string Url { get; set; }

        [JsonProperty("assets_url")]
        public required string AssetsUrl { get; set; }

        [JsonProperty("upload_url")]
        public required string UploadUrl { get; set; }

        [JsonProperty("html_url")]
        public required string HtmlUrl { get; set; }

        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("author")]
        public required User User { get; set; }

        [JsonProperty("node_id")]
        public required string NodeId { get; set; }

        [JsonProperty("tag_name")]
        public required string TagName { get; set; }

        [JsonProperty("target_commitish")]
        public required string TargetCommitish { get; set; }

        [JsonProperty("name")]
        public required string Name { get; set; }

        [JsonProperty("draft")]
        public bool Draft { get; set; }

        [JsonProperty("prerelease")]
        public bool Prerelease { get; set; }

        [JsonProperty("created_at")]
        public required string CreatedAt { get; set; }

        [JsonProperty("published_at")]
        public required string PublishedAt { get; set; }

        [JsonProperty("assets")]
        public required Asset[] Assets { get; set; }

        [JsonProperty("tarball_url")]
        public required string TarballUrl { get; set; }

        [JsonProperty("zipball_url")]
        public required string ZipballUrl { get; set; }

        [JsonProperty("body")]
        public required string Body { get; set; }

        [JsonProperty("mentions_count")]
        public int MentionsCount { get; set; }
    }

    public class User {
        [JsonProperty("login")]
        public required string Login { get; set; }

        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("node_id")]
        public required string NodeId { get; set; }

        [JsonProperty("avatar_url")]
        public required string AvatarUrl { get; set; }

        [JsonProperty("gravatar_id")]
        public required string GravatarId { get; set; }

        [JsonProperty("url")]
        public required string Url { get; set; }

        [JsonProperty("html_url")]
        public required string HtmlUrl { get; set; }

        [JsonProperty("followers_url")]
        public required string FollowersUrl { get; set; }

        [JsonProperty("following_url")]
        public required string FollowingUrl { get; set; }

        [JsonProperty("gists_url")]
        public required string GistsUrl { get; set; }

        [JsonProperty("starred_url")]
        public required string StarredUrl { get; set; }

        [JsonProperty("subscriptions_url")]
        public required string SubscriptionsUrl { get; set; }

        [JsonProperty("organizations_url")]
        public required string OrganizationsUrl { get; set; }

        [JsonProperty("repos_url")]
        public required string ReposUrl { get; set; }

        [JsonProperty("events_url")]
        public required string EventsUrl { get; set; }

        [JsonProperty("received_events_url")]
        public required string ReceivedEventsUrl { get; set; }

        [JsonProperty("type")]
        public required string Type { get; set; }

        [JsonProperty("user_view_type")]
        public required string UserViewType { get; set; }

        [JsonProperty("site_admin")]
        public bool SiteAdmin { get; set; }
    }

    public class Asset {
        [JsonProperty("url")]
        public required string Url { get; set; }
        [JsonProperty("id")]
        public int Id { get; set; }
        [JsonProperty("node_id")]
        public required string NodeId { get; set; }
        [JsonProperty("name")]
        public required string Name { get; set; }
        [JsonProperty("label")]
        public required object Label { get; set; }
        [JsonProperty("uploader")]
        public required User Uploader { get; set; }
        [JsonProperty("content_type")]
        public required string ContentType { get; set; }
        [JsonProperty("state")]
        public required string State { get; set; }
        [JsonProperty("size")]
        public int Size { get; set; }
        [JsonProperty("download_count")]
        public int DownloadCount { get; set; }
        [JsonProperty("created_at")]
        public required string CreatedAt { get; set; }
        [JsonProperty("updated_at")]
        public required string UpdatedAt { get; set; }
        [JsonProperty("browser_download_url")]
        public required string BrowserDownloadUrl { get; set; }
    }


    public static async Task<(SemVersion version, string downloadUrl)?> CheckForUpdates() {
        const string apiUrl = "https://api.github.com/repos/{0}/{1}/releases";
        
        var client = new HttpClient {
            DefaultRequestHeaders = {
                { "User-Agent", $"AErender-Launcher/{App.Version} ({Platform}; .NET {Environment.Version})" }
            }
        };
        
        var response = await client.GetAsync(new Uri(string.Format(apiUrl, "Night-Sky-Studio", "AErender-Launcher")));

        if (!response.IsSuccessStatusCode) return null;
        
        var content = await response.Content.ReadAsStringAsync();
        var release = JsonConvert.DeserializeObject<List<Release>>(content);

        if (release is null) return null;

        var latest = SemVersion.Parse(release.First().TagName, style: SemVersionStyles.AllowLowerV);
        
        if (App.Version.WithoutMetadata().ComparePrecedenceTo(latest) == -1) {
            return (
                latest,
                release.First().Assets.First(a => a.Name.Contains(Platform == OS.Windows ? "Windows" : "macOS"))
                    .BrowserDownloadUrl
            );
        }

        return null;
    }
}