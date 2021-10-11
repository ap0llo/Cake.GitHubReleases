using System;
using Cake.Common.Build;

namespace Build
{
    public class GitContext
    {
        private readonly BuildContext m_Context;


        public string BranchName
        {
            get
            {
                string branchName;
                if (m_Context.AzurePipelines.IsActive)
                {
                    branchName = m_Context.AzurePipelines().Environment.Repository.SourceBranch;
                }

                else
                {
                    //TODO: Make this independent of Nerdbank.GitVersioning
                    var gitContext = Nerdbank.GitVersioning.GitContext.Create(m_Context.RootDirectory.FullPath);
                    branchName = gitContext.HeadCanonicalName!;
                }

                if (branchName.StartsWith("refs/heads/"))
                {
                    branchName = branchName["refs/heads/".Length..];
                }

                return branchName;
            }
        }

        public string CommitId
        {
            get
            {
                string commitId;
                if (m_Context.AzurePipelines.IsActive)
                {
                    commitId = m_Context.AzurePipelines().Environment.Repository.SourceVersion;
                }

                else
                {
                    var gitContext = Nerdbank.GitVersioning.GitContext.Create(m_Context.RootDirectory.FullPath);
                    commitId = gitContext.GitCommitId!;
                }

                return commitId;
            }
        }

        public bool IsMasterBranch => BranchName.Equals("master", StringComparison.OrdinalIgnoreCase);

        public bool IsReleaseBranch => BranchName.StartsWith("release/", StringComparison.OrdinalIgnoreCase);


        public GitContext(BuildContext context)
        {
            m_Context = context ?? throw new ArgumentNullException(nameof(context));

        }
    }
}
