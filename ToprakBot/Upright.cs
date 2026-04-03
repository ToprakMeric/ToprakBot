using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

public class Upright {

	// Fetches image width & height from desired wiki
	// and returns the aspect ratio (width/height) as float.
	public static async Task<float> FileRatio(string file, bool fairUse) {
		string apiUrl;
		if (fairUse) apiUrl = "https://tr.wikipedia.org/w/api.php?action=query&prop=imageinfo&titles=File:" + file + "&iiprop=dimensions&format=json";
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
		if (fairUse&&(Math.Min(imageWidth, imageHeight)<=300)) return -1;
		return (float)imageWidth/imageHeight;
	}

    /// Scans the text for image thumbnails that specify explicit pixel dimensions
    /// and converts them into |upright= value by dividing the used width by 220
    public static string Main(string ArticleText) {
		Regex thumb = new Regex(@"\[\[\s*?(Dosya|Resim|File|Image)\:\s*?.*?\s*?\|\s*?(thumb|küçükresim)\s*?.*?\]\]", RegexOptions.IgnoreCase);
		Regex pixel = new Regex(@"\|\s*?(\d{0,4})(x(\d{1,4})|)\s*?(pik|px)", RegexOptions.IgnoreCase);
		Regex removeZero = new Regex(@"(\d)(\.00|(\.\d)0)");
		Regex fileName = new Regex(@"(Dosya|Resim|File|Image)\s*:\s*(.*?)\s*\|", RegexOptions.IgnoreCase);

		if (thumb.Match(ArticleText).Success) {

			ArticleText = thumb.Replace(ArticleText, delegate (Match match) {
				string fileText = match.Value, width = null;

				if (pixel.Match(ArticleText).Success) {
					var match1 = pixel.Match(fileText);
					var match2 = fileName.Match(fileText);
					string px1 = match1.Groups[1].Value;
					string px2 = match1.Groups[3].Value;
					float ratio = FileRatio(match2.Groups[2].Value, false).Result;

					//Handle wiki syntax to find used width:
					if((px1==px2)&&(px1!=""||px2!="")) {
						width = px1;
						if (ratio<1) {
							width = (ratio * float.Parse(width)).ToString();
						} else width = px1;
					} else if ((px1=="")&&(px2!="")) {
						width = px2;
						width = (ratio * float.Parse(width)).ToString();
					} else width = px1;
				}

				if (float.TryParse(width, out float upright)) {
					upright = (float)Math.Round(upright/220, 2);
					string uprightStr = upright.ToString();
					uprightStr = uprightStr.Replace(",", ".");

					if (removeZero.Match(uprightStr).Success) uprightStr = removeZero.Replace(uprightStr, "$1$3");

					fileText = pixel.Replace(fileText, "|upright=" + uprightStr);

					return fileText;
				} else return match.Value;
			});
		}
		return ArticleText;
	}
}