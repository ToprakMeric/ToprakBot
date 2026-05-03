using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using WikiFunctions.Parse;

public class Baslik {
    //Finds citation templates (excluding nested templates) titles to be converted to Title Case.
	//todo: AI?
	private static readonly Regex kaynakbaslik = new Regex(@"(\{\{\s*?(?:[\p{L}]*? kaynağı|[Kk]aynak\s*?\||[Cc]ite [\p{L}]?).*?\|\s*?(?:başlık|title)\s*?\=[^\p{L}]*)([\p{Lu}\W\d]*?)([^\p{L}]*(?:\||\}\}))");
	private static readonly Regex sablonicisablon = new Regex(@"\{\{(?:[^{}]*\{\{[^{}]*\}\}[^{}]*){1,}");
	private static readonly Regex roma = new Regex(@"^M{0,4}(CM|CD|D?C{0,3})(XC|XL|L?X{0,3})(IX|IV|V?I{0,3})$");
	private static readonly Regex tirnak = new Regex(@"([\u0027\u0060\u00B4\u02B9\u02BB\u02BC\u02BD\u02BE\u02BF\u02C8\u02CA\u02EE\u0301\u0313\u0314\u0315\u0341\u0343\u0374\u0384\u055A\u059C\u059D\u05F3\u1FBD\u1FBF\u2018\u2019\u201B\u2032\u2035\uA78B\uA78C\uFF07])([\p{Lu}])");

	private static readonly HashSet<string> englishExceptions = new HashSet<string>(StringComparer.OrdinalIgnoreCase) {
		"A", "An", "As", "The", "And", "Or", "But", "Nor", "In", "On", "At", "By",
        "For", "To", "With", "From", "Into", "Onto", "Up", "Upon", "Within",
        "Without", "Of", "Versus"
	};
	
	private static readonly HashSet<string> turkishExceptions = new HashSet<string>(StringComparer.OrdinalIgnoreCase) {
        "Mi", "Mı", "Mu", "Mü", "İle", "Ve", "Ya", "Da", "De", "Ki"
    };

	private static readonly CultureInfo cultureTr = new CultureInfo("tr-TR");
	private static readonly CultureInfo cultureEn = new CultureInfo("en-US");
	private static readonly TextInfo textInfoTr = cultureTr.TextInfo;
	private static readonly TextInfo textInfoEn = cultureEn.TextInfo;

	public static string Main(string ArticleText) {
		List<string> sablonlar = Parsers.GetAllTemplateDetail(ArticleText);
		
		
		foreach(string sablon in sablonlar) {
			if (kaynakbaslik.Match(sablon).Success&&!sablonicisablon.Match(sablon).Success) {
				var match = kaynakbaslik.Match(sablon);
				string baslik = match.Groups[2].Value;
				string islenmisBaslik = Hallet(baslik);

				if (baslik != islenmisBaslik) {
					string yenisablon = kaynakbaslik.Replace(sablon, "$1" + islenmisBaslik + "$3");
					ArticleText = ArticleText.Replace(sablon, yenisablon);
				}
			}
		}
		return ArticleText;
	}

	//If the title consists only of uppercase letters and contains no lowercase letters, convert it to title case
	public static string Hallet(string baslik) {
		if (string.IsNullOrWhiteSpace(baslik)) return baslik;

		Regex buyukharf = new Regex(@"^[\p{Lu}\W\d]+$");
		Regex kucukharf = new Regex(@"[\p{Ll}]");
		Regex separatorRegex = new Regex(@"[^a-zA-Z\p{L}]+");

		if (buyukharf.Match(baslik).Success && !kucukharf.Match(baslik).Success) {
			string[] kelime = separatorRegex.Split(baslik);
			string[] modifiedWords = new string[kelime.Length];
			string[] ayiricilar = new string[kelime.Length];

			if (kelime.Length < 3) return baslik; //title shorter than three words, skip

			MatchCollection matchCollection = Regex.Matches(baslik, "[^a-zA-Z\\p{L}]+");
			int ü = 0;
			foreach (Match match in matchCollection) {
				ayiricilar[ü] = match.Value;
				ü++;
			}

			Regex turkceHarfRegex = new Regex(@"[İıĞğÜüŞşÖöÇç]");
			Regex ingilizceHarfRegex = new Regex(@"[QqWwXx]");
			Regex rusca = new Regex(@"[\u0401\u0451\u0410-\u044f]");
			Regex yunan = new Regex(@"[\u0370-\u03ff\u1f00-\u1fff]");
			
			string lang = "enx";
			int enuzun = 0;

			for (int k = 0; k < kelime.Length; k++) {
				if (rusca.Match(kelime[k]).Success || yunan.Match(kelime[k]).Success) return baslik; //Russian or Greek characters, skip
				if (enuzun < kelime[k].Length) enuzun = kelime[k].Length;
				if (turkceHarfRegex.IsMatch(kelime[k])) {
					if (lang != "en") lang = "tr";
					else lang = "belirsiz";
				} else if (ingilizceHarfRegex.IsMatch(kelime[k])) {
					if (lang != "tr") lang = "en";
					else lang = "belirsiz";
				}
				if (lang == "belirsiz") return baslik; //Turkish and English characters mixed, skip
			}
			if (lang == "") lang = "enx"; //Undecidable, default to English

			if (enuzun <= 3) return baslik; //longest word is three characters or less, likely an acronym, skip

			baslik = "";
			TextInfo currentTextInfo = (lang == "tr") ? textInfoTr : textInfoEn;

			for (int i = 0; i < kelime.Length; i++) {
				string word = kelime[i];
				if (string.IsNullOrEmpty(word)) continue;
				if (roma.Match(word).Success) modifiedWords[i] = word; //save Roman numerals
				else if (word == "ST"||word == "ND"||word == "RD"||word=="TH") modifiedWords[i] = currentTextInfo.ToLower(word);
				else {
					string lowerWord = currentTextInfo.ToLower(word);
					modifiedWords[i] = currentTextInfo.ToUpper(lowerWord[0]) + lowerWord.Substring(1);
				}
				if (i!=0) modifiedWords[i] = Cevirmen(modifiedWords[i], lang);
				baslik += modifiedWords[i] + ayiricilar[i];
			}
			
			if (tirnak.IsMatch(baslik)) baslik = tirnak.Replace(baslik, m => m.Groups[1].Value + currentTextInfo.ToLower(m.Groups[2].Value));
		}
		return baslik;
	}

	//Exception words
	public static string Cevirmen(string word, string lang) {
		if(lang=="en"||lang=="enx") {
			if (englishExceptions.Contains(word))
				return textInfoEn.ToLower(word);
		}
		if (lang=="tr"||lang=="enx") {
			if (turkishExceptions.Contains(word))
				return textInfoTr.ToLower(word);
		}
		return word;
	}
}