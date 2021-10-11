using System;
using Cake.Common.Build;

namespace Build
{
    public class AzurePipelinesContext
    {
        public class ArtifactNameSettings
        {
            /// <summary>
            /// The name of the main artifact
            /// </summary>
            public string Binaries => "Binaries";

            /// <summary>
            /// The artifact name under which to save test result files
            /// </summary>
            public string TestResults => "TestResults";

            /// <summary>
            /// The artifact name for the auto-generated change log.
            /// </summary>
            public string ChangeLog => "ChangeLog";
        }

        private readonly BuildContext m_Context;


        public ArtifactNameSettings ArtifactNames { get; } = new();

        public bool IsActive =>
            m_Context.AzurePipelines().IsRunningOnAzurePipelines ||
            m_Context.AzurePipelines().IsRunningOnAzurePipelinesHosted;


        public AzurePipelinesContext(BuildContext context)
        {
            m_Context = context ?? throw new ArgumentNullException(nameof(context));
        }
    }
}
