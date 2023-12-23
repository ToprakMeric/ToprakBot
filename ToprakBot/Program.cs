//ToprakBot
//Yazar: ToprakM
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using WikiFunctions.API;
using WikiFunctions.Parse;
using WikiFunctions;
using Newtonsoft.Json;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.Net.Http;

public class ToprakBot {
	static async Task<List<string>> TitleList() {
		List<string> titles = new List<string>();

		for(int i = 0;i<30;i++) {
			string apiUrl = "https://tr.wikipedia.org/w/api.php?action=query&formatversion=2&list=recentchanges&rcdir=older&rcend="+DateTime.Now.AddDays(-i-1).ToString("yyyy-MM-dd")+"T00:00:00.000Z&rclimit=max&rcnamespace=0&rcprop=title&rcstart="+DateTime.Now.AddDays(-i).ToString("yyyy-MM-dd")+"T00:00:00.000Z&rctype=new&format=json";

			using(HttpClient client = new HttpClient()) {
				string json = await client.GetStringAsync(apiUrl);

				var jsonObject = JsonConvert.DeserializeObject<JObject>(json);
				var recentChanges = jsonObject["query"]["recentchanges"];

				foreach(var change in recentChanges) {
					string title = change["title"].ToString();
					titles.Add(title);
				}
			}
		}
		return titles;
	}
	public static async Task Main(string[] args) {
		bool manual = false; //Sayfa listesini false ise API'den, true ise elle eklenmiş dosyadan alır
		bool makine = false; //Nerede çalışlacağına göre dosya konumlarını ayarlar, true ise makinede false ise pc de

		ApiEdit editor = new ApiEdit("https://tr.wikipedia.org/w/");

		string password;
		if(!makine) password=File.ReadAllText("D:\\AWB\\password.txt");
		else password=File.ReadAllText("C:\\Users\\Administrator\\Desktop\\password.txt");
		editor.Login("ToprakBot@aws", password);

		List<string> titles;
		if(!manual) titles=await TitleList();
		else {
			StreamReader reader;
			if(!makine) reader=new StreamReader("D:\\AWB\\liste.txt");
			else reader=new StreamReader("C:\\Users\\Administrator\\Desktop\\liste.txt");

			using(reader) {
				titles=new List<string>();
				string line;
				while((line=reader.ReadLine())!=null) titles.Add(line);
			}
		}

		int n = titles.Count;

		DateTime bugun = DateTime.Today;
		string bugunformat = bugun.ToString("yyyy-MM-dd");
		string filePath;
		if(!makine) filePath=@"D:\AWB\log\"+bugunformat+".txt";
		else filePath=@"C:\Users\Administrator\Desktop\log\"+bugunformat+".txt";
		StreamWriter sw = File.AppendText(filePath);

		WikiRegexes.RenamedTemplateParameters=Parsers.LoadRenamedTemplateParameters(editor.Open("Project:AutoWikiBrowser/Rename template parameters"));
		WikiRegexes.TemplateRedirects=Parsers.LoadTemplateRedirects(editor.Open("Project:AutoWikiBrowser/Template redirects"));

		var loglist = new List<string>();

		int i = -1;
		foreach(string sayfa in titles) {
			i++;
			string ArticleText = editor.Open(sayfa); //içeriği alıyor
			string madde = ArticleText;
			string ekozet = "";

			Regex degistirmemeli = new Regex(@"\{\{\s*?(sil|çalışma|bekletmeli sil)\s*?(\||\}\})", RegexOptions.IgnoreCase);
			if(!degistirmemeli.Match(ArticleText).Success) {
				var tuple = Edit(ArticleText, sayfa);
				ArticleText=tuple.Item1;
				ekozet=tuple.Item2;

			}

			if(ArticleText==madde) Console.ForegroundColor=ConsoleColor.Red;
			else {
				string summary = "Düzenlemeler ve imla"+ekozet;
				Console.ForegroundColor=ConsoleColor.Green;
				string line = $"{sayfa}";
				loglist.Add(sayfa);
				editor.Save(ArticleText, summary, true, WatchOptions.NoChange);
			}
			Console.WriteLine(i+1+"/"+n+":\t"+sayfa+"\t");
		}

		foreach(var item in loglist) sw.WriteLine(item);
		sw.Close();
		//Console.ReadKey();
		await Task.Delay(TimeSpan.FromSeconds(1));
	}

