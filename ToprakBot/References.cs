using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using WikiFunctions;
using WikiFunctions.Parse;

public class Ref {
// FixReferenceTags copied from WikiFunctions.Parse.Parsers.FixReferenceTags
// its regex was deprecated by https://meta.wikimedia.org/wiki/WMDE_Technical_Wishes/Sub-referencing
// Maybe copy all the functions?
	public struct RegexReplacement {
		public readonly Regex Regex;
		public readonly string Replacement;
		public RegexReplacement(Regex regex, string replacement) {
			Regex=regex;
			Replacement=replacement;
		}
		public RegexReplacement(string pattern, RegexOptions options, string replacement)
			: this(new Regex(pattern, options), replacement) {
		}
		public RegexReplacement(string pattern, string replacement)
			: this(pattern, RegexOptions.None, replacement) {
		}
	}
	private static readonly Regex AllTagsSpace=new Regex("<[^<>]+> *");
	private static readonly Regex EmptyRefTags=new Regex("<ref>\\s*</ref>");
	private static readonly Regex RedRef=new Regex("(<\\s*ref(?:\\s+name\\s*=[^<>]*?)?\\s*>[^<>\"]+?)<\\s*(?:/\\s*red|ref\\s*/)\\s*>", RegexOptions.IgnoreCase);
    private static readonly Regex PossiblyBadRefTags = new Regex("<\\s*[Rr][Ee][Ff][^<>]*>(?<!(?:<ref (?:details\\s*=\\s*\"[^\"]*\"\\s+)?name *= *[\\w0-9\\-.]+( ?/)?>|<ref>|<ref (?:details\\s*=\\s*\"[^\"]*\"\\s+)?name *= *\"[^{}\"<>]+\"(?:\\s+details\\s*=\\s*\"[^\"]*\")?( ?/)?>))");
	//this regex was edited
	private static readonly Regex NamedRefExcessQuotes = new Regex("(<ref\\s+(?:details\\s*=\\s*\"[^\"]*\"\\s+)?name\\s*=\\s*\")([^=<>]+?)(\"(?:\\s+details\\s*=\\s*\"[^\"]*\")?\\s*/?\\s*>)");
	private static readonly RegexReplacement[] RefSimple = {
		new RegexReplacement(new Regex("<\\s*(?:\\s+ref\\s*|\\s*ref\\s+)>", RegexOptions.Singleline), "<ref>"),
		new RegexReplacement(new Regex("(<\\s*ref\\s+name\\s*)(?:[-\\+\\:]|= ?=+|(\") *=)"), "$1=$2"),
		new RegexReplacement(new Regex("(?:(<\\s*ref\\s+name\\s*=\\s*\"?[^<>=\"\\/]+?\"?)\\s*/\\s*/|(<\\s*ref\\s+name\\s*=\\s*\"[^<>=\"\\/]+?\")\\s*/\\s*ref)\\s*>", RegexOptions.IgnoreCase), "$1$2/>"),
		new RegexReplacement(new Regex("(<\\s*ref\\s+name\\s*=\\s*\"?[^<>=\"\\/]+?)(?:(\")[\"\\.']|(\")?=)(\\s*/?)>", RegexOptions.IgnoreCase), "$1$2$3$4>"),
		new RegexReplacement(new Regex("(<\\s*ref\\s+name\\s*=\\s*)[“‘”’\"]\"([^<>=\"\\/]+?)\"*(\\s*/?)>", RegexOptions.IgnoreCase), "$1\"$2\"$3>"),
		new RegexReplacement(new Regex("(<\\s*ref\\s+name\\s*=\\s*)(?:[“‘”’]+(?<val>[^<>=\"\\/]+?)[“‘”’]*|[“‘”’]*(?<val>[^<>=\"\\/]+?)[“‘”’]+)(\\s*/?>)", RegexOptions.IgnoreCase), "$1\"${val}\"$2"),
		new RegexReplacement(new Regex("(<\\s*ref\\s+name\\s*=\\s*)(?:''+(?<val>[^<>=\"\\/]+?)'+|'+(?<val>[^<>=\"\\/]+?)''+)(\\s*/?>)", RegexOptions.IgnoreCase), "$1\"${val}\"$2"),
		new RegexReplacement(new Regex("(<\\s*ref\\s+name\\s*=\\s*)([^<>=\"']+?[ /][^<>=\"'/ ]+?)(\\s*/?>)", RegexOptions.IgnoreCase), "$1\"$2\"$3"),
		new RegexReplacement(new Regex("(<\\s*ref\\s+name\\s*=\\s*)([^<>=\"'\\/]*?[^\\x00-\\xff]+?[^<>=\"'\\/]*?)(\\s*/?>)", RegexOptions.IgnoreCase), "$1\"$2\"$3"),
		new RegexReplacement(new Regex("(<\\s*ref\\s+name\\s*=?\\s*)(?:['`”]*(?<val>[^<>=\"\\/]+?)\"|\"(?<val>[^<>=\"\\/]+?)['`”]*)(\\s*/?>)", RegexOptions.IgnoreCase), "$1\"${val}\"$2"),
		new RegexReplacement(new Regex("(<\\s*ref\\s+name\\s*)[\\+\\-\"]?(\\s*\"[^<>=\"\\/]+?\"\\s*/?)=?>", RegexOptions.IgnoreCase), "$1=$2>"),
		new RegexReplacement(new Regex("(<\\s*ref\\s+)=?\\s*(\"[^<>=\"\\/]+?\"\\s*/?>)", RegexOptions.IgnoreCase), "$1name=$2"),
		new RegexReplacement(new Regex("(<\\s*ref\\s+n)(me\\s*=)", RegexOptions.IgnoreCase), "$1a$2"),
		new RegexReplacement(new Regex("(<\\s*ref\\s+name\\s*=\\s*\"[^<>=\"\\/]+?)\"([^<>=\"\\/]{2,}?)(?<!\\s+)(?=\\s*/?>)", RegexOptions.IgnoreCase), "$1$2\""),
		new RegexReplacement(new Regex("<\\s*ref(?:\\s+NAME|name|\\s+name\\s*=\\s*name|\\s+name\\s*=\\s*\"\\s*ref\\s+name)(\\s*=)"), "<ref name$1")
	};

