#ToprakBot dosya yükleyici
#This program includes code adapted from the work of David (https://stackoverflow.com/users/4998865/david)
#available under the Creative Commons Attribution-ShareAlike 3.0 license (CC-BY-SA 3.0). The original code
#can be found at https://stackoverflow.com/a/41536728.
import requests
import sys

api_url = 'https://tr.wikipedia.org/w/api.php'

makine = True
if not makine:
	with open("D:\\AWB\\password.txt", "r") as file:
		password = file.read()
else:
	with open("C:\\Users\\Administrator\\Desktop\\password.txt", "r") as file:
		password = file.read()
		
USER,PASS = u'ToprakBot', password

FILENAME='D:\\ResizedImage' + sys.argv[2]
SUMMARY="Dosya çözünürlüğü düşürülüyor"
USER_AGENT='ToprakBot trwiki'

REMOTENAME=sys.argv[1]

payload = {'action': 'query', 'format': 'json', 'utf8': '',
		   'meta': 'tokens', 'type': 'login'}

r1 = requests.post(api_url, data=payload)
login_token=r1.json()['query']['tokens']['logintoken']

login_payload = {'action': 'login', 'format': 'json', 'utf8': '',
		   'lgname': USER, 'lgpassword': PASS, 'lgtoken': login_token}

r2 = requests.post(api_url, data=login_payload, cookies=r1.cookies)
cookies=r2.cookies.copy()

def get_edit_token(cookies):
		edit_token_response=requests.post(api_url, data={'action': 'query',
													'format': 'json',
													'meta': 'tokens'}, cookies=cookies)
		print("CSRF Token:", edit_token_response.json()['query']['tokens']['csrftoken'])
		return edit_token_response.json()['query']['tokens']['csrftoken']

upload_payload={'action': 'upload',
			'format':'json',
			'filename':REMOTENAME,
			'comment':SUMMARY,
			'text':'', #sayfa oluşturmayacağı için lüzumsuz
			'ignorewarnings':'1',
			'token':get_edit_token(cookies)}

files={'file': (REMOTENAME, open(FILENAME,'rb'))}

headers={'User-Agent': USER_AGENT}

upload_response=requests.post(api_url, data=upload_payload,files=files,cookies=cookies,headers=headers)
print(upload_response.text)