	static public Tuple<string, string> Edit(string ArticleText, string ArticleTitle) {
		string summary = "";

		ArticleText=Upright.Main(ArticleText);

		var tuple = Kaynakca.Main(ArticleText);
		ArticleText=tuple.Item1;
		summary+=tuple.Item2;

		ArticleText=KaynakCevir.Main(ArticleText);

		var tuple2 = YalinURL.Main(ArticleText);
		ArticleText=tuple2.Item1;
		summary+=tuple2.Item2;

		var tuple3 = GorunmezKarakter.Main(ArticleText, ArticleTitle);
		ArticleText=tuple3.Item1;
		summary+=tuple3.Item2;

		//ArticleText = KaynakNoktalama.Main(ArticleText);

		Regex regex1 = new Regex(@"(\[\[\s*?)([Iıİi]mage|[Ff]ile)(\s*?\:)");
		if(regex1.Match(ArticleText).Success) ArticleText=regex1.Replace(ArticleText, "$1Dosya$3");

		Regex regex2 = new Regex(@"\<\s*?references\s*?\/\s*?>");
		if(regex2.Match(ArticleText).Success) ArticleText=regex2.Replace(ArticleText, "{{kaynakça}}");

		Regex regex3 = new Regex(@"==\s*?Referanslar\s*?==", RegexOptions.IgnoreCase);
		if(regex3.Match(ArticleText).Success) ArticleText=regex3.Replace(ArticleText, "== Kaynakça ==");

		Regex regex4 = new Regex(@"\|(ölüurl|ölü-url|bozukurl|bozukURL|deadurl|dead-url)\s*=\s*no");
		if(regex4.Match(ArticleText).Success) ArticleText=regex4.Replace(ArticleText, "|ölüurl=hayır");

		Regex regex5 = new Regex(@"\|\s*(ölüurl|ölü-url|bozukurl|bozukURL|deadurl|dead-url)\s*=\s*yes");
		if(regex5.Match(ArticleText).Success) ArticleText=regex5.Replace(ArticleText, "|ölüurl=evet");

		Regex regex6 = new Regex(@"\|\s*url-status\s*=\s*dead");
		if(regex6.Match(ArticleText).Success) ArticleText=regex6.Replace(ArticleText, "|ölüurl=evet");

		Regex regex7 = new Regex(@"\|\s*thumb\s*(\||]])");
		if(regex7.Match(ArticleText).Success) ArticleText=regex7.Replace(ArticleText, "|küçükresim$1");

		Regex regex8 = new Regex(@"\=\=(\s*?)Ayrıca bkz(\.|)(\s*?)\=\=", RegexOptions.IgnoreCase);
		if(regex8.Match(ArticleText).Success) ArticleText=regex8.Replace(ArticleText, "==$1Ayrıca bakınız$3==");

		Regex regex9 = new Regex(@"(\[\[\s*?)(Category|Kategori)(\s*?\:\s*?)Articles with 'species' microformats(\s*?\]\])", RegexOptions.IgnoreCase);
		if(regex9.Match(ArticleText).Success) ArticleText=regex9.Replace(ArticleText, "$1$2$3Tür mikroformatı olan maddeler$4");

		Regex regex10 = new Regex(@"(\[\[\s*?)(Category|Kategori)(\s*?\:\s*?)Articles with hCards(\s*?\]\])", RegexOptions.IgnoreCase);
		if(regex10.Match(ArticleText).Success) ArticleText=regex10.Replace(ArticleText, "$1$2$3hCards'lı maddeler$4");

		Regex regex11 = new Regex(@"(\[\[\s*?)(Category|Kategori)(\s*?\:\s*?)Articles with short description(\s*?\]\])", RegexOptions.IgnoreCase);
		if(regex11.Match(ArticleText).Success) ArticleText=regex11.Replace(ArticleText, "$1$2$3Kısa açıklamalı maddeler$4");

		Regex regex12 = new Regex(@"(\[\[\s*?)(Category|Kategori)(\s*?\:\s*?)Short description is different from Wikidata(\s*?\]\])", RegexOptions.IgnoreCase);
		if(regex12.Match(ArticleText).Success) ArticleText=regex12.Replace(ArticleText, "");

		ArticleText=Parsers.TemplateRedirects(ArticleText, WikiRegexes.TemplateRedirects);
		ArticleText=Parsers.RenameTemplateParameters(ArticleText, WikiRegexes.RenamedTemplateParameters);

		//AWB düzeltmeleri
		Parsers parser = new Parsers(500, false);
		ArticleText=Parsers.FixTemperatures(ArticleText);
		ArticleText=parser.FixNonBreakingSpaces(ArticleText);
		ArticleText=parser.FixBrParagraphs(ArticleText).Trim();
		ArticleText=Parsers.FixLinkWhitespace(ArticleText, ArticleTitle);
		ArticleText=Parsers.FixSyntax(ArticleText);
		ArticleText=Parsers.FixCategories(ArticleText);
		ArticleText=Parsers.FixImages(ArticleText);
		ArticleText=Parsers.SimplifyLinks(ArticleText);
		//ArticleText = Parsers.FixCitationTemplates(ArticleText);
		ArticleText=Parsers.FixMainArticle(ArticleText);
		ArticleText=Parsers.FixReferenceListTags(ArticleText);
		ArticleText=Parsers.FixEmptyLinksAndTemplates(ArticleText);
		ArticleText=Parsers.SimplifyReferenceTags(ArticleText);
		ArticleText=Parsers.FixReferenceTags(ArticleText);
		ArticleText=Parsers.DuplicateNamedReferences(ArticleText);
		ArticleText=Parsers.DuplicateUnnamedReferences(ArticleText);
		ArticleText=Parsers.SameRefDifferentName(ArticleText);
		ArticleText=Parsers.RefsAfterPunctuation(ArticleText);
		ArticleText=Parsers.ReorderReferences(ArticleText);
		//ArticleText = parser.FixDatesA(ArticleText).Trim();
		ArticleText=parser.SortMetaData(ArticleText, ArticleTitle);
		ArticleText=ArticleText.Trim();

		return new Tuple<string, string>(ArticleText, summary);
	}
}