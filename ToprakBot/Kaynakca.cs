using System;
using System.Text.RegularExpressions;

public class Kaynakca
{
    public static Tuple<string, string> Main(string ArticleText)
    {
        string summary = "";

        Match kat = Regex.Match(ArticleText, @"\[\[[KkCc]ategor[yi]:");
        Match disbag = Regex.Match(ArticleText, @"==(\s*|)[Dd][Iıİi][SsŞş]\s*[Bb][Aa][ĞğGg][Ll][Aa][Nn][Tt][Iıİi][Ll][Aa][[Rr](\s*|)==");
        Match taslak = Regex.Match(ArticleText, @"{{.*(-|)[Tt]aslak(\s*|)}}");

        Regex one = new Regex(@"([Kk][Aa][Yy][Nn][Aa][Kk][ÇçCc][Aa]|==(\s*|)[Kk][Aa][Yy][Nn][Aa][Kk](\s*|)==)", RegexOptions.IgnoreCase);
        Regex two = new Regex(@"(R|r)eferences", RegexOptions.IgnoreCase);
        Regex three = new Regex(@"==\s*(R|r)eferans(lar)\s*==", RegexOptions.IgnoreCase);
        Regex four = new Regex(@"{{\s*reflist", RegexOptions.IgnoreCase);
        Regex five = new Regex(@"==\s*[Kk]aynaklar\s*==", RegexOptions.IgnoreCase);

        Regex six = new Regex(@"\=\=\s*Kaynakça\s*\=\=\r\n\[\[(Kategori|Category)\:\s*", RegexOptions.Singleline);
        Regex seven = new Regex(@"\=\=\s*Kaynakça\s*\=\=\r\n\r\n\[\[(Kategori|Category)\:\s*", RegexOptions.Singleline);

        Regex eight = new Regex(@"\{\{\s*([Kk]aynakça|[Rr]eflist|[Rr]eferences|[Rr]eference|[Kk]aynak listesi|[Rr]eferanslar|[Rr]efs)\s*(\||\}\})");
        Regex nine = new Regex(@"<\s*references\s*(\/|)\s*\>");

        if (ArticleText.Contains("<ref"))
        {
            if (!(eight.Match(ArticleText).Success || nine.Match(ArticleText).Success))
            {
                if (!(one.Match(ArticleText).Success || two.Match(ArticleText).Success || three.Match(ArticleText).Success || four.Match(ArticleText).Success || five.Match(ArticleText).Success))
                {
                    if (disbag.Success) ArticleText = ArticleText.Insert(disbag.Index, "== Kaynakça ==\n{{kaynakça|30em}}\r\n\n");
                    else if (taslak.Success) ArticleText = ArticleText.Insert(taslak.Index, "\n== Kaynakça ==\n{{kaynakça|30em}}\r\n\n");
                    else if (kat.Success) ArticleText = ArticleText.Insert(kat.Index, "== Kaynakça ==\n{{kaynakça|30em}}\r\n\n");
                    else ArticleText += "\r\n\r\n== Kaynakça ==\n{{kaynakça|30em}}";

                    ArticleText = ArticleText.Replace("\n\n== Kaynakça ==", "\n== Kaynakça ==");
                    summary += "; kaynakça başlığı ekleniyor";
                }

                if (six.Match(ArticleText).Success)
                {
                    Regex R1 = new Regex(@"\=\=\s*Kaynakça\s*\=\=");
                    ArticleText = R1.Replace(ArticleText, "== Kaynakça ==\r\n{{kaynakça|30em}}\n");
                    summary += "; kaynakça şablonu ekleniyor";
                }
                else if (seven.Match(ArticleText).Success)
                {
                    Regex R2 = new Regex(@"\=\=\s*Kaynakça\s*\=\=");
                    ArticleText = R2.Replace(ArticleText, "== Kaynakça ==\r\n{{kaynakça|30em}}");
                    summary += "; kaynakça şablonu ekleniyor";
                }
            }
        }
        return new Tuple<string, string>(ArticleText, summary);
    }
}