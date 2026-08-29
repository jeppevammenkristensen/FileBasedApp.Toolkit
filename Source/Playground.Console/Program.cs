// See https://aka.ms/new-console-template for more information

using System.Net.Http.Json;
using System.Text.Json.Serialization;
using FileBasedApp.Toolkit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Spectre.Console;

var url = AbsoluteWebUri.Create("https://www.dr.dk")
          / UriPathSegment.From("first") 
          / UriPathSegment.From("second") 
          / UriQueryString.From("a=1&b=2")
          / UriFragment.From("Fragment");

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((ctx, services) =>
    {
        services.AddHttpClient<GithubClient>(cfg =>
        {
            cfg.DefaultRequestHeaders.Add("User-Agent", "FileBasedApp.Toolkit");
        });
    }).Build();

var service = host.Services.GetRequiredService<GithubClient>();
var githubRootObject = await service.GetRepos();

foreach (var item in githubRootObject)
{
    AnsiConsole.MarkupLineInterpolated($"[green]{item.Name}[/]");
    AnsiConsole.MarkupLineInterpolated($"[dim]\t{item.Description}[/]");
    AnsiConsole.MarkupLineInterpolated($"[dim]\t{item.HtmlUrl}[/]");
}

//AnsiConsole.Write(new JsonText(githubRootObject.SerializeToJson(GithubRepoJsonContext.Default.RepoObjectArray)));

public class GithubClient
{
        public AbsoluteWebUri BaseUrl = AbsoluteWebUri.Create("https://api.github.com/users/jeppevammenkristensen");
    
    private readonly HttpClient _client;

    public GithubClient(HttpClient client)
    {
        _client = client;
    }
    
    public async Task<GithubUserRootObject> GetRoot()
    {
        var httpResponseMessage = await _client.GetAsync(BaseUrl);
        return await httpResponseMessage.Content.ReadFromJsonAsync(GithubUserRootJsonContext.Default.GithubUserRootObject) ?? throw new InvalidOperationException();
    }

    public async Task<RepoObject[]> GetRepos()
    {
        var reposUri = BaseUrl / UriPathSegment.From("repos");
        return await (await _client.GetAsync(reposUri)).Content.ReadFromJsonAsync(GithubRepoJsonContext.Default.RepoObjectArray) ?? throw new InvalidOperationException();
    }
}

[JsonSerializable(typeof(GithubUserRootObject))]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.SnakeCaseLower)]
public partial class GithubUserRootJsonContext : JsonSerializerContext
{
    
}

[JsonSerializable(typeof(RepoObject[]))]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.SnakeCaseLower)]
public partial class GithubRepoJsonContext : JsonSerializerContext
{
    
}

public record GithubUserRootObject(
    string Login,
    int Id,
    string NodeId,
    string AvatarUrl,
    string GravatarId,
    string Url,
    string HtmlUrl,
    string FollowersUrl,
    string FollowingUrl,
    string GistsUrl,
    string StarredUrl,
    string SubscriptionsUrl,
    string OrganizationsUrl,
    string ReposUrl,
    string EventsUrl,
    string ReceivedEventsUrl,
    string Type,
    string UserViewType,
    bool SiteAdmin,
    string Name,
    string Company,
    string Blog,
    string Location,
    object Email,
    object Hireable,
    object Bio,
    object TwitterUsername,
    int PublicRepos,
    int PublicGists,
    int Followers,
    int Following,
    string CreatedAt,
    string UpdatedAt
);

public record RepoObject(
    int Id,
    string NodeId,
    string Name,
    string FullName,
    bool Private,
    Owner Owner,
    string HtmlUrl,
    string Description,
    bool Fork,
    string Url,
    string ForksUrl,
    string KeysUrl,
    string CollaboratorsUrl,
    string TeamsUrl,
    string HooksUrl,
    string IssueEventsUrl,
    string EventsUrl,
    string AssigneesUrl,
    string BranchesUrl,
    string TagsUrl,
    string BlobsUrl,
    string GitTagsUrl,
    string GitRefsUrl,
    string TreesUrl,
    string StatusesUrl,
    string LanguagesUrl,
    string StargazersUrl,
    string ContributorsUrl,
    string SubscribersUrl,
    string SubscriptionUrl,
    string CommitsUrl,
    string GitCommitsUrl,
    string CommentsUrl,
    string IssueCommentUrl,
    string ContentsUrl,
    string CompareUrl,
    string MergesUrl,
    string ArchiveUrl,
    string DownloadsUrl,
    string IssuesUrl,
    string PullsUrl,
    string MilestonesUrl,
    string NotificationsUrl,
    string LabelsUrl,
    string ReleasesUrl,
    string DeploymentsUrl,
    string CreatedAt,
    string UpdatedAt,
    string PushedAt,
    string GitUrl,
    string SshUrl,
    string CloneUrl,
    string SvnUrl,
    string Homepage,
    int Size,
    int StargazersCount,
    int WatchersCount,
    string Language,
    bool HasIssues,
    bool HasProjects,
    bool HasDownloads,
    bool HasWiki,
    bool HasPages,
    bool HasDiscussions,
    int ForksCount,
    object MirrorUrl,
    bool Archived,
    bool Disabled,
    int OpenIssuesCount,
    License License,
    bool AllowForking,
    bool IsTemplate,
    bool WebCommitSignoffRequired,
    bool HasPullRequests,
    string PullRequestCreationPolicy,
    string[] Topics,
    string Visibility,
    int Forks,
    int OpenIssues,
    int Watchers,
    string DefaultBranch
);

public record Owner(
    string Login,
    int Id,
    string NodeId,
    string AvatarUrl,
    string GravatarId,
    string Url,
    string HtmlUrl,
    string FollowersUrl,
    string FollowingUrl,
    string GistsUrl,
    string StarredUrl,
    string SubscriptionsUrl,
    string OrganizationsUrl,
    string ReposUrl,
    string EventsUrl,
    string ReceivedEventsUrl,
    string Type,
    string UserViewType,
    bool SiteAdmin
);

public record License(
    string Key,
    string Name,
    string SpdxId,
    string Url,
    string NodeId
);



    
    