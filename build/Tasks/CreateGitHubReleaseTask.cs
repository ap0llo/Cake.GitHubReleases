using System;
using System.Threading.Tasks;
using Cake.Core.Diagnostics;
using Cake.Frosting;
using Cake.GitHubReleases;
using Cake.GitVersioning;

namespace Build.Tasks
{
    [TaskName("CreateGitHubRelease")]
    [IsDependentOn(typeof(GenerateChangeLogTask))]
    public class CreateGitHubReleaseTask : AsyncFrostingTask<BuildContext>
    {
        public override bool ShouldRun(BuildContext context)
        {
            return context.IsRunningInCI && (context.IsMasterBranch || context.IsReleaseBranch);
        }

        public override async Task RunAsync(BuildContext context)
        {
            context.Log.Information("Creating GitHub Releases");

            var versionInfo = context.GitVersioningGetVersion(context.RootDirectory.FullPath);

            var changeLog = context.FileSystem.GetFile(context.ChangeLogOutputPath).ReadAllText();

            if (context.TryGetGitHubAccessToken() is not string accessToken)
            {
                throw new Exception("No GitHub access token specified. Cannot create a GitHub Release");
            }

            //
            // For builds on master, create a *draft* release
            //
            if (context.IsMasterBranch)
            {
                await CreateDraftRelease(context, accessToken, versionInfo.NuGetPackageVersion, changeLog);
            }

            //
            // For builds on release branches, create a non-draft release
            //
            if (context.IsReleaseBranch)
            {
                await CreateRelease(context, accessToken, versionInfo.NuGetPackageVersion, changeLog);
            }
        }


        private async Task CreateDraftRelease(BuildContext context, string accessToken, string version, string changeLog)
        {
            var releaseSettings = new GitHubReleaseCreateSettings(context.GitHubProject.RepositoryOwner, context.GitHubProject.RepositoryName, "vNext")
            {
                Draft = true,
                Prerelease = true,
                Name = $"v{version}",
                Body = changeLog,
                Overwrite = true,
                TargetCommitish = context.SourceVersion,
                AccessToken = accessToken
            };

            await context.GitHubReleaseCreateAsync(releaseSettings);
        }

        private async Task CreateRelease(BuildContext context, string accessToken, string version, string changeLog)
        {
            var releaseSettings = new GitHubReleaseCreateSettings(context.GitHubProject.RepositoryOwner, context.GitHubProject.RepositoryName, $"v{version}")
            {
                Name = $"v{version}",
                Body = changeLog,
                TargetCommitish = context.SourceVersion,
                AccessToken = accessToken
            };

            await context.GitHubReleaseCreateAsync(releaseSettings);
        }
    }
}
