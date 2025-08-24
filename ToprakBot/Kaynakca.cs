using System;
using System.Text.RegularExpressions;

public class Kaynakca {
	//tr
	public static Tuple<string, string> Tr(string ArticleText) {
		string summary = "";

		//Ref tag bulur, sonuçta kaynak yoksa kaynakça da eklenmez
		Regex reftag = new Regex(@"<\s*?ref.*?>", RegexOptions.Singleline);

		//Başlığın yerini belirleme regex
		Match kat = Regex.Match(ArticleText, @"\[\[[kc]ategor[yi]\s*?:", RegexOptions.IgnoreCase);
		Match disbag = Regex.Match(ArticleText, @"==\s*?d[ıiİ][Şşs]\s*ba[Ğğg]lant[İiı]lar\s*?==", RegexOptions.IgnoreCase);
		Match taslak = Regex.Match(ArticleText, @"{{(?:.{1,}\-|)taslak\s*?}}", RegexOptions.IgnoreCase);

		//Sayfada kaynakça şablonu var mı regex - bulursa atlar
		Regex kaynaksablon = new Regex(@"\{\{\s*(kaynakça|reflist|references|reference|kaynak listesi|referanslar|refs)\s*(\||\}\})", RegexOptions.IgnoreCase);
		Regex reflisttag = new Regex(@"<\s*references\s*(\/|)\s*\>");

		//Sayfada kaynakça başlığı var mı regex
		Regex kaynakca1 = new Regex(@"(kaynak[çÇ]a|==\s*?kaynak\s*?==)", RegexOptions.IgnoreCase);
		Regex kaynakca2 = new Regex(@"references", RegexOptions.IgnoreCase);
		Regex kaynakca3 = new Regex(@"==\s*referans(lar)\s*==", RegexOptions.IgnoreCase);
		Regex kaynakca4 = new Regex(@"==\s*kaynaklar\s*==", RegexOptions.IgnoreCase);

		//Başlık var ama şablon yok regex - şablonu eklemek için
		Regex baslikvekat = new Regex(@"\={2,}\s*(?:Kaynakça|Kaynaklar)\s*\={2,}(?:\r\n){1,}\[\[(Kategori|Category)\:\s*", RegexOptions.Singleline);
		Regex baslikveson = new Regex(@"\={2,}\s*(?:Kaynakça|Kaynaklar)\s*\={2,}\s*?\Z", RegexOptions.IgnoreCase);
		Regex baslikvebaslik = new Regex(@"(\={2,}\s*(?:Kaynakça|Kaynaklar)\s*\={2,})\s*?\r\n(\={2,}.*?\={2,})\r\n", RegexOptions.IgnoreCase);

		if (reftag.Match(ArticleText).Success&&(!(kaynaksablon.Match(ArticleText).Success||reflisttag.Match(ArticleText).Success))) {
			if (!(kaynakca1.Match(ArticleText).Success||kaynakca2.Match(ArticleText).Success||kaynakca3.Match(ArticleText).Success||kaynakca4.Match(ArticleText).Success)) {
				if (disbag.Success) ArticleText = ArticleText.Insert(disbag.Index, "== Kaynakça ==\n{{kaynakça}}\r\n\n");
				else if (taslak.Success) ArticleText = ArticleText.Insert(taslak.Index, "\n== Kaynakça ==\n{{kaynakça}}\r\n\n");
				else if (kat.Success) ArticleText = ArticleText.Insert(kat.Index, "== Kaynakça ==\n{{kaynakça}}\r\n\n");
				else ArticleText += "\r\n\r\n== Kaynakça ==\n{{kaynakça}}";

				ArticleText = ArticleText.Replace("\n\n== Kaynakça ==", "\n== Kaynakça ==");
				summary += "; kaynakça başlığı ekleniyor";
			}

			if (baslikvekat.Match(ArticleText).Success||baslikveson.Match(ArticleText).Success) {
				Regex R1 = new Regex(@"\={2,}\s*(?:Kaynakça|Kaynaklar)\s*\={2,}", RegexOptions.IgnoreCase);
				ArticleText = R1.Replace(ArticleText, "== Kaynakça ==\r\n{{kaynakça}}");
				summary += "; kaynakça şablonu ekleniyor";
			} else if(baslikvebaslik.Match(ArticleText).Success) {
				ArticleText=baslikvebaslik.Replace(ArticleText, "$1\n{{kaynakça}}\n\n$2\n");
				summary+="; kaynakça şablonu ekleniyor";
			}

			Regex kaydz = new Regex(@"(\S\n{1})(\=\= Kaynakça ==)");
			ArticleText = kaydz.Replace(ArticleText, "$1\n$2");
		}
		return new Tuple<string, string>(ArticleText, summary);
	}

