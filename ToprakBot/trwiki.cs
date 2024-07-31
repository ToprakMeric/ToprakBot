using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using WikiFunctions.API;
using WikiFunctions.Parse;
using WikiFunctions;
public class Trwiki {

	//trwiki de yeni oluşturulan sayfalar burada düzenlenir.
	public static async Task trwiki() {

		ApiEdit editor = new ApiEdit("https://" + ToprakBot.wiki + ".org/w/");
		ToprakBot.login(editor);

		List<string> titles;
		if(!ToprakBot.manual) titles = await ToprakBot.TitleList(ToprakBot.wiki);
		else {
			StreamReader reader;
			if(!ToprakBot.makine) reader = new StreamReader("D:\\AWB\\liste.txt");
			else reader = new StreamReader("C:\\Users\\Administrator\\Desktop\\liste.txt");

			using(reader) {
				titles = new List<string>();
				string line;
				while((line=reader.ReadLine())!=null) titles.Add(line);
			}
		}
		List<string> hatalıkorumaşablist = await ToprakBot.korumalist(ToprakBot.wiki);
		if(!ToprakBot.manual) titles.AddRange(hatalıkorumaşablist);

		List<string> wikiliste = await ToprakBot.wikiliste(ToprakBot.wiki);
		titles.AddRange(wikiliste);

		titles = titles.Distinct().ToList();
		int n = titles.Count;

		DateTime bugun = DateTime.Today;
		string bugunformat = bugun.ToString("yyyy-MM-dd");
		string filePath;
		if(!ToprakBot.makine) filePath = @"D:\AWB\log\tr\" + bugunformat + ".txt";
		else filePath = @"C:\Users\Administrator\Desktop\log\tr\" + bugunformat + ".txt";
		StreamWriter sw = File.AppendText(filePath);

		WikiRegexes.RenamedTemplateParameters = Parsers.LoadRenamedTemplateParameters(editor.Open("Project:AutoWikiBrowser/Rename template parameters"));
		WikiRegexes.TemplateRedirects = Parsers.LoadTemplateRedirects(editor.Open("Project:AutoWikiBrowser/Template redirects"));

		var loglist = new List<string>();

		var türlü = new ToprakBot();
		int i = -1;
		foreach(string sayfa in titles) {
			i++;
			string ArticleText = "", madde = "", ekozet = "";
			int NameSpace = türlü.NameSpaceDedector(sayfa);
			ArticleText = editor.Open(sayfa); //içeriği alıyor
			madde = ArticleText;

			Regex degistirmemeli = new Regex(@"\{\{\s*?(sil|çalışma|bekletmeli sil)\s*?(\||\}\})", RegexOptions.IgnoreCase);
			if((!degistirmemeli.Match(ArticleText).Success)&&(NameSpace == 0)) { //Sadece ana ad alanı, diğer ad alanı kodları için bkz VP:İA
				var tuple = tredit(ArticleText, sayfa);
				ArticleText = tuple.Item1;
				ekozet = tuple.Item2;
			}

			//Lüzumsuz koruma şablonu kaldır
			if (hatalıkorumaşablist.Contains(sayfa)) {
				bool protectionStatus = await ToprakBot.GetProtectionStatus(sayfa, ToprakBot.wiki);
				if (!protectionStatus) {
					Regex koruma = new Regex(@"(?:\/\*)?\s*(?:<noinclude>)?\s*\{\{\s*(koruma|koruma-|pp-|yarı koruma)[^{}]*?\s*?\}\}\s*(?:<\/noinclude>)?\s*(?:\*\/)?\s*", RegexOptions.IgnoreCase | RegexOptions.Multiline);
					ArticleText = koruma.Replace(ArticleText, "");
					ekozet += "; lüzumsuz koruma şablonu kaldırıldı";
				} else {
					Console.ForegroundColor = ConsoleColor.DarkYellow;
					Console.WriteLine(i+1 + "/" + n + ":\t" + sayfa + "\t");
					continue;
				}
			}

			if (ArticleText == madde) {
				if(NameSpace!=0) Console.ForegroundColor = ConsoleColor.Yellow;
				else Console.ForegroundColor = ConsoleColor.Red;
			} else {
				string summary = "Düzenlemeler ve imla" + ekozet;
				Console.ForegroundColor = ConsoleColor.Green;
				loglist.Add(sayfa);
				editor.Save(ArticleText, summary, true, WatchOptions.NoChange);
			}
			Console.WriteLine(i+1 + "/" + n + ":\t" + sayfa + "\t");
		}

		foreach(var item in loglist) sw.WriteLine(item);
		sw.Close();
	}

