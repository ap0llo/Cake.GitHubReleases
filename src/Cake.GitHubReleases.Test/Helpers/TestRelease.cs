using Octokit;

namespace Cake.GitHubReleases.Test.Helpers
{
    internal class TestRelease : Release
    {
        public new int Id
        {
            get => base.Id;
            set => base.Id = value;              
        }

    }
}
