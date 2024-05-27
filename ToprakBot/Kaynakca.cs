using System;
using System.Text.RegularExpressions;

public class Kaynakca {
	public static Tuple<string, string> Tr(string ArticleText) { //tr
		string summary = "";

		Match kat = Regex.Match(ArticleText, @"\[\[[KkCc]ategor[yi]\s*?:");
		Match disbag = Regex.Match(ArticleText, @"==\s*?d[ıiİ][Şşs]\s*ba[Ğğg]lant[İiı]lar\s*?==", RegexOptions.IgnoreCase);
		Match taslak = Regex.Match(ArticleText, @"{{(?:.{1,}\-|)[Tt]aslak\s*?}}");

		Regex one = new Regex(@"(kaynak[çÇ]a|==\s*?kaynak\s*?==)", RegexOptions.IgnoreCase);
		Regex two = new Regex(@"(R|r)eferences", RegexOptions.IgnoreCase);
		Regex three = new Regex(@"==\s*(R|r)eferans(lar)\s*==", RegexOptions.IgnoreCase);
		Regex four = new Regex(@"{{\s*reflist", RegexOptions.IgnoreCase);
		Regex five = new Regex(@"==\s*[Kk]aynaklar\s*==", RegexOptions.IgnoreCase);

		Regex six = new Regex(@"\=\=\s*Kaynakça\s*\=\=\r\n\[\[(Kategori|Category)\:\s*", RegexOptions.Singleline);
		Regex seven = new Regex(@"\=\=\s*Kaynakça\s*\=\=\r\n\r\n\[\[(Kategori|Category)\:\s*", RegexOptions.Singleline);

		Regex eight = new Regex(@"\{\{\s*([Kk]aynakça|[Rr]eflist|[Rr]eferences|[Rr]eference|[Kk]aynak listesi|[Rr]eferanslar|[Rr]efs)\s*(\||\}\})");
		Regex nine = new Regex(@"<\s*references\s*(\/|)\s*\>");

		if(ArticleText.Contains("<ref")) {
			if(!(eight.Match(ArticleText).Success||nine.Match(ArticleText).Success)) {
				if(!(one.Match(ArticleText).Success||two.Match(ArticleText).Success||three.Match(ArticleText).Success||four.Match(ArticleText).Success||five.Match(ArticleText).Success)) {
					if(disbag.Success) ArticleText = ArticleText.Insert(disbag.Index, "== Kaynakça ==\n{{kaynakça}}\r\n\n");
					else if(taslak.Success) ArticleText = ArticleText.Insert(taslak.Index, "\n== Kaynakça ==\n{{kaynakça}}\r\n\n");
					else if(kat.Success) ArticleText = ArticleText.Insert(kat.Index, "== Kaynakça ==\n{{kaynakça}}\r\n\n");
					else ArticleText += "\r\n\r\n== Kaynakça ==\n{{kaynakça}}";

					ArticleText = ArticleText.Replace("\n\n== Kaynakça ==", "\n== Kaynakça ==");
					summary += "; kaynakça başlığı ekleniyor";
				}

				if(six.Match(ArticleText).Success) {
					Regex R1 = new Regex(@"\=\=\s*Kaynakça\s*\=\=");
					ArticleText = R1.Replace(ArticleText, "== Kaynakça ==\r\n{{kaynakça}}\n");
					summary += "; kaynakça şablonu ekleniyor";
				} else if(seven.Match(ArticleText).Success) {
					Regex R2 = new Regex(@"\=\=\s*Kaynakça\s*\=\=");
					ArticleText = R2.Replace(ArticleText, "== Kaynakça ==\r\n{{kaynakça}}");
					summary += "; kaynakça şablonu ekleniyor";
				}

				Regex kaydz = new Regex(@"(\S\n{1})(\=\= Kaynakça ==)");
				ArticleText = kaydz.Replace(ArticleText, "$1\n$2");
			}
		}
		return new Tuple<string, string>(ArticleText, summary);
	}

