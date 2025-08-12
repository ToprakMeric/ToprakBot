"""
Bu betik, Türkçe Vikipedi'deki anlık bekleyen değişiklik sayısını toplar, verileri JSON
formatında ilgili (Kullanıcı:ToprakBot/Bekleyen değişiklikler/data) viki sayfasında saklar
ve çeşitli zaman aralıkları için (gün, hafta, ay, yıl) grafiksel zaman çizelgesi (timeline)
oluşturup ilgili (Kullanıcı:ToprakBot/Bekleyen değişiklikler) viki sayfasına kaydeder.
"""
import pywikibot
import json
from datetime import datetime
from datetime import timedelta

def fetch_pending_changes_count(site):
    params = {
        'action': 'query',
        'list': 'oldreviewedpages',
        'orlimit': '5000',
        'format': 'json'
    }
    total_count = 0

    while True:
        data = site.simple_request(**params).submit()
        pages = data.get('query', {}).get('oldreviewedpages', [])
        total_count += len(pages)

        if 'continue' in data:
            params.update(data['continue'])
        else:
            break
    return total_count

def update_wiki_page(site, page_title, count):
    page = pywikibot.Page(site, page_title)
    text = page.text
    if text.strip():
        try:
            data = json.loads(text)
        except json.JSONDecodeError:
            data = {}
    else:
        data = {}

    now = datetime.utcnow()
    date_str = now.strftime("%Y-%m-%d")
    hour_str = now.strftime("%H")

    if date_str not in data:
        data[date_str] = {}

    data[date_str][hour_str] = count

    new_text = json.dumps(data, ensure_ascii=False, indent=2)
    page.text = new_text
    page.save(summary=f"Saatlik bekleyen değişiklik sayısı güncellemesi: {count}")

def read_json_data(site, data_page_title):
    page = pywikibot.Page(site, data_page_title)
    text = page.text
    if not text.strip():
        return {}
    try:
        return json.loads(text)
    except json.JSONDecodeError:
        return {}