	//az
	public static Tuple<string, string> Az(string ArticleText) {
		string summary = "";

		//Ref tag bulur, sonuçta kaynak yoksa kaynakça da eklenmez
		Regex reftag = new Regex(@"<\s*?ref.*?>", RegexOptions.Singleline);
		
		//Başlığın yerini belirleme regex
		Match kat = Regex.Match(ArticleText, @"\[\[(?:category|kateqoriya)\s*?:", RegexOptions.IgnoreCase);
		Match disbag = Regex.Match(ArticleText, @"==\s*?xar[iİ]c[iİ]\s* ke[çÇ][iİ]dl[əƏ]r\s*?==", RegexOptions.IgnoreCase);
		Match taslak = Regex.Match(ArticleText, @"{{(?:.{1,}\-|)qaralama\s*?}}", RegexOptions.IgnoreCase);

		//Sayfada kaynakça şablonu var mı regex - bulursa atlar
		Regex kaynaksablon = new Regex(@"\{\{\s*([İi]stinadlar|[İi]stinad siyahısı|[İi]stinadlar siyahısı|reflist|references|reference|refs)\s*(\||\}\})", RegexOptions.IgnoreCase);
		Regex reflisttag = new Regex(@"<\s*references\s*(\/|)\s*\>");

		//Sayfada kaynakça başlığı var mı regex
		Regex kaynakca1 = new Regex(@"([iİ]stinadlar|==\s*?[iİ]stinad\s*?==)", RegexOptions.IgnoreCase);
		Regex kaynakca2 = new Regex(@"references", RegexOptions.IgnoreCase);

		//Başlık var ama şablon yok regex - şablonu eklemek için
		Regex baslikvekat = new Regex(@"\={2,}\s*[İi]stinadlar\s*\={2,}(?:\r\n){1,}\[\[(Kateqoriya|Category)\:\s*", RegexOptions.Singleline);
		Regex baslikveson = new Regex(@"\={2,}\s*[İi]stinadlar\s*\={2,}\s*?\Z", RegexOptions.IgnoreCase);
		Regex baslikvebaslik = new Regex(@"(\={2,}\s*[İi]stinadlar\s*\={2,})\s*?\r\n(\={2,}.*?\={2,})\r\n", RegexOptions.IgnoreCase);

		if (reftag.Match(ArticleText).Success&&(!(kaynaksablon.Match(ArticleText).Success||reflisttag.Match(ArticleText).Success))) {
			if (!(kaynakca1.Match(ArticleText).Success||kaynakca2.Match(ArticleText).Success)) {
				if (disbag.Success) ArticleText = ArticleText.Insert(disbag.Index, "== İstinadlar ==\n{{istinad siyahısı}}\r\n\n");
				else if (taslak.Success) ArticleText = ArticleText.Insert(taslak.Index, "\n== İstinadlar ==\n{{istinad siyahısı}}\r\n\n");
				else if (kat.Success) ArticleText = ArticleText.Insert(kat.Index, "== İstinadlar ==\n{{istinad siyahısı}}\r\n\n");
				else ArticleText += "\r\n\r\n== İstinadlar ==\n{{istinad siyahısı}}";

				ArticleText = ArticleText.Replace("\n\n== İstinadlar ==", "\n== İstinadlar ==");
				summary += "; istinadlar başlığı əlavə edildi";
			}

			if (baslikvekat.Match(ArticleText).Success||baslikveson.Match(ArticleText).Success) {
				Regex R1 = new Regex(@"\=\=\s*[İi]stinadlar\s*\=\=");
				ArticleText = R1.Replace(ArticleText, "== İstinadlar ==\r\n{{istinad siyahısı}}");
				summary += "; istinadlar şablonu əlavə edildi";
			} else if(baslikvebaslik.Match(ArticleText).Success) {
				ArticleText=baslikvebaslik.Replace(ArticleText, "$1\n{{istinad siyahısı}}\n\n$2\n");
				summary+="; istinadlar şablonu əlavə edildi";
			}

			Regex kaydz = new Regex(@"(\S\n{1})(\=\= İstinadlar ==)");
			ArticleText = kaydz.Replace(ArticleText, "$1\n$2");
		}
		return new Tuple<string, string>(ArticleText, summary);
	}

