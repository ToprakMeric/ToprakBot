using System;
using System.Globalization;
using System.Text.RegularExpressions;

public class YalinURL {
	public static Tuple<string, string> Main(string ArticleText) {
		string summary = "";
		int bos = 0;

		Regex sablonkontrol = new Regex(@"\{\{\s*?(Şablon\:|)\s*?(Yalın(_|\s)URL\'leri(_|\s)temizle|Satır(_|\s)içi(_|\s)yalın(_|\s)URL|kaynakları(_|\s)düzenle|düzenle|çoklu(_|\s)sorun)", RegexOptions.IgnoreCase);
		Regex yalinurl = new Regex(@"<ref[^>]*?\>\s*\[?\s*https?:[^>< \|\[\]]+\s*\]?\s*<\s*\/\s*ref", RegexOptions.IgnoreCase);
		Regex satirici = new Regex(@"(<ref[^>]*?\>)\s*\[?\s*(https?:[^>< \|\[\]]+)\s*\]?\s*(<\s*\/\s*ref)", RegexOptions.IgnoreCase);

		foreach(Match match in yalinurl.Matches(ArticleText)) bos++;

		CultureInfo ci = new CultureInfo("tr-TR");
		if((bos!=0)&&!(sablonkontrol.Match(ArticleText).Success)&&(yalinurl.Match(ArticleText).Success)) {
			if(bos>=3) ArticleText = "{{Yalın URL'leri temizle|tarih=" + DateTime.UtcNow.ToString("MMMM yyyy", ci) + "}}\n" + ArticleText;
			else ArticleText = satirici.Replace(ArticleText, "$1$2 {{Satır içi yalın URL|tarih=" + DateTime.UtcNow.ToString("MMMM yyyy", ci) + "}}$3");
			summary += "; yalın URL bakım şablonu eklendi";
		}
		return new Tuple<string, string>(ArticleText, summary);
	}
}