using System.Collections.Generic;
using System.Threading.Tasks;
using Cake.GitHubReleases.Internal;
using Cake.GitHubReleases.Test.Helpers;
using Moq;
using Octokit;
using Xunit;
using Xunit.Abstractions;

namespace Cake.GitHubReleases.Test.Internal
{
    /// <summary>
    /// Tests for <see cref="GitHubReleaseCreator"/>
    /// </summary>
    public class GitHubReleaseCreatorTest
    {
        private readonly XunitCakeLog m_TestLog;
        private readonly Mock<IGitHubClientFactory> m_ClientFactoryMock;
        private readonly GitHubClientMock m_ClientMock;

        public GitHubReleaseCreatorTest(ITestOutputHelper testOutputHelper)
        {
            m_TestLog = new(testOutputHelper);
            m_ClientMock = new GitHubClientMock();
            m_ClientFactoryMock = new Mock<IGitHubClientFactory>(MockBehavior.Strict);

            m_ClientFactoryMock
                .Setup(x => x.CreateClient(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(m_ClientMock.Object);
        }

        public static IEnumerable<object[]> CreateReleaseTestCases()
        {
            object[] TestCase(string id, GitHubReleaseCreateSettings settings, NewRelease expected)
            {
                return new object[] { id, settings, expected };
            }

            yield return TestCase(
                "T01",
                new GitHubReleaseCreateSettings("owner", "repo", "tagName"),
                new NewRelease("tagName")
            );

            yield return TestCase(
                "T02",
                new GitHubReleaseCreateSettings("owner", "repo", "tagName") { Name = "ReleaseName" },
                new NewRelease("tagName") { Name = "ReleaseName" }
            );

            yield return TestCase(
                "T03",
                new GitHubReleaseCreateSettings("owner", "repo", "tagName") { TargetCommitish = "abc123" },
                new NewRelease("tagName") { TargetCommitish = "abc123" }
            );

            yield return TestCase(
                "T04",
                new GitHubReleaseCreateSettings("owner", "repo", "tagName") { Body = "Release Body" },
                new NewRelease("tagName") { Body = "Release Body" }
            );

            yield return TestCase(
                "T05",
                new GitHubReleaseCreateSettings("owner", "repo", "tagName") { Draft = false },
                new NewRelease("tagName") { Draft = false }
            );
            yield return TestCase(
                "T06",
                new GitHubReleaseCreateSettings("owner", "repo", "tagName") { Draft = true },
                new NewRelease("tagName") { Draft = true }
            );

            yield return TestCase(
                "T07",
                new GitHubReleaseCreateSettings("owner", "repo", "tagName") { Prerelease = false },
                new NewRelease("tagName") { Prerelease = false }
            );
            yield return TestCase(
                "T08",
                new GitHubReleaseCreateSettings("owner", "repo", "tagName") { Prerelease = true },
                new NewRelease("tagName") { Prerelease = true }
            );
        }

        [Theory]
        [MemberData(nameof(CreateReleaseTestCases))]
        public async Task CreateRelease_creates_the_expected_release(string id, GitHubReleaseCreateSettings settings, NewRelease expected)
        {
            // ARRANGE
            _ = id;
            var sut = new GitHubReleaseCreator(m_TestLog, m_ClientFactoryMock.Object);

            m_ClientMock.Repository.Release
                .Setup(x => x.Create(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<NewRelease>()))
                .ReturnsAsync(new TestRelease() { Id = 123 });

            // ACT 
            var release = await sut.CreateRelease(settings);

            // ASSERT
            Assert.NotNull(release);
            Assert.Equal(123, release.Id);
            m_ClientMock.Repository.Release.Verify(x => x.Create(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<NewRelease>()), Times.Once);
            m_ClientMock.Repository.Release.Verify(x => x.Create(settings.RepositoryOwner, settings.RepositoryName, It.Is<NewRelease>(actual => MatchRelease(expected, actual))), Times.Once);
        }

        [Fact]
        public async Task CreateRelease_uses_the_specified_access_token()
        {
            // ARRANGE
            var sut = new GitHubReleaseCreator(m_TestLog, m_ClientFactoryMock.Object);

            var settings = new GitHubReleaseCreateSettings("owner", "repo", "tag")
            {
                AccessToken = "some-access-token"
            };

            m_ClientMock.Repository.Release
                .Setup(x => x.Create(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<NewRelease>()))
                .ReturnsAsync(new TestRelease() { Id = 123 });

            // ACT 
            _ = await sut.CreateRelease(settings);

            // ASSERT
            m_ClientFactoryMock.Verify(x => x.CreateClient(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            m_ClientFactoryMock.Verify(x => x.CreateClient(It.IsAny<string>(), settings.AccessToken), Times.Once);
        }

        [Fact]
        public async Task CreateRelease_uses_the_specified_host_name()
        {
            // ARRANGE
            var sut = new GitHubReleaseCreator(m_TestLog, m_ClientFactoryMock.Object);

            var settings = new GitHubReleaseCreateSettings("owner", "repo", "tag")
            {
                HostName = "my-github-enterprise.com"
            };

            m_ClientMock.Repository.Release
                .Setup(x => x.Create(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<NewRelease>()))
                .ReturnsAsync(new TestRelease() { Id = 123 });

            // ACT 
            _ = await sut.CreateRelease(settings);

            // ASSERT
            m_ClientFactoryMock.Verify(x => x.CreateClient(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            m_ClientFactoryMock.Verify(x => x.CreateClient(settings.HostName, It.IsAny<string>()), Times.Once);
        }

        private bool MatchRelease(NewRelease expected, NewRelease actual)
        {
            Assert.Equal(expected.TagName, actual.TagName);
            Assert.Equal(expected.TargetCommitish, actual.TargetCommitish);
            Assert.Equal(expected.Name, actual.Name);
            Assert.Equal(expected.Body, actual.Body);
            Assert.Equal(expected.Draft, actual.Draft);
            Assert.Equal(expected.Prerelease, actual.Prerelease);
            return true;
        }

    }
}
