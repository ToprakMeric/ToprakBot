using System.Text.RegularExpressions;

public class KaynakCevir {
	static string Tercuman(string eskiay) {
		switch (eskiay.ToLower()) {
			case "january":
			   return "Ocak";
			case "february":
				return "Şubat";
			case "march":
				return "Mart";
			case "april":
				return "Nisan";
			case "may":
				return "Mayıs";
			case "june":
				return "Haziran";
			case "july":
				return "Temmuz";
			case "august":
				return "Ağustos";
			case "september":
				return "Eylül";
			case "october":
				return "Ekim";
			case "november":
				return "Kasım";
			case "december":
				return "Aralık";
			default:
				return string.Empty;
	}
}

	public static string Main(string ArticleText) {
		//tarih - çeviri
		//January 23, 2021
		Regex tarihRegex1 = new Regex(@"(\s*?\|\s*?(erişim(\-|\s|)tarihi|tarih|access(\-|\s|)date|accessdate|date|archive(\-|)date|arşiv(\-|)tarihi)\s*?\=\s*?)(January|February|March|April|May|June|July|August|September|October|November|December)\s*?(\d{1,2})\s*?(,|)\s*?(\d{4})", RegexOptions.IgnoreCase);
		ArticleText = tarihRegex1.Replace(ArticleText, match => {
			var ay = match.Groups[7].Value;
			ay = Tercuman(ay);
			return match.Groups[1].Value + match.Groups[8].Value + " " + ay + " " + match.Groups[10].Value;
		});

		//23 January 2021
		Regex tarihRegex2 = new Regex(@"(\s*?\|\s*?(erişim(\-|\s|)tarihi|tarih|access(\-|\s|)date|accessdate|date|archive(\-|)date|arşiv(\-|)tarihi)\s*?\=\s*?)(\d{1,2})\s*?(January|February|March|April|May|June|July|August|September|October|November|December)\s*?(\d{4})\s*?", RegexOptions.IgnoreCase);
		ArticleText = tarihRegex2.Replace(ArticleText, match => {
			var ay = match.Groups[8].Value;
			ay = Tercuman(ay);
			return match.Groups[1].Value + match.Groups[7].Value + " " + ay + " " + match.Groups[9].Value;
		});

		//2021-01-23
		Regex tarihRegex3 = new Regex(@"(\s*?\|\s*?(erişim(\-|\s|)tarihi|tarih|access(\-|\s|)date|accessdate|date|archive(\-|)date|arşiv(\-|)tarihi)\s*?\=\s*?)(\d{4})\-(\d{1,2})\-(\d{1,2})", RegexOptions.IgnoreCase);
		ArticleText = tarihRegex3.Replace(ArticleText, match => {
			var ay = match.Groups[8].Value;
			string gun = match.Groups[9].Value;

			switch (ay) {
				case "1":
				case "01":
					ay = "Ocak";
					break;
				case "2":
				case "02":
					ay = "Şubat";
					break;
				case "3":
				case "03":
					ay = "Mart";
					break;
				case "4":
				case "04":
					ay = "Nisan";
					break;
				case "5":
				case "05":
					ay = "Mayıs";
					break;
				case "6":
				case "06":
					ay = "Haziran";
					break;
				case "7":
				case "07":
					ay = "Temmuz";
					break;
				case "8":
				case "08":
					ay = "Ağustos";
					break;
				case "9":
				case "09":
					ay = "Eylül";
					break;
				case "10":
					ay = "Ekim";
					break;
				case "11":
					ay = "Kasım";
					break;
				case "12":
					ay = "Aralık";
					break;
				default:
					break;
			}

			Regex sıfırçıkarıcı = new Regex(@"0(\d)");
			gun = sıfırçıkarıcı.Replace(gun, "$1");

			return match.Groups[1].Value + gun + " " + ay + " " + match.Groups[7].Value;
		});

		//tarih - düzelt
		//21 Ocak, 2021 ve Ocak, 2021
		Regex tarihRegex4 = new Regex(@"(\s*?\|\s*?(erişim(\-|\s|)tarihi|tarih|access(\-|\s|)date|accessdate|date|archive(\-|)date|arşiv(\-|)tarihi)\s*?\=\s*?)(\d{1,2}|)\s*?(Ocak|Şubat|Mart|Nisan|Mayıs|Haziran|Temmuz|Ağustos|Eylül|Ekim|Kas[Iı]m|Aral[Iı]k)\s*?\,\s*?(\d{4})\s*?", RegexOptions.IgnoreCase);
		ArticleText = tarihRegex4.Replace(ArticleText, match => {
			return match.Groups[1].Value + match.Groups[7].Value + " " + match.Groups[8].Value + " " + match.Groups[9].Value;
		});

		//21 Ocak 2021 Pazartesi
		Regex tarihRegex5 = new Regex(@"(\s*?\|\s*?(erişim(\-|\s|)tarihi|tarih|access(\-|\s|)date|accessdate|date|archive(\-|)date|arşiv(\-|)tarihi)\s*?\=\s*?)(\d{1,2})\s*?(Ocak|Şubat|Mart|Nisan|Mayıs|Haziran|Temmuz|Ağustos|Eylül|Ekim|Kas[Iı]m|Aral[Iı]k)\s*?(\d{4})\s*?(Pazartesi|Sal[Iı]|Çarşamba|Perşembe|Cumartes[iİ]|Cuma|Pazar)\s*?", RegexOptions.IgnoreCase);
		ArticleText = tarihRegex5.Replace(ArticleText, match => {
			return match.Groups[1].Value + match.Groups[7].Value + " " + match.Groups[8].Value + " " + match.Groups[9].Value;
		});

		//21 ocAk 2021
		Regex tarihRegex6 = new Regex(@"(\s*?\|\s*?(erişim(\-|)tarihi|tarih|access(\-|)date|accessdate|date|archive(\-|)date|arşiv(\-|)tarihi)\s*?\=\s*?)(\d{1,2})\s*?(Ocak|Şubat|Mart|Nisan|Mayıs|Haziran|Temmuz|Ağustos|Eylül|Ekim|Kas[Iı]m|Aral[Iı]k)\s*?(\d{4})", RegexOptions.IgnoreCase);
		ArticleText = tarihRegex6.Replace(ArticleText, match => {
			return match.Groups[1].Value + match.Groups[7].Value + " " + match.Groups[8].Value.Substring(0, 1).ToUpper() + match.Groups[8].Value.Substring(1).ToLower() + " " + match.Groups[9].Value;
		});

		//Aralık 2020-Ocak 2021
		Regex tarihRegex7 = new Regex(@"(\s*?\|\s*?(erişim(\-|)tarihi|tarih|access(\-|)date|accessdate|date|archive(\-|)date|arşiv(\-|)tarihi)\s*?\=\s*?)((Ocak|Şubat|Mart|Nisan|Mayıs|Haziran|Temmuz|Ağustos|Eylül|Ekim|Kas[Iı]m|Aral[Iı]k)\s*\d{4})\-((Ocak|Şubat|Mart|Nisan|Mayıs|Haziran|Temmuz|Ağustos|Eylül|Ekim|Kas[Iı]m|Aral[Iı]k)\s*\d{4})", RegexOptions.IgnoreCase);
		ArticleText = tarihRegex7.Replace(ArticleText, match => {
			return match.Groups[1].Value + match.Groups[7].Value + " - " + match.Groups[9].Value;
		});

		//Ocak 2021
		Regex tarihRegex8 = new Regex(@"(\s*?\|\s*?(erişim(\-|)tarihi|tarih|access(\-|)date|accessdate|date|archive(\-|)date|arşiv(\-|)tarihi)\s*?\=\s*?)(\d{4})\s*(Ocak|Şubat|Mart|Nisan|Mayıs|Haziran|Temmuz|Ağustos|Eylül|Ekim|Kas[Iı]m|Aral[Iı]k)\s*(\||\}\})", RegexOptions.IgnoreCase);
		ArticleText = tarihRegex8.Replace(ArticleText, match => {
			return match.Groups[1].Value + match.Groups[8].Value + " " + match.Groups[7].Value + match.Groups[9].Value;
		});

		Regex tarihRegex9 = new Regex(@"(\s*?\|\s*?(erişim(\-|\s|)tarihi|tarih|access(\-|\s|)date|accessdate|date|archive(\-|)date|arşiv(\-|)tarihi)\s*?\=\s*?)0(\d)\s*(Ocak|Şubat|Mart|Nisan|Mayıs|Haziran|Temmuz|Ağustos|Eylül|Ekim|Kas[Iı]m|Aral[Iı]k)\s*(\d{4})", RegexOptions.IgnoreCase);
		ArticleText = tarihRegex9.Replace(ArticleText, match => {
			return match.Groups[1].Value + match.Groups[7].Value + " " + match.Groups[8].Value + " " + match.Groups[9].Value;
		});

		//basım
		Regex basımçekici = new Regex(@"\|\s*?basım\s*?\=\s*?((\d{1,2})(st|nd|rd|th))\s*?(\||\}\})", RegexOptions.IgnoreCase);
		ArticleText = basımçekici.Replace(ArticleText, match => {
			string basım = match.Groups[1].Value;

			Regex sayı = new Regex(@"(\d{1,2})(st|nd|rd|th)", RegexOptions.IgnoreCase);
			basım = sayı.Replace(basım, "$1.");

			return "|basım=" + basım + match.Groups[2].Value;
		});
		return ArticleText;
	}
}