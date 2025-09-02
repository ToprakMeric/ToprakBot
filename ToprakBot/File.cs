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

	/// <summary>
	/// Processes files that are requested to be reduced in size within the scope of fair use.
	/// Steps:
	/// 1. List of files is retrieved with <c>ImageTest.dosyalist</c>.
	/// 2. File download with <c>ImageTest.DownloadImage</c>.
	/// 3. Aspect ratio (width/height) is queried with <c>Upright.FileRatio</c>
	/// 4. Resize the file to with 300px with <c>ImageTest.ResizeImage</c> and temporarily save to disk.
	/// 5. Upload resized file with <c>ImageTest.PythonUpload</c>.
	/// 6. Remove the maintenance template.
	/// 7. Revision delete for the old versions of the file with <c>ImageTest.PythonRevDel</c>.
	/// </summary>
	/// <remarks>
	/// No parameters. Uses <c>ToprakBot.wiki</c> to determine the wiki.
	/// Extensions processed via System.Drawing: .png, .jpg, .jpeg, .gif, .tiff
	/// </remarks>
	public async static Task FairUse() {
		ApiEdit editor = new ApiEdit("https://" + ToprakBot.wiki + ".org/w/");
		try {
			ToprakBot.login(editor);
		} catch(Exception ex) { ToprakBot.LogException("D01", ex); }

		List<string> filelist = await FairUseList();
		Regex extensionRegex = new Regex(@"(\.[a-z0-9]{3,4})$", RegexOptions.IgnoreCase);
		int l = filelist.Count, m = -1;

		foreach(string file in filelist) {
			string filename = file.Replace ("Dosya:", "");
			Console.ForegroundColor = ConsoleColor.Yellow;
			Console.WriteLine(++m+1 + "/" + l + ": " + filename);
			Console.ForegroundColor = ConsoleColor.White;
			string apiUrl = "http://" + ToprakBot.wiki + ".org/wiki/Special:FilePath/" + WebUtility.UrlEncode(filename).Replace("+", "_");
			var match = extensionRegex.Match(filename);
			string extension = match.Groups[1].Value;

			string[] allowedExtensions = { ".png", ".jpg", ".jpeg", ".gif", ".tiff" };
			if (allowedExtensions.Contains(extension.ToLower())) {
				byte[] imageBytes = await DownloadImage(apiUrl); //Retreive the file

				//Resize the image and upload
				float ratio = Upright.FileRatio(filename, true).Result;
				Console.WriteLine("Aspect ratio: " + ratio);
				if (ratio!=-1) {
					int newWidth = 300;
					int newHeight = (int)(300 / ratio);
					
					Bitmap resizedImage = ResizeImage(imageBytes, newWidth, newHeight);

					string outputPath;
					if (ToprakBot.makine) outputPath = "C:\\Users\\Administrator\\Desktop\\file\\ResizedImage" + extension;
					else outputPath = "D:\\ResizedImage" + extension;
					try {
						switch(extension.ToLower()) {
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
					} catch(Exception ex) { ToprakBot.LogException("D02", ex); continue; }

					Console.WriteLine("Step 1: File resized and saved locally.");

					try {
						PythonUpload(filename, extension);
					} catch(Exception ex) { ToprakBot.LogException("D03", ex); continue; }

				} else Console.WriteLine("Step 1: No need to resize.");

				//Remove maintance template {{Adil kullanım kalitesini düşür}}
				string ArticleText = null;
				try {
					ArticleText = editor.Open("File:" + filename);
				} catch(Exception ex) { ToprakBot.LogException("D04", ex); continue; }

				Regex maintanceTempReg = new Regex(@"\{\{\s*?(adil kullanım kalitesini düşür|non-free reduce)(\|.*?|\s*?)\}\}\n*", RegexOptions.IgnoreCase);
				if (maintanceTempReg.Match(ArticleText).Success) {
					ArticleText = maintanceTempReg.Replace(ArticleText, "");
					Console.WriteLine("Step 2: Removed maintance template.");
					
					try {
						editor.Save(ArticleText, "Adil kullanım kalitesini düşür bakım şablonu kaldırıldı.", true, WatchOptions.NoChange);
					} catch(Exception ex) { ToprakBot.LogException("D05", ex); continue; }

				} else Console.WriteLine("Step 2: Maintance template not found.");

				//Delete old revisions of the file
				string apiUrl2 = "https://" + ToprakBot.wiki + ".org/w/api.php?action=query&format=json&prop=imageinfo&titles=Dosya:" + filename + "&formatversion=2&iiprop=archivename&iilimit=max";
				try {
					using (HttpClient client = new HttpClient()) {
						client.DefaultRequestHeaders.Add("User-Agent", ToprakBot.userAgent);
						HttpResponseMessage response = await client.GetAsync(apiUrl2);

						if (response.IsSuccessStatusCode) {
							string json = await response.Content.ReadAsStringAsync();

							JObject responseObject = JObject.Parse(json);

							JArray imageInfo = responseObject["query"]["pages"][0]["imageinfo"] as JArray;

							int k = 0;
							foreach (JToken info in imageInfo) {
								string archivename = info["archivename"]?.ToString();
								if (!string.IsNullOrEmpty(archivename)) k++;
							}

							if (k==0) Console.WriteLine("Step 3: No revisions to delete.");
							else {
								Console.WriteLine("Step 3: Revdel");
								foreach (JToken info in imageInfo) {
									k++;
									string archivename = info["archivename"]?.ToString();
									if (!string.IsNullOrEmpty(archivename)) {
										string timestamp = archivename.Split('!')[0];
										PythonRevDel(filename, timestamp);
										Console.WriteLine(k + ": " + timestamp + " deleted.");
									}
								}
							}
						}
					}
				} catch(Exception ex) { ToprakBot.LogException("D06", ex); }
			} else Console.WriteLine("Unknown extension.");
		}
	}

	/// <summary>
	/// Retrieves list of files to be processed from the category.
	/// </summary>
	/// <remarks>
	/// Maximum number of files to be retrieved per run is determined by <c>runLimit</c> variable.
	/// </remarks>
	/// <returns>
	/// List containing the full titles of the target files (e.g., "File:Example.png").
	/// </returns>
	public static async Task<List<string>> FairUseList() {
		int runLimit = 10; //max num of files to handle per run
		List<string> titles = new List<string>();
		string apiUrl = "https://" + ToprakBot.wiki + ".org/w/api.php?action=query&format=json&list=categorymembers&formatversion=2&cmtitle=Kategori%3AAdil%20kullan%C4%B1m%20i%C3%A7in%20boyut%20k%C3%BC%C3%A7%C3%BClt%C3%BClmesi%20istenen%20dosyalar&cmprop=title&cmnamespace=6&cmlimit=" + (++runLimit);
		try {
			using (var client = new WebClient()) {
				client.Encoding = Encoding.UTF8;
				client.Headers.Add("User-Agent", ToprakBot.userAgent);
				string jsonContent = await client.DownloadStringTaskAsync(apiUrl);
				
				dynamic data = JsonConvert.DeserializeObject(jsonContent);
				foreach (var item in data.query.categorymembers) titles.Add(item.title.ToString());
			}
		} catch(Exception ex) { ToprakBot.LogException("D07", ex); }
		return titles;
	}

	/// <summary>
	/// Downloads an image from the specified URL using HttpClient.
	/// </summary>
	/// <param name="url">Direct URL of the image resource to download.</param>
	/// <returns>Byte array containing the image data on success.</returns>
	public static async Task<byte[]> DownloadImage(string url) {
		using (HttpClient client = new HttpClient()) {
			client.DefaultRequestHeaders.Add("User-Agent", ToprakBot.userAgent);
			try {
				return await client.GetByteArrayAsync(url);
			} catch(Exception ex) { ToprakBot.LogException("D08", ex); return null; }
		}
	}

	/// <summary>
	/// Resizes an image (provided as a byte array) to the specified dimensions.
	/// </summary>
	/// <param name="imageBytes">Source image as a byte array.</param>
	/// <param name="newWidth">Target width in pixels.</param>
	/// <param name="newHeight">Target height in pixels.</param>
	/// <returns>
	/// A Bitmap object containing the resized image.
	/// </returns>
	public static Bitmap ResizeImage(byte[] imageBytes, int newWidth, int newHeight) {
		try {
			using (MemoryStream ms = new MemoryStream(imageBytes)) {
				Bitmap originalImage = new Bitmap(ms);
				Bitmap resizedImage = new Bitmap(newWidth, newHeight);

				using (Graphics g = Graphics.FromImage(resizedImage)) {
					g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
					g.DrawImage(originalImage, 0, 0, newWidth, newHeight);
				}

				return resizedImage;
			}
		} catch (Exception ex) { ToprakBot.LogException("D09", ex); return null; }
	}

	/// <summary>
	/// Invokes the external Python script to upload the locally resized image to the target wiki.
	/// </summary>
	/// <param name="file">Title of the file without the namespace prefix</param>
	/// <param name="extension">Original file extension (e.g. ".png", ".jpg")</param>
	/// <returns>None.</returns>
	public static void PythonUpload(string file, string extension) {
		string pythonScriptPath;
		if (ToprakBot.makine) pythonScriptPath = @"C:\Users\Administrator\Desktop\test.py";
		else pythonScriptPath = @"D:\\upload\\main.py";
		int b = ToprakBot.makine ? 0 : 1;
		string args = file.Replace(" ", "_") + " " + extension + " upload " + b;

		try {
			ProcessStartInfo start = new ProcessStartInfo();
			start.FileName = "python";
			start.Arguments = $"{pythonScriptPath} {args}";
			start.UseShellExecute = false;
			start.RedirectStandardOutput = true;

			using (Process process = Process.Start(start)) {
				using (StreamReader reader = process.StandardOutput) {
					string result = reader.ReadToEnd();
					Console.ForegroundColor = ConsoleColor.Cyan;
					Console.WriteLine(result);
					Console.ForegroundColor = ConsoleColor.White;
				}

				process.WaitForExit();
			}
		} catch (Exception ex) { ToprakBot.LogException("D10", ex); }
	}

	/// <summary>
	/// Invokes the external Python script to revision-delete a specific version of a file.
	/// </summary>
	/// <param name="file">Title of the file without namespace</param>
	/// <param name="id">File version timestamp to be deleted.</param>
	/// <returns>None.</returns>
	/// <remarks>MediaWiki uses timestamps for file history instead of regular revision IDs.</remarks>
	public static void PythonRevDel(string file, string id) {
		string pythonScriptPath;
		if (ToprakBot.makine) pythonScriptPath = @"C:\Users\Administrator\Desktop\test.py";
		else pythonScriptPath = @"D:\\upload\\main.py";
		int b = ToprakBot.makine ? 0 : 1;
		string arguments = file.Replace(" ", "_") + " " + id + " revdel " + b;

			try {
			ProcessStartInfo start = new ProcessStartInfo();
			if (ToprakBot.makine) start.FileName = "C:\\Users\\Administrator\\AppData\\Local\\Programs\\Python\\Python310\\python.exe";
			else start.FileName = "python";
			start.Arguments = $"{pythonScriptPath} {arguments}";
			start.UseShellExecute = false;
			start.RedirectStandardOutput = true;

			using (Process process = Process.Start(start)) {
				using (StreamReader reader = process.StandardOutput) {
					string result = reader.ReadToEnd();
					Console.ForegroundColor = ConsoleColor.Cyan;
					Console.WriteLine(result);
					Console.ForegroundColor = ConsoleColor.White;
				}

				process.WaitForExit();
			}
		} catch (Exception ex) { ToprakBot.LogException("D11", ex); }
	}
}