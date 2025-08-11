//ToprakBot
//Yazar: ToprakM
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using WikiFunctions.API;
using Newtonsoft.Json;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Net;
using System.Linq;
using System.Text;
using System.Diagnostics;

public class ToprakBot {
	public static bool manual = false; //Sayfa listesini false ise API'den, true ise txt dosyadan alır
	public static bool makine = false; //Çalıştığı yere göre dosya konumlarını seçer, true ise makine false ise pc konumları
	public static string wiki = "tr.wikipedia";
	public static string wiki2 = "az.wikipedia";
	public static string wiki3 = "ka.wikipedia";

	public const string userAgent = "ToprakBot/1.7 (https://meta.wikimedia.org/wiki/User:ToprakBot; toprak@tprk.tr) C#/.NET";

	//Giriş: kod çalışmaya buradan başlıyor.
	public static async Task Main(string[] args) {
		
		//log listener
        DateTime bugun = DateTime.Today;
        string filePath, bugunformat = bugun.ToString("yyyy-MM-dd");
        if (!makine) filePath = @"D:\AWB\log\error\" + bugunformat + ".txt";
        else filePath = @"C:\Users\Administrator\Desktop\log\error\" + bugunformat + ".txt";
        Trace.Listeners.Add(new TextWriterTraceListener(filePath));
        Trace.AutoFlush=true;

		//trwiki
		Console.ForegroundColor = ConsoleColor.White;
		Console.WriteLine("trwiki");
		await Trwiki.trwiki();	// yeni madde dz
		await ImageTest.AK();	// adil kullanım dosya dz

		Console.ForegroundColor = ConsoleColor.White;
		Console.WriteLine("------\ntrwiki 5k");
		//await Trwiki.trwiki5k(); //5k madde dz

		//azwiki
		Console.ForegroundColor = ConsoleColor.White;
		Console.WriteLine("------\nazwiki");
		await Azwiki.azwiki();	// yeni madde dz

		//kawiki
		Console.ForegroundColor = ConsoleColor.White;
		Console.WriteLine("------\nkawiki");
		await Kawiki.kawiki();	// yeni madde dz

		Console.ForegroundColor = ConsoleColor.White;
		Console.WriteLine("Bitti.");
		Console.ReadKey();
	}

	//Giriş yapma fonksiyonu. Parola dosyadan çekiliyor.
	public static void login(ApiEdit editor) {
		string path, password;
		if (!makine) path = "D:\\AWB\\password.txt";
		else path = "C:\\Users\\Administrator\\Desktop\\password.txt";
		using (var reader = new StreamReader(path)) {
			password = reader.ReadToEnd();
		}

		try {
			editor.Login("ToprakBot@aws", password);
		} catch(Exception ex) { LogException("P01", ex); }
	}

	//Son x günde oluşturulan maddeleri API üzerinden alma fonksiyonu. String list olarak çıktı veriyor.
	public static int songun = 1; //son 1 gün
	public static async Task<List<string>> TitleList(string wiki) {
		List<string> titles = new List<string>();
		using(HttpClient client = new HttpClient()) {
			client.DefaultRequestHeaders.UserAgent.ParseAdd(userAgent);
			for(int i = 0; i < songun; i++) {
				string startDate = DateTime.UtcNow.AddDays(-i).ToString("yyyy-MM-dd") + "T00:00:00.000Z";
				string endDate = DateTime.UtcNow.AddDays(-i-1).ToString("yyyy-MM-dd") + "T00:00:00.000Z";
				string apiUrl = $"https://{wiki}.org/w/api.php?action=query&formatversion=2&list=recentchanges&rcdir=older&rcend={endDate}&rclimit=max&rcnamespace=0&rcprop=title&rcstart={startDate}&rctype=new&format=json";

				try {
					string json = await client.GetStringAsync(apiUrl);
					var jsonObject = JsonConvert.DeserializeObject<JObject>(json);
					var recentChanges = jsonObject["query"]?["recentchanges"];

					if(recentChanges!=null) {
						foreach(var change in recentChanges) {
							string title = change["title"]?.ToString();
							if(!string.IsNullOrEmpty(title)) titles.Add(title);
						}
					}
				} catch(Exception ex) { LogException("P02", ex); }
			}
		}
		return titles;
	}

