using System;

namespace Cake.GitHubReleases
{
    public class AmbiguousTagNameException : Exception
    {
        public AmbiguousTagNameException(string message) : base(message)
        { }
    }
}
