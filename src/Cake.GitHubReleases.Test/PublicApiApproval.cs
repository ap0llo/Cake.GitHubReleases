using System.Threading.Tasks;
using PublicApiGenerator;
using VerifyXunit;
using Xunit;

namespace Cake.GitHubReleases.Test
{
    [Trait("Category", "SkipWhenLiveUnitTesting")]
    [UsesVerify]
    public class PublicApiApproval
    {
        [Fact]
        public Task Assembly_does_not_have_unapproved_API_changes()
        {
            // ARRANGE
            var assembly = typeof(GitHubReleaseAliases).Assembly;

            // ACT
            var publicApi = ApiGenerator.GeneratePublicApi(assembly, options: null);

            // ASSERT
            return Verifier.Verify(publicApi);
        }
    }

}
