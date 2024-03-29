﻿using System;
using System.Threading.Tasks;
using Cake.Core;
using Cake.Core.Annotations;
using Cake.GitHubReleases.Internal;

namespace Cake.GitHubReleases
{
    [CakeAliasCategory("GitHubRelease")]
    public static class GitHubReleaseAliases
    {
        /// <summary>
        /// Creates a new GitHub Release with the specified settings.
        /// </summary>
        [CakeMethodAlias]
        public static async Task<GitHubRelease> GitHubReleaseCreateAsync(this ICakeContext context, GitHubReleaseCreateSettings settings)
        {
            if (settings is null)
                throw new ArgumentNullException(nameof(settings));

            var releaseCreator = new GitHubReleaseCreator(context.Log, context.FileSystem, new GitHubClientFactory());
            return await releaseCreator.CreateRelease(settings);
        }
    }
}
