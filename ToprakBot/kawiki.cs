using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using WikiFunctions.API;
using WikiFunctions.Parse;
using WikiFunctions;

public class Kawiki {
	public static ApiEdit editor = new ApiEdit("https://" + ToprakBot.wiki3 + ".org/w/");

	//kawiki de yeni oluşturulan sayfalar burada düzenlenir.
	public static async Task kawiki() {
		try {
			ToprakBot.login(editor);
		} catch(Exception ex) { ToprakBot.LogException("K01", ex); }

		List<string> titles;
		if (!ToprakBot.manual) titles = await ToprakBot.TitleList(ToprakBot.wiki3);
		else titles = new List<string>();

		//List<string> wikiliste = await ToprakBot.wikiliste(ToprakBot.wiki3);
		//titles.AddRange(wikiliste);

		titles = titles.Distinct().ToList();
		int n = titles.Count;

		DateTime bugun = DateTime.Today;
		string bugunformat = bugun.ToString("yyyy-MM-dd");
		string filePath;
		if (!ToprakBot.makine) filePath = @"D:\AWB\log\ka\" + bugunformat + ".txt";
		else filePath = @"C:\Users\Administrator\Desktop\log\ka\" + bugunformat + ".txt";
		StreamWriter sw = File.AppendText(filePath);

		//WikiRegexes.RenamedTemplateParameters = Parsers.LoadRenamedTemplateParameters(editor.Open("Project:AutoWikiBrowser/Rename template parameters"));
		//WikiRegexes.TemplateRedirects = Parsers.LoadTemplateRedirects(editor.Open("Project:AutoWikiBrowser/Template redirects"));

		var loglist = new List<string>();

		var türlü = new ToprakBot();
		int i = -1;
		foreach(string sayfa in titles) {
			i++;
			string ArticleText = "", madde = "", ekozet = "";
			int NameSpace = await ToprakBot.NameSpaceDedector(sayfa);
			
			try {
				ArticleText = editor.Open(sayfa); //içeriği alıyor
			} catch(Exception ex) { ToprakBot.LogException("K02", ex); continue; }
			madde = ArticleText;

			Regex degistirmemeli = new Regex(@"\{\{\s*?(delete|წასაშლელი)\s*?(\||\}\})", RegexOptions.IgnoreCase);
			if ((!degistirmemeli.Match(ArticleText).Success)&&(NameSpace == 0)) { //Sadece ana ad alanı, diğer ad alanı kodları için bkz Special:NamespaceInfo
				var tuple = kaedit(ArticleText, sayfa);
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
				} catch(Exception ex) { ToprakBot.LogException("K03", ex); continue; }
			}
			Console.WriteLine(i+1 + "/" + n + ":\t" + sayfa + "\t");
		}

		foreach(var item in loglist) sw.WriteLine(item);
		sw.Close();
	}

	//Düzenlenecek yeni sayfa buraya düşüyor. Gerekli düzenlemeler yapılıp geri gidiyor.
	public static Tuple<string, string> kaedit(string ArticleText, string ArticleTitle) {
		string initialText = ArticleText;
		string summary = "სხვადასხვა სქოლიო შესწორებები";

		var tuple = Kaynakca.Ka(ArticleText);
		ArticleText = tuple.Item1;
		summary += tuple.Item2;

		Regex regex2 = new Regex(@"\<\s*?references\s*?\/\s*?>");
		if(regex2.Match(ArticleText).Success) ArticleText = regex2.Replace(ArticleText, "{{სქოლიო}}");

		Parsers parser = new Parsers(500, false);
		ArticleText = Parsers.DuplicateNamedReferences(ArticleText);
		ArticleText = Parsers.DuplicateUnnamedReferences(ArticleText);
		ArticleText = Parsers.SameRefDifferentName(ArticleText);
		ArticleText = Parsers.RefsAfterPunctuation(ArticleText);
		ArticleText = Parsers.ReorderReferences(ArticleText);

		if (initialText != ArticleText) {
			ArticleText = Parsers.SimplifyReferenceTags(ArticleText);
			ArticleText = Parsers.FixReferenceTags(ArticleText);
		}

		return new Tuple<string, string>(ArticleText, summary);
	}

}