	private static string FixReferenceTagsME(Match m) {
        return RefSimple.Aggregate(m.Value, (string current, RegexReplacement rr) => rr.Regex.Replace(current, rr.Replacement));
    }

	public static string FixReferenceTags(string articleText) {
			List<string> source = Tools.DeduplicateList((from Match m in AllTagsSpace.Matches(articleText)
														 where m.Value.IndexOf("re", StringComparison.OrdinalIgnoreCase)>0&&!m.Value.Equals("<ref>")&&!m.Value.Equals("</ref>")&&!m.Value.StartsWith("<references")
														 select m.Value).ToList());
			articleText=Regex.Replace(articleText, "</ref ?\r\n", "</ref>\r\n");
			if(source.Any((string s) => Regex.IsMatch(s, "R[Ee][Ff]|r[Ee]F"))) {
				articleText=Regex.Replace(articleText, "(<\\s*\\/?\\s*)(?:R[Ee][Ff]|r[Ee]F)(\\s*(?:>|name\\s*=))", "$1ref$2");
			}

			if(source.Any((string s) => s.EndsWith(" "))) {
				articleText=Regex.Replace(articleText, "(</ref>|<ref\\s*name\\s*=[^{}<>]+?\\s*\\/\\s*>) +(?=<ref(?:\\s*name\\s*=[^{}<>]+?\\s*\\/?\\s*)?>)", "$1");
			}

			articleText=Regex.Replace(articleText, "(</ref>|<ref\\s*name\\s*=[^{}<>]+?\\s*\\/\\s*>)(\\w)", "$1 $2");
			if(articleText.Contains(" <ref")) {
				articleText=Regex.Replace(articleText, "(?<=[,\\.:;]) +(<ref(?:\\s*name\\s*=[^{}<>]+?\\s*\\/?\\s*)?>)", "$1");
			}

			while(EmptyRefTags.IsMatch(articleText)) {
				articleText=EmptyRefTags.Replace(articleText, "");
			}

			if(source.Any((string s) => s.EndsWith(" "))) {
				articleText=Regex.Replace(articleText, "(<ref[^<>\\{\\}\\/]*>) +", "$1");
			}

			if(source.Any((string s) => Regex.IsMatch(s, "<(?:\\s*/(?:\\s+ref\\s*|\\s*ref\\s+)|\\s+/\\s*ref\\s*)>"))) {
				articleText=Regex.Replace(articleText, "<(?:\\s*/(?:\\s+ref\\s*|\\s*ref\\s+)|\\s+/\\s*ref\\s*)>", "</ref>");
			}

			if(articleText.Contains(" </ref>")) {
				articleText=Regex.Replace(articleText, " +</ref>", "</ref>");
			}

			if(source.Any((string s) => s.StartsWith("<ref/>")||s.StartsWith("</red>"))) {
				articleText=RedRef.Replace(articleText, "$1</ref>");
			}

			if(Variables.LangCode.Equals("zh")) {
				articleText=Regex.Replace(articleText, "(</ref>|<ref\\s*name\\s*=[^{}<>]+?\\s*\\/\\s*>) +", "$1");
			}

			if(source.Any((string s) => !Regex.IsMatch(s, "(?:<ref name *= *[\\w0-9\\-.]+( ?/)?>|<ref name *= *\"[^{}\"<>]+\"( ?/)?>)|</ref>"))) {
            if(!source.Any((string t) => Regex.IsMatch(t, "name *= *\"\" */>"))) {
                articleText=Regex.Replace(articleText, "<\\s*ref\\s+name[\\s\"]*=?[\\s\"]*(?:group\\s*=\\s*)?>", "<ref>");
            }

            articleText=PossiblyBadRefTags.Replace(articleText, FixReferenceTagsME);
            articleText=NamedRefExcessQuotes.Replace(articleText, (Match m) => WikiRegexes.RefsGrouped.IsMatch(m.Groups[3].Value.Contains("/") ? m.Value : (m.Value+"a</ref>")) ? m.Value : (m.Groups[1].Value+m.Groups[2].Value.Replace("\"", "")+m.Groups[3].Value));
        }

        return articleText;
    }
}