def choose_period_max(max_value):
    thresholds = [500, 1000, 2500, 5000, 10000, 15000]
    for t in thresholds:
        if max_value <= t:
            return t
    # Eğer max_value 15000'den büyükse, 5000'in katı olarak yuvarla yukarı
    multiple = ((max_value + 4999) // 5000) * 5000
    return multiple

def generate_timeline_for_day(data_for_day):
    color_ids = [
        "green1", "green2", "green3", "green4",
        "yellow1", "orange1", "orange2", "orange3",
        "red1", "red2"
    ]

    def get_color_id(value):
        if value is None or value <= 0:
            return "green1"
        index = int((min(value, 10000) - 500) / (10000 - 500) * (len(color_ids) - 1))
        index = max(0, min(index, len(color_ids) - 1))
        return color_ids[index]

    max_val = max([v for v in data_for_day.values() if isinstance(v, int)], default=0)
    period_max = choose_period_max(max_val)

    timeline_lines = [
        "<timeline>",
        "Colors =",
        "  id:green1\tvalue:rgb(0,0.39,0)",
        "  id:green2\tvalue:rgb(0.10,0.39,0)",
        "  id:green3\tvalue:rgb(0.20,0.38,0)",
        "  id:green4\tvalue:rgb(0.30,0.32,0)",
        "  id:yellow1\tvalue:rgb(0.40,0.26,0)",
        "  id:orange1\tvalue:rgb(0.50,0.22,0)",
        "  id:orange2\tvalue:rgb(0.60,0.18,0)",
        "  id:orange3\tvalue:rgb(0.70,0.15,0)",
        "  id:red1\tvalue:rgb(0.80,0.10,0.10)",
        "  id:red2\tvalue:rgb(0.54,0,0)",
        "",
        "ImageSize =  width:auto height:250 barincrement:45",
        "PlotArea  = left:50 bottom:20 top:10 right:10",
        "AlignBars = justify",
        "DateFormat=yyyy",
        f"Period    = from:0 till:{period_max}",
        "TimeAxis  = orientation:vertical",
        f"ScaleMajor=increment:{max(1, period_max // 10)} start:0",
        f"ScaleMinor=increment:{max(1, period_max // 40)} start:0",
        "PlotData  = ",
        " width:25"
    ]

    for hour in range(24):
        hh = f"{hour:02d}"
        value = data_for_day.get(hh)
        color_id = get_color_id(value)
        timeline_lines.append(f" color:{color_id}")
        if value is None:
            timeline_lines.append(f" bar:{hh}.00")
        else:
            timeline_lines.append(f" bar:{hh}.00 from:start till:{value}")

    timeline_lines.append("</timeline>")
    return "\n".join(timeline_lines)

def save_timeline_page(site, timeline_page_title, timeline_text):
    page = pywikibot.Page(site, timeline_page_title)
    page.text = timeline_text
    page.save(summary="Bekleyen değişiklikler timeline güncellemesi")


def get_daily_zero_hour_values(data, start_date, days=7):
    daily_values = {}
    for i in range(days):
        day = start_date - timedelta(days=i)
        day_str = day.strftime("%Y-%m-%d")
        day_data = data.get(day_str, {})
        zero_hour_val = day_data.get("00")  # saat 00 değeri
        if isinstance(zero_hour_val, int):
            daily_values[day_str] = zero_hour_val
        else:
            daily_values[day_str] = 0
    return daily_values


def generate_timeline_for_week(daily_zero_values):
    max_val = max(daily_zero_values.values(), default=0)
    period_max = choose_period_max(max_val)

    color_ids = [
        "green1", "green2", "green3", "green4",
        "yellow1", "orange1", "orange2", "orange3",
        "red1", "red2"
    ]

    def get_color_id(value):
        if value is None or value <= 0:
            return "green1"
        index = int((min(value, 10000) - 500) / (10000 - 500) * (len(color_ids) - 1))
        index = max(0, min(index, len(color_ids) - 1))
        return color_ids[index]

    timeline_lines = [
        "<timeline>",
        "Colors =",
        "  id:green1\tvalue:rgb(0,0.39,0)",
        "  id:green2\tvalue:rgb(0.10,0.39,0)",
        "  id:green3\tvalue:rgb(0.20,0.38,0)",
        "  id:green4\tvalue:rgb(0.30,0.32,0)",
        "  id:yellow1\tvalue:rgb(0.40,0.26,0)",
        "  id:orange1\tvalue:rgb(0.50,0.22,0)",
        "  id:orange2\tvalue:rgb(0.60,0.18,0)",
        "  id:orange3\tvalue:rgb(0.70,0.15,0)",
        "  id:red1\tvalue:rgb(0.80,0.10,0.10)",
        "  id:red2\tvalue:rgb(0.54,0,0)",
        "",
        f"ImageSize =  width:auto height:250 barincrement:120",
        "PlotArea  = left:50 bottom:20 top:10 right:10",
        "AlignBars = justify",
        "DateFormat=yyyy",
        f"Period    = from:0 till:{period_max}",
        "TimeAxis  = orientation:vertical",
        f"ScaleMajor=increment:{max(1, period_max // 10)} start:0",
        f"ScaleMinor=increment:{max(1, period_max // 40)} start:0",
        "PlotData  = ",
        " width:25"
    ]

    days_sorted = sorted(daily_zero_values.keys())
    for day_str in days_sorted:
        val = daily_zero_values[day_str]
        color_id = get_color_id(val)
        timeline_lines.append(f" color:{color_id}")
        timeline_lines.append(f" bar:{day_str} from:start till:{val}")

    timeline_lines.append("</timeline>")
    return "\n".join(timeline_lines)

def get_daily_zero_hour_values_for_month(data, start_date, days=30):
    daily_values = {}
    for i in range(days):
        day = start_date - timedelta(days=i)
        day_str = day.strftime("%Y-%m-%d")
        day_data = data.get(day_str, {})
        zero_hour_val = day_data.get("00")  # saat 00 değeri
        if isinstance(zero_hour_val, int):
            daily_values[day_str] = zero_hour_val
        else:
            daily_values[day_str] = 0
    return daily_values


def generate_timeline_for_month(daily_zero_values):
    max_val = max(daily_zero_values.values(), default=0)
    period_max = choose_period_max(max_val)

    color_ids = [
        "green1", "green2", "green3", "green4",
        "yellow1", "orange1", "orange2", "orange3",
        "red1", "red2"
    ]

    def get_color_id(value):
        if value is None or value <= 0:
            return "green1"
        index = int((min(value, 10000) - 500) / (10000 - 500) * (len(color_ids) - 1))
        index = max(0, min(index, len(color_ids) - 1))
        return color_ids[index]

    timeline_lines = [
        "<timeline>",
        "Colors =",
        "  id:green1\tvalue:rgb(0,0.39,0)",
        "  id:green2\tvalue:rgb(0.10,0.39,0)",
        "  id:green3\tvalue:rgb(0.20,0.38,0)",
        "  id:green4\tvalue:rgb(0.30,0.32,0)",
        "  id:yellow1\tvalue:rgb(0.40,0.26,0)",
        "  id:orange1\tvalue:rgb(0.50,0.22,0)",
        "  id:orange2\tvalue:rgb(0.60,0.18,0)",
        "  id:orange3\tvalue:rgb(0.70,0.15,0)",
        "  id:red1\tvalue:rgb(0.80,0.10,0.10)",
        "  id:red2\tvalue:rgb(0.54,0,0)",
        "",
        f"ImageSize =  width:auto height:250 barincrement:35",
        "PlotArea  = left:50 bottom:20 top:10 right:10",
        "AlignBars = justify",
        "DateFormat=yyyy",
        f"Period    = from:0 till:{period_max}",
        "TimeAxis  = orientation:vertical",
        f"ScaleMajor=increment:{max(1, period_max // 10)} start:0",
        f"ScaleMinor=increment:{max(1, period_max // 40)} start:0",
        "PlotData  = ",
        " width:10"
    ]

    days_sorted = sorted(daily_zero_values.keys())
    for day_str in days_sorted:
        val = daily_zero_values[day_str]
        color_id = get_color_id(val)
        timeline_lines.append(f" color:{color_id}")
        timeline_lines.append(f" bar:{day_str} from:start till:{val}")

    timeline_lines.append("</timeline>")
    return "\n".join(timeline_lines)

def get_daily_zero_hour_values_for_year(data, start_date, days=365):
    daily_values = {}
    for i in range(days):
        day = start_date - timedelta(days=i)
        day_str = day.strftime("%Y-%m-%d")
        day_data = data.get(day_str, {})
        zero_hour_val = day_data.get("00")
        if isinstance(zero_hour_val, int):
            daily_values[day_str] = zero_hour_val
        else:
            daily_values[day_str] = 0
    return daily_values

def generate_timeline_for_year(daily_zero_values):
    max_val = max(daily_zero_values.values(), default=0)
    period_max = choose_period_max(max_val)

    color_ids = [
        "green1", "green2", "green3", "green4",
        "yellow1", "orange1", "orange2", "orange3",
        "red1", "red2"
    ]

    def get_color_id(value):
        if value is None or value <= 0:
            return "green1"
        index = int((min(value, 10000) - 500) / (10000 - 500) * (len(color_ids) - 1))
        index = max(0, min(index, len(color_ids) - 1))
        return color_ids[index]

    timeline_lines = [
        "<timeline>",
        "Colors =",
        "  id:green1\tvalue:rgb(0,0.39,0)",
        "  id:green2\tvalue:rgb(0.10,0.39,0)",
        "  id:green3\tvalue:rgb(0.20,0.38,0)",
        "  id:green4\tvalue:rgb(0.30,0.32,0)",
        "  id:yellow1\tvalue:rgb(0.40,0.26,0)",
        "  id:orange1\tvalue:rgb(0.50,0.22,0)",
        "  id:orange2\tvalue:rgb(0.60,0.18,0)",
        "  id:orange3\tvalue:rgb(0.70,0.15,0)",
        "  id:red1\tvalue:rgb(0.80,0.10,0.10)",
        "  id:red2\tvalue:rgb(0.54,0,0)",
        "",
        f"ImageSize =  width:auto height:250 barincrement:3",
        "PlotArea  = left:50 bottom:20 top:10 right:10",
        "AlignBars = justify",
        "DateFormat=yyyy",
        f"Period    = from:0 till:{period_max}",
        "TimeAxis  = orientation:vertical",
        f"ScaleMajor=increment:{max(1, period_max // 10)} start:0",
        f"ScaleMinor=increment:{max(1, period_max // 40)} start:0",
        "PlotData  = ",
        " width:2"
    ]

    days_sorted = sorted(daily_zero_values.keys())
    for day_str in days_sorted:
        val = daily_zero_values[day_str]
        color_id = get_color_id(val)
        timeline_lines.append(f" color:{color_id}")
        timeline_lines.append(f" bar:{day_str} from:start till:{val}")

    timeline_lines.append("</timeline>")
    return "\n".join(timeline_lines)

def main():
    site = pywikibot.Site('tr', 'wikipedia')
    site.login()

    count = fetch_pending_changes_count(site)
    print(f"Toplam bekleyen değişiklik sayfası sayısı: {count}")

    data_page = "Kullanıcı:ToprakBot/Bekleyen_değişiklikler/data"
    timeline_page = "Kullanıcı:ToprakBot/Bekleyen değişiklikler"
    update_wiki_page(site, data_page, count)

    data = read_json_data(site, data_page)

    now = datetime.utcnow()
    today_str = now.strftime("%Y-%m-%d")
    yesterday = now - timedelta(days=1)
    yesterday_str = yesterday.strftime("%Y-%m-%d")

    # Bugünün ve dünkü veriler
    data_today = data.get(today_str, {})
    data_yesterday = data.get(yesterday_str, {})

    # Haftalık veriler: Bugünden geriye 7 gün (bu hafta)
    weekly_start = now
    this_week_data = get_daily_zero_hour_values(data, weekly_start, days=7)

    # Geçen hafta: 8-14 gün öncesi
    last_week_start = now - timedelta(days=7)
    last_week_data = get_daily_zero_hour_values(data, last_week_start, days=7)

    # Bu ay (son 30 gün)
    this_month_data = get_daily_zero_hour_values_for_month(data, now, days=30)

    # Geçen ay (önceki 30 gün)
    last_month_start = now - timedelta(days=30)
    last_month_data = get_daily_zero_hour_values_for_month(data, last_month_start, days=30)

    # Son 1 yıl
    this_year_data = get_daily_zero_hour_values_for_year(data, now, days=365)

    timeline_text = (
        "{{/başlık}}\n"
        f"== Bugün ({today_str}) ==\n"
        f"{generate_timeline_for_day(data_today)}\n\n"
        f"== Dün ({yesterday_str}) ==\n"
        f"{generate_timeline_for_day(data_yesterday)}\n\n"
        "== Bu hafta ==\n"
        f"{generate_timeline_for_week(this_week_data)}\n\n"
        "== Geçen hafta ==\n"
        f"{generate_timeline_for_week(last_week_data)}\n\n"
        "== Bu ay ==\n"
        f"{generate_timeline_for_month(this_month_data)}\n\n"
        "== Geçen ay ==\n"
        f"{generate_timeline_for_month(last_month_data)}\n\n"
        "== Son bir yıl ==\n"
        f"{generate_timeline_for_year(this_year_data)}"
    )

    save_timeline_page(site, timeline_page, timeline_text)


if __name__ == "__main__":
    main()
