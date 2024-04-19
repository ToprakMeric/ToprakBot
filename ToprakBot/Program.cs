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
using System.Net;
using System.Web;
using System.Linq;
using System.Text;
using static System.Net.Mime.MediaTypeNames;
using System.Drawing;

public class ToprakBot {
	public static bool manual = true; //Sayfa listesini false ise API'den, true ise elle eklenmiş dosyadan alır
	public static bool makine = false; //Nerede çalışlacağına göre dosya konumlarını ayarlar, true ise makinede false ise pc de
	public static string wiki = "test.wikipedia";

	public static async Task Main(string[] args) {

		ApiEdit editor = new ApiEdit("https://" + wiki + ".org/w/");
		login(editor);

		List<string> titles;
		if(!manual) titles = await TitleList(wiki);
		else {
			StreamReader reader;
			if(!makine) reader = new StreamReader("D:\\AWB\\liste.txt");
			else reader = new StreamReader("C:\\Users\\Administrator\\Desktop\\liste.txt");

			using(reader) {
				titles = new List<string>();
				string line;
				while((line=reader.ReadLine())!=null) titles.Add(line);
			}
		}
		List<string> hatalıkorumaşablist = await korumalist(wiki);
		if(!manual) titles.AddRange(hatalıkorumaşablist);

		List<string> wikiliste = await ToprakBot.wikiliste(wiki);
		titles.AddRange(wikiliste);

		titles = titles.Distinct().ToList();
		int n = titles.Count;

		DateTime bugun = DateTime.Today;
		string bugunformat = bugun.ToString("yyyy-MM-dd");
		string filePath;
		if(!makine) filePath = @"D:\AWB\log\" + bugunformat + ".txt";
		else filePath = @"C:\Users\Administrator\Desktop\log\" + bugunformat + ".txt";
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
				var tuple = Edit(ArticleText, sayfa);
				ArticleText = tuple.Item1;
				ekozet = tuple.Item2;
			}

