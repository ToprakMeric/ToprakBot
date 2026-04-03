<p align="center">
  <strong>Automated Wikipedia maintenance bot for Turkish, Azerbaijani, and Georgian Wikipedias.</strong>
</p>

<p align="center">
  <a href="https://tr.wikipedia.org/wiki/Kullanıcı:ToprakBot"><img src="https://img.shields.io/badge/Wikipedia-Türkçe-blue?logo=wikipedia" alt="trwiki" /></a>
  <a href="https://az.wikipedia.org/wiki/İstifadəçi:ToprakBot"><img src="https://img.shields.io/badge/Wikipedia-Azərbaycanca-green?logo=wikipedia" alt="azwiki" /></a>
  <a href="https://ka.wikipedia.org/wiki/მომხმარებელი:ToprakBot"><img src="https://img.shields.io/badge/Wikipedia-ქართული-red?logo=wikipedia" alt="kawiki" /></a>
</p>

---

## What It Does

ToprakBot is a Wikipedia bot that automatically fixes formatting and style issues on articles. It runs on three language editions:

| Wiki | Bot Page |
|------|----------|
| [Turkish](https://tr.wikipedia.org/wiki/Kullanıcı:ToprakBot) | `tr.wikipedia.org` |
| [Azerbaijani](https://az.wikipedia.org/wiki/İstifadəçi:ToprakBot) | `az.wikipedia.org` |
| [Georgian](https://ka.wikipedia.org/wiki/მომხმარებელი:ToprakBot) | `ka.wikipedia.org` |

Built with **C# (.NET Framework 4.7.2)** using the [WikiFunctions](https://en.wikipedia.org/wiki/Wikipedia:WikiFunctions) library, with a **Python 3** helper for file upload/revision deletion operations.

---

## Features

- **References cleanup** — Adds missing `{{kaynakça}}` / `{{istinad siyahısı}}` / `{{სქოლიო}}` sections when `<ref>` tags are present without a reference list
- **Notes list** — Inserts `{{not listesi}}` when `{{efn}}` / `{{adn}}` templates are used without a corresponding notes section
- **Date localization** — Converts English citation dates (`January 23, 2021` → `23 Ocak 2021`) and edition ordinals (`1st` → `1.`) to Turkish
- **Title case** — Converts ALL-CAPS citation titles to proper title case with language detection (Turkish/English) and exception handling
- **Image thumbnails** — Replaces pixel dimensions (`|250px`) with responsive `|upright=` values based on actual image aspect ratio
- **Fair-use image reduction** — Resizes fair-use images to 300px, re-uploads, and hides old revisions
- **Invisible characters** — Detects and makes visible (or removes) 12+ types of invisible Unicode characters (non-breaking spaces, zero-width spaces, BOM, etc.)
- **Bare URL tagging** — Adds maintenance templates for untemplated URLs in references
- **Wikiformat fixes** — Template redirects, parameter renames, syntax corrections, reference deduplication, category/link fixes via WikiFunctions Parsers
- **Namespace normalization** — `File:`/`Image:` → `Dosya:`, `thumb` → `küçükresim`, `Referanslar` → `Kaynakça`, etc.
- **Protection template cleanup** — Removes misplaced protection templates from unprotected pages

---

## Project Structure

```
ToprakBot/
├── ToprakBot/                  # C# project
│   ├── Program.cs              # Entry point, API utilities, configuration
│   ├── trwiki.cs               # Turkish Wikipedia orchestrator
│   ├── azwiki.cs               # Azerbaijani Wikipedia orchestrator
│   ├── kawiki.cs               # Georgian Wikipedia orchestrator
│   ├── Kaynakca.cs             # References section (TR/AZ/KA)
│   ├── NotListesi.cs           # Notes list (TR)
│   ├── KaynakCevir.cs          # Date & edition translation (TR)
│   ├── Baslik.cs               # Title case conversion (TR)
│   ├── Upright.cs              # Image upright normalization
│   ├── YalinURL.cs             # Bare URL detection (TR)
│   ├── GorunmezKarakter.cs     # Invisible character handling (TR/AZ)
│   ├── File.cs                 # Fair-use image processing pipeline
│   └── ToprakBot.csproj        # .NET Framework 4.7.2
├── main.py                     # Python helper: file upload & revision delete
├── LICENSE.txt                 # GPL-2.0
└── README.md
```

---

## How It Works

1. **Fetch pages** — Retrieves newly created articles from the MediaWiki API (or a local text file)
2. **Skip protected/deleted** — Pages with deletion templates or non-main namespaces are ignored
3. **Apply transformations** — Each page goes through the editing pipeline (formatting, references, dates, invisible chars, etc.)
4. **Save if changed** — Only saves when actual changes were made, with a descriptive edit summary
5. **Log** — Writes edited page titles to daily log files

---

## Dependencies

| Package | Purpose |
|---------|---------|
| [WikiFunctions](https://en.wikipedia.org/wiki/Wikipedia:WikiFunctions) | MediaWiki API & wikitext parsing (AutoWikiBrowser engine) |
| Newtonsoft.Json 13.0.4 | JSON handling for API responses |
| SixLabors.ImageSharp 3.1.12 | Image processing & resizing |
| Python `requests` | File upload & revision deletion (C# workaround) |

---

## Configuration

Key settings in `Program.cs`:

| Field | Default | Description |
|-------|---------|-------------|
| `manual` | `true` | `true` = read page list from file, `false` = fetch from API |
| `makine` | `false` | Toggles between local and server file paths |
| `songun` | `1` | Days to look back for new pages |

Requires `password.txt` (bot credentials) and optionally `liste.txt` (manual page list) and `WikiFunctions.dll`.

---

## License

[GNU General Public License v2.0](LICENSE.txt)

---

## Contact

- **Author**: ToprakM
- **Email**: toprak@tprk.tr
- **Meta page**: [meta.wikimedia.org/wiki/User:ToprakBot](https://meta.wikimedia.org/wiki/User:ToprakBot)
