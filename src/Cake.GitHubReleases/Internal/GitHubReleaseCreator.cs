using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Cake.Core.Diagnostics;
using Cake.Core.IO;
using Octokit;

namespace Cake.GitHubReleases.Internal
{
    internal sealed class GitHubReleaseCreator
    {
        private readonly ICakeLog m_CakeLog;
        private readonly IFileSystem m_FileSystem;
        private readonly IGitHubClientFactory m_ClientFactory;


        public GitHubReleaseCreator(ICakeLog cakeLog, IFileSystem fileSystem, IGitHubClientFactory clientFactory)
        {
            m_CakeLog = cakeLog ?? throw new ArgumentNullException(nameof(cakeLog));
            m_FileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
            m_ClientFactory = clientFactory ?? throw new ArgumentNullException(nameof(clientFactory));
        }


        public async Task<GitHubRelease> CreateRelease(GitHubReleaseCreateSettings settings)
        {
            if (settings is null)
                throw new ArgumentNullException(nameof(settings));

            ValidateSettings(settings);

            m_CakeLog.Information("Creating GitHub Release");

            // TODO: Add option to update an existing release

            var release = await CreateNewRelease(settings);
            return release;
        }


        private void ValidateSettings(GitHubReleaseCreateSettings settings)
        {
            var assetNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var filePath in settings.AssetsOrEmptyList)
            {
                var file = m_FileSystem.GetFile(filePath);

                if (!file.Exists)
                    throw new FileNotFoundException($"Cannot create GitHub Release with asset '{filePath}' because the file does not exist");

                var fileName = file.Path.GetFilename().ToString();
                if (assetNames.Contains(fileName))
                {
                    throw new AssetConflictException($"Cannot create GitHub release with multiple assets named '{fileName}'");
                }
                assetNames.Add(fileName);
            }
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

            var createdRelease = await client.Repository.Release.Create(settings.RepositoryOwner, settings.RepositoryName, newRelease);

            //TODO: More logging

            // upload assets
            foreach (var filePath in settings.AssetsOrEmptyList)
            {
                using var stream = m_FileSystem.GetFile(filePath).OpenRead();
                var assetUpload = new ReleaseAssetUpload()
                {
                    FileName = filePath.GetFilename().ToString(),
                    ContentType = "application/octet-stream",
                    RawData = stream
                };
                var asset = await client.Repository.Release.UploadAsset(createdRelease, assetUpload);
            }

            //TODO: Include asset information in return value
            return new GitHubRelease(createdRelease.Id);
        }
    }
}
