using System;

namespace Cake.GitHubReleases
{
    public class ReleaseExistsException : Exception
    {
        public ReleaseExistsException(string message) : base(message)
        { }
    }
}
