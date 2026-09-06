using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using WikiFunctions.API;

public class Trwikt {
	private enum TitleResult {
		EnglishPageMissing,
		NoAlsoTemplate,
		NoTurkishAlsoPage,
		NoChange,
		Changed
	}

	public static ApiEdit editor = new ApiEdit("https://" + ToprakBot.wikt + ".org/w/");
	private static readonly HttpClient HttpClient = new HttpClient();
	private const int PagesPerRun = 10000;
	private const string ProgressPageTitle = "User:ToprakBot/Liste";
	private const string SeeAlsoPattern = @"\{\{\s*(?:see\s+also|also|bak|bakınız|ayrıca\s+bakınız)\s*\|([^}]+)\}\}";
	private const string BakinizTemplatePattern = @"\{\{\s*(?:bakınız|bak|also|see\s+also|ayrıca\s+bakınız)\s*\|([^{}]*)\}\}";
	private static string lastScannedTitle;

	static Trwikt() {
		HttpClient.DefaultRequestHeaders.UserAgent.ParseAdd(ToprakBot.userAgent);
	}

	public static async Task trwikt() {
		try {
			ToprakBot.login(editor);
		} catch(Exception ex) { ToprakBot.LogException("WT01", ex); }

		string lastTitle;
		List<string> titles;
		try {
			lastTitle = (editor.Open(ProgressPageTitle) ?? "").Trim();
			titles = await GetTitlesFromLastTitle(lastTitle);
		} catch(Exception ex) {
			ToprakBot.LogException("WT02", ex);
			return;
		}
		if (titles.Count == 0) return;

		for (int index = 0; index < titles.Count; index++) {
			string title = titles[index];
			TitleResult result;
			try {
				result = await ProcessTitle(title);
			} catch(Exception ex) {
				ToprakBot.LogException("WT03", ex);
				continue;
			}

			if (result == TitleResult.EnglishPageMissing) Console.ForegroundColor = ConsoleColor.Yellow;
			else if (result == TitleResult.Changed) Console.ForegroundColor = ConsoleColor.Green;
			else Console.ForegroundColor = ConsoleColor.Red;

			Console.WriteLine((index + 1) + "/" + titles.Count + ": " + title);
		}
		Console.ForegroundColor = ConsoleColor.White;

		if (!string.IsNullOrEmpty(lastScannedTitle)) {
			try {
				editor.Open(ProgressPageTitle);
				editor.Save(lastScannedTitle, "Son taranan madde: " + lastScannedTitle, true, WatchOptions.NoChange);
			} catch(Exception ex) { ToprakBot.LogException("WT04", ex); }
		}
		Console.WriteLine("Son taranan sayfa: " + lastScannedTitle);
	}

	private static async Task<TitleResult> ProcessTitle(string title) {
		if (!(await GetExistingPages("en.wiktionary", new[] { title })).Contains(title))
			return TitleResult.EnglishPageMissing;

		string englishPageText = await GetPageText("en.wiktionary", title);

		List<List<string>> alsoGroups = ExtractAlsoGroups(englishPageText);
		if (alsoGroups.Count == 0) return TitleResult.NoAlsoTemplate;

		List<List<string>> existingTurkishGroups = new List<List<string>>();
		foreach (List<string> alsoGroup in alsoGroups) {
			List<string> candidateTitles = alsoGroup
				.Where(alsoTitle => !string.Equals(title.Trim(), alsoTitle.Trim(), StringComparison.Ordinal))
				.ToList();
			HashSet<string> existingTitles = await GetExistingPages(ToprakBot.wikt, candidateTitles);
			List<string> existingGroup = new List<string>();
			foreach (string alsoTitle in candidateTitles) {
				if (existingTitles.Contains(alsoTitle)) existingGroup.Add(alsoTitle);
			}

			if (existingGroup.Count > 0) existingTurkishGroups.Add(existingGroup);
		}
		if (existingTurkishGroups.Count == 0) return TitleResult.NoTurkishAlsoPage;

		return AddBakinizTemplates(title, existingTurkishGroups)
			? TitleResult.Changed
			: TitleResult.NoChange;
	}

	private static List<List<string>> ExtractAlsoGroups(string pageText) {
		List<List<string>> alsoGroups = new List<List<string>>();
		MatchCollection matches = Regex.Matches(pageText ?? "", SeeAlsoPattern, RegexOptions.IgnoreCase);

		foreach (Match match in matches) {
			List<string> group = new List<string>();
			foreach (string value in match.Groups[1].Value.Split('|')) {
				string title = value.Trim();
				if (!string.IsNullOrEmpty(title) && !group.Contains(title)) group.Add(title);
			}

			if (group.Count > 0) alsoGroups.Add(group);
		}

		return alsoGroups;
	}

