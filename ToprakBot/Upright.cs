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
			httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(ToprakBot.userAgent);
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
			} catch (Exception ex) { ToprakBot.LogException("U01", ex); }

		}
		if (adil&&(Math.Min(imageWidth, imageHeight)<=300)) return -1;
		return (float)imageWidth/imageHeight;
	}

	public static string Main(string ArticleText) {
		Regex Thumb = new Regex(@"\[\[\s*?(Dosya|Resim|File|Image)\:\s*?.*?\s*?\|\s*?(thumb|küçükresim)\s*?.*?\]\]", RegexOptions.IgnoreCase);
		Regex Pixel = new Regex(@"\|\s*?(\d{0,4})(x(\d{1,4})|)\s*?(pik|px)", RegexOptions.IgnoreCase);
		Regex RemoveZero = new Regex(@"(\d)(\.00|(\.\d)0)");
		Regex FileName = new Regex(@"(Dosya|Resim|File|Image)\s*:\s*(.*?)\s*\|", RegexOptions.IgnoreCase);

		if (Thumb.Match(ArticleText).Success) {

			ArticleText = Thumb.Replace(ArticleText, delegate (Match match) {
				string DosyaMetin = match.Value, piksel = null;

				if (Pixel.Match(ArticleText).Success) {
					var aracı = Pixel.Match(DosyaMetin);
					var aracı2 = FileName.Match(DosyaMetin);
					string px1 = aracı.Groups[1].Value;
					string px2 = aracı.Groups[3].Value;
					float oran = FileRatio(aracı2.Groups[2].Value, false).Result;
					
					if ((px1==px2)&&(px1!=""||px2!="")) {
						piksel = px1;
						if (oran<1) {
							float ara = oran * float.Parse(piksel);
							piksel = ara.ToString();
						} else piksel = px1;
					} else if ((px1=="")&&(px2!="")) {
						piksel = px2;
						float ara = oran * float.Parse(piksel);
						piksel = ara.ToString();
					} else piksel = px1;
				}

				if (float.TryParse(piksel, out float upright)) {
					upright = (float)Math.Round(upright/220, 2);
					string uprightStr = upright.ToString();
					uprightStr = uprightStr.Replace(",", ".");

					if(RemoveZero.Match(uprightStr).Success) uprightStr = RemoveZero.Replace(uprightStr, "$1$3");

					DosyaMetin = Pixel.Replace(DosyaMetin, "|upright=" + uprightStr);

					return DosyaMetin;
				} else return match.Value;
			});
		}
		return ArticleText;
	}
}