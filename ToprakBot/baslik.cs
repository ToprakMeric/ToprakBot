using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using WikiFunctions;
using static System.Net.Mime.MediaTypeNames;

//Henüz tamamlanmamış özellik - deaktif
public class Baslik {
    public static string Main(string baslik) {
        Regex buyukharf = new Regex(@"^[\p{Lu}\W\d]+$");
        Regex kucukharf = new Regex(@"[\p{Ll}]");
        Regex separatorRegex = new Regex(@"[^a-zA-Z\p{L}]+");

        if ((buyukharf.Match(baslik).Success) && !kucukharf.Match(baslik).Success) {
            string[] kelime = separatorRegex.Split(baslik);
            string[] modifiedWords = new string[kelime.Length];
            string[] ayiricilar = new string[kelime.Length];

            if (kelime.Length < 3) return baslik; //Üç kelimeden kısa başlıklar istisna

            MatchCollection matchCollection = Regex.Matches(baslik, "[^a-zA-Z\\p{L}]+");
            int ü = 0;
            foreach (Match match in matchCollection) {
                ayiricilar[ü] = match.Value;
                ü++;
            }

            Regex turkceHarfRegex = new Regex(@"[İıĞğÜüŞşÖöÇç]");
            Regex ingilizceHarfRegex = new Regex(@"[QqWwXx]");
            string lang = "";

            for (int k = 0; k < kelime.Length; k++) {
                if(turkceHarfRegex.IsMatch(kelime[k])) {
                    if (lang != "en") lang = "tr";
                    else lang = "belirsiz";
                } else if (ingilizceHarfRegex.IsMatch(kelime[k])) {
                    if (lang != "tr") lang = "en";
                    else lang = "belirsiz";
                }
                if (lang == "belirsiz") return baslik; //Türkçe mi İngilizce mi belli değil, geç
            }
            if (lang == "") lang = "enx"; //Belirlenemediyle İngilizce devam et

            baslik = "";
            for (int i = 0; i < kelime.Length; i++) {
                string word = kelime[i];
                if (lang=="en"||lang=="enx") modifiedWords[i] = char.ToUpper(word[0]) + word.Substring(1).ToLower(new CultureInfo("en-US"));
                else modifiedWords[i] = char.ToUpper(word[0]) + word.Substring(1).ToLower(new CultureInfo("tr-TR"));
                if(i!=0) modifiedWords[i] = Cevirmen(modifiedWords[i], lang);
                baslik += modifiedWords[i] + ayiricilar[i];
            }
            
            Regex tirnak = new Regex(@"([\u0027\u0060\u00B4\u02B9\u02BB\u02BC\u02BD\u02BE\u02BF\u02C8\u02CA\u02EE\u0301\u0313\u0314\u0315\u0341\u0343\u0374\u0384\u055A\u059C\u059D\u05F3\u1FBD\u1FBF\u2018\u2019\u201B\u2032\u2035\uA78B\uA78C\uFF07])([\p{Lu}])");
            if (tirnak.Match(baslik).Success) {
                var hmmm = tirnak.Match(baslik);
                if (lang=="en"||lang=="enx") baslik = baslik.Replace(hmmm.Groups[1].Value + hmmm.Groups[2].Value, hmmm.Groups[1].Value + hmmm.Groups[2].Value.ToLower(new CultureInfo("en-US")));
                else baslik = baslik.Replace(hmmm.Groups[1].Value + hmmm.Groups[2].Value, hmmm.Groups[1].Value + hmmm.Groups[2].Value.ToLower(new CultureInfo("tr-TR")));
            }
        }

        return baslik;
    }
    public static string Cevirmen(string word, string lang) {
        if (lang=="en"||lang=="enx") {
            if (word == "A" || word == "An" || word == "The" ||
                word == "And" || word == "Or" || word == "But" || word == "Nor" ||
                word == "In" || word == "On" || word == "At" || word == "By" || 
                word == "For" || word == "To" || word == "With" || word == "From" || 
                word == "Into" || word == "Onto" || word == "Upon" || word == "Within" || 
                word == "Without")
                return word.ToLower(new CultureInfo("en-US"));
        }
        if (lang=="tr"||lang=="enx") {
            Regex soru = new Regex(@"^M[iıuü]");
            if (soru.Match(word).Success) {

            } else if (word == "İle"||word == "Ve"||word == "Ya"||
              word == "Da"||word == "De"||word == "Ki")
                return word.ToLower(new CultureInfo("tr-TR"));
        }
        return word;
    }
}