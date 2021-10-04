using System;

namespace Cake.GitHubReleases
{
    //TODO: Add common base type for all exception thrown by GitHubRelease aliases ??
    public sealed class AssetConflictException : Exception
    {
        public AssetConflictException(string message) : base(message)
        { }
    }
}
