using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cake.Core.Diagnostics;
using Xunit.Abstractions;

namespace Cake.GitHubReleases.Test.Helpers
{
    /// <summary>
    /// Implementation of <see cref="ICakeLog"/> that logs to xunit's <see cref="ITestOutputHelper"/>
    /// </summary>
    internal class XunitCakeLog : ICakeLog
    {
        private readonly ITestOutputHelper m_TestOutputHelper;


        /// <inheritdoc />
        public Verbosity Verbosity { get; set; }


        public XunitCakeLog(ITestOutputHelper testOutputHelper)
        {
            m_TestOutputHelper = testOutputHelper ?? throw new ArgumentNullException(nameof(testOutputHelper));
        }

        /// <inheritdoc />
        public void Write(Verbosity verbosity, LogLevel level, string format, params object[] args)
        {
            var message = $"{level} - {String.Format(format, args)}";
            m_TestOutputHelper.WriteLine(message);
        }
    }
}