	//Türkçe Vikipedi'de ana ad alanında yaklaşık 1 milyon sayfa var (yönlendirme, anlam ayrımı vs. dahil)
	//Bunun listesini AWB ile alıp 5kliste.txt olarak oluşturdum. //to-do: liste bittiyse yeniden oluşturacak
	//Araç her gün 5 bin sayfa alarak onları tarıyor, bazı değişiklikler yapıyor.
	//Bütün viki yaklaşık her 200 günde bir taranmış olacak.
    public static async Task trwiki5k() {

		ApiEdit editor = new ApiEdit("https://" + ToprakBot.wiki + ".org/w/");
		ToprakBot.login(editor);

		List<string> titles;
		string dizin;
		if(!ToprakBot.makine) dizin = "D:\\AWB\\5kliste";
		else dizin = "C:\\Users\\Administrator\\Desktop\\5kliste";

		StreamReader reader = new StreamReader(dizin + ".txt");
		StreamWriter writer = new StreamWriter(dizin + "-temp.txt");

		using (reader) {
			titles = new List<string>();
			for (int iii = 0; iii < 5000; iii++) {
				string line = reader.ReadLine();
				if (line == null) break;
				titles.Add(line);
			}

			using (writer) {
                string line;
                while ((line = reader.ReadLine()) != null) writer.WriteLine(line);
            }
		}

		File.Delete(dizin + ".txt");
		File.Move(dizin + "-temp.txt", dizin + ".txt");

		titles = titles.Distinct().ToList();
		int n = titles.Count;

		DateTime bugun = DateTime.Today;
		string bugunformat = bugun.ToString("yyyy-MM-dd");
		string filePath;
		if(!ToprakBot.makine) filePath = @"D:\AWB\log\tr\" + bugunformat + ".txt";
		else filePath = @"C:\Users\Administrator\Desktop\log\tr\" + bugunformat + ".txt";
		StreamWriter sw = File.AppendText(filePath);

		WikiRegexes.RenamedTemplateParameters = Parsers.LoadRenamedTemplateParameters(editor.Open("Project:AutoWikiBrowser/Rename template parameters"));
		WikiRegexes.TemplateRedirects = Parsers.LoadTemplateRedirects(editor.Open("Project:AutoWikiBrowser/Template redirects"));

		var loglist = new List<string>();

		var türlü = new ToprakBot();
		int i = -1;
		foreach(string sayfa in titles) {
			i++;
			string ArticleText = "", madde = "", ekozet = "";
			int NameSpace = türlü.NameSpaceDedector(sayfa);
			ArticleText = editor.Open(sayfa); //içeriği alıyor
			madde = ArticleText;

			Regex degistirmemeli = new Regex(@"\{\{\s*?(sil|çalışma|bekletmeli sil)\s*?(\||\}\})", RegexOptions.IgnoreCase);
			if((!degistirmemeli.Match(ArticleText).Success)&&(NameSpace == 0)) { //Sadece ana ad alanı, diğer ad alanı kodları için bkz VP:İA
				
				//Yapılacak değişiklikler
				ArticleText = Upright.Main(ArticleText);

				var tuple = Kaynakca.Tr(ArticleText);
				ArticleText = tuple.Item1;
				ekozet += tuple.Item2;

				var tuple4 = NotListesi.Main(ArticleText);
				ArticleText = tuple4.Item1;
				ekozet += tuple4.Item2;

				Regex regex4 = new Regex(@"\|(ölüurl|ölü-url|bozukurl|bozukURL|deadurl|dead-url|url-status|urlstatus)\s*=\s*(no|live)");
				if(regex4.Match(ArticleText).Success) ArticleText = regex4.Replace(ArticleText, "|ölüurl=hayır");

				Regex regex5 = new Regex(@"\|\s*(ölüurl|ölü-url|bozukurl|bozukURL|deadurl|dead-url|url-status|urlstatus)\s*=\s*(yes|dead)");
				if(regex5.Match(ArticleText).Success) ArticleText = regex5.Replace(ArticleText, "|ölüurl=evet");

			}

			if (ArticleText == madde) {
				if(NameSpace!=0) Console.ForegroundColor = ConsoleColor.Yellow;
				else Console.ForegroundColor = ConsoleColor.Red;
			} else {
				string summary = "Düzenlemeler ve imla" + ekozet;
				Console.ForegroundColor = ConsoleColor.Green;
				loglist.Add(sayfa);
				editor.Save(ArticleText, summary, true, WatchOptions.NoChange);
			}
			Console.WriteLine(i+1 + "/" + n + ":\t" + sayfa + "\t");
		}

		foreach(var item in loglist) sw.WriteLine(item);
		sw.Close();
	}

