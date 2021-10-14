# Cake.GitHubReleases

[![NuGet](https://img.shields.io/nuget/v/Cake.GitHubReleases.svg?logo=nuget)](https://www.nuget.org/packages/Cake.GitHubReleases)
[![Azure Artifacts](https://img.shields.io/badge/Azure%20Artifacts-prerelease-yellow.svg?logo=azuredevops)](https://dev.azure.com/ap0llo/OSS/_packaging?_a=feed&feed=PublicCI)

[![Build Status](https://dev.azure.com/ap0llo/OSS/_apis/build/status/Cake.GitHubReleases?branchName=master)](https://dev.azure.com/ap0llo/OSS/_build/latest?definitionId=22&branchName=master)
[![Conventional Commits](https://img.shields.io/badge/Conventional%20Commits-1.0.0-green.svg)](https://conventionalcommits.org)

A [Cake](https://cakebuild.net/) Addin to create [GitHub Releases](https://docs.github.com/en/repositories/releasing-projects-on-github/about-releases) from a Cake Build.

## Usage

The addin currently provides a single Cake script alias `GitHubReleaseCreateAsync()`.

To use the module in a Cake script file, perform the following steps

1. Add the [Azure Artifacts feed](https://www.nuget.org/packages/Cake.GitHubReleases) to your `nuget.config` (the module is not yet available on NuGet.org):

    ```xml
    <?xml version="1.0" encoding="utf-8"?>
    <configuration>
        <packageSources>
        ...
        <add key="Cake.GitHubReleases" value="https://pkgs.dev.azure.com/ap0llo/OSS/_packaging/Cake.GitHubReleases/nuget/v3/index.json" />
    </packageSources>
    </configuration>
    ```

1. Add the preprocessor directive to install the module

    ```cs
    #addin nuget:?package=Cake.GitHubReleases&version=VERSION
    ```

1. To create a GitHub Release, call `GitHubReleaseCreateAsync()` from a task:

    ```cs
    Task("CreateGitHubRelease")
    .Does(async () =>
    {
        var settings = new GitHubReleaseCreateSettings(
            repositoryOwner: "owner", 
            repositoryName: "repo", 
            tagName: "v1.2.3")
        {
            // Set the name of the release (defaults to the tag name when not specified)
            Name = $"v1.2.3",
            
            // The release's description as Markdown string (optional)
            Body = "Description",
            
            // Set to true to create a draft release (default: false)
            Draft = false,
            
            // Set to true to mark the release as prerelease (default: false)
            Prerelease = false,

            // The id of the commit to create the release from 
            // (uses the HEAD commit of the repo's default branch if not specified)
            TargetCommitish = "abc123",
            
            // Set the GitHub access token to use for authentication
            AccessToken = accessToken,

            // Overwrite will delete any existing commit with the same tag name if it exists
            Overwrite = false
        };

        GitHubReleaseCreateAsync(settings);
    });
    ```

## Building from source

Building the project from source requires the .NET 5 SDK (version 5.0.400 as specified in [global.json](./global.json)).

To build the project, run

```ps1
.\build.ps1
```

This will 

- Download the required version of the .NET SDK
- Build the project
- Run all tests 
- Pack the NuGet package.


### Versioning and Branching

The version of the library is automatically derived from git and the information
in `version.json` using [Nerdbank.GitVersioning](https://github.com/AArnott/Nerdbank.GitVersioning):

- The master branch  always contains the latest version. Packages produced from
  master are always marked as pre-release versions (using the `-pre` suffix).
- Stable versions are built from release branches. Build from release branches
  will have no `-pre` suffix
- Builds from any other branch will have both the `-pre` prerelease tag and the git
  commit hash included in the version string

To create a new release branch use the [`nbgv` tool](https://www.nuget.org/packages/nbgv/):

```ps1
dotnet tool install --global nbgv
nbgv prepare-release
```
