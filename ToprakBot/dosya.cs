using System.Diagnostics;
using System.Drawing;
using System;
using System.Text;
using WikiFunctions.API;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.IO;
using System.Net.Http;
using Newtonsoft.Json.Linq;
using System.Linq;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net;

public class ImageTest {
	public async static Task AK() {
		ApiEdit editor = new ApiEdit("https://" + ToprakBot.wiki + ".org/w/");
		ToprakBot.login(editor);

		List<string> liste = await dosyalist();
		Regex uzantiregex = new Regex(@"(\.[a-z0-9]{3,4})$", RegexOptions.IgnoreCase);
		int l = liste.Count, m = -1;

		foreach(string file in liste) {
			string dosya = file.Replace ("Dosya:", "");
			Console.ForegroundColor = ConsoleColor.Yellow;
			Console.WriteLine(++m+1 + "/" + l + ": " + dosya);
			Console.ForegroundColor = ConsoleColor.White;
			string apiUrl = "http://" + ToprakBot.wiki + ".org/wiki/Special:FilePath/" + dosya;
			var falan = uzantiregex.Match(dosya);
			string uzanti = falan.Groups[1].Value;

			string[] allowedExtensions = { ".png", ".jpg", ".jpeg", ".gif", ".tiff" };
			if (allowedExtensions.Contains(uzanti.ToLower())) {
				//Dosyayı indir
				byte[] imageBytes = await DownloadImage(apiUrl);

				//Dosyayı ufalt ve yükle
				float oran = Upright.FileRatio(dosya, true).Result;
				Console.WriteLine("Oran:" + oran);
				if(oran!=-1) {
					int newWidth = 300;
					int newHeight = (int)(300 / oran);
					Bitmap resizedImage = ResizeImage(imageBytes, newWidth, newHeight);

					string outputPath;
					if (ToprakBot.makine) outputPath = "C:\\Users\\Administrator\\Desktop\\file\\ResizedImage" + uzanti;
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

					PythonUpload(dosya, uzanti);

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
				string apiUrl2 = "https://" + ToprakBot.wiki + ".org/w/api.php?action=query&format=json&prop=imageinfo&titles=Dosya:" + dosya + "&formatversion=2&iiprop=archivename&iilimit=max";
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
										PythonRevDel(dosya, timestamp);
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
	}

	public static async Task<List<string>> dosyalist() {
		List<string> titles = new List<string>();
		string apiUrl = "https://" + ToprakBot.wiki + ".org/w/api.php?action=query&format=json&list=categorymembers&formatversion=2&cmtitle=Kategori%3AAdil%20kullan%C4%B1m%20i%C3%A7in%20boyut%20k%C3%BC%C3%A7%C3%BClt%C3%BClmesi%20istenen%20dosyalar&cmprop=title&cmnamespace=6&cmlimit=2";
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

	public static async Task<byte[]> DownloadImage(string url) {
		using (HttpClient client = new HttpClient()) {
			client.DefaultRequestHeaders.Add("User-Agent", "ToprakBot trwiki");
			try {
				return await client.GetByteArrayAsync(url);
			} catch (HttpRequestException ex) {
				Console.WriteLine($"Error downloading image: {ex.Message}");
				return null;
			}
		}
	}

	public static Bitmap ResizeImage(byte[] imageBytes, int newWidth, int newHeight) {
		using (MemoryStream ms = new MemoryStream(imageBytes)) {
			Bitmap originalImage = new Bitmap(ms);
			Bitmap resizedImage = new Bitmap(newWidth, newHeight);

			using (Graphics g = Graphics.FromImage(resizedImage)) {
				g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
				g.DrawImage(originalImage, 0, 0, newWidth, newHeight);
			}

			return resizedImage;
		}
	}

	public static void PythonUpload(string file, string uzanti) {
		string pythonScriptPath;
		if (ToprakBot.makine) pythonScriptPath = @"C:\Users\Administrator\Desktop\test.py";
		else pythonScriptPath = @"D:\\upload\\main.py";
		int b = ToprakBot.makine ? 0 : 1;
		string arguments = file.Replace(" ", "_") + " " + uzanti + " upload " + b;

		ProcessStartInfo start = new ProcessStartInfo();
		start.FileName = "python";
		start.Arguments = $"{pythonScriptPath} {arguments}";
		start.UseShellExecute = false;
		start.RedirectStandardOutput = true;

		using (Process process = Process.Start(start)) {
			using (StreamReader reader = process.StandardOutput) {
				string result = reader.ReadToEnd();
				Console.WriteLine(result);
			}

			process.WaitForExit();
		}
	}

	public static void PythonRevDel(string file, string id) {
		string pythonScriptPath;
		if (ToprakBot.makine) pythonScriptPath = @"C:\Users\Administrator\Desktop\test.py";
		else pythonScriptPath = @"D:\\upload\\main.py";
		int b = ToprakBot.makine ? 0 : 1;
		string arguments = file.Replace(" ", "_") + " " + id + " revdel " + b;

		ProcessStartInfo start = new ProcessStartInfo();
		start.FileName = "python";
		start.Arguments = $"{pythonScriptPath} {arguments}";
		start.UseShellExecute = false;
		start.RedirectStandardOutput = true;

		using (Process process = Process.Start(start)) {
			using (StreamReader reader = process.StandardOutput) {
				string result = reader.ReadToEnd();
				Console.WriteLine(result);
			}

			process.WaitForExit();
		}
	}
}