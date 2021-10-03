namespace Cake.GitHubReleases
{
    public sealed class GitHubRelease
    {
        //TODO: Add additional properties

        /// <summary>
        /// Gets the release's id
        /// </summary>
        public int Id { get; }


        /// <summary>
        /// Initializes a new instance of <see cref="GitHubRelease"/>
        /// </summary>
        /// <param name="id"></param>
        public GitHubRelease(int id)
        {
            Id = id;
        }
    }
}
