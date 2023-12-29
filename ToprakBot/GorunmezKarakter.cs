using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class GorunmezKarakter {
	public static Tuple<string, string> Main(string ArticleText, string ArticleTitle) {
		string summary = "";

		List<string> karakterler = new List<string>();
		List<string> gereksiz = new List<string>();

		Regex arapça = new Regex(@"[\u0621-\u064A]"); //Arapça dedektörü
		Regex ibranice = new Regex(@"[\u0590-\u05FF]"); //İbranice dedektörü


		//lüzumsuz karakter kaldırıcı
		Regex lefttoright = new Regex(@"(\u200E)"); //left-to-rigt
		Regex righttoleft = new Regex(@"(\u200F)"); //rigt-to-left

		if(!(arapça.Match(ArticleText).Success||ibranice.Match(ArticleText).Success)) {
			if(lefttoright.Match(ArticleText).Success) {
				ArticleText = lefttoright.Replace(ArticleText, "");
				gereksiz.Add("\"[[en:left-to-right mark|left-to-right]]\" (U+200E)");
			} else if(righttoleft.Match(ArticleText).Success) {
				ArticleText = righttoleft.Replace(ArticleText, "");
				gereksiz.Add("\"[[en:right-to-left mark|right-to-left]]\" (U+200F)");
			}
		}

		bool kirilmaz = false;
		Regex luzumsuz1 = new Regex(@"\u00a0\r(\n*)");
		if(luzumsuz1.Match(ArticleText).Success) { //Satır sonunda kırılmaz boşluk
			ArticleText=luzumsuz1.Replace(ArticleText, "$1");
			gereksiz.Add("[[Ayrılmaz alan|kırılmaz boşluk]] (U+00A0)");
			kirilmaz = true;
		}

		Regex luzumsuz2 = new Regex(@"\r(\n*)\u00a0");
		if(luzumsuz2.Match(ArticleText).Success) { //Satır başında kırılmaz boşluk
			ArticleText = luzumsuz2.Replace(ArticleText, "$1");
			if(!kirilmaz) {
				gereksiz.Add("[[Ayrılmaz alan|kırılmaz boşluk]] (U+00A0)");
				kirilmaz = true;	
			}
		}

		Regex luzumsuz3 = new Regex(@"(\u00a0(\s)){1,}");
		if(luzumsuz3.Match(ArticleText).Success) { //Art arda kırılmaz boşluk-boşluk
			ArticleText = luzumsuz3.Replace(ArticleText, "$2");
			if(!kirilmaz) {
				gereksiz.Add("[[Ayrılmaz alan|kırılmaz boşluk]] (U+00A0)");
				kirilmaz = true;	
			}
		}

		Regex luzumsuz4 = new Regex(@"(\={2,4})(.*?\u00a0.*?)(\={2,4})");
		if(luzumsuz4.Match(ArticleText).Success) { //Başlık içinde kırılmaz boşluk
			bool bittimi = false;
			while(bittimi==false) {
				var cikar = luzumsuz4.Match(ArticleText);
				if(luzumsuz4.Match(ArticleText).Success) { //İlki kırılmaz boşluk
					var baslik = cikar.Groups[2].Value;
					baslik = baslik.Replace(" ", " ");
					ArticleText = luzumsuz4.Replace(ArticleText, "$1"+baslik+"$3", 1);
				} else bittimi = true;
			}
			if(!kirilmaz) {
				gereksiz.Add("[[Ayrılmaz alan|kırılmaz boşluk]] (U+00A0)");
			}
		}

		Regex luzumsuz5 = new Regex(@"(\s*)\u200B{1,}");
		if(luzumsuz5.Match(ArticleText).Success) { //Gereksiz zero-width space
			ArticleText = luzumsuz5.Replace(ArticleText, "$1");
			gereksiz.Add("[[en:Zero-width space|Zero-width space]] (U+200B)");
		}

		Regex luzumsuz6 = new Regex(@"(\s*)\uFEFF{1,}");
		if(luzumsuz6.Match(ArticleText).Success) { //Gereksiz byte order mark
			ArticleText = luzumsuz6.Replace(ArticleText, "$1");
			gereksiz.Add("[[en:Byte order mark|Byte order mark]] (U+FEFF)");
		}

		Regex luzumsuz7 = new Regex(@"\u0307");
		if(luzumsuz7.Match(ArticleText).Success) { //combining dot above
			if(!(ArticleTitle=="Ayırıcı im"||ArticleTitle=="2021 Filistin-İsrail çatışmaları"||ArticleTitle=="Denesulinece"||ArticleTitle=="Hamilton mekaniği")) { //İstisna maddeler
				Regex luzumsuz7a = new Regex(@"ı\u0307");
				Regex luzumsuz7b = new Regex(@"i\u0307");
				if(luzumsuz7a.Match(ArticleText).Success) {
					ArticleText = luzumsuz7a.Replace(ArticleText, "i");
					gereksiz.Add("[[:en:Dot (diacritic)|Combining Dot Above]] (U+0307)");
				} else if(luzumsuz7b.Match(ArticleText).Success) {
					ArticleText = luzumsuz7b.Replace(ArticleText, "i");
					gereksiz.Add("[[:en:Dot (diacritic)|Combining Dot Above]] (U+0307)");
				}
			}
		}

		//karakter gösterici
		Regex bulucu1 = new Regex(@"(\u00a0)");
		if(bulucu1.Match(ArticleText).Success) { //Kırılmaz boşluk
			ArticleText = bulucu1.Replace(ArticleText, "&nbsp;");
			karakterler.Add("[[Ayrılmaz alan|kırılmaz boşluk]] (U+00A0)");
		}

		Regex bulucu2 = new Regex(@"(\u200B)");
		if(bulucu2.Match(ArticleText).Success) { //Zero-width space
			ArticleText = bulucu2.Replace(ArticleText, "&#8203;");
			karakterler.Add("[[en:Zero-width space|Zero-width space]] (U+200B)");
		}

		Regex bulucu3 = new Regex(@"(\u2009)");
		if(bulucu3.Match(ArticleText).Success) { //Thin space
			ArticleText = bulucu3.Replace(ArticleText, "&thinsp;");
			karakterler.Add("[[en:Thin space|Thin space]] (U+2009)");
		}

		Regex bulucu4 = new Regex(@"(\u200c)");
		if(bulucu4.Match(ArticleText).Success) { //Zero-width non-joiner
			ArticleText = bulucu4.Replace(ArticleText, "&zwnj;");
			karakterler.Add("[[en:Zero-width non-joiner|Zero-width non-joiner]] (U+200C)");
		}

		Regex bulucu5 = new Regex(@"(\u3000)");
		if(bulucu5.Match(ArticleText).Success) { //Ideographic space
			ArticleText = bulucu5.Replace(ArticleText, "&#x3000;");
			karakterler.Add("[[en:Whitespace_character#Unicode|Ideographic space]] (U+3000)");
		}

		Regex bulucu6 = new Regex(@"(\u2002)");
		if(bulucu6.Match(ArticleText).Success) { //En space
			ArticleText = bulucu6.Replace(ArticleText, "&ensp;");
			karakterler.Add("[[en:En (typography)|En space]] (U+2002)");
		}

		Regex bulucu7 = new Regex(@"(\u200A)");
		if(bulucu7.Match(ArticleText).Success) { //Hair space
			ArticleText = bulucu7.Replace(ArticleText, "&hairsp;");
			karakterler.Add("[[en:Whitespace_character#Hair_spaces_around_dashes|Hair space]] (U+200A)");
		}

		Regex bulucu8 = new Regex(@"(\uFEFF)");
		if(bulucu8.Match(ArticleText).Success) { //Byte order mark
			ArticleText = bulucu8.Replace(ArticleText, "&#65279;");
			karakterler.Add("[[en:Byte order mark|Byte order mark]] (U+FEFF)");
		}

		Regex bulucu9 = new Regex(@"(\u2008)");
		if(bulucu9.Match(ArticleText).Success) { //Punctuation space
			ArticleText = bulucu9.Replace(ArticleText, "&#8200;");
			karakterler.Add("[[wikt:en:punctuation_space|Punctuation space]] (U+2008)");
		}

		Regex bulucu10 = new Regex(@"(\u200D)");
		if(bulucu10.Match(ArticleText).Success) { //Zero-width joiner
			ArticleText = bulucu10.Replace(ArticleText, "&zwj;");
			karakterler.Add("[[en:Zero-width joiner|Zero-width joiner]] (U+200D)");
		}

		Regex bulucu11 = new Regex(@"(\u202F)");
		if(bulucu11.Match(ArticleText).Success) { //Narrow no-break space
			ArticleText = bulucu11.Replace(ArticleText, "&#8239;");
			karakterler.Add("[[en:Non-breaking_space#Width_variation|Narrow no-break space]] (U+202F)");
		}

		Regex bulucu12 = new Regex(@"(\u00AD)");
		if(bulucu12.Match(ArticleText).Success) { //Soft hyphen	
			ArticleText = bulucu12.Replace(ArticleText, "&shy;");
			karakterler.Add("[[en:Soft hyphen|Soft hyphen]] (U+00AD)");
		}

		//değişiklik özeti
		string str = string.Empty;
		foreach(var item in karakterler) str = str + item + ", ";

		string str2 = string.Empty;
		foreach(var item2 in gereksiz) str2 = str2 + item2 + ", ";

		if(str!="") {
			str=str.Remove(str.Length-2);
			summary += "; Görünmez karakterler fark edilebilir kılındı: " + str;
		}
		if(!(str2=="")) {
			str2=str2.Remove(str2.Length-2);
			summary += "; Gereksiz görünmez karakterler kaldırıldı: "+str2;
		}

		return new Tuple<string, string>(ArticleText, summary);
	}
}