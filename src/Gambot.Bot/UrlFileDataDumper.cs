using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gambot.Core;
using Gambot.Data;

namespace Gambot.Bot
{
    public class UrlFileDataDumper : IDataDumper
    {
        private readonly string _dataDumpDirectory;
        private readonly IDataStoreProvider _dataStoreProvider;
        private readonly IConfig _config;

        public UrlFileDataDumper(string dataDumpDirectory, IConfig config, IDataStoreProvider dataStoreProvider)
        {
            _dataDumpDirectory = dataDumpDirectory;
            _config = config;
            _dataStoreProvider = dataStoreProvider;
        }

        public async Task<string> Dump(string dataStore, string key)
        {
            var store = await _dataStoreProvider.GetDataStore(dataStore);
            var data = await store.GetAll(key);

            var subdirName = GetSlug(dataStore);
            var directory = Path.Combine(_dataDumpDirectory, subdirName);

            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            var filename = GetSlug(key);
            if (String.IsNullOrWhiteSpace(filename))
                filename = "_";
            filename = $"{filename}.txt";

            var dumpfile = Path.Combine(directory, filename);

            await File.WriteAllLinesAsync(dumpfile, data.Select(dsv => dsv.ToFullString()), Encoding.UTF8);

            var url = await _config.Get("DataDumpUrl", "http://example.com");
            var relative = $"{subdirName}/{filename}";

            return new Uri(new Uri(url), relative).ToString();
        }

        public static string GetSlug(string input)
        {
            var sb = new StringBuilder();
            var normalized = input.ToLowerInvariant().Normalize(NormalizationForm.FormD);

            var spaced = false;
            foreach (var c in normalized)
            {
                if (c > 127) continue;
                switch (CharUnicodeInfo.GetUnicodeCategory(c))
                {
                    case UnicodeCategory.LowercaseLetter:
                    case UnicodeCategory.UppercaseLetter:
                    case UnicodeCategory.DecimalDigitNumber:
                        sb.Append(c);
                        spaced = false;
                        break;
                    case UnicodeCategory.SpaceSeparator:
                    case UnicodeCategory.ConnectorPunctuation:
                    case UnicodeCategory.DashPunctuation:
                        if (!spaced)
                        {
                            sb.Append('-');
                            spaced = true;
                        }
                        break;
                }
            }

            return sb.ToString();
        }
    }
}