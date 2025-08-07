using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using WikiFunctions.Parse;

public class Baslik {
	public static string Main(string ArticleText) {
		List<string> sablonlar = Parsers.GetAllTemplateDetail(ArticleText);
		Regex kaynakbaslik = new Regex(@"(\{\{\s*?(?:[\p{L}]*? kaynağı|[Kk]aynak\s*?\||[Cc]ite [\p{L}]?).*?\|\s*?(?:başlık|title)\s*?\=[^\p{L}]*)([\p{Lu}\W\d]*?)([^\p{L}]*(?:\||\}\}))");
		Regex sablonicisablon = new Regex(@"\{\{(?:[^{}]*\{\{[^{}]*\}\}[^{}]*){1,}");
		foreach(string sablon in sablonlar) {
			if (kaynakbaslik.Match(sablon).Success&&!sablonicisablon.Match(sablon).Success) {
				var hmmm = kaynakbaslik.Match(sablon);
				string başlık = hmmm.Groups[2].Value;
				başlık = Hallet(başlık);
				string yenisablon = kaynakbaslik.Replace(sablon, "$1" + başlık + "$3");
				ArticleText = ArticleText.Replace(sablon, yenisablon);
			}
		}
		return ArticleText;
	}

	public static string Hallet(string baslik) {
		Regex buyukharf = new Regex(@"^[\p{Lu}\W\d]+$");
		Regex kucukharf = new Regex(@"[\p{Ll}]");
		Regex separatorRegex = new Regex(@"[^a-zA-Z\p{L}]+");

		if (buyukharf.Match(baslik).Success && !kucukharf.Match(baslik).Success) {
			string[] kelime = separatorRegex.Split(baslik);
			string[] modifiedWords = new string[kelime.Length];
			string[] ayiricilar = new string[kelime.Length];

			if (kelime.Length < 3) return baslik; //Başlık üç kelimeden kısaysa geç

			MatchCollection matchCollection = Regex.Matches(baslik, "[^a-zA-Z\\p{L}]+");
			int ü = 0;
			foreach (Match match in matchCollection) {
				ayiricilar[ü] = match.Value;
				ü++;
			}

			Regex turkceHarfRegex = new Regex(@"[İıĞğÜüŞşÖöÇç]");
			Regex ingilizceHarfRegex = new Regex(@"[QqWwXx]");
			string lang = "";

			Regex rusca = new Regex(@"[\u0401\u0451\u0410-\u044f]");
			Regex yunan = new Regex(@"[\u0370-\u03ff\u1f00-\u1fff]");
			int enuzun = 0;

			for (int k = 0; k < kelime.Length; k++) {
				if (rusca.Match(kelime[k]).Success || yunan.Match(kelime[k]).Success) return baslik; //Rusça ya da Yunanca, geç
				if (enuzun < kelime[k].Length) enuzun = kelime[k].Length;
				if (turkceHarfRegex.IsMatch(kelime[k])) {
					if (lang != "en") lang = "tr";
					else lang = "belirsiz";
				} else if (ingilizceHarfRegex.IsMatch(kelime[k])) {
					if (lang != "tr") lang = "en";
					else lang = "belirsiz";
				}
				if (lang == "belirsiz") return baslik; //Türkçe İngilizce karışık?, geç
			}
			if (lang == "") lang = "enx"; //Belirlenemediyle İngilizce devam et

			if (enuzun <= 3) return baslik; //en uzun kelime üç harfli, geç

			Regex roma = new Regex(@"^M{0,4}(CM|CD|D?C{0,3})(XC|XL|L?X{0,3})(IX|IV|V?I{0,3})$");

			baslik = "";
			for (int i = 0; i < kelime.Length; i++) {
				string word = kelime[i];
				if (roma.Match(word).Success) modifiedWords[i] = word;
				else if (word == "ST"||word == "ND"||word == "RD"||word=="TH") modifiedWords[i] = word.ToLower(new CultureInfo("en-US"));
				else if (lang=="en"||lang=="enx") modifiedWords[i] = char.ToUpper(word[0]) + word.Substring(1).ToLower(new CultureInfo("en-US"));
				else modifiedWords[i] = char.ToUpper(word[0]) + word.Substring(1).ToLower(new CultureInfo("tr-TR"));
				if (i!=0) modifiedWords[i] = Cevirmen(modifiedWords[i], lang);
				baslik += modifiedWords[i] + ayiricilar[i];
			}
			
			Regex tirnak = new Regex(@"([\u0027\u0060\u00B4\u02B9\u02BB\u02BC\u02BD\u02BE\u02BF\u02C8\u02CA\u02EE\u0301\u0313\u0314\u0315\u0341\u0343\u0374\u0384\u055A\u059C\u059D\u05F3\u1FBD\u1FBF\u2018\u2019\u201B\u2032\u2035\uA78B\uA78C\uFF07])([\p{Lu}])");
			if (tirnak.Match(baslik).Success) {
				var hmmm = tirnak.Match(baslik);
				if (lang=="en"||lang=="enx") baslik = baslik.Replace(hmmm.Groups[1].Value + hmmm.Groups[2].Value, hmmm.Groups[1].Value + hmmm.Groups[2].Value.ToLower(new CultureInfo("en-US")));
				else baslik = baslik.Replace(hmmm.Groups[1].Value + hmmm.Groups[2].Value, hmmm.Groups[1].Value + hmmm.Groups[2].Value.ToLower(new CultureInfo("tr-TR")));
			}
		}
		return baslik;
	}

	public static string Cevirmen(string word, string lang) {
		if (lang=="en"||lang=="enx") {
			if (word == "A" || word == "An" || word == "The" ||
				word == "And" || word == "Or" || word == "But" || word == "Nor" ||
				word == "In" || word == "On" || word == "At" || word == "By" || 
				word == "For" || word == "To" || word == "With" || word == "From" || 
				word == "Into" || word == "Onto" || word == "Upon" || word == "Within" || 
				word == "Without" || word == "Of")
				return word.ToLower(new CultureInfo("en-US"));
		}
		if (lang=="tr"||lang=="enx") {
			Regex soru = new Regex(@"^M[iıuü]");
			if (!soru.Match(word).Success&&(word=="İle"||word=="Ve"||word=="Ya"||word=="Da"||word=="De"||word=="Ki"))
				return word.ToLower(new CultureInfo("tr-TR"));
		}
		return word;
	}
}