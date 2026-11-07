using OfficeFileGuard.Whitelist;
using Xunit;

namespace OfficeFileGuard.Whitelist.Tests
{
    public class WhitelistParseResultTests
    {
        private static WhitelistParseResult List(string text) => WhitelistParser.Parse(text);

        [Theory]
        [InlineData("esempio.it")]
        [InlineData("ESEMPIO.IT")]
        [InlineData("  esempio.it ")]
        public void IsInWhitelist_ListedDomain_ReturnsTrue(string domain) =>
            Assert.True(List("esempio.it").IsInWhitelist(domain));

        [Fact]
        public void IsInWhitelist_Subdomain_ReturnsFalse() =>
            Assert.False(List("esempio.it").IsInWhitelist("mail.esempio.it"));

        [Fact]
        public void IsInWhitelist_InternationalDomain_MatchesPunycodeEntry() =>
            Assert.True(List("xn--bcher-kva.de").IsInWhitelist("bücher.de"));

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("nome@esempio.it")]
        public void IsInWhitelist_InvalidInput_ReturnsFalse(string domain) =>
            Assert.False(List("esempio.it").IsInWhitelist(domain));

        [Fact]
        public void IsInWhitelist_EmptyList_ReturnsFalse() =>
            Assert.False(List(string.Empty).IsInWhitelist("esempio.it"));

        [Fact]
        public void IsInWhitelist_ListWithInvalidLines_StillMatchesValidDomains()
        {
            var list = WhitelistParser.Parse("esempio.it\r\nhttp://sbagliato\r\naltro.it");

            Assert.False(list.IsValid);
            Assert.True(list.IsInWhitelist("esempio.it"));
            Assert.True(list.IsInWhitelist("altro.it"));
            Assert.False(list.IsInWhitelist("sbagliato"));
        }
    }
}