	//ka
	public static Tuple<string, string> Ka(string ArticleText) {
		string summary = "";

		//Ref tag bulur, sonuçta kaynak yoksa kaynakça da eklenmez
		Regex reftag = new Regex(@"<\s*?ref.*?>", RegexOptions.Singleline);
		
		//Başlığın yerini belirleme regex
		Match kat = Regex.Match(ArticleText, @"\[\[(?:category|კატეგორია)\s*?:", RegexOptions.IgnoreCase);

		//Sayfada kaynakça şablonu var mı regex - bulursa atlar
		Regex kaynaksablon = new Regex(@"\{\{\s*(სქოლიოს სია|reflist|references)\s*(\||\}\})", RegexOptions.IgnoreCase);
		Regex reflisttag = new Regex(@"<\s*references\s*(\/|)\s*\>");

		//Sayfada kaynakça başlığı var mı regex
		Regex kaynakca1 = new Regex(@"(სქოლიო|==\s*?სქოლიო\s*?==)");
		Regex kaynakca2 = new Regex(@"references", RegexOptions.IgnoreCase);

		//Başlık var ama şablon yok regex - şablonu eklemek için
		Regex baslikvekat = new Regex(@"\=\=\s*სქოლიო\s*\=\=\r\n\[\[(კატეგორია|Category)\:\s*", RegexOptions.Singleline);
		Regex baslikveson = new Regex(@"\={2,}\s*სქოლიო\s*\={2,}\s*?\Z", RegexOptions.IgnoreCase);
		Regex baslikvebaslik = new Regex(@"(\={2,}\s*სქოლიო\s*\={2,})\s*?\r\n(\={2,}.*?\={2,})\r\n", RegexOptions.IgnoreCase);

		if (reftag.Match(ArticleText).Success&&(!(kaynaksablon.Match(ArticleText).Success||reflisttag.Match(ArticleText).Success))) {
			if (!(kaynakca1.Match(ArticleText).Success||kaynakca2.Match(ArticleText).Success)) {
				if (kat.Success) ArticleText = ArticleText.Insert(kat.Index, "== სქოლიო ==\n{{სქოლიო}}\r\n\n");
				else ArticleText += "\r\n\r\n== სქოლიო ==\n{{სქოლიო}}";

				ArticleText = ArticleText.Replace("\n\n== სქოლიო ==", "\n== სქოლიო ==");
				summary += "; დაემატა სქოლიოს სექცია";
			}

			if (baslikvekat.Match(ArticleText).Success||baslikveson.Match(ArticleText).Success) {
				Regex R1 = new Regex(@"\={2,}\s*სქოლიო\s*\={2,}", RegexOptions.IgnoreCase);
				ArticleText = R1.Replace(ArticleText, "== სქოლიო ==\r\n{{სქოლიო}}");
				summary += "; დაემატა სქოლიოს თარგი";
			} else if(baslikvebaslik.Match(ArticleText).Success) {
				ArticleText=baslikvebaslik.Replace(ArticleText, "$1\n{{kaynakça}}\n\n$2\n");
				summary+="; დაემატა სქოლიოს თარგი";
			}

			Regex kaydz = new Regex(@"(\S\n{1})(\=\= სქოლიო ==)");
			ArticleText = kaydz.Replace(ArticleText, "$1\n$2");
		}
		return new Tuple<string, string>(ArticleText, summary);
	}
}