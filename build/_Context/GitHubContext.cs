using System;
using Cake.Common;
using Cake.Core.Diagnostics;

namespace Build
{
    public class GitHubContext
    {
        private readonly BuildContext m_Context;


        //TODO: Determine from git url or similar
        public string RepositoryOwner => "ap0llo";

        public string RepositoryName => "Cake.GitHubReleases";


        public GitHubContext(BuildContext context)
        {
            m_Context = context ?? throw new ArgumentNullException(nameof(context));
        }


        public void PrintToLog(int indentWidth = 0)
        {
            string prefix = new String(' ', indentWidth);

            m_Context.Log.Information($"{prefix}{nameof(RepositoryOwner)}: {RepositoryOwner}");
            m_Context.Log.Information($"{prefix}{nameof(RepositoryName)}: {RepositoryName}");
        }

        public string? TryGetAccessToken()
        {
            if (m_Context.EnvironmentVariable("GITHUB_ACCESSTOKEN") is string { Length: > 0 } accessToken)
            {
                return accessToken;
            }
            else
            {
                return null;
            }
        }
    }
}