	private static bool AddBakinizTemplates(string title, List<List<string>> alsoGroups) {
	string pageText = editor.Open(title) ?? "";
	bool changed = false;

	foreach (List<string> alsoGroup in alsoGroups) {
		Match existingTemplate = FindBakinizTemplate(pageText, alsoGroup);
		
		if (existingTemplate == null) {
			string newTemplate = "{{bakınız|" + string.Join("|", alsoGroup) + "}}";
			pageText = newTemplate + "\n\n" + pageText;
			changed = true;
			continue;
		}

		List<string> existingParameters = existingTemplate.Groups[1].Value
			.Split('|')
			.Select(parameter => parameter.Trim())
			.Where(parameter => !string.IsNullOrEmpty(parameter))
			.ToList();

		List<string> missingParameters = alsoGroup
			.Where(parameter => !existingParameters.Any(existing =>
				string.Equals(existing, parameter, StringComparison.OrdinalIgnoreCase)))
			.ToList();

		if (missingParameters.Count > 0) {
			int closingBracesIndex = existingTemplate.Value.LastIndexOf("}}", StringComparison.Ordinal);
			string replacement = existingTemplate.Value.Substring(0, closingBracesIndex) +
				"|" + string.Join("|", missingParameters) + "}}";
			
			pageText = pageText.Replace(existingTemplate.Value, replacement);
			changed = true;
		}
	}

	if (!changed) return false;

	editor.Save(pageText, "İngilizce Vikisözlük'teki also bağlantıları eklendi", true, WatchOptions.NoChange);
	return true;
}

private static Match FindBakinizTemplate(string pageText, List<string> requiredParameters) {
	MatchCollection matches = Regex.Matches(pageText, BakinizTemplatePattern, RegexOptions.IgnoreCase);
	
	if (matches.Count > 0) {
		return matches[0];
	}

	return null;
}

	private static async Task<List<string>> GetTitlesFromLastTitle(string lastTitle) {
		List<string> titles = new List<string>();
		string continueValue = null;
		bool firstRequest = true;

		do {
				string apiUrl = "https://" + ToprakBot.wikt + ".org/w/api.php" +
					"?action=query&list=allpages&apnamespace=0&aplimit=1000&format=json";

				if (!firstRequest && !string.IsNullOrEmpty(continueValue))
					apiUrl += "&apcontinue=" + Uri.EscapeDataString(continueValue);
				else if (firstRequest && !string.IsNullOrEmpty(lastTitle))
					apiUrl += "&apfrom=" + Uri.EscapeDataString(lastTitle);

				string json = await HttpClient.GetStringAsync(apiUrl);
				JObject response = JObject.Parse(json);
				JToken pages = response["query"]?["allpages"];

				if (pages != null) {
					foreach (JToken page in pages) {
						string title = (string)page["title"];
						if (string.IsNullOrEmpty(title)) continue;
						lastScannedTitle = title;
						if (!string.Equals(title, lastTitle, StringComparison.Ordinal)) titles.Add(title);
						if (titles.Count >= PagesPerRun) break;
					}
				}

				continueValue = (string)response["continue"]?["apcontinue"];
				firstRequest = false;
		} while (titles.Count < PagesPerRun && !string.IsNullOrEmpty(continueValue));

		//son girdiye vardıysak baştan başlayalım
		if(titles.Count == 0 && !string.IsNullOrEmpty(lastTitle)) {
			lastScannedTitle = null;
			return await GetTitlesFromLastTitle("");
		}

		return titles;
	}

	private static async Task<HashSet<string>> GetExistingPages(string wiki, IEnumerable<string> titles) {
		HashSet<string> existingPages = new HashSet<string>(StringComparer.Ordinal);
		List<string> titleList = titles
			.Where(title => !string.IsNullOrEmpty(title))
			.Distinct(StringComparer.Ordinal)
			.ToList();

		for (int offset = 0; offset < titleList.Count; offset += 50) {
			List<string> batch = titleList.Skip(offset).Take(50).ToList();
			JToken pages = (await QueryWiki(wiki, "&prop=info&titles=" +
				Uri.EscapeDataString(string.Join("|", batch))))?["pages"];
			if (pages == null) continue;

			foreach (JToken page in pages) {
				if (page["missing"] == null) existingPages.Add((string)page["title"]);
			}
		}

		return existingPages;
	}

	private static async Task<string> GetPageText(string wiki, string title) {
		JToken page = (await QueryWiki(wiki, "&prop=revisions&rvprop=content&rvslots=main&titles=" +
			Uri.EscapeDataString(title)))?["pages"]?[0];
		return (string)page?["revisions"]?[0]?["slots"]?["main"]?["content"] ?? "";
	}

	private static async Task<JToken> QueryWiki(string wiki, string query) {
		string apiUrl = "https://" + wiki + ".org/w/api.php?action=query&format=json&formatversion=2" + query;
		JObject response = JObject.Parse(await HttpClient.GetStringAsync(apiUrl));
		return response["query"];
	}

}