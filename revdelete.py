# ToprakBot dosya silici
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

REVISION_ID = sys.argv[2]
HIDE_REASON = 'Çözünürlüğü düşürülen dosyanın geçmiş sürümleri gizleniyor.'
FILE = sys.argv[1]

payload = {'action': 'query', 'format': 'json', 'utf8': '',
		   'meta': 'tokens', 'type': 'login'}

r1 = requests.post(api_url, data=payload)
login_token = r1.json()['query']['tokens']['logintoken']

login_payload = {'action': 'login', 'format': 'json', 'utf8': '',
				 'lgname': USER, 'lgpassword': PASS, 'lgtoken': login_token}

r2 = requests.post(api_url, data=login_payload, cookies=r1.cookies)
cookies = r2.cookies.copy()

def get_edit_token(cookies):
	edit_token_response = requests.post(api_url, data={'action': 'query',
													   'format': 'json',
													   'meta': 'tokens'}, cookies=cookies)
	print("CSRF Token:", edit_token_response.json()['query']['tokens']['csrftoken'])
	return edit_token_response.json()['query']['tokens']['csrftoken']

revision_delete_payload = {'action': 'revisiondelete',
						   'format': 'json',
						   'type': 'oldimage',
						   'ids': REVISION_ID,
						   'hide': 'content',
						   'reason': HIDE_REASON,
						   'target': 'Dosya:' + FILE,
						   'token': get_edit_token(cookies)}

headers = {'User-Agent': 'ToprakBot trwiki'}

revision_delete_response = requests.post(api_url, data=revision_delete_payload, cookies=cookies, headers=headers)
print(revision_delete_response.text)
