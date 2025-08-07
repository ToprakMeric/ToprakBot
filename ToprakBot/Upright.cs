using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

public class Upright {
	public static async Task<float> FileRatio(string file, bool adil) {
		string apiUrl;
		if (adil) apiUrl = "https://tr.wikipedia.org/w/api.php?action=query&prop=imageinfo&titles=File:" + file + "&iiprop=dimensions&format=json";
		else apiUrl = "https://commons.wikimedia.org/w/api.php?action=query&prop=imageinfo&titles=File:" + file + "&iiprop=dimensions&format=json";

		int imageHeight = -1, imageWidth = -1;

		using(var httpClient = new HttpClient()) {
			try {
				var response = await httpClient.GetStringAsync(apiUrl);
				var jsonObject = JObject.Parse(response);

				JObject pages = jsonObject["query"]["pages"] as JObject;

				if (pages!=null) {
					foreach(JProperty pageProperty in pages.Properties()) {

						JObject page = pageProperty.Value as JObject;

						if (page!=null&&page["imageinfo"] is JArray imageInfoArray&&imageInfoArray.Count>0) {
							int.TryParse(imageInfoArray[0]["height"].ToString(), out imageHeight);
							int.TryParse(imageInfoArray[0]["width"].ToString(), out imageWidth);
							break;
						}
					}
				}

			} catch(Exception ex) {
				Console.WriteLine("Hata: "+ex.Message);
			}
		}
		if (adil&&(Math.Min(imageWidth, imageHeight)<=300)) return -1;
		return (float)imageWidth/imageHeight;
	}

	public static string Main(string ArticleText) {
		Regex kucukresim = new Regex(@"\[\[\s*?(Dosya|Resim|File|Image)\:\s*?.*?\s*?\|\s*?(thumb|küçükresim)\s*?.*?\]\]", RegexOptions.IgnoreCase);

		if (kucukresim.Match(ArticleText).Success) {

			ArticleText=kucukresim.Replace(ArticleText, delegate (Match match) {
				string DosyaMetin = match.Value;
				string piksel = "";

				Regex pikrgx = new Regex(@"\|\s*?(\d{0,4})(x(\d{1,4})|)\s*?(pik|px)", RegexOptions.IgnoreCase);

				if (pikrgx.Match(ArticleText).Success) {

					var aracı = pikrgx.Match(DosyaMetin);
					string px1 = aracı.Groups[1].Value;
					string px2 = aracı.Groups[3].Value;

					Regex isim = new Regex(@"(Dosya|Resim|File|Image)\s*:\s*(.*?)\s*\|", RegexOptions.IgnoreCase);
					var aracı2 = isim.Match(DosyaMetin);
					string file = aracı2.Groups[2].Value;

					if ((px1==px2)&&(px1!=""||px2!="")) {
						piksel = px1;
						float oran = FileRatio(file, false).Result;
						if (oran<1) {
							float ara = oran*float.Parse(piksel);
							piksel = ara.ToString();
						} else piksel = px1;
					} else if ((px1=="")&&(px2!="")) {
						piksel = px2;
						float oran = FileRatio(file, false).Result;
						float ara = oran*float.Parse(piksel);
						piksel = ara.ToString();
					} else piksel = px1;
				}

				if (float.TryParse(piksel, out float upright)) {
					upright = (float)Math.Round(upright/220, 2);
					string uprightStr = upright.ToString();
					uprightStr = uprightStr.Replace(",", ".");

					Regex sıfıryokedici = new Regex(@"(\d)(\.00|(\.\d)0)");
					if(sıfıryokedici.Match(uprightStr).Success) uprightStr = sıfıryokedici.Replace(uprightStr, "$1$3");

					DosyaMetin = pikrgx.Replace(DosyaMetin, "|upright=" + uprightStr);

					return DosyaMetin;
				} else return match.Value;
			});
		}
		return ArticleText;
	}
}