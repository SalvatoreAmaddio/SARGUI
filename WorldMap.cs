using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace SARGUI
{
    public static class WorldMap
    {
        public static readonly Countries Countries = new ();
        private static readonly List<Language> Languages = new ();
        static readonly CultureInfo[] AllCultures = CultureInfo.GetCultures(CultureTypes.AllCultures & ~CultureTypes.NeutralCultures)
        .Where(s => !s.CultureTypes.ToString().ToLower().Contains("usercustomculture")).ToArray();

        public static void GetData()
        {
            Parallel.ForEach(AllCultures, (culture) => 
            {
                Countries.AddCountry(new(culture.LCID));
                Languages.Add(new(culture.LCID));
            });
        }

        public static Task GetDataAsync()
        {
            Parallel.ForEach(AllCultures, (culture) =>
            {
                Countries.AddCountry(new(culture.LCID));
                Languages.Add(new(culture.LCID));
            });
            return Task.CompletedTask;
        }

        public static void ConsoleWriteLine(string str)
        {
            int threadId = Environment.CurrentManagedThreadId;
            Console.ForegroundColor = threadId == 1 ? ConsoleColor.White : ConsoleColor.Cyan;
            Console.WriteLine($"{str}{new string(' ', 26 - str.Length)}   Thread {threadId}");
        }
    }

    public class Countries : List<Country>
    {
        public void AddCountry(Country country)
        {
            if (!Contains(country))
                Add(country);
        }
    }

    public class Country
    {
        public string Name { get; }
        public string DisplayName { get; }
        public string EnglishName { get; }
        public string NativeName { get; }
        public string ISO_3 { get; }
        public string ISO { get; }
        public string WinISO { get; }
        public int CountryID { get; }
        public Currency Currency { get; }
        public int ID { get; }
   
        public Country(int ID)
        {
            this.ID= ID;
            RegionInfo info = new(ID);
            Currency = new(ID);
            CountryID = info.GeoId;
            Name = info.Name;
            DisplayName = info.DisplayName;
            EnglishName = info.EnglishName;
            NativeName = info.NativeName;
            ISO_3 = info.ThreeLetterISORegionName;
            ISO = info.TwoLetterISORegionName;
            WinISO = info.ThreeLetterWindowsRegionName;
        }

        public override string? ToString() => EnglishName;

        public override bool Equals(object? obj) =>
        obj is Country country && CountryID == country.CountryID;

        public override int GetHashCode() =>
        HashCode.Combine(CountryID);
    }

    public class Currency
    {
        public string CurrencySymbol { get; }
        public string CurrencyEnglishName { get; }
        public string ISO { get; }
        public string CurrencyNativeName { get; }
        public int ID { get; }

        public Currency(int ID)
        {
            this.ID = ID;
            RegionInfo info = new(ID);
            CurrencySymbol = info.CurrencySymbol;
            CurrencyEnglishName = info.CurrencyEnglishName;
            ISO = info.ISOCurrencySymbol;
            CurrencyNativeName = info.CurrencyNativeName;
        }
    }

    public class Language
    {
        public int LanguageID { get; }
        public string Name { get; }
        public string WinISO { get; }
        public string IetfLanguageTag { get; }
        public string DisplayName { get; }
        public string EnglishName { get; }
        public string NativeName { get; }
        public string ISO_3 { get; }
        public string ISO { get; }
        public int CountryID { get; }
        public NumberFormatInfo NumberFormatInfo { get; }

        public Language(int LanguageID)
        {
            this.LanguageID = LanguageID;
            RegionInfo info2 = new(LanguageID);
            CountryID = info2.GeoId;

            CultureInfo info = new(LanguageID);
            IetfLanguageTag = info.IetfLanguageTag;

            Name = info.Name;
            DisplayName = info.DisplayName;
            EnglishName = info.EnglishName;
            NativeName = info.NativeName;

            NumberFormatInfo = info.NumberFormat;
            WinISO = info.ThreeLetterWindowsLanguageName;
            ISO_3 = info.ThreeLetterISOLanguageName;
            ISO = info.TwoLetterISOLanguageName;
        }

        public override bool Equals(object? obj)
        {
            return obj is Language language &&
                   LanguageID == language.LanguageID;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(LanguageID);
        }

        public override string? ToString() => EnglishName;

    }
}
