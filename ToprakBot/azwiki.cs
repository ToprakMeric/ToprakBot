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

public class Azwiki {
	public static ApiEdit editor = new ApiEdit("https://" + ToprakBot.wiki2 + ".org/w/");

	//azwiki de yeni oluşturulan sayfalar burada düzenlenir.
	public static async Task azwiki() {
		
		try {
			ToprakBot.login(editor);
		} catch(Exception ex) { ToprakBot.LogException("A01", ex); }

		List<string> titles;
		if (!ToprakBot.manual) titles = await ToprakBot.TitleList(ToprakBot.wiki2);
		else titles = new List<string>();

		List<string> wikiliste = await ToprakBot.wikiliste(ToprakBot.wiki2);
		titles.AddRange(wikiliste);

		titles = titles.Distinct().ToList();
		int n = titles.Count;

		DateTime bugun = DateTime.Today;
		string bugunformat = bugun.ToString("yyyy-MM-dd");
		string filePath;
		if (!ToprakBot.makine) filePath = @"D:\AWB\log\az\" + bugunformat + ".txt";
		else filePath = @"C:\Users\Administrator\Desktop\log\az\" + bugunformat + ".txt";
		StreamWriter sw = File.AppendText(filePath);

		WikiRegexes.RenamedTemplateParameters = Parsers.LoadRenamedTemplateParameters(editor.Open("Project:AutoWikiBrowser/Rename template parameters"));
		WikiRegexes.TemplateRedirects = Parsers.LoadTemplateRedirects(editor.Open("Project:AutoWikiBrowser/Template redirects"));

		var loglist = new List<string>();

		var türlü = new ToprakBot();
		int i = -1;
		foreach(string sayfa in titles) {
			i++;
			string ArticleText = "", madde = "", ekozet = "";
			int NameSpace = await ToprakBot.NameSpaceDedector(sayfa);
			
			try {
				ArticleText = editor.Open(sayfa); //içeriği alıyor
			} catch(Exception ex) { ToprakBot.LogException("A02", ex); }
			madde = ArticleText;

			Regex degistirmemeli = new Regex(@"\{\{\s*?(sil|[İi]ş gedir)\s*?(\||\}\})", RegexOptions.IgnoreCase);
			if ((!degistirmemeli.Match(ArticleText).Success)&&(NameSpace == 0)) { //Sadece ana ad alanı, diğer ad alanı kodları için bkz Special:NamespaceInfo
				var tuple = azedit(ArticleText, sayfa);
				ArticleText = tuple.Item1;
				ekozet = tuple.Item2;
			}

			if (ArticleText == madde) {
				if (NameSpace!=0) Console.ForegroundColor = ConsoleColor.Yellow;
				else Console.ForegroundColor = ConsoleColor.Red;
			} else {
				string summary = "" + ekozet;
				Console.ForegroundColor = ConsoleColor.Green;
				loglist.Add(sayfa);
				
				try { 
					editor.Save(ArticleText, summary, true, WatchOptions.NoChange);
				} catch(Exception ex) { ToprakBot.LogException("A03", ex); }
			}
			Console.WriteLine(i+1 + "/" + n + ":\t" + sayfa + "\t");
		}

		foreach(var item in loglist) sw.WriteLine(item);
		sw.Close();
	}

	//Düzenlenecek yeni sayfa buraya düşüyor. Gerekli düzenlemeler yapılıp geri gidiyor.
	public static Tuple<string, string> azedit(string ArticleText, string ArticleTitle) {
		string summary = "";

		var tuple = Kaynakca.Az(ArticleText);
		ArticleText = tuple.Item1;
		summary += tuple.Item2;

		var tuple3 = GorunmezKarakter.Main(ArticleText, ArticleTitle, "az");
		ArticleText = tuple3.Item1;
		summary += tuple3.Item2;

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