using System;
using Cake.Common;
using Cake.Core.IO;

namespace Build
{
    public class OutputContext
    {
        private readonly BuildContext m_Context;


        /// <summary>
        /// Gets the root output directory
        /// </summary>
        public DirectoryPath BinariesDirectory
        {
            get
            {
                var binariesDirectory = m_Context.EnvironmentVariable("BUILD_BINARIESDIRECTORY");
                return String.IsNullOrEmpty(binariesDirectory) ? m_Context.RootDirectory.Combine("Binaries") : binariesDirectory;
            }
        }

        /// <summary>
        /// Gets the output path for NuGet packages
        /// </summary>
        public DirectoryPath PackagesDirectory => BinariesDirectory.Combine(m_Context.BuildSettings.Configuration).Combine("packages");

        public DirectoryPath TestResultsDirectory => BinariesDirectory.Combine(m_Context.BuildSettings.Configuration).Combine("TestResults");

        public FilePath ChangeLogFile => BinariesDirectory.CombineWithFilePath("changelog.md");


        public OutputContext(BuildContext context)
        {
            m_Context = context ?? throw new ArgumentNullException(nameof(context));
        }
    }
}