	//User:ToprakBot/Liste sayfasından düzenlenmesi istenen sayfaları çekiyor. (ilk 100)
	//String list olarak çıktı veriyor.
	public static Task<List<string>> wikiliste(string wiki) {
		ApiEdit editor = new ApiEdit("https://" + wiki + ".org/w/");
		try {
			login(editor);
		} catch(Exception ex) { LogException("P03", ex); }

		string sayfa = null;
		try {
			sayfa = editor.Open("User:ToprakBot/Liste");
		} catch(Exception ex) { LogException("P04", ex); }

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
		if (reg2.Match(sayfa).Success || i == 0 || sayfa == "" || i != 100) sayfa = "{{/başlık}}\r\n\r\n# Örnek 1\r\n# Örnek 2\r\n# ...";

		if (bas == sayfa) Console.ForegroundColor = ConsoleColor.Yellow;
		else Console.ForegroundColor = ConsoleColor.Green;
		editor.Save(sayfa, i + " sayfa alındı.", true, WatchOptions.NoChange);
		Console.WriteLine("ToprakBot/Liste: " + i + " sayfa alındı");

		return Task.FromResult(titles);
	}
	
	//Hatalı koruma şablonuna sahip sayfalar kategorisindeki sayfaları çekiyor. String list olarak çıktı veriyor.
	public static async Task<List<string>> korumalist(string wiki) {
		List<string> titles = new List<string>();
		string apiUrl = "https://" + wiki + ".org/w/api.php?action=query&format=json&list=categorymembers&formatversion=2&cmtitle=Kategori:Hatalı koruma şablonuna sahip sayfalar&cmprop=title&cmlimit=max";
		try {
			using (var client = new WebClient()) {
				client.Encoding = Encoding.UTF8;
				client.Headers.Add(HttpRequestHeader.UserAgent, userAgent);
				string jsonContent = await client.DownloadStringTaskAsync(apiUrl);
				
				dynamic data = JsonConvert.DeserializeObject(jsonContent);
				foreach (var item in data.query.categorymembers) titles.Add(item.title.ToString());
			}
		} catch(Exception ex) { LogException("P05", ex); }
		return titles;
	}

	//Sayfanın koruma altında olup olmadığını kontrol ediyor. bool çıktısı veriyor
	public static async Task<bool> GetProtectionStatus(string title, string wiki) {
		string apiUrl = "https://" + wiki + ".org/w/api.php?action=query&format=json&prop=info&titles="+title+"&formatversion=2&inprop=protection";
		try {
			using(var client = new HttpClient()) {
				client.DefaultRequestHeaders.UserAgent.ParseAdd(userAgent);
				HttpResponseMessage response = await client.GetAsync(apiUrl);
				response.EnsureSuccessStatusCode();
				string responseBody = await response.Content.ReadAsStringAsync();
				JObject jsonResponse = JObject.Parse(responseBody);
				JToken protectionToken = jsonResponse.SelectToken("query.pages[0].protection");
				if (protectionToken != null && protectionToken.Count() > 0) return true;
				else return false;
			}
		} catch(Exception ex) { LogException("P06", ex); }
		return true; //hata durumunda değişiklik yapmaması için true dönüyor
	}

	//Title üzerinden sayfanın ad alanını tarıyor
	public static async Task<int> NameSpaceDedector(string articleTitle) {
		if (!articleTitle.Contains(":")) return 0;

		string apiUrl = $"https://{wiki}.org/w/api.php?action=query&format=json&titles={Uri.EscapeDataString(articleTitle)}&formatversion=2";
		try {
			using(var client = new HttpClient()) {
				client.DefaultRequestHeaders.UserAgent.ParseAdd(userAgent);
					var response = await client.GetAsync(apiUrl);
					response.EnsureSuccessStatusCode();
					var json = await response.Content.ReadAsStringAsync();
					var obj = JObject.Parse(json);
					var nsToken = obj.SelectToken("query.pages[0].ns");
					if (nsToken!=null&&int.TryParse(nsToken.ToString(), out int nsId))
						return nsId;
				}
		} catch(Exception ex) { LogException("P07", ex); }
		return 0;
	}

	public static void LogException(string code, Exception ex) {
		Trace.WriteLine($"{DateTime.Now} ({code}): Exception: {ex.GetType().FullName}");
		Trace.WriteLine($"Message: {ex.Message}");
		Trace.WriteLine($"StackTrace: {ex.StackTrace}");
		if (ex.InnerException != null) {
			Trace.WriteLine($"InnerException: {ex.InnerException.GetType().FullName}");
			Trace.WriteLine($"InnerException Message: {ex.InnerException.Message}");
			Trace.WriteLine($"InnerException StackTrace: {ex.InnerException.StackTrace}");
		}
		Trace.WriteLine("--------------------------------------------------");
	}

}