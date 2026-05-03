using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class GorunmezKarakter {
	//Makes invisible characters visible and removes unnecessary invisible characters.
	public static Tuple<string, string> Main(string ArticleText, string ArticleTitle, string lang) {
		string summary = "";

		List<string> chars = new List<string>();
		List<string> unnecessary = new List<string>();

		Regex arabic = new Regex(@"[\u0621-\u064A]"); //Arabic dedector
		Regex hebrew = new Regex(@"[\u0590-\u05FF]"); //Hebrew dedector

		//lüzumsuz karakter kaldırıcı
		Regex lefttoright = new Regex(@"(\u200E)"); //left-to-rigt
		Regex righttoleft = new Regex(@"(\u200F)"); //rigt-to-left

		if (!(arabic.Match(ArticleText).Success||hebrew.Match(ArticleText).Success)) {
			if (lefttoright.Match(ArticleText).Success) {
				ArticleText = lefttoright.Replace(ArticleText, "");
				unnecessary.Add("\"[[en:left-to-right mark|left-to-right]]\" (U+200E)");
			} else if(righttoleft.Match(ArticleText).Success) {
				ArticleText = righttoleft.Replace(ArticleText, "");
				unnecessary.Add("\"[[en:right-to-left mark|right-to-left]]\" (U+200F)");
			}
		}

		bool nonbreaking = false;
		Regex unnecessary1 = new Regex(@"\u00a0\r(\n*)");
		if (unnecessary1.Match(ArticleText).Success) { //Non-breaking space at the end of the line
			ArticleText=unnecessary1.Replace(ArticleText, "$1");
			nonbreaking = true;
		}

		Regex unnecessary2 = new Regex(@"\r(\n*)\u00a0");
		if (unnecessary2.Match(ArticleText).Success) { //Non-breaking space at the beginning of the line
			ArticleText = unnecessary2.Replace(ArticleText, "$1");
			nonbreaking = true;
		}

		Regex unnecessary3 = new Regex(@"(\u00a0(\s)){1,}");
		if (unnecessary3.Match(ArticleText).Success) { //Series of a non-breaking space/space
			ArticleText = unnecessary3.Replace(ArticleText, "$2");
			nonbreaking = true;
		}

		Regex unnecessary4 = new Regex(@"(\={2,4})(.*?\u00a0.*?)(\={2,4})");
		if (unnecessary4.Match(ArticleText).Success) { //Non-breaking space in titles
			bool bittimi = false;
			while(bittimi==false) {
				var cikar = unnecessary4.Match(ArticleText);
				if(unnecessary4.Match(ArticleText).Success) { //First char non-breaking space
					var baslik = cikar.Groups[2].Value;
					baslik = baslik.Replace(" ", " ");
					ArticleText = unnecessary4.Replace(ArticleText, "$1"+baslik+"$3", 1);
				} else bittimi = true;
			}
			nonbreaking = true;
		}

		if (nonbreaking) unnecessary.Add("[[Ayrılmaz alan|kırılmaz boşluk]] (U+00A0)");

		Regex unnecessary5 = new Regex(@"(\s*)\u200B{1,}");
		if (unnecessary5.Match(ArticleText).Success) { //Unnecessary zero-width space
			ArticleText = unnecessary5.Replace(ArticleText, "$1");
			unnecessary.Add("[[en:Zero-width space|Zero-width space]] (U+200B)");
		}

		Regex unnecessary6 = new Regex(@"(\s*)\uFEFF{1,}");
		if (unnecessary6.Match(ArticleText).Success) { //Unnecessary byte order mark
			ArticleText = unnecessary6.Replace(ArticleText, "$1");
			unnecessary.Add("[[en:Byte order mark|Byte order mark]] (U+FEFF)");
		}

		Regex unnecessary7 = new Regex(@"\u0307");
		if (unnecessary7.Match(ArticleText).Success) { //combining dot above
			if (!(ArticleTitle=="Ayırıcı im"||ArticleTitle=="2021 Filistin-İsrail çatışmaları"||ArticleTitle=="Denesulinece"||ArticleTitle=="Hamilton mekaniği")) { //İstisna maddeler
				Regex unnecessary7a = new Regex(@"ı\u0307");
				Regex unnecessary7b = new Regex(@"i\u0307");
				if (unnecessary7a.Match(ArticleText).Success) {
					ArticleText = unnecessary7a.Replace(ArticleText, "i");
					unnecessary.Add("[[:en:Dot (diacritic)|Combining Dot Above]] (U+0307)");
				} else if(unnecessary7b.Match(ArticleText).Success) {
					ArticleText = unnecessary7b.Replace(ArticleText, "i");
					unnecessary.Add("[[:en:Dot (diacritic)|Combining Dot Above]] (U+0307)");
				}
			}
		}

		//character replacer
		Regex finder1 = new Regex(@"(\u00a0)");
		if (finder1.Match(ArticleText).Success) { //Non-breaking space
			ArticleText = finder1.Replace(ArticleText, "&nbsp;");
			chars.Add("[[Ayrılmaz alan|kırılmaz boşluk]] (U+00A0)");
		}

		Regex finder2 = new Regex(@"(\u200B)");
		if (finder2.Match(ArticleText).Success) { //Zero-width space
			ArticleText = finder2.Replace(ArticleText, "&#8203;");
			chars.Add("[[en:Zero-width space|Zero-width space]] (U+200B)");
		}

		Regex finder3 = new Regex(@"(\u2009)");
		if (finder3.Match(ArticleText).Success) { //Thin space
			ArticleText = finder3.Replace(ArticleText, "&thinsp;");
			chars.Add("[[en:Thin space|Thin space]] (U+2009)");
		}

		Regex finder4 = new Regex(@"(\u200c)");
		if (finder4.Match(ArticleText).Success) { //Zero-width non-joiner
			ArticleText = finder4.Replace(ArticleText, "&zwnj;");
			chars.Add("[[en:Zero-width non-joiner|Zero-width non-joiner]] (U+200C)");
		}

		Regex finder5 = new Regex(@"(\u3000)");
		if (finder5.Match(ArticleText).Success) { //Ideographic space
			ArticleText = finder5.Replace(ArticleText, "&#x3000;");
			chars.Add("[[en:Whitespace_character#Unicode|Ideographic space]] (U+3000)");
		}

		Regex finder6 = new Regex(@"(\u2002)");
		if (finder6.Match(ArticleText).Success) { //En space
			ArticleText = finder6.Replace(ArticleText, "&ensp;");
			chars.Add("[[en:En (typography)|En space]] (U+2002)");
		}

		Regex finder7 = new Regex(@"(\u200A)");
		if (finder7.Match(ArticleText).Success) { //Hair space
			ArticleText = finder7.Replace(ArticleText, "&hairsp;");
			chars.Add("[[en:Whitespace_character#Hair_spaces_around_dashes|Hair space]] (U+200A)");
		}

		Regex finder8 = new Regex(@"(\uFEFF)");
		if (finder8.Match(ArticleText).Success) { //Byte order mark
			ArticleText = finder8.Replace(ArticleText, "&#65279;");
			chars.Add("[[en:Byte order mark|Byte order mark]] (U+FEFF)");
		}

		Regex finder9 = new Regex(@"(\u2008)");
		if (finder9.Match(ArticleText).Success) { //Punctuation space
			ArticleText = finder9.Replace(ArticleText, "&#8200;");
			chars.Add("[[wikt:en:punctuation_space|Punctuation space]] (U+2008)");
		}

		Regex finder10 = new Regex(@"(\u200D)");
		if (finder10.Match(ArticleText).Success) { //Zero-width joiner
			ArticleText = finder10.Replace(ArticleText, "&zwj;");
			chars.Add("[[en:Zero-width joiner|Zero-width joiner]] (U+200D)");
		}

		Regex finder11 = new Regex(@"(\u202F)");
		if (finder11.Match(ArticleText).Success) { //Narrow no-break space
			ArticleText = finder11.Replace(ArticleText, "&#8239;");
			chars.Add("[[en:Non-breaking_space#Width_variation|Narrow no-break space]] (U+202F)");
		}

		Regex finder12 = new Regex(@"(\u00AD)");
		if (finder12.Match(ArticleText).Success) { //Soft hyphen	
			ArticleText = finder12.Replace(ArticleText, "&shy;");
			chars.Add("[[en:Soft hyphen|Soft hyphen]] (U+00AD)");
		}

		//edit summary
		string str = string.Empty;
		foreach(var item in chars) str = str + item + ", ";

		string str2 = string.Empty;
		foreach(var item2 in unnecessary) str2 = str2 + item2 + ", ";


		if (!string.IsNullOrEmpty(str)) {
			str = str.Remove(str.Length - 2);
			summary += lang == "tr" ? "; Görünmez karakterler fark edilebilir kılındı: " + str :
					   lang == "az" ? "; Görünməz boşluqlar görünür edildi: " + str :
					   lang == "en" ? "; Invisible characters made visible: " + str : "";
		}
		if (!string.IsNullOrEmpty(str2)) {
			str2 = str2.Remove(str2.Length - 2);
			summary += lang == "tr" ? "; Gereksiz görünmez karakterler kaldırıldı: " + str2 :
					   lang == "az" ? "; Lazımsız görünməz boşluqlar silindi: " + str2 :
					   lang == "en" ? "; Unnecessary invisible characters removed: " + str2 : "";
		}

		return new Tuple<string, string>(ArticleText, summary);
	}
}