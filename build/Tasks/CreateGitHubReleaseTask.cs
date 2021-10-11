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
            return context.IsRunningInCI && context.IsMasterBranch;
        }

        public override async Task RunAsync(BuildContext context)
        {
            //
            // For builds on master, create a *draft* release
            //
            context.Log.Information("Creating GitHub draft release");
            var versionInfo = context.GitVersioningGetVersion(context.RootDirectory.FullPath);
            var releaseName = versionInfo.NuGetPackageVersion;

            var changeLog = context.FileSystem.GetFile(context.ChangeLogOutputPath).ReadAllText();

            var releaseSettings = new GitHubReleaseCreateSettings(context.GitHubProject.RepositoryOwner, context.GitHubProject.RepositoryName, "vNext")
            {
                Draft = true,
                Prerelease = true,
                Name = releaseName,
                Body = changeLog,
                Overwrite = true,
                TargetCommitish = context.SourceVersion,
            };

            if (context.TryGetGitHubAccessToken() is string accessToken)
            {
                releaseSettings.AccessToken = accessToken;
            }
            else
            {
                throw new Exception("No GitHub access token specified. Cannot create a GitHub Release");
            }

            context.Log.Information($"Creating release '{releaseSettings.Name}' for commit '{releaseSettings.TargetCommitish}'");
            await context.GitHubReleaseCreateAsync(releaseSettings);



            //TODO: For release builds, create non-draft release
        }
    }
}
