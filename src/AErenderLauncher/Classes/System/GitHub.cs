using Newtonsoft.Json;

namespace AErenderLauncher.Classes.System;

public abstract class GitHub {
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
}