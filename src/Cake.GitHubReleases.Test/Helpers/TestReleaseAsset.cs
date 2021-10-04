using Octokit;

namespace Cake.GitHubReleases.Test.Helpers
{
    public class TestReleaseAsset : ReleaseAsset
    {
        public new string Name
        {
            get => base.Name;
            set => base.Name = value;
        }
    }
}
