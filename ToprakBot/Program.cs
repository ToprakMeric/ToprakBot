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
using System.Linq;
using System.Text;

public class ToprakBot {
	public static bool manual = true; //Sayfa listesini false ise API'den, true ise elle eklenmiş dosyadan alır
	public static bool makine = false; //Nerede çalışlacağına göre dosya konumlarını ayarlar, true ise makinede false ise pc de
	public static string wiki = "tr.wikipedia"; //bazen test wikide çalıştırıyorum
	public static string wiki2 = "az.wikipedia";

	//Giriş: kod çalışmaya buradan başlıyor.
	public static async Task Main(string[] args) {
		Console.ForegroundColor = ConsoleColor.White;
		Console.WriteLine("trwiki");
		await Trwiki.trwiki();		// trwiki madde dz
		await ImageTest.AK();	// trwiki adil kullanım dosya dz

		Console.ForegroundColor = ConsoleColor.White;
		Console.WriteLine("------\ntrwiki 5k");
		await Trwiki.trwiki5k();

		Console.ForegroundColor = ConsoleColor.White;
		Console.WriteLine("------\nazwiki");
		await Azwiki.azwiki();			// azwiki madde dz

		Console.ForegroundColor = ConsoleColor.White;
		Console.WriteLine("Bitti.");
		Console.ReadKey();
	}

	//Giriş yapma fonksiyonu. Parola dosyadan çekiliyor.
	static public void login(ApiEdit editor) {
		string password;
		if(!makine) password = File.ReadAllText("D:\\AWB\\password.txt");
		else password = File.ReadAllText("C:\\Users\\Administrator\\Desktop\\password.txt");
		editor.Login("ToprakBot@aws", password);

	return;
	}

	//Son 24 saatte oluşturulan maddeleri alma fonksiyonu. String list olarak çıktı veriyor.
	static public async Task<List<string>> TitleList(string wiki) {
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

	//User:ToprakBot/Liste sayfasından düzenlenmesi istenen sayfaları çekiyor. (ilk 100)
	//String list olarak çıktı veriyor.
	static public async Task<List<string>> wikiliste(string wiki) {

		ApiEdit editor = new ApiEdit("https://" + wiki + ".org/w/");
		login(editor);
		string sayfa = editor.Open("User:ToprakBot/Liste");
		string bas = sayfa;
		string pattern = @"^\#\s*(.*)$";
		MatchCollection matches = Regex.Matches(sayfa, pattern, RegexOptions.Multiline);
		List<string> titles = new List<string>();
		int i = 0;

		foreach (Match match in matches) {
			if (match.Success && i < 100) {
				string title = match.Groups[1].Value.Trim();
				
				if (title=="Örnek 1") break;
				else {
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
		if(reg2.Match(sayfa).Success || i == 0 || sayfa == "" || i != 100) sayfa = "{{/başlık}}\r\n\r\n# Örnek 1\r\n# Örnek 2\r\n# ...";

		if (bas == sayfa) Console.ForegroundColor = ConsoleColor.Yellow;
		else Console.ForegroundColor = ConsoleColor.Green;
		editor.Save(sayfa, i + " sayfa alındı.", true, WatchOptions.NoChange);
		Console.WriteLine("ToprakBot/Liste: " + i + " sayfa alındı");
		return titles;
	}
	
	//Hatalı koruma şablonuna sahip sayfalar kategorisindeki sayfaları çekiyor. String list olarak çıktı veriyor.
	static public async Task<List<string>> korumalist(string wiki) {
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

	//Sayfanın koruma altında olup olmadığını kontrol ediyor. bool çıktısı veriyor
	static public async Task<bool> GetProtectionStatus(string title, string wiki) {
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

	//ArticleTitle üzerinden sayfanın ad alanını tarıyor (yalnızca Türkçe)
	//Her ad alanının önceden atanmış bir kodu var. Çıktı olarak bu kodu veriyor.
	//to-do bir şekilde çok dilli yapılacak
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