using System.Collections.Generic;
using Cake.Core;
using Cake.Core.IO;
using Cake.Frosting;

namespace Build
{
    public class BuildContext : FrostingContext
    {
        /// <summary>
        /// Gets whether the current build is running in a CI environment
        /// </summary>
        public bool IsRunningInCI => AzurePipelines.IsActive;

        public AzurePipelinesContext AzurePipelines { get; }

        public GitContext Git { get; }

        public GitHubContext GitHub { get; }

        public IReadOnlyCollection<PushTarget> PushTargets { get; } = new[]
        {
            new PushTarget(
                PushTargetType.AzureArtifacts,
                "https://pkgs.dev.azure.com/ap0llo/OSS/_packaging/Cake.GitHubReleases/nuget/v3/index.json",
                context => context.Git.IsMasterBranch || context.Git.IsReleaseBranch
            ),
            new PushTarget(
                PushTargetType.NuGetOrg,
                "https://api.nuget.org/v3/index.json",
                context => context.Git.IsReleaseBranch
            ),
        };

        public OutputContext Output { get; }

        public BuildSettings BuildSettings { get; }



        /// <summary>
        /// Gets the root directory of the current repository
        /// </summary>
        public DirectoryPath RootDirectory { get; }

        /// <summary>
        /// Gets the path of the Visual Studio Solution to build
        /// </summary>
        public FilePath SolutionPath => RootDirectory.CombineWithFilePath("Cake.GitHubReleases.sln");




        public BuildContext(ICakeContext context) : base(context)
        {
            RootDirectory = context.Environment.WorkingDirectory;

            AzurePipelines = new(this);
            Git = new(this);
            GitHub = new(this);
            Output = new(this);
            BuildSettings = new(this);
        }





    }
}
