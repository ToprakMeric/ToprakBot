#ToprakBot Python modülü
#Burada C# ile yapmayı beceremediğim kısımları ekliyorum.
#This program includes code adapted from the work of David (https://stackoverflow.com/users/4998865/david)
#available under the Creative Commons Attribution-ShareAlike 3.0 license (CC-BY-SA 3.0). The original code
#can be found at https://stackoverflow.com/a/41536728.
import requests
import sys
def get_edit_token(cookies):
	edit_token_response=requests.post(api_url, data={'action': 'query',
		'format': 'json',
		'meta': 'tokens'}, cookies=cookies)
	#print("CSRF Token:", edit_token_response.json()['query']['tokens']['csrftoken'])
	return edit_token_response.json()['query']['tokens']['csrftoken']

api_url = 'https://tr.wikipedia.org/w/api.php'

print(sys.argv[4])
if sys.argv[4] == "1":
	with open("D:\\AWB\\password.txt", "r") as file:
		password = file.read()
elif sys.argv[4] == "0":
	with open("C:\\Users\\Administrator\\Desktop\\password.txt", "r") as file:
		password = file.read()

USER,PASS = u'ToprakBot@aws', password
USER_AGENT='ToprakBot (https://meta.wikimedia.org/wiki/User:ToprakBot; toprak@tprk.tr) Python/3'

payload = {'action': 'query', 'format': 'json', 'utf8': '',
		   'meta': 'tokens', 'type': 'login'}

r1 = requests.post(api_url, data=payload)
login_token=r1.json()['query']['tokens']['logintoken']

login_payload = {'action': 'login', 'format': 'json', 'utf8': '',
	'lgname': USER, 'lgpassword': PASS, 'lgtoken': login_token}

r2 = requests.post(api_url, data=login_payload, cookies=r1.cookies)
cookies = r2.cookies.copy()
headers = {'User-Agent': USER_AGENT}

if sys.argv[3] == "upload":
	FILENAME='D:\\ResizedImage' + sys.argv[2]
	SUMMARY="Dosya çözünürlüğü düşürülüyor"
	REMOTENAME = sys.argv[1]

	upload_payload = {'action': 'upload',
		'format': 'json',
		'filename': REMOTENAME,
		'comment': SUMMARY,
		'text': '',  # sayfa oluşturmayacağı için lüzumsuz
		'ignorewarnings': '1',
		'token': get_edit_token(cookies)}

	files = {'file': (REMOTENAME, open(FILENAME, 'rb'))}
	upload_response = requests.post(api_url, data=upload_payload, files=files, cookies=cookies, headers=headers)
	print("Yüklendi")
	#print(upload_response.text)

elif sys.argv[3] == "revdel":
	REVISION_ID = sys.argv[2]
	HIDE_REASON = 'Çözünürlüğü düşürülen dosyanın geçmiş sürümleri gizleniyor.'
	FILE = sys.argv[1]

	revision_delete_payload = {'action': 'revisiondelete',
		'format': 'json',
		'type': 'oldimage',
		'ids': REVISION_ID,
		'hide': 'content',
		'reason': HIDE_REASON,
		'target': 'Dosya:' + FILE,
		'token': get_edit_token(cookies)}

	revision_delete_response = requests.post(api_url, data=revision_delete_payload, cookies=cookies, headers=headers)
	print("Sürüm gizlendi")
	#print(revision_delete_response.text)