	public static Tuple<string, string> Az(string ArticleText) { //az
		string summary = "";

		Match kat = Regex.Match(ArticleText, @"\[\[(?:[Cc]ategory|[Kk]ateqoriya)\s*?:");
		Match disbag = Regex.Match(ArticleText, @"==\s*?xar[iİ]c[iİ]\s* ke[çÇ][iİ]dl[əƏ]r\s*?==", RegexOptions.IgnoreCase);
		Match taslak = Regex.Match(ArticleText, @"{{(?:.{1,}\-|)[Qq]aralama\s*?}}");

		Regex one = new Regex(@"([iİ]stinadlar|==\s*?[iİ]stinad\s*?==)", RegexOptions.IgnoreCase);
		Regex two = new Regex(@"(R|r)eferences", RegexOptions.IgnoreCase);
		//Regex three = new Regex(@"==\s*(R|r)eferans(lar)\s*==", RegexOptions.IgnoreCase);
		Regex four = new Regex(@"{{\s*reflist", RegexOptions.IgnoreCase);
		//Regex five = new Regex(@"==\s*[Kk]aynaklar\s*==", RegexOptions.IgnoreCase);

		Regex six = new Regex(@"\=\=\s*İstinadlar\s*\=\=\r\n\[\[(Kateqoriya|Category)\:\s*", RegexOptions.Singleline);
		Regex seven = new Regex(@"\=\=\s*İstinadlar\s*\=\=\r\n\r\n\[\[(Kateqoriya|Category)\:\s*", RegexOptions.Singleline);

		Regex eight = new Regex(@"\{\{\s*([İi]stinadlar|[İi]stinad siyahısı|[İi]stinadlar siyahısı|[Rr]eflist|[Rr]eferences|[Rr]eference|[Rr]efs)\s*(\||\}\})");
		Regex nine = new Regex(@"<\s*references\s*(\/|)\s*\>");

		if(ArticleText.Contains("<ref")) {
			if(!(eight.Match(ArticleText).Success||nine.Match(ArticleText).Success)) {
				if(!(one.Match(ArticleText).Success||two.Match(ArticleText).Success||four.Match(ArticleText).Success)) {
					if(disbag.Success) ArticleText = ArticleText.Insert(disbag.Index, "== İstinadlar ==\n{{istinad siyahısı}}\r\n\n");
					else if(taslak.Success) ArticleText = ArticleText.Insert(taslak.Index, "\n== İstinadlar ==\n{{istinad siyahısı}}\r\n\n");
					else if(kat.Success) ArticleText = ArticleText.Insert(kat.Index, "== İstinadlar ==\n{{istinad siyahısı}}\r\n\n");
					else ArticleText += "\r\n\r\n== İstinadlar ==\n{{istinad siyahısı}}";

					ArticleText = ArticleText.Replace("\n\n== İstinadlar ==", "\n== İstinadlar ==");
					summary += "; istinadlar başlığı əlavə edildi";
				}

				if(six.Match(ArticleText).Success) {
					Regex R1 = new Regex(@"\=\=\s*İstinadlar\s*\=\=");
					ArticleText = R1.Replace(ArticleText, "== İstinadlar ==\r\n{{istinad siyahısı}}\n");
					summary += "; istinadlar şablonu əlavə edildi";
				} else if(seven.Match(ArticleText).Success) {
					Regex R2 = new Regex(@"\=\=\s*İstinadlar\s*\=\=");
					ArticleText = R2.Replace(ArticleText, "== İstinadlar ==\r\n{{istinad siyahısı}}");
					summary += "; istinadlar şablonu əlavə edildi";
				}

				Regex kaydz = new Regex(@"(\S\n{1})(\=\= İstinadlar ==)");
				ArticleText = kaydz.Replace(ArticleText, "$1\n$2");
			}
		}
		return new Tuple<string, string>(ArticleText, summary);
	}
}