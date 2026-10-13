using OfficeFileGuard.Whitelist;
using System.Linq;
using Xunit;

namespace OfficeFileGuard.Whitelist.Tests
{
    public class WhitelistParserTests
    {
        // ---------- voci valide: normalizzazione ----------

        [Theory]
        [InlineData("esempio.it", "esempio.it")]
        [InlineData("Esempio.IT", "esempio.it")]            // minuscolo
        [InlineData("  esempio.it  ", "esempio.it")]        // spazi ai lati
        [InlineData("@esempio.it", "esempio.it")]           // @ iniziale
        [InlineData("esempio.it.", "esempio.it")]           // punto finale
        [InlineData("esempio.it # cliente A", "esempio.it")] // commento a fine riga
        [InlineData("mail.esempio.it", "mail.esempio.it")]  // sottodominio: resta com'è
        [InlineData("a-b.esempio.it", "a-b.esempio.it")]    // trattino interno
        [InlineData("bücher.de", "xn--bcher-kva.de")]       // internazionale -> punycode
        public void Parse_ValidEntry_ReturnsNormalizedDomain(string input, string expected)
        {
            var result = WhitelistParser.Parse(input);

            Assert.True(result.IsValid);
            Assert.Equal(new[] { expected }, result.Domains);
        }

        // ---------- righe da ignorare ----------

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("# solo un commento")]
        [InlineData("\r\n\r\n")]
        [InlineData(null)]
        public void Parse_EmptyOrCommentOnly_ReturnsEmptyValidList(string input)
        {
            var result = WhitelistParser.Parse(input);

            Assert.True(result.IsValid);
            Assert.Empty(result.Domains);
        }

        // ---------- voci non valide ----------

        [Theory]
        [InlineData("http://esempio.it")]       // URL incollato
        [InlineData("esempio.it/pagina")]       // percorso
        [InlineData("esempio.it:8080")]         // porta
        [InlineData("nome@esempio.it")]         // indirizzo email
        [InlineData("*.esempio.it")]            // jolly non previsto
        [InlineData("esempio .it")]             // spazio interno
        [InlineData("com")]                     // solo il suffisso
        [InlineData("-a.esempio.it")]           // etichetta che inizia con trattino
        [InlineData("a..esempio.it")]           // etichetta vuota
        [InlineData("esempio_it.it")]           // trattino basso
        [InlineData("esempio.123")]             // suffisso numerico
        [InlineData("esempio.i")]               // suffisso di una lettera
        public void Parse_InvalidEntry_IsRejected(string input)
        {
            var result = WhitelistParser.Parse(input);

            Assert.False(result.IsValid);
            Assert.Empty(result.Domains);
            Assert.Single(result.Errors);
        }

        [Fact]
        public void Parse_LabelOf63Characters_IsAccepted()
        {
            string domain = new string('a', 63) + ".it";

            var result = WhitelistParser.Parse(domain);

            Assert.True(result.IsValid);
        }

        [Fact]
        public void Parse_LabelOf64Characters_IsRejected()
        {
            string domain = new string('a', 64) + ".it";

            var result = WhitelistParser.Parse(domain);

            Assert.False(result.IsValid);
        }

        [Fact]
        public void Parse_DomainLongerThan253Characters_IsRejected()
        {
            string domain = string.Join(".", Enumerable.Repeat(new string('a', 60), 5)) + ".it";

            var result = WhitelistParser.Parse(domain);

            Assert.False(result.IsValid);
        }

        // ---------- comportamento sull'elenco ----------

        [Fact]
        public void Parse_DuplicateEntries_AreKeptOnce()
        {
            var result = WhitelistParser.Parse("esempio.it\r\nESEMPIO.IT\r\n@esempio.it");

            Assert.True(result.IsValid);
            Assert.Equal(new[] { "esempio.it" }, result.Domains);
        }

        [Fact]
        public void Parse_MultipleEntries_PreservesOrder()
        {
            var result = WhitelistParser.Parse("b.it\r\na.it\r\nc.it");

            Assert.Equal(new[] { "b.it", "a.it", "c.it" }, result.Domains);
        }

        [Fact]
        public void Parse_MixedLineEndings_ReportsCorrectLineNumbers()
        {
            // righe: 1 = ok, 2 = errore, 3 = ok, 4 = errore (separate da \r\n, \n e \r)
            var result = WhitelistParser.Parse("ok.it\r\nbad\nok2.it\rhttp://x.it");

            Assert.Equal(new[] { 2, 4 }, result.Errors.Select(e => e.Line));
        }

        [Fact]
        public void Parse_InvalidEntry_ReportsOriginalTextAndReason()
        {
            var result = WhitelistParser.Parse("  http://x.it  ");

            var error = Assert.Single(result.Errors);
            Assert.Equal("http://x.it", error.Text);
            Assert.False(string.IsNullOrWhiteSpace(error.Reason));
        }

        [Fact]
        public void Parse_ValidAndInvalidLines_KeepsValidDomainsAndReportsErrors()
        {
            var result = WhitelistParser.Parse("ok.it\r\nhttp://x.it\r\nok2.it");

            Assert.False(result.IsValid);
            Assert.Equal(new[] { "ok.it", "ok2.it" }, result.Domains);
        }

        // ---------- TryNormalize (singola voce) ----------

        [Fact]
        public void TryNormalize_ValidEntry_ReturnsTrueAndDomain()
        {
            bool ok = WhitelistParser.TryNormalize("@Esempio.IT", out string domain, out string reason);

            Assert.True(ok);
            Assert.Equal("esempio.it", domain);
            Assert.Null(reason);
        }

        [Fact]
        public void TryNormalize_InvalidEntry_ReturnsFalseWithReason()
        {
            bool ok = WhitelistParser.TryNormalize("http://x.it", out string domain, out string reason);

            Assert.False(ok);
            Assert.Null(domain);
            Assert.False(string.IsNullOrWhiteSpace(reason));
        }
    }
}
