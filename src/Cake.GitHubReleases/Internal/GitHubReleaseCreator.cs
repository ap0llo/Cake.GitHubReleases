using System;
using System.Threading.Tasks;
using Cake.Core.Diagnostics;
using Octokit;

namespace Cake.GitHubReleases.Internal
{
    internal sealed class GitHubReleaseCreator
    {
        private readonly ICakeLog m_CakeLog;
        private readonly IGitHubClientFactory m_ClientFactory;


        public GitHubReleaseCreator(ICakeLog cakeLog, IGitHubClientFactory clientFactory)
        {
            m_CakeLog = cakeLog ?? throw new ArgumentNullException(nameof(cakeLog));
            m_ClientFactory = clientFactory ?? throw new ArgumentNullException(nameof(clientFactory));
        }


        public async Task<GitHubRelease> CreateRelease(GitHubReleaseCreateSettings settings)
        {
            if (settings is null)
                throw new ArgumentNullException(nameof(settings));

            m_CakeLog.Information("Creating GitHub Release");

            // TODO: Add option to update an existing release

            var release = await CreateNewRelease(settings);
            return release;
        }

        private async Task<GitHubRelease> CreateNewRelease(GitHubReleaseCreateSettings settings)
        {
            var client = m_ClientFactory.CreateClient(settings.HostName, settings.AccessToken);

            var newRelease = new NewRelease(settings.TagName)
            {
                TargetCommitish = settings.TargetCommitish,
                Name = settings.Name,
                Body = settings.Body,
                Draft = settings.Draft,
                Prerelease = settings.Prerelease
            };

            var result = await client.Repository.Release.Create(settings.RepositoryOwner, settings.RepositoryName, newRelease);

            return new GitHubRelease(result.Id);
        }
    }
}
