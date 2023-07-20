# Cake.GitHubReleases

---

**This Repository is no longer maintained**

**The functionality of this Cake Addin has been integrated into [Cake.GitHub](https://github.com/cake-contrib/Cake.GitHub)**

---

A [Cake](https://cakebuild.net/) Addin to create [GitHub Releases](https://docs.github.com/en/repositories/releasing-projects-on-github/about-releases) from a Cake Build.

## Usage

The addin currently provides a single Cake script alias `GitHubReleaseCreateAsync()`.

To use the addin in a Cake script file, perform the following steps

1. Add the preprocessor directive to install the addin

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
