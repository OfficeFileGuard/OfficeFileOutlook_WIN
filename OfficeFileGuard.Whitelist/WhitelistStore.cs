using System;
using System.IO;
using System.Text;

namespace OfficeFileGuard.Whitelist
{
    public sealed class WhitelistStore
    {
        // path di default di whitelist.txt
        public static string DefaultFilePath { get; } = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "OfficeFileGuard", "whitelist.txt");

        // path di whitelist.txt (dello specifico oggetto)
        public string FilePath { get; }

        // costruttore senza parametri -> usa path default
        public WhitelistStore() : this(DefaultFilePath) { }

        // costruttore con path (ad es per i test)
        public WhitelistStore(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("Path is required.", nameof(filePath));
            FilePath = filePath;
        }


        // lettura whitelist.txt
        public string LoadText()
        {
            return File.Exists(FilePath)
                ? File.ReadAllText(FilePath, Encoding.UTF8)
                : string.Empty;
        }

        // salva text in whitelist.txt passando prima per un file temporaneo (evita problemi in caso malfunzionamenti durante la scrittura)
        public void SaveText(string text)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(FilePath)); // crea la directory se non esiste
            string tmp = FilePath + ".tmp";
            File.WriteAllText(tmp, text ?? string.Empty, new UTF8Encoding(false));  // salva text sul file tmp

            try
            {
                if (File.Exists(FilePath))
                {
                    File.SetAttributes(FilePath, FileAttributes.Normal);   // toglie la sola lettura
                    File.Replace(tmp, FilePath, null);
                }
                else
                {
                    File.Move(tmp, FilePath);   // se whitelist.txt non esiste, rinomina tmp in whitelist.txt
                }
            }
            finally
            {
                if (File.Exists(tmp)) File.Delete(tmp); // pulisce via tmp
                if (File.Exists(FilePath))
                    File.SetAttributes(FilePath, FileAttributes.ReadOnly); // rimette la protezione
            }
        }
    }
}