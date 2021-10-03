using Octokit;

namespace Cake.GitHubReleases.Internal
{
    internal interface IGitHubClientFactory
    {
        IGitHubClient CreateClient(string hostName, string? accessToken);
    }
}

