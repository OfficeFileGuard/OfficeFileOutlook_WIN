using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace OfficeFileGuard.Whitelist
{
    public sealed class WhitelistError
    {
        public int Line { get; }    // n.ro linea errata (get lo valorizza nel costruttore)
        public string Text { get; } // testo della linea errata
        public string Reason { get; }   // motivo dell'errore
        public WhitelistError(int line, string text, string reason)
        { Line = line; Text = text; Reason = reason; }
    }


    public sealed class WhitelistParseResult
    {
        public IReadOnlyList<string> Domains { get; }   // domini validi (normalizzati)
        public IReadOnlyList<WhitelistError> Errors { get; }    // errori
        public bool IsValid => Errors.Count == 0;
        public WhitelistParseResult(IReadOnlyList<string> domains, IReadOnlyList<WhitelistError> errors)
        { Domains = domains; Errors = errors; }
    }


    public static class WhitelistParser
    {

        // regex per validare le etichette (prima parola) dei domini (RFC 1035)
        private static readonly Regex Label =
            new Regex("^[a-z0-9]([a-z0-9-]{0,61}[a-z0-9])?$", RegexOptions.Compiled);
        // regex per validare i suffissi (TLD) dei domini (RFC 1035)
        private static readonly Regex Tld =
            new Regex("^([a-z]{2,63}|xn--[a-z0-9-]{1,59})$", RegexOptions.Compiled);

        public static WhitelistParseResult Parse(string text)
        {
            var domains = new List<string>();
            var errors = new List<WhitelistError>();
            var seen = new HashSet<string>(StringComparer.Ordinal); //faccio hashing delle righe per riconoscere i duplicati
            // splitto il testo in righe, supportando tutti i tipi di terminatori di riga (se text è null allora uso stringa vuota)
            var lines = (text ?? string.Empty).Split(new[] { "\r\n", "\n", "\r" }, StringSplitOptions.None);

            for (int i = 0; i < lines.Length; i++)
            {
                string raw = lines[i];
                int hash = raw.IndexOf('#');    // indice della posizione di un eventuale commento
                string entry = (hash >= 0 ? raw.Substring(0, hash) : raw).Trim();   // linea senza commento
                if (entry.Length == 0) continue;       // vuota o solo commento

                string domain, reason;
                if (!TryNormalize(entry, out domain, out reason))
                    errors.Add(new WhitelistError(i + 1, raw.Trim(), reason));  // n.ro linea, testo, reason
                else if (seen.Add(domain))          // duplicati eliminati (seen.add = true se riesce cioè se non c'è ancora)
                    domains.Add(domain);
            }
            return new WhitelistParseResult(domains, errors);
        }

        public static bool TryNormalize(string entry, out string domain, out string reason)
        {
            domain = null; reason = null;
            string s = (entry ?? string.Empty).Trim().ToLowerInvariant();
            if (s.StartsWith("@")) s = s.Substring(1);
            if (s.EndsWith(".")) s = s.Substring(0, s.Length - 1);

            if (s.Length == 0) { reason = "empty line"; return false; }
            if (s.IndexOfAny(new[] { '/', ':', '@', '*', '?', '\\' }) >= 0 || s.Any(char.IsWhiteSpace))
            { reason = "invalid characters in the domain"; return false; }

            string ascii;
            try { ascii = new IdnMapping { UseStd3AsciiRules = true }.GetAscii(s).ToLowerInvariant(); }
            catch (ArgumentException) { reason = "invalid domain"; return false; }

            if (ascii.Length > 253) { reason = "too long domain"; return false; }
            var labels = ascii.Split('.');
            if (labels.Length < 2) { reason = "complete domain required"; return false; }
            if (labels.Any(l => !Label.IsMatch(l))) { reason = "invalid label (characters or hyphens)"; return false; }
            if (!Tld.IsMatch(labels[labels.Length - 1])) { reason = "invalid suffix (TLD)"; return false; }

            domain = ascii;
            return true;
        }
    }
}