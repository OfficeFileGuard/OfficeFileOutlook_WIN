using System;
using System.IO;
using OfficeFileGuard.Whitelist;
using Xunit;

namespace OfficeFileGuard.Whitelist.Tests
{
    public sealed class WhitelistStoreTests : IDisposable   // IDisposable classe xunit per ripulire tutto (Dispose) dopo ogni test
    {
        private readonly string _dir;
        private readonly string _file;
        private readonly WhitelistStore _store;

        // xUnit crea una nuova istanza della classe per ogni test:
        // xUnit lancia automaticamente il costruttore prima di ogni test, e Dispose dopo ogni test
        public WhitelistStoreTests()
        {
            // costruisco directory nella cartella temporanea, con un guid nel nome per evitare conflitti tra test paralleli 
            _dir = Path.Combine(Path.GetTempPath(), "OfficeFileGuardTests", Guid.NewGuid().ToString("N"));
            _file = Path.Combine(_dir, "whitelist.txt");
            _store = new WhitelistStore(_file);
        }

        public void Dispose()
        {
            if (!Directory.Exists(_dir)) return;

            // setto su normal gli attributi di tutti i file presenti nella cartella per poterli cancellare (WhitelisteStore li setta su readonly
            foreach (string f in Directory.GetFiles(_dir))
                File.SetAttributes(f, FileAttributes.Normal);

            Directory.Delete(_dir, recursive: true);    // directory delete recursive elimina dir e suo contenuto
        }


        private bool IsReadOnly(string path)
        {
            return (File.GetAttributes(path) & FileAttributes.ReadOnly) == FileAttributes.ReadOnly;
        }

        // ---------- costruzione ----------

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Constructor_EmptyPath_Throws(string path)
        {
            Assert.Throws<ArgumentException>(() => new WhitelistStore(path));
        }


        [Fact]
        public void DefaultFilePath_PointsToOfficeFileGuardFolder()
        {
            Assert.EndsWith(Path.Combine("OfficeFileGuard", "whitelist.txt"), WhitelistStore.DefaultFilePath);
        }


        // ---------- lettura ----------

        [Fact]
        public void LoadText_FileMissing_ReturnsEmptyString()
        {
            Assert.Equal(string.Empty, _store.LoadText());
        }


        // ---------- scrittura ----------

        [Fact]
        public void SaveText_ThenLoadText_ReturnsSameText()
        {
            string text = "bücher.de\r\n# commento\r\nesempio.it";

            _store.SaveText(text);

            Assert.Equal(text, _store.LoadText());   // legge anche un file in sola lettura
        }


        [Fact]
        public void SaveText_MissingDirectory_CreatesIt()
        {
            _store.SaveText("esempio.it");

            Assert.True(Directory.Exists(_dir));
            Assert.True(File.Exists(_file));
        }

        [Fact]
        public void SaveText_SetsReadOnlyAttribute()
        {
            _store.SaveText("esempio.it");

            Assert.True(IsReadOnly(_file));
        }

        [Fact]
        public void SaveText_SecondSave_OverwritesReadOnlyFileAndKeepsProtection()
        {
            _store.SaveText("a.it");
            _store.SaveText("b.it");

            Assert.Equal("b.it", _store.LoadText());
            Assert.True(IsReadOnly(_file));
        }

        [Fact]
        public void SaveText_LeavesNoTemporaryFile()
        {
            _store.SaveText("a.it");
            _store.SaveText("b.it");

            string only = Assert.Single(Directory.GetFiles(_dir));
            Assert.Equal(_file, only);
        }

        [Fact]
        public void SaveText_Null_WritesEmptyFile()
        {
            _store.SaveText(null);

            Assert.True(File.Exists(_file));
            Assert.Equal(string.Empty, _store.LoadText());
        }

        [Fact]
        public void SaveText_WritesUtf8WithoutBom()
        {
            _store.SaveText("esempio.it");

            byte[] bytes = File.ReadAllBytes(_file);
            bool hasBom = bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF;
            Assert.False(hasBom);
        }

        [Fact]
        public void SaveText_WhenTemporaryFileCannotBeWritten_KeepsOriginalAndProtection()
        {
            _store.SaveText("original.it");
            Directory.CreateDirectory(_file + ".tmp");   // blocca la scrittura del file temporaneo

            Assert.ThrowsAny<Exception>(() => _store.SaveText("new.it"));

            Assert.Equal("original.it", _store.LoadText());
            Assert.True(IsReadOnly(_file));
        }
    }
}