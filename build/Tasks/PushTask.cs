using System;
using System.Collections.Generic;
using Cake.Common;
using Cake.Common.Tools.DotNetCore;
using Cake.Common.Tools.DotNetCore.NuGet.Push;
using Cake.Common.Tools.DotNetCore.NuGet.Source;
using Cake.Core.Diagnostics;
using Cake.Core.IO;
using Cake.Frosting;

namespace Build.Tasks
{
    [TaskName("Push")]
    [IsDependentOn(typeof(PackTask))]
    public class PushTask : FrostingTask<BuildContext>
    {
        public override bool ShouldRun(BuildContext context)
        {
            return context.IsRunningInCI && (context.IsMasterBranch || context.IsReleaseBranch);
        }

        public override void Run(BuildContext context)
        {
            var packages = context.FileSystem.GetFilePaths(context.PackageOutputPath, "*.nupkg", SearchScope.Current);
            context.Log.Information($"Found {packages.Count} packages in the package output directory '{context.PackageOutputPath}'");

            //
            // NuGet push (CI Feed)
            //
            PushToAzureArtifacts(context, packages);

            // NuGet push (nuget.org)
            if (context.IsReleaseBranch)
            {
                PushToNuGetOrg(context, packages);
            }
        }


        private void PushToAzureArtifacts(BuildContext context, IEnumerable<FilePath> packages)
        {
            // See https://www.daveaglick.com/posts/pushing-packages-from-azure-pipelines-to-azure-artifacts-using-cake
            var accessToken = context.EnvironmentVariable("SYSTEM_ACCESSTOKEN");
            if (String.IsNullOrEmpty(accessToken))
            {
                throw new InvalidOperationException("Could not resolve SYSTEM_ACCESSTOKEN.");
            }

            context.DotNetCoreNuGetAddSource(
                "AzureArtifacts",
                new DotNetCoreNuGetSourceSettings()
                {
                    Source = context.CINuGetFeedUrl,
                    UserName = "AzureArtifacts",
                    Password = accessToken
                });

            context.Log.Information($"Pushing packages to Azure Artifacts feed '{context.CINuGetFeedUrl}'");
            foreach (var package in packages)
            {
                var pushSettings = new DotNetCoreNuGetPushSettings()
                {
                    Source = "AzureArtifacts",
                    ApiKey = "AzureArtifacts"
                };

                context.DotNetCoreNuGetPush(package.FullPath, pushSettings);
            }
        }

        private void PushToNuGetOrg(BuildContext context, IEnumerable<FilePath> packages)
        {
            var apiKey = context.EnvironmentVariable("NUGET_ORG_APIKEY");
            if (String.IsNullOrEmpty(apiKey))
            {
                throw new InvalidOperationException("Could not determine nuget.org API key. Enviornment variable 'NUGET_ORG_APIKEY' is empty.");
            }

            context.Log.Information($"Pushing packages to nuget.org (feed {context.NuGetOrgFeedUrl})");
            foreach (var package in packages)
            {
                var pushSettings = new DotNetCoreNuGetPushSettings()
                {
                    Source = context.NuGetOrgFeedUrl,
                    ApiKey = apiKey
                };

                context.DotNetCoreNuGetPush(package.FullPath, pushSettings);
            }
        }
    }
}
