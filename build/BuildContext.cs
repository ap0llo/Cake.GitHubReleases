﻿using System;
using Cake.Common;
using Cake.Common.Build;
using Cake.Common.Tools.DotNetCore.MSBuild;
using Cake.Core;
using Cake.Core.IO;
using Cake.Frosting;

namespace Build
{
    //TODO: Clean up
    public class BuildContext : FrostingContext
    {
        public class ArtifactNameSettings
        {
            /// <summary>
            /// The name of the main artifact on Azure Pipelines
            /// </summary>
            public string Binaries => "Binaries";

            /// <summary>
            /// The Azure Pipelines artifact name under which to save test result files
            /// </summary>
            public string TestResults => "TestResults";

            public string ChangeLog => "ChangeLog";
        }

        public class GitHubProjectInformation
        {
            //TODO: Determine from git url or similar
            public string RepositoryOwner => "ap0llo";

            public string RepositoryName => "Cake.GitHubReleases";
        }

        /// <summary>
        /// Gets the names of the Artifacts to publish (when running in Azure DevOps)
        /// </summary>
        public ArtifactNameSettings ArtifactNames { get; } = new();


        /// <summary>
        /// Gets the configuration to build (Debug/Relesae)
        /// </summary>
        public string BuildConfiguration { get; set; }

        /// <summary>
        /// Gets the root directory of the current repository
        /// </summary>
        public DirectoryPath RootDirectory { get; }

        /// <summary>
        /// Gets the path of the Visual Studio Solution to build
        /// </summary>
        public FilePath SolutionPath => RootDirectory.CombineWithFilePath("Cake.GitHubReleases.sln");

        /// <summary>
        /// Gets whether the current build is running in a CI environment
        /// </summary>
        public bool IsRunningInCI => this.IsRunningOnAzurePipelines();

        /// <summary>
        /// Gets the root output directory
        /// </summary>
        public DirectoryPath BinariesDirectory { get; }

        /// <summary>
        /// Gets the output directory for test results
        /// </summary>
        public DirectoryPath TestResultsPath { get; }

        /// <summary>
        /// Gets the output path for NuGet packages
        /// </summary>
        public DirectoryPath PackageOutputPath { get; }

        /// <summary>
        /// Determines whether to use deterministic build settigns
        /// </summary>
        public bool DeterministicBuild { get; }

        public string CINuGetFeedUrl => "https://pkgs.dev.azure.com/ap0llo/OSS/_packaging/Cake.GitHubReleases/nuget/v3/index.json";

        public string NuGetOrgFeedUrl => "https://api.nuget.org/v3/index.json";

        public string GitBranchName { get; }

        public bool IsMasterBranch => GitBranchName.Equals("master", StringComparison.OrdinalIgnoreCase);

        public bool IsReleaseBranch => GitBranchName.StartsWith("release/", StringComparison.OrdinalIgnoreCase);

        public FilePath ChangeLogOutputPath { get; }

        public GitHubProjectInformation GitHubProject { get; } = new();

        public string SourceVersion { get; }



        public BuildContext(ICakeContext context) : base(context)
        {
            BuildConfiguration = context.Argument("configuration", "Release");
            DeterministicBuild = context.Argument("deterministic", IsRunningInCI);

            RootDirectory = context.Environment.WorkingDirectory;

            var binariesDirectory = context.EnvironmentVariable("BUILD_BINARIESDIRECTORY");
            BinariesDirectory = String.IsNullOrEmpty(binariesDirectory) ? RootDirectory.Combine("Binaries") : binariesDirectory;

            PackageOutputPath = BinariesDirectory.Combine(BuildConfiguration).Combine("packages");
            TestResultsPath = BinariesDirectory.Combine(BuildConfiguration).Combine("TestResults");
            ChangeLogOutputPath = BinariesDirectory.CombineWithFilePath("changelog.md");

            GitBranchName = GetGitBranchName();
            SourceVersion = GetSourceVersion();
        }



        public DotNetCoreMSBuildSettings GetDefaultMSBuildSettings() => new()
        {
            TreatAllWarningsAs = MSBuildTreatAllWarningsAs.Error
        };



        public string? TryGetGitHubAccessToken()
        {
            if (this.EnvironmentVariable("GITHUB_ACCESSTOKEN") is string { Length: > 0 } accessToken)
            {
                return accessToken;
            }
            else
            {
                return null;
            }
        }


        private string GetGitBranchName()
        {
            string branchName;
            if (this.IsRunningOnAzurePipelines())
            {
                branchName = this.AzurePipelines().Environment.Repository.SourceBranch;
            }

            else
            {
                var gitContext = Nerdbank.GitVersioning.GitContext.Create(RootDirectory.FullPath);
                branchName = gitContext.HeadCanonicalName!;
            }

            if (branchName.StartsWith("refs/heads/"))
            {
                branchName = branchName.Substring("refs/heads/".Length);
            }

            return branchName;
        }

        private string GetSourceVersion()
        {
            string sourceVersion;
            if (this.IsRunningOnAzurePipelines())
            {
                sourceVersion = this.AzurePipelines().Environment.Repository.SourceVersion;
            }

            else
            {
                var gitContext = Nerdbank.GitVersioning.GitContext.Create(RootDirectory.FullPath);
                sourceVersion = gitContext.GitCommitId!;
            }

            return sourceVersion;
        }
    }
}