public class RefTitle {
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

public class KaynakCevir {
	//Kaynaklardaki tarihleri Türkçeye çevirir.
	//basım bilgisi varsa onu da çevirir.
	public static string Main(string ArticleText) {
		
		//tarih çevirileri:
		//January 23, 2021
		Regex tarihRegex1 = new Regex(@"(\s*?\|\s*?(erişim(\-|\s|)tarihi|tarih|access(\-|\s|)date|accessdate|date|archive(\-|)date|arşiv(\-|)tarihi)\s*?\=\s*?)(January|February|March|April|May|June|July|August|September|October|November|December)\s*?(\d{1,2})\s*?(,|)\s*?(\d{4})", RegexOptions.IgnoreCase);
		ArticleText = tarihRegex1.Replace(ArticleText, match => {
			var ay = match.Groups[7].Value;
			ay = Tercuman(ay);
			return match.Groups[1].Value + match.Groups[8].Value + " " + ay + " " + match.Groups[10].Value;
		});

		//23 January 2021
		Regex tarihRegex2 = new Regex(@"(\s*?\|\s*?(erişim(\-|\s|)tarihi|tarih|access(\-|\s|)date|accessdate|date|archive(\-|)date|arşiv(\-|)tarihi)\s*?\=\s*?)(\d{1,2})\s*?(January|February|March|April|May|June|July|August|September|October|November|December)\s*?(\d{4})\s*?", RegexOptions.IgnoreCase);
		ArticleText = tarihRegex2.Replace(ArticleText, match => {
			var ay = match.Groups[8].Value;
			ay = Tercuman(ay);
			return match.Groups[1].Value + match.Groups[7].Value + " " + ay + " " + match.Groups[9].Value;
		});

		//2021-01-23
		Regex tarihRegex3 = new Regex(@"(\s*?\|\s*?(erişim(\-|\s|)tarihi|tarih|access(\-|\s|)date|accessdate|date|archive(\-|)date|arşiv(\-|)tarihi)\s*?\=\s*?)(\d{4})\-(\d{1,2})\-(\d{1,2})", RegexOptions.IgnoreCase);
		ArticleText = tarihRegex3.Replace(ArticleText, match => {
			var ay = match.Groups[8].Value;
			string gun = match.Groups[9].Value;

			switch (ay) {
				case "1":
				case "01":
					ay = "Ocak";
					break;
				case "2":
				case "02":
					ay = "Şubat";
					break;
				case "3":
				case "03":
					ay = "Mart";
					break;
				case "4":
				case "04":
					ay = "Nisan";
					break;
				case "5":
				case "05":
					ay = "Mayıs";
					break;
				case "6":
				case "06":
					ay = "Haziran";
					break;
				case "7":
				case "07":
					ay = "Temmuz";
					break;
				case "8":
				case "08":
					ay = "Ağustos";
					break;
				case "9":
				case "09":
					ay = "Eylül";
					break;
				case "10":
					ay = "Ekim";
					break;
				case "11":
					ay = "Kasım";
					break;
				case "12":
					ay = "Aralık";
					break;
				default:
					break;
			}

			Regex sıfırçıkarıcı = new Regex(@"0(\d)");
			gun = sıfırçıkarıcı.Replace(gun, "$1");

			return match.Groups[1].Value + gun + " " + ay + " " + match.Groups[7].Value;
		});

		//tarih düzeltmeleri:
		//21 Ocak, 2021 ve Ocak, 2021
		Regex tarihRegex4 = new Regex(@"(\s*?\|\s*?(erişim(\-|\s|)tarihi|tarih|access(\-|\s|)date|accessdate|date|archive(\-|)date|arşiv(\-|)tarihi)\s*?\=\s*?)(\d{1,2}|)\s*?(Ocak|Şubat|Mart|Nisan|Mayıs|Haziran|Temmuz|Ağustos|Eylül|Ekim|Kas[Iı]m|Aral[Iı]k)\s*?\,\s*?(\d{4})\s*?", RegexOptions.IgnoreCase);
		ArticleText = tarihRegex4.Replace(ArticleText, match => {
			return match.Groups[1].Value + match.Groups[7].Value + " " + match.Groups[8].Value + " " + match.Groups[9].Value;
		});

		//21 Ocak 2021 Pazartesi
		Regex tarihRegex5 = new Regex(@"(\s*?\|\s*?(erişim(\-|\s|)tarihi|tarih|access(\-|\s|)date|accessdate|date|archive(\-|)date|arşiv(\-|)tarihi)\s*?\=\s*?)(\d{1,2})\s*?(Ocak|Şubat|Mart|Nisan|Mayıs|Haziran|Temmuz|Ağustos|Eylül|Ekim|Kas[Iı]m|Aral[Iı]k)\s*?(\d{4})\s*?(Pazartesi|Sal[Iı]|Çarşamba|Perşembe|Cumartes[iİ]|Cuma|Pazar)\s*?", RegexOptions.IgnoreCase);
		ArticleText = tarihRegex5.Replace(ArticleText, match => {
			return match.Groups[1].Value + match.Groups[7].Value + " " + match.Groups[8].Value + " " + match.Groups[9].Value;
		});

		//21 ocAk 2021
		Regex tarihRegex6 = new Regex(@"(\s*?\|\s*?(erişim(\-|)tarihi|tarih|access(\-|)date|accessdate|date|archive(\-|)date|arşiv(\-|)tarihi)\s*?\=\s*?)(\d{1,2})\s*?(Ocak|Şubat|Mart|Nisan|Mayıs|Haziran|Temmuz|Ağustos|Eylül|Ekim|Kas[Iı]m|Aral[Iı]k)\s*?(\d{4})", RegexOptions.IgnoreCase);
		ArticleText = tarihRegex6.Replace(ArticleText, match => {
			return match.Groups[1].Value + match.Groups[7].Value + " " + match.Groups[8].Value.Substring(0, 1).ToUpper() + match.Groups[8].Value.Substring(1).ToLower() + " " + match.Groups[9].Value;
		});

		//Aralık 2020-Ocak 2021
		Regex tarihRegex7 = new Regex(@"(\s*?\|\s*?(erişim(\-|)tarihi|tarih|access(\-|)date|accessdate|date|archive(\-|)date|arşiv(\-|)tarihi)\s*?\=\s*?)((Ocak|Şubat|Mart|Nisan|Mayıs|Haziran|Temmuz|Ağustos|Eylül|Ekim|Kas[Iı]m|Aral[Iı]k)\s*\d{4})\-((Ocak|Şubat|Mart|Nisan|Mayıs|Haziran|Temmuz|Ağustos|Eylül|Ekim|Kas[Iı]m|Aral[Iı]k)\s*\d{4})", RegexOptions.IgnoreCase);
		ArticleText = tarihRegex7.Replace(ArticleText, match => {
			return match.Groups[1].Value + match.Groups[7].Value + " - " + match.Groups[9].Value;
		});

		//Ocak 2021
		Regex tarihRegex8 = new Regex(@"(\s*?\|\s*?(erişim(\-|)tarihi|tarih|access(\-|)date|accessdate|date|archive(\-|)date|arşiv(\-|)tarihi)\s*?\=\s*?)(\d{4})\s*(Ocak|Şubat|Mart|Nisan|Mayıs|Haziran|Temmuz|Ağustos|Eylül|Ekim|Kas[Iı]m|Aral[Iı]k)\s*(\||\}\})", RegexOptions.IgnoreCase);
		ArticleText = tarihRegex8.Replace(ArticleText, match => {
			return match.Groups[1].Value + match.Groups[8].Value + " " + match.Groups[7].Value + match.Groups[9].Value;
		});

		Regex tarihRegex9 = new Regex(@"(\s*?\|\s*?(erişim(\-|\s|)tarihi|tarih|access(\-|\s|)date|accessdate|date|archive(\-|)date|arşiv(\-|)tarihi)\s*?\=\s*?)0(\d)\s*(Ocak|Şubat|Mart|Nisan|Mayıs|Haziran|Temmuz|Ağustos|Eylül|Ekim|Kas[Iı]m|Aral[Iı]k)\s*(\d{4})", RegexOptions.IgnoreCase);
		ArticleText = tarihRegex9.Replace(ArticleText, match => {
			return match.Groups[1].Value + match.Groups[7].Value + " " + match.Groups[8].Value + " " + match.Groups[9].Value;
		});

		//basım çevirici
		Regex basımçekici = new Regex(@"\|\s*?basım\s*?\=\s*?((\d{1,2})(st|nd|rd|th))\s*?(\||\}\})", RegexOptions.IgnoreCase);
		ArticleText = basımçekici.Replace(ArticleText, match => {
			string basım = match.Groups[1].Value;

			Regex sayı = new Regex(@"(\d{1,2})(st|nd|rd|th)", RegexOptions.IgnoreCase);
			basım = sayı.Replace(basım, "$1.");

			return "|basım=" + basım + match.Groups[4].Value;
		});
		return ArticleText;
	}

	//İngilizce ay Türkçe çeviriler
	static string Tercuman(string eskiay) {
		switch (eskiay.ToLower()) {
			case "january":
			   return "Ocak";
			case "february":
				return "Şubat";
			case "march":
				return "Mart";
			case "april":
				return "Nisan";
			case "may":
				return "Mayıs";
			case "june":
				return "Haziran";
			case "july":
				return "Temmuz";
			case "august":
				return "Ağustos";
			case "september":
				return "Eylül";
			case "october":
				return "Ekim";
			case "november":
				return "Kasım";
			case "december":
				return "Aralık";
			default:
				return string.Empty;
		}
	}
}