	//Düzenlenecek yeni sayfa buraya düşüyor. Sayfada yapılacak değişiklikler burada yapılıyor.
	static public Tuple<string, string> tredit(string ArticleText, string ArticleTitle) {
		string summary = "";

		ArticleText = Baslik.Main(ArticleText);

		ArticleText = Upright.Main(ArticleText);

		var tuple = Kaynakca.Tr(ArticleText);
		ArticleText = tuple.Item1;
		summary += tuple.Item2;

		var tuple4 = NotListesi.Main(ArticleText);
		ArticleText = tuple4.Item1;
		summary += tuple4.Item2;

		ArticleText = KaynakCevir.Main(ArticleText);

		var tuple2 = YalinURL.Main(ArticleText);
		ArticleText = tuple2.Item1;
		summary += tuple2.Item2;

		var tuple3 = GorunmezKarakter.Main(ArticleText, ArticleTitle, "tr");
		ArticleText = tuple3.Item1;
		summary += tuple3.Item2;

		Regex regex1 = new Regex(@"(\[\[\s*?)([Iıİi]mage|[Ff]ile)(\s*?\:)");
		if(regex1.Match(ArticleText).Success) ArticleText = regex1.Replace(ArticleText, "$1Dosya$3");

		Regex regex2 = new Regex(@"\<\s*?references\s*?\/\s*?>");
		if(regex2.Match(ArticleText).Success) ArticleText = regex2.Replace(ArticleText, "{{kaynakça}}");

		Regex regex3 = new Regex(@"==\s*?Referanslar\s*?==", RegexOptions.IgnoreCase);
		if(regex3.Match(ArticleText).Success) ArticleText = regex3.Replace(ArticleText, "== Kaynakça ==");

		Regex regex4 = new Regex(@"\|(ölüurl|ölü-url|bozukurl|bozukURL|deadurl|dead-url|url-status|urlstatus)\s*=\s*(no|live)");
		if(regex4.Match(ArticleText).Success) ArticleText = regex4.Replace(ArticleText, "|ölüurl=hayır");

		Regex regex5 = new Regex(@"\|\s*(ölüurl|ölü-url|bozukurl|bozukURL|deadurl|dead-url|url-status|urlstatus)\s*=\s*(yes|dead)");
		if(regex5.Match(ArticleText).Success) ArticleText = regex5.Replace(ArticleText, "|ölüurl=evet");

		Regex regex6 = new Regex(@"(\|\s*?(pages|sayfalar)\s*?\=\s*?\d{1,5}\s*?)[\u2012\u2013\u2014\u2015\u2010](\s*?\d{1,5}\s*?)(\||\}\})");
		if(regex6.Match(ArticleText).Success) ArticleText = regex6.Replace(ArticleText, "$1-$3$4");

		Regex regex7 = new Regex(@"\|\s*thumb\s*(\||]])");
		if(regex7.Match(ArticleText).Success) ArticleText = regex7.Replace(ArticleText, "|küçükresim$1");

		Regex regex8 = new Regex(@"\=\=(\s*?)Ayrıca bkz(\.|)(\s*?)\=\=", RegexOptions.IgnoreCase);
		if(regex8.Match(ArticleText).Success) ArticleText = regex8.Replace(ArticleText, "==$1Ayrıca bakınız$3==");

		Regex regex9 = new Regex(@"(\[\[\s*?)(Category|Kategori)(\s*?\:\s*?)Articles with 'species' microformats(\s*?\]\])", RegexOptions.IgnoreCase);
		if(regex9.Match(ArticleText).Success) ArticleText = regex9.Replace(ArticleText, "$1$2$3Tür mikroformatı olan maddeler$4");

		Regex regex10 = new Regex(@"(\[\[\s*?)(Category|Kategori)(\s*?\:\s*?)Articles with hCards(\s*?\]\])", RegexOptions.IgnoreCase);
		if(regex10.Match(ArticleText).Success) ArticleText = regex10.Replace(ArticleText, "$1$2$3hCards'lı maddeler$4");

		Regex regex11 = new Regex(@"(\[\[\s*?)(Category|Kategori)(\s*?\:\s*?)Articles with short description(\s*?\]\])", RegexOptions.IgnoreCase);
		if(regex11.Match(ArticleText).Success) ArticleText = regex11.Replace(ArticleText, "$1$2$3Kısa açıklamalı maddeler$4");

		Regex regex12 = new Regex(@"(\[\[\s*?)(Category|Kategori)(\s*?\:\s*?)Short description is different from Wikidata(\s*?\]\])", RegexOptions.IgnoreCase);
		if(regex12.Match(ArticleText).Success) ArticleText = regex12.Replace(ArticleText, "");

		Regex regex13 = new Regex(@"(\[\[\s*?(category|kategori)\s*?\:.*?\]\])\n{2,}(\[\[\s*?(category|kategori)\s*?\:.*?\]\])", RegexOptions.IgnoreCase);
		if(regex13.Match(ArticleText).Success) ArticleText = regex13.Replace(ArticleText, "$1\n$3");

		Regex regex14 = new Regex(@"(\[\[\s*?)category(\s*?\:.*?\]\])", RegexOptions.IgnoreCase);
		if(regex14.Match(ArticleText).Success) ArticleText = regex14.Replace(ArticleText, "$1Kategori$2");

		ArticleText = Parsers.TemplateRedirects(ArticleText, WikiRegexes.TemplateRedirects);
		ArticleText = Parsers.RenameTemplateParameters(ArticleText, WikiRegexes.RenamedTemplateParameters);

		//AWB düzeltmeleri
		Parsers parser = new Parsers(500, false);
		ArticleText = Parsers.FixTemperatures(ArticleText);
		ArticleText = parser.FixBrParagraphs(ArticleText).Trim();
		ArticleText = Parsers.FixLinkWhitespace(ArticleText, ArticleTitle);
		ArticleText = Parsers.FixSyntax(ArticleText);
		ArticleText = Parsers.FixCategories(ArticleText);
		ArticleText = Parsers.FixImages(ArticleText);
		ArticleText = Parsers.SimplifyLinks(ArticleText);
		ArticleText = Parsers.FixMainArticle(ArticleText);
		ArticleText = Parsers.FixReferenceListTags(ArticleText);
		ArticleText = Parsers.FixEmptyLinksAndTemplates(ArticleText);
		ArticleText = Parsers.SimplifyReferenceTags(ArticleText);
		ArticleText = Parsers.FixReferenceTags(ArticleText);
		ArticleText = Parsers.DuplicateNamedReferences(ArticleText);
		ArticleText = Parsers.DuplicateUnnamedReferences(ArticleText);
		ArticleText = Parsers.SameRefDifferentName(ArticleText);
		ArticleText = Parsers.RefsAfterPunctuation(ArticleText);
		ArticleText = Parsers.ReorderReferences(ArticleText);
		ArticleText = parser.SortMetaData(ArticleText, ArticleTitle);
		ArticleText = ArticleText.Trim();
		//ArticleText = parser.FixNonBreakingSpaces(ArticleText); //AWB bug bkz w.wiki/9p6C
		//ArticleText = parser.FixDatesA(ArticleText).Trim();
		//ArticleText = Parsers.FixCitationTemplates(ArticleText);
		
		return new Tuple<string, string>(ArticleText, summary);
	}
}
