# Cake.GitHubReleases

A [Cake](https://cakebuild.net/) Addin to create [GitHub Releases](https://docs.github.com/en/repositories/releasing-projects-on-github/about-releases) from a Cake Build.


## Table of Contents

- [Usage](#usage)
- [License](#license)


## Usage

You can read the latest documentation at https://github.com/ap0llo/Cake.GitHubReleases


The addin currently provides a single Cake script alias `GitHubReleaseCreateAsync()`.

To use the addin in a Cake script file, perform the following steps:

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

## License

Cake.GitHubReleases is licensed under the MIT License.

For details see https://github.com/ap0llo/Cake.GitHubReleases/blob/master/LICENSE