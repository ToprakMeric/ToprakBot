using System.Text.RegularExpressions;

public class KaynakCevir {
	static string Tercuman(string eskiay) {
		string yeniay = "";
		if(eskiay.ToLower()=="january") yeniay="Ocak";
		else if(eskiay.ToLower()=="february") yeniay="Şubat";
		else if(eskiay.ToLower()=="march") yeniay="Mart";
		else if(eskiay.ToLower()=="april") yeniay="Nisan";
		else if(eskiay.ToLower()=="may") yeniay="Mayıs";
		else if(eskiay.ToLower()=="june") yeniay="Haziran";
		else if(eskiay.ToLower()=="july") yeniay="Temmuz";
		else if(eskiay.ToLower()=="august") yeniay="Ağustos";
		else if(eskiay.ToLower()=="september") yeniay="Eylül";
		else if(eskiay.ToLower()=="october") yeniay="Ekim";
		else if(eskiay.ToLower()=="november") yeniay="Kasım";
		else if(eskiay.ToLower()=="december") yeniay="Aralık";

		return yeniay;
	}

	public static string Main(string ArticleText) {
		//tarih - çeviri
		//January 23, 2021
		Regex tarihRegex1 = new Regex(@"(\s*?\|\s*?(erişim(\-|\s|)tarihi|tarih|access(\-|\s|)date|accessdate|date|archive(\-|)date|arşiv(\-|)tarihi)\s*?\=\s*?)(January|February|March|April|May|June|July|August|September|October|November|December)\s*?(\d{1,2})\s*?(,|)\s*?(\d{4})", RegexOptions.IgnoreCase);
		ArticleText=tarihRegex1.Replace(ArticleText, match => {
			var ay = match.Groups[7].Value;
			ay=Tercuman(ay);
			return match.Groups[1].Value+match.Groups[8].Value+" "+ay+" "+match.Groups[10].Value;
		});

		//23 January 2021
		Regex tarihRegex2 = new Regex(@"(\s*?\|\s*?(erişim(\-|\s|)tarihi|tarih|access(\-|\s|)date|accessdate|date|archive(\-|)date|arşiv(\-|)tarihi)\s*?\=\s*?)(\d{1,2})\s*?(January|February|March|April|May|June|July|August|September|October|November|December)\s*?(\d{4})\s*?", RegexOptions.IgnoreCase);
		ArticleText=tarihRegex2.Replace(ArticleText, match => {
			var ay = match.Groups[8].Value;
			ay=Tercuman(ay);
			return match.Groups[1].Value+match.Groups[7].Value+" "+ay+" "+match.Groups[9].Value;
		});

		//

		//2021-01-23
		Regex tarihRegex3 = new Regex(@"(\s*?\|\s*?(erişim(\-|\s|)tarihi|tarih|access(\-|\s|)date|accessdate|date|archive(\-|)date|arşiv(\-|)tarihi)\s*?\=\s*?)(\d{4})\-(\d{1,2})\-(\d{1,2})", RegexOptions.IgnoreCase);
		ArticleText=tarihRegex3.Replace(ArticleText, match => {
			var ay = match.Groups[8].Value;
			string gun = match.Groups[9].Value;

			if(ay=="1"||ay=="01") ay="Ocak";
			else if(ay=="2"||ay=="02") ay="Şubat";
			else if(ay=="3"||ay=="03") ay="Mart";
			else if(ay=="4"||ay=="04") ay="Nisan";
			else if(ay=="5"||ay=="05") ay="Mayıs";
			else if(ay=="6"||ay=="06") ay="Haziran";
			else if(ay=="7"||ay=="07") ay="Temmuz";
			else if(ay=="8"||ay=="08") ay="Ağustos";
			else if(ay=="9"||ay=="09") ay="Eylül";
			else if(ay=="10") ay="Ekim";
			else if(ay=="11") ay="Kasım";
			else if(ay=="12") ay="Aralık";

			Regex sıfırçıkarıcı = new Regex(@"0(\d)");
			gun=sıfırçıkarıcı.Replace(gun, "$1");

			return match.Groups[1].Value+gun+" "+ay+" "+match.Groups[7].Value;
		});

		//tarih - düzelt
		//21 Ocak, 2021 ve Ocak, 2021
		Regex tarihRegex4 = new Regex(@"(\s*?\|\s*?(erişim(\-|\s|)tarihi|tarih|access(\-|\s|)date|accessdate|date|archive(\-|)date|arşiv(\-|)tarihi)\s*?\=\s*?)(\d{1,2}|)\s*?(Ocak|Şubat|Mart|Nisan|Mayıs|Haziran|Temmuz|Ağustos|Eylül|Ekim|Kas[Iı]m|Aral[Iı]k)\s*?\,\s*?(\d{4})\s*?", RegexOptions.IgnoreCase);
		ArticleText=tarihRegex4.Replace(ArticleText, match => {
			return match.Groups[1].Value+match.Groups[7].Value+" "+match.Groups[8].Value+" "+match.Groups[9].Value;
		});

		//21 Ocak 2021 Pazartesi
		Regex tarihRegex5 = new Regex(@"(\s*?\|\s*?(erişim(\-|\s|)tarihi|tarih|access(\-|\s|)date|accessdate|date|archive(\-|)date|arşiv(\-|)tarihi)\s*?\=\s*?)(\d{1,2})\s*?(Ocak|Şubat|Mart|Nisan|Mayıs|Haziran|Temmuz|Ağustos|Eylül|Ekim|Kas[Iı]m|Aral[Iı]k)\s*?(\d{4})\s*?(Pazartesi|Sal[Iı]|Çarşamba|Perşembe|Cumartes[iİ]|Cuma|Pazar)\s*?", RegexOptions.IgnoreCase);
		ArticleText=tarihRegex5.Replace(ArticleText, match => {
			return match.Groups[1].Value+match.Groups[7].Value+" "+match.Groups[8].Value+" "+match.Groups[9].Value;
		});

		//21 ocAk 2021
		Regex tarihRegex6 = new Regex(@"(\s*?\|\s*?(erişim(\-|)tarihi|tarih|access(\-|)date|accessdate|date|archive(\-|)date|arşiv(\-|)tarihi)\s*?\=\s*?)(\d{1,2})\s*?(Ocak|Şubat|Mart|Nisan|Mayıs|Haziran|Temmuz|Ağustos|Eylül|Ekim|Kas[Iı]m|Aral[Iı]k)\s*?(\d{4})", RegexOptions.IgnoreCase);
		ArticleText=tarihRegex6.Replace(ArticleText, match => {
			return match.Groups[1].Value+match.Groups[7].Value+" "+match.Groups[8].Value.Substring(0, 1).ToUpper()+match.Groups[8].Value.Substring(1).ToLower()+" "+match.Groups[9].Value;
		});

		//Aralık 2020-Ocak 2021
		Regex tarihRegex7 = new Regex(@"(\s*?\|\s*?(erişim(\-|)tarihi|tarih|access(\-|)date|accessdate|date|archive(\-|)date|arşiv(\-|)tarihi)\s*?\=\s*?)((Ocak|Şubat|Mart|Nisan|Mayıs|Haziran|Temmuz|Ağustos|Eylül|Ekim|Kas[Iı]m|Aral[Iı]k)\s*\d{4})\-((Ocak|Şubat|Mart|Nisan|Mayıs|Haziran|Temmuz|Ağustos|Eylül|Ekim|Kas[Iı]m|Aral[Iı]k)\s*\d{4})", RegexOptions.IgnoreCase);
		ArticleText=tarihRegex7.Replace(ArticleText, match => {
			return match.Groups[1].Value+match.Groups[7].Value+" - "+match.Groups[9].Value;
		});

		//Ocak 2021
		Regex tarihRegex8 = new Regex(@"(\s*?\|\s*?(erişim(\-|)tarihi|tarih|access(\-|)date|accessdate|date|archive(\-|)date|arşiv(\-|)tarihi)\s*?\=\s*?)(\d{4})\s*(Ocak|Şubat|Mart|Nisan|Mayıs|Haziran|Temmuz|Ağustos|Eylül|Ekim|Kas[Iı]m|Aral[Iı]k)\s*(\||\}\})", RegexOptions.IgnoreCase);
		ArticleText=tarihRegex8.Replace(ArticleText, match => {
			return match.Groups[1].Value+match.Groups[8].Value+" "+match.Groups[7].Value+match.Groups[9].Value;
		});

		Regex tarihRegex9 = new Regex(@"(\s*?\|\s*?(erişim(\-|\s|)tarihi|tarih|access(\-|\s|)date|accessdate|date|archive(\-|)date|arşiv(\-|)tarihi)\s*?\=\s*?)0(\d)\s*(Ocak|Şubat|Mart|Nisan|Mayıs|Haziran|Temmuz|Ağustos|Eylül|Ekim|Kas[Iı]m|Aral[Iı]k)\s*(\d{4})", RegexOptions.IgnoreCase);
		ArticleText=tarihRegex9.Replace(ArticleText, match => {
			return match.Groups[1].Value+match.Groups[7].Value+" "+match.Groups[8].Value+" "+match.Groups[9].Value;
		});

		//basım
		Regex basımçekici = new Regex(@"\|\s*?basım\s*?\=\s*?((\d{1,2})(st|nd|rd|th))\s*?(\||\}\})", RegexOptions.IgnoreCase);
		ArticleText=basımçekici.Replace(ArticleText, match => {
			string basım = match.Groups[1].Value;

			Regex sayı = new Regex(@"(\d{1,2})(st|nd|rd|th)", RegexOptions.IgnoreCase);
			basım=sayı.Replace(basım, "$1.");

			return "|basım="+basım+match.Groups[2].Value;
		});
		return ArticleText;
	}
}