			//Lüzumsuz koruma şablonu kaldır
			if (hatalıkorumaşablist.Contains(sayfa)) {
				bool protectionStatus = await GetProtectionStatus(sayfa, wiki);
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

			if(ArticleText==madde) {
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

		//Adil kullanım
		List<string> liste = await ImageTest.dosyalist();
		Regex uzantiregex = new Regex(@"(\.[a-z0-9]{3,4})$", RegexOptions.IgnoreCase);
		int l = liste.Count, m = -1;

		foreach(string file in liste) {
			string dosya = file.Replace ("Dosya:", "");
			Console.ForegroundColor = ConsoleColor.Yellow;
			Console.WriteLine(++m+1 + "/" + l + ": " + dosya);
			Console.ForegroundColor = ConsoleColor.White;
			string apiUrl = "http://" + wiki + ".org/wiki/Special:FilePath/" + dosya;
			var falan = uzantiregex.Match(dosya);
			string uzanti = falan.Groups[1].Value;

			string[] allowedExtensions = { ".png", ".jpg", ".jpeg", ".gif", ".tiff" };
			if (allowedExtensions.Contains(uzanti.ToLower())) {
				//Dosyayı indir
				byte[] imageBytes = await ImageTest.DownloadImage(apiUrl);

				//Dosyayı ufalt ve yükle
				float oran = Upright.FileRatio(dosya, true).Result;
				Console.WriteLine("Oran:" + oran);
				if(oran!=-1) {
					int newWidth = 300;
					int newHeight = (int)(300 / oran);
					Bitmap resizedImage = ImageTest.ResizeImage(imageBytes, newWidth, newHeight);

					string outputPath;
					if (makine) outputPath = "C:\\Users\\Administrator\\Desktop\\file\\ResizedImage" + uzanti;
					else outputPath = "D:\\ResizedImage" + uzanti;
					switch(uzanti.ToLower()) {
						case ".png":
							resizedImage.Save(outputPath, System.Drawing.Imaging.ImageFormat.Png);
							break;
						case ".jpg":
						case ".jpeg":
							resizedImage.Save(outputPath, System.Drawing.Imaging.ImageFormat.Jpeg);
							break;
						case ".gif":
							resizedImage.Save(outputPath, System.Drawing.Imaging.ImageFormat.Gif);
							break;
						case ".tiff":
							resizedImage.Save(outputPath, System.Drawing.Imaging.ImageFormat.Tiff);
							break;
					}

					Console.WriteLine("Adım 1: Resim başarıyla boyutlandırıldı ve kaydedildi.");

					ImageTest.PythonUpload(dosya, uzanti);

				} else Console.WriteLine("Adım 1: Görsel zaten küçük.");

				//{{Adil kullanım kalitesini düşür}} kaldır
				string ArticleText = editor.Open("Dosya:" + dosya);

				Regex dusursablon = new Regex(@"\{\{\s*?(adil kullanım kalitesini düşür|non-free reduce)(\|.*?|\s*?)\}\}\n*", RegexOptions.IgnoreCase);
				if(dusursablon.Match(ArticleText).Success) {
					ArticleText = dusursablon.Replace(ArticleText, "");
					Console.WriteLine("Adım 2: Bakım şablonu kaldırıldı.");
					editor.Save(ArticleText, "Adil kullanım kalitesini düşür bakım şablonu kaldırıldı.", true, WatchOptions.NoChange);
				} else Console.WriteLine("Adım 2: Şablon bulunamadı.");

				//Eski sürümleri gizle
				string apiUrl2 = "https://" + wiki + ".org/w/api.php?action=query&format=json&prop=imageinfo&titles=Dosya:" + dosya + "&formatversion=2&iiprop=archivename&iilimit=max";
				try {
					using (HttpClient client = new HttpClient()) {
						HttpResponseMessage response = await client.GetAsync(apiUrl2);

						if (response.IsSuccessStatusCode) {
							string json = await response.Content.ReadAsStringAsync();

							JObject responseObject = JObject.Parse(json);

							JArray imageInfo = responseObject["query"]["pages"][0]["imageinfo"] as JArray;

							int ii = 0;
							foreach (JToken info in imageInfo) {
								string archivename = info["archivename"]?.ToString();
								if (!string.IsNullOrEmpty(archivename)) ii++;
							}
							if (ii==0) Console.WriteLine("Adım 3: Gizlenecek sürüm bulunamadı.");
							else {
								Console.WriteLine("Adım 3: Sürümler gizleniyor.");
								foreach (JToken info in imageInfo) {
									ii++;
									string archivename = info["archivename"]?.ToString();
									if (!string.IsNullOrEmpty(archivename)) {
										string timestamp = archivename.Split('!')[0];
										ImageTest.PythonRevDel(dosya, timestamp);
										Console.WriteLine(ii + ": " + timestamp + " gizlendi.");
									}
								}
							}
						}
					}
				}
				catch (Exception ex) {
					Console.WriteLine("Exception: " + ex.Message);
				}
			 } else Console.WriteLine("Bilinmeyen uzantı.");
		}
		Console.WriteLine("Bitti.");
		Console.ReadKey();
	}

	static void login(ApiEdit editor) {
		string password;
		if(!makine) password = File.ReadAllText("D:\\AWB\\password.txt");
		else password = File.ReadAllText("C:\\Users\\Administrator\\Desktop\\password.txt");
		editor.Login("ToprakBot@aws", password);

	return;
	}
	static async Task<List<string>> TitleList(string wiki) {
		List<string> titles = new List<string>();
		for(int i=-1;i<1;i++) {
			string apiUrl = "https://" + wiki + ".org/w/api.php?action=query&formatversion=2&list=recentchanges&rcdir=older&rcend="+DateTime.Now.AddDays(-i-1).ToString("yyyy-MM-dd")+"T00:00:00.000Z&rclimit=max&rcnamespace=0&rcprop=title&rcstart="+DateTime.Now.AddDays(-i).ToString("yyyy-MM-dd")+"T00:00:00.000Z&rctype=new&format=json";
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
	static async Task<List<string>> wikiliste(string wiki) {

		ApiEdit editor = new ApiEdit("https://" + wiki + ".org/w/");
		login(editor);
		string sayfa = editor.Open("User:ToprakBot/Liste");
		string pattern = @"^\#\s*(.*)$";
		MatchCollection matches = Regex.Matches(sayfa, pattern, RegexOptions.Multiline);
		List<string> titles = new List<string>();
		int i = 0;

		foreach (Match match in matches) {
			if (match.Success && i < 100) {
				string title = match.Groups[1].Value.Trim();
				
				if (title=="Örnek 1") {
					break;
				} else {
					titles.Add(title);
					i++;
				}
			}
		}

		foreach (string title in titles) {
			Regex reg = new Regex(@"\#\s*" + Regex.Escape(title) + "(?:\r\n|$)");
			sayfa = reg.Replace(sayfa, "");
		}
		Regex reg2 = new Regex(@"{{/başlık}}[\r\n\s]*");
		if(reg2.Match(sayfa).Success || i == 0) sayfa = "{{/başlık}}\r\n\r\n# Örnek 1\r\n# Örnek 2\r\n# ...\r\n";

		editor.Save(sayfa, i + " sayfa alındı.", true, WatchOptions.NoChange);

		return titles;
	}
	
	static async Task<List<string>> korumalist(string wiki) {
		List<string> titles = new List<string>();
		string apiUrl = "https://" + wiki + ".org/w/api.php?action=query&format=json&list=categorymembers&formatversion=2&cmtitle=Kategori:Hatalı koruma şablonuna sahip sayfalar&cmprop=title&cmlimit=max";
		try {
			using (var client = new WebClient()) {
				client.Encoding = Encoding.UTF8;
				string jsonContent = await client.DownloadStringTaskAsync(apiUrl);
				
				dynamic data = JsonConvert.DeserializeObject(jsonContent);
				foreach (var item in data.query.categorymembers) titles.Add(item.title.ToString());
			}
		}
		catch (Exception ex) {
			Console.WriteLine("Hata oluştu: " + ex.Message);
		}
		return titles;
	}

	static async Task<bool> GetProtectionStatus(string title, string wiki) {
		string apiUrl = "https://" + wiki + ".org/w/api.php?action=query&format=json&prop=info&titles=" + title + "&formatversion=2&inprop=protection";
		using (var client = new HttpClient()) {
			HttpResponseMessage response = await client.GetAsync(apiUrl);
			response.EnsureSuccessStatusCode();
			string responseBody = await response.Content.ReadAsStringAsync();
			JObject jsonResponse = JObject.Parse(responseBody);
			JToken protectionToken = jsonResponse.SelectToken("query.pages[0].protection");
			if (protectionToken != null && protectionToken.Count() > 0) return true;
			else return false;
		}
	}

	static public Tuple<string, string> Edit(string ArticleText, string ArticleTitle) {
		string summary = "";

		//Full capital başlık dz - Henüz tamamlanmamış özellik
		/* 
		List<string> sablonlar = Parsers.GetAllTemplateDetail(ArticleText);
		Regex kaynakbaslik = new Regex(@"(\{\{\s*?(?:[\p{L}]*? kaynağı|[Kk]aynak\s*?\||[Cc]ite [\p{L}]?).*?\|\s*?(?:başlık|title)\s*?\=[^\p{L}]*)([\p{Lu}\W\d]*?)([^\p{L}]*(?:\||\}\}))");
		Regex sablonicisablon = new Regex(@"\{\{([^{}]*\{\{[^{}]*\}\}[^{}]*)*\}\}");
		foreach(string sablon in sablonlar) {
			//Console.WriteLine(sablon);
			if (kaynakbaslik.Match(sablon).Success&&!sablonicisablon.Match(sablon).Success) {
				//Console.WriteLine("ok");
				var hmmm = kaynakbaslik.Match(sablon);
				string başlık = hmmm.Groups[2].Value;
				başlık = Baslik.Main(başlık);
				string yenisablon = kaynakbaslik.Replace(sablon, "$1" + başlık + "$3");
				ArticleText = ArticleText.Replace(sablon, yenisablon);
			}
			//Console.WriteLine("---");
		}
		*/

		ArticleText = Upright.Main(ArticleText);

		var tuple = Kaynakca.Main(ArticleText);
		ArticleText = tuple.Item1;
		summary += tuple.Item2;

		var tuple4 = NotListesi.Main(ArticleText);
		ArticleText = tuple4.Item1;
		summary += tuple4.Item2;

		ArticleText = KaynakCevir.Main(ArticleText);

		var tuple2 = YalinURL.Main(ArticleText);
		ArticleText = tuple2.Item1;
		summary += tuple2.Item2;

		var tuple3 = GorunmezKarakter.Main(ArticleText, ArticleTitle);
		ArticleText = tuple3.Item1;
		summary += tuple3.Item2;

		Regex regex1 = new Regex(@"(\[\[\s*?)([Iıİi]mage|[Ff]ile)(\s*?\:)");
		//if(regex1.Match(ArticleText).Success) ArticleText = regex1.Replace(ArticleText, "$1Dosya$3");

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
		//ArticleText = parser.FixNonBreakingSpaces(ArticleText); //AWB bug bkz w.wiki/9p6C
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
		//ArticleText = parser.FixDatesA(ArticleText).Trim();
		//ArticleText = Parsers.FixCitationTemplates(ArticleText);
		return new Tuple<string, string>(ArticleText, summary);
	}

		public int NameSpaceDedector(string ArticleTitle) {
		Regex colon = new Regex(@"^(.*?)\:");
		if (colon.Match(ArticleTitle).Success) {
			var aracı = colon.Match(ArticleTitle);
			string alan = aracı.Groups[1].Value;
			switch(alan) {
				case "Ortam":
					return -2;
				case "Özel":
					return -1;
				case "Tartışma":
					return 1;
				case "Kullanıcı":
					return 2;
				case "Kullanıcı mesaj":
					return 3;
				case "Vikipedi":
					return 4;
				case "Vikipedi tartışma":
					return 5;
				case "Dosya":
				case "Resim":
					return 6;
				case "Dosya tartışma":
					return 7;
				case "MediaWiki":
					return 8;
				case "MediaWiki tartışma":
					return 9;
				case "Şablon":
					return 10;
				case "Şablon tartışma":
					return 11;
				case "Yardım":
					return 12;
				case "Yardım tartışma":
					return 13;
				case "Kategori":
					return 14;
				case "Kategori tartışma":
					return 15;
				case "Portal":
					return 100;
				case "Portal tartışma":
					return 101;
				case "Vikiproje":
					return 102;
				case "Vikiproje tartışma":
					return 103;
				case "TimedText":
					return 710;
				case "TimedText talk":
					return 711;
				case "Modül":
					return 828;
				case "Modül tartışma":
					return 829;
				case "Gadget":
					return 2300;
				case "Gadget talk":
					return 2301;
				case "Gadget definition":
					return 2302;
				case "Gadget definition talk":
					return 2303;
				default:
					return 0;
			}
		}
		return 0;
	}
}