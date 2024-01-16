using System;
using System.Text.RegularExpressions;
using WikiFunctions;

public class NotListesi {
	public static Tuple<string, string> Main(string ArticleText) {
		string summary = "";
		Regex şablon = new Regex(@"{{\s*?(efn|adn|adn\-la|adş)\s*?(\||\})", RegexOptions.IgnoreCase);
		Regex notlistesi = new Regex(@"{{\s*?(not\slistesi|notlar|notes|notelist)\s*?(\||\})", RegexOptions.IgnoreCase);
		Regex notbaslik = new Regex(@"==\s*(not listesi|notlar|note list|notes|notelist)\s*==", RegexOptions.IgnoreCase);
		Regex refgroup = new Regex(@"(\<|{{)\s*?(references|ref|kaynakça)\s*?\|?(group|grup)", RegexOptions.IgnoreCase);
	
		if (şablon.Match(ArticleText).Success) {
			if (!notlistesi.Match(ArticleText).Success&&!refgroup.Match(ArticleText).Success&&!notbaslik.Match(ArticleText).Success) {
			
				Match kategori = Regex.Match(ArticleText, @"\[\[\s*?[kc]ategor[iy]\s*?\:", RegexOptions.IgnoreCase);
				Match dışbağ = Regex.Match(ArticleText, @"\=\=\s*?Dış\sbağlantılar\s*?\=\=", RegexOptions.IgnoreCase);
				Match kaynakça = Regex.Match(ArticleText, @"\=\=\s*?Kaynakça\s*?\=\=", RegexOptions.IgnoreCase);
				Match taslak = Regex.Match(ArticleText, @"{{.*(-|)taslak(\s*|)}}", RegexOptions.IgnoreCase);
			
				if (kaynakça.Success) ArticleText = ArticleText.Insert(kaynakça.Index, "== Not listesi ==\n{{not listesi}}\r\n\n");
				else if (dışbağ.Success) ArticleText = ArticleText.Insert(dışbağ.Index, "== Not listesi ==\n{{not listesi}}\r\n\n");
				else if (taslak.Success) ArticleText = ArticleText.Insert(taslak.Index, "== Not listesi ==\n{{not listesi}}\r\n\n");
				else if (kategori.Success) ArticleText = ArticleText.Insert(kategori.Index, "== Not listesi ==\n{{not listesi}}\r\n\n");
				else ArticleText += "\r\n\r\n== Not listesi ==\n{{not listesi}}";
				summary = "; not listesi ekleniyor";

				Regex notdz = new Regex(@"(\S\n{1})(\=\= Not listesi ==)");
				ArticleText = notdz.Replace(ArticleText, "$1\n$2");
			}
		}
		return new Tuple<string, string>(ArticleText, summary);
	}
}