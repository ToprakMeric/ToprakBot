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
using System.Text.Json;

public class ToprakBot {
	public static bool manual = false; //Sayfa listesini false ise API'den, true ise txt dosyadan alır
	public static bool makine = true; //Çalıştığı yere göre dosya konumlarını seçer, true ise makine false ise pc konumları
	public static string wiki = "tr.wikipedia";
	public static string wiki2 = "az.wikipedia";

	//Giriş: kod çalışmaya buradan başlıyor.
	public static async Task Main(string[] args) {
		//trwiki
		Console.ForegroundColor = ConsoleColor.White;
		Console.WriteLine("trwiki");
		await Trwiki.trwiki();	// yeni madde dz
		await ImageTest.AK();	// adil kullanım dosya dz

		Console.ForegroundColor = ConsoleColor.White;
		Console.WriteLine("------\ntrwiki 5k");
		await Trwiki.trwiki5k(); //5k madde dz

		//azwiki
		Console.ForegroundColor = ConsoleColor.White;
		Console.WriteLine("------\nazwiki");
		await Azwiki.azwiki();	// yeni madde dz

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

	//Son x günde oluşturulan maddeleri API üzerinden alma fonksiyonu. String list olarak çıktı veriyor.
	static public int songun = 1; //son 1 gün
	static public async Task<List<string>> TitleList(string wiki) {
		List<string> titles = new List<string>();
		using (HttpClient client = new HttpClient()) {
			for (int i = 0; i < songun; i++) {
				string startDate = DateTime.UtcNow.AddDays(-i).ToString("yyyy-MM-dd") + "T00:00:00.000Z";
				string endDate = DateTime.UtcNow.AddDays(-i - 1).ToString("yyyy-MM-dd") + "T00:00:00.000Z";
				string apiUrl = $"https://{wiki}.org/w/api.php?action=query&formatversion=2&list=recentchanges&rcdir=older&rcend={endDate}&rclimit=max&rcnamespace=0&rcprop=title&rcstart={startDate}&rctype=new&format=json";

				try {
					string json = await client.GetStringAsync(apiUrl);
					var jsonObject = JsonConvert.DeserializeObject<JObject>(json);
					var recentChanges = jsonObject["query"]?["recentchanges"];

					if (recentChanges != null) {
						foreach (var change in recentChanges) {
							string title = change["title"]?.ToString();
							if (!string.IsNullOrEmpty(title)) {
								titles.Add(title);
							}
						}
					}
				}
				catch (Exception ex) {
					Console.WriteLine($"Error fetching changes for day {i}: {ex.Message}");
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
		var nameSpaces = new Dictionary<string, int> {
			{ "Ortam", -2 }, { "Özel", -1 }, { "Tartışma", 1 }, { "Kullanıcı", 2 },
			{ "Kullanıcı mesaj", 3 }, { "Vikipedi", 4 }, { "Vikipedi tartışma", 5 },
			{ "Dosya", 6 }, { "Resim", 6 }, { "Dosya tartışma", 7 }, { "MediaWiki", 8 },
			{ "MediaWiki tartışma", 9 }, { "Şablon", 10 }, { "Şablon tartışma", 11 },
			{ "Yardım", 12 }, { "Yardım tartışma", 13 }, { "Kategori", 14 },
			{ "Kategori tartışma", 15 }, { "Portal", 100 }, { "Portal tartışma", 101 },
			{ "Vikiproje", 102 }, { "Vikiproje tartışma", 103 }, { "TimedText", 710 },
			{ "TimedText talk", 711 }, { "Modül", 828 }, { "Modül tartışma", 829 },
			{ "Gadget", 2300 }, { "Gadget talk", 2301 }, { "Gadget definition", 2302 },
			{ "Gadget definition talk", 2303 }
		};

		var match = Regex.Match(ArticleTitle, @"^(.*?):");
		return (match.Success && nameSpaces.TryGetValue(match.Groups[1].Value, out int nsId)) ? nsId : 0;
	}
}