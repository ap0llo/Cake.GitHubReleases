using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Cake.Core.IO;
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
        private readonly Mock<IFileSystem> m_FileSystemMock;


        public GitHubReleaseCreatorTest(ITestOutputHelper testOutputHelper)
        {
            m_TestLog = new(testOutputHelper);
            m_ClientMock = new GitHubClientMock();
            m_ClientFactoryMock = new Mock<IGitHubClientFactory>(MockBehavior.Strict);
            m_FileSystemMock = new Mock<IFileSystem>(MockBehavior.Strict);

            m_ClientFactoryMock
                .Setup(x => x.CreateClient(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(m_ClientMock.Object);
        }


        public static IEnumerable<object[]> CreateReleaseTestCases()
        {
            object[] TestCase(string id, GitHubReleaseCreateSettings settings, NewRelease expectedRelease, IReadOnlyList<ReleaseAssetUpload> expectedAssetUploads)
            {
                return new object[] { id, settings, expectedRelease, expectedAssetUploads };
            }

            yield return TestCase(
                "T01",
                new GitHubReleaseCreateSettings("owner", "repo", "tagName"),
                new NewRelease("tagName"),
                Array.Empty<ReleaseAssetUpload>()
            );

            yield return TestCase(
                "T02",
                new GitHubReleaseCreateSettings("owner", "repo", "tagName") { Name = "ReleaseName" },
                new NewRelease("tagName") { Name = "ReleaseName" },
                Array.Empty<ReleaseAssetUpload>()
            );

            yield return TestCase(
                "T03",
                new GitHubReleaseCreateSettings("owner", "repo", "tagName") { TargetCommitish = "abc123" },
                new NewRelease("tagName") { TargetCommitish = "abc123" },
                Array.Empty<ReleaseAssetUpload>()
            );

            yield return TestCase(
                "T04",
                new GitHubReleaseCreateSettings("owner", "repo", "tagName") { Body = "Release Body" },
                new NewRelease("tagName") { Body = "Release Body" },
                Array.Empty<ReleaseAssetUpload>()
            );

            yield return TestCase(
                "T05",
                new GitHubReleaseCreateSettings("owner", "repo", "tagName") { Draft = false },
                new NewRelease("tagName") { Draft = false },
                Array.Empty<ReleaseAssetUpload>()
            );
            yield return TestCase(
                "T06",
                new GitHubReleaseCreateSettings("owner", "repo", "tagName") { Draft = true },
                new NewRelease("tagName") { Draft = true },
                Array.Empty<ReleaseAssetUpload>()
            );

            yield return TestCase(
                "T07",
                new GitHubReleaseCreateSettings("owner", "repo", "tagName") { Prerelease = false },
                new NewRelease("tagName") { Prerelease = false },
                Array.Empty<ReleaseAssetUpload>()
            );
            yield return TestCase(
                "T08",
                new GitHubReleaseCreateSettings("owner", "repo", "tagName") { Prerelease = true },
                new NewRelease("tagName") { Prerelease = true },
                Array.Empty<ReleaseAssetUpload>()
            );

            // Assets can be null or empty => no upload
            yield return TestCase(
                "T09",
                new GitHubReleaseCreateSettings("owner", "repo", "tagName") { Assets = null! },
                new NewRelease("tagName"),
                Array.Empty<ReleaseAssetUpload>()
            );
            yield return TestCase(
                 "T10",
                 new GitHubReleaseCreateSettings("owner", "repo", "tagName") { Assets = Array.Empty<FilePath>() },
                 new NewRelease("tagName"),
                 Array.Empty<ReleaseAssetUpload>()
             );

            yield return TestCase(
                "T11",
                new GitHubReleaseCreateSettings("owner", "repo", "tagName")
                {
                    Assets = new List<FilePath>()
                    {
                        new FilePath("file1.ext"),
                        new FilePath("dir/file2.ext")
                    }
                },
                new NewRelease("tagName"),
                new[]
                {
                    new ReleaseAssetUpload() { FileName = "file1.ext", ContentType = "application/octet-stream"},
                    new ReleaseAssetUpload() { FileName = "file2.ext", ContentType = "application/octet-stream"},
                }
            );

        }

        
        [Theory]
        [MemberData(nameof(CreateReleaseTestCases))]
        public async Task CreateRelease_creates_the_expected_release(string id, GitHubReleaseCreateSettings settings, NewRelease expectedRelease, IReadOnlyList<ReleaseAssetUpload> expectedAssetUploads)
        {
            // ARRANGE
            _ = id;
            var sut = new GitHubReleaseCreator(m_TestLog, m_FileSystemMock.Object, m_ClientFactoryMock.Object);

            var createdRelease = new TestRelease() { Id = 123 };

            m_ClientMock.Repository.Release
                .Setup(x => x.Create(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<NewRelease>()))
                .ReturnsAsync(createdRelease);

            m_ClientMock.Repository.Release
                .Setup(x => x.UploadAsset(It.IsAny<Release>(), It.IsAny<ReleaseAssetUpload>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Release release, ReleaseAssetUpload assetUpload, CancellationToken _) => new TestReleaseAsset() { Name = assetUpload.FileName });

            foreach (var filePath in settings.AssetsOrEmpty)
            {
                m_FileSystemMock
                    .Setup(x => x.GetFile(filePath))
                    .Returns((FilePath path) => new FakeFile(path) { Exists = true });
            }

            // ACT 
            var release = await sut.CreateRelease(settings);

            // ASSERT
            Assert.NotNull(release);
            Assert.Equal(123, release.Id);

            m_ClientMock.Repository.Release.Verify(x => x.Create(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<NewRelease>()), Times.Once);
            m_ClientMock.Repository.Release.Verify(x => x.Create(settings.RepositoryOwner, settings.RepositoryName, It.Is<NewRelease>(actual => MatchRelease(expectedRelease, actual))), Times.Once);

            m_ClientMock.Repository.Release.Verify(x => x.UploadAsset(It.IsAny<Release>(), It.IsAny<ReleaseAssetUpload>(), It.IsAny<CancellationToken>()), Times.Exactly(expectedAssetUploads.Count));
            foreach (var expectedAssetUpload in expectedAssetUploads)
            {
                m_ClientMock.Repository.Release.Verify(x => x.UploadAsset(createdRelease, It.Is<ReleaseAssetUpload>(actual => MatchReleaseAssetUpload(expectedAssetUpload, actual)), It.IsAny<CancellationToken>()), Times.Once);
            }

            m_FileSystemMock.Verify(x => x.GetFile(It.IsAny<FilePath>()), Times.Exactly(expectedAssetUploads.Count * 2));
        }

        [Fact]
        public async Task CreateRelease_fails_if_asset_to_upload_does_not_exis()
        {
            // ARRANGE
            var assetPath = new FilePath("does-not-exist");

            var sut = new GitHubReleaseCreator(m_TestLog, m_FileSystemMock.Object, m_ClientFactoryMock.Object);

            m_FileSystemMock
                    .Setup(x => x.GetFile(assetPath))
                    .Returns((FilePath path) => new FakeFile(path) { Exists = false });

            var settings = new GitHubReleaseCreateSettings("owner", "repo", "tagName")
            {
                Assets = new[] { assetPath }
            };

            // ACT 
            var ex = await Record.ExceptionAsync(async () => await sut.CreateRelease(settings));

            // ASSERT
            Assert.IsType<FileNotFoundException>(ex);
        }

        [Fact]
        public async Task CreateRelease_fails_if_multiple_assets_with_the_same_name_are_added()
        {
            // ARRANGE
            var asset1 = new FilePath("asset1.zip");
            var asset2 = new FilePath("dir/asset1.zip");

            var sut = new GitHubReleaseCreator(m_TestLog, m_FileSystemMock.Object, m_ClientFactoryMock.Object);

            m_FileSystemMock
                    .Setup(x => x.GetFile(asset1))
                    .Returns((FilePath path) => new FakeFile(path) { Exists = true });

            m_FileSystemMock
                    .Setup(x => x.GetFile(asset2))
                    .Returns((FilePath path) => new FakeFile(path) { Exists = true });


            var settings = new GitHubReleaseCreateSettings("owner", "repo", "tagName")
            {
                Assets = new[] { asset1, asset2 }
            };

            // ACT 
            var ex = await Record.ExceptionAsync(async () => await sut.CreateRelease(settings));

            // ASSERT
            Assert.IsType<AssetConflictException>(ex);
            Assert.Equal("Cannot create GitHub release with multiple assets named 'asset1.zip'", ex.Message);
        }

        [Fact]
        public async Task CreateRelease_uses_the_specified_access_token()
        {
            // ARRANGE
            var sut = new GitHubReleaseCreator(m_TestLog, m_FileSystemMock.Object, m_ClientFactoryMock.Object);

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
            var sut = new GitHubReleaseCreator(m_TestLog, m_FileSystemMock.Object, m_ClientFactoryMock.Object);

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
            var match = true;

            match &= StringComparer.Ordinal.Equals(expected.TagName, actual.TagName);
            match &= StringComparer.Ordinal.Equals(expected.TargetCommitish, actual.TargetCommitish);
            match &= StringComparer.Ordinal.Equals(expected.Name, actual.Name);
            match &= StringComparer.Ordinal.Equals(expected.Body, actual.Body);
            match &= (expected.Draft == actual.Draft);
            match &= (expected.Prerelease == actual.Prerelease);

            return match;
        }

        private bool MatchReleaseAssetUpload(ReleaseAssetUpload expected, ReleaseAssetUpload actual)
        {
            var match = true;

            match &= StringComparer.Ordinal.Equals(expected.FileName, actual.FileName);
            match &= StringComparer.Ordinal.Equals(expected.ContentType, actual.ContentType);
            match &= StringComparer.Ordinal.Equals(expected.Timeout, actual.Timeout);
            match &= (actual.RawData is not null);

            return match;
        }
    }
}
