using System.Text.RegularExpressions;

public class KaynakNoktalama {
    public static string Main(string ArticleText) {
        //Ünlem hariç tutuldu, tabloları kırabiliyor
        Regex one = new Regex(@"\s?((\<\s*?ref(\s*?|\s{1,}.*?^|\s*(name|group)\s*\=\s*\""?[a-zA-Z0-9ğüşöçıİĞÜŞÖÇ\:\.\?\,\s\-]*?\""?)\>.*?\<\s*?\/\s*?ref\s*?\>|\s?\<\s*?ref\s*(name|group)\s*?\=\s*?\""?[a-zA-Z0-9ğüşöçıİĞÜŞÖÇ\:\.\?\,\s\-]*?\""?\s*?\/\s*?>)+)(\s*([\.\,\:\;\?]))?", RegexOptions.Singleline);

        ArticleText=one.Replace(ArticleText, "$7$1");
        Regex dz = new Regex(@"([a-zA-Z0-9ğüşöçıİĞÜŞÖÇ(\)\'""\:\=][\,\;\.\:\?])\s*?[\,\;\.\:\?](\s*?\<ref.*?\>)", RegexOptions.Singleline);
        ArticleText=dz.Replace(ArticleText, "$1$2");

        return ArticleText;
    }
}