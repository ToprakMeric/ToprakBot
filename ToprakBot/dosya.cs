using System.Diagnostics;
using System.Drawing;
using System;
using System.Text;
using System.Web;
using WikiFunctions;
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
	public static async Task<List<string>> dosyalist() {
		List<string> titles = new List<string>();
		string apiUrl = "https://tr.wikipedia.org/w/api.php?action=query&format=json&list=categorymembers&formatversion=2&cmtitle=Kategori%3AAdil%20kullan%C4%B1m%20i%C3%A7in%20boyut%20k%C3%BC%C3%A7%C3%BClt%C3%BClmesi%20istenen%20dosyalar&cmprop=title&cmnamespace=6&cmlimit=2";
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
		string pythonScriptPath = "D:\\upload\\main.py";
		string pythonInterpreterPath;
		if (ToprakBot.makine) pythonInterpreterPath = "C:\\Users\\Administrator\\AppData\\Local\\Programs\\Python\\Python310";
		else pythonInterpreterPath = "C:\\Users\\topra\\AppData\\Local\\Programs\\Python\\Python310\\python.exe";
		int b = ToprakBot.makine ? 0 : 1;
		string scriptArguments = file.Replace(" ", "_") + " " + uzanti + " upload " + b;
		string fullCommand = $"{pythonScriptPath} {scriptArguments}";

		ProcessStartInfo startInfo = new ProcessStartInfo {
			FileName = pythonInterpreterPath,
			Arguments = fullCommand,
			RedirectStandardOutput = true,
			RedirectStandardError = true,
			UseShellExecute = false,
			CreateNoWindow = true
		};

		using (Process process = new Process { StartInfo = startInfo }) {
			process.Start();

			string output = process.StandardOutput.ReadToEnd();
			string error = process.StandardError.ReadToEnd();

			//Console.WriteLine("Output:" + output);
			Console.WriteLine(error);
		}
	}

	public static void PythonRevDel(string file, string id) {
		string pythonScriptPath = "D:\\upload\\main.py";
		string pythonInterpreterPath;
		if (ToprakBot.makine) pythonInterpreterPath = "C:\\Users\\Administrator\\AppData\\Local\\Programs\\Python\\Python310";
		else pythonInterpreterPath = "C:\\Users\\topra\\AppData\\Local\\Programs\\Python\\Python310\\python.exe";
		int b = ToprakBot.makine ? 0 : 1;
		string scriptArguments = file.Replace(" ", "_") + " " + id + " revdel " + b;
		string fullCommand = $"{pythonScriptPath} {scriptArguments}";

		ProcessStartInfo startInfo = new ProcessStartInfo {
			FileName = pythonInterpreterPath,
			Arguments = fullCommand,
			RedirectStandardOutput = true,
			RedirectStandardError = true,
			UseShellExecute = false,
			CreateNoWindow = true
		};

		using (Process process = new Process { StartInfo = startInfo }) {
			process.Start();

			string output = process.StandardOutput.ReadToEnd();
			string error = process.StandardError.ReadToEnd();

			//Console.WriteLine("Output:" + output);
			Console.WriteLine(error);
		}
	}
}
