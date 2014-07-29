import urllib.request
import os

CUR_DIR = os.path.dirname(__file__) #Directory of the python filter

#Run by the webserver
def webFilter(text):

	#Split text into a list of words
	words = text.split()

	commonWordSearch(words)
	punctuationSearch(words)
	onlineDictSearch(words)
	nameSearch(words)

	#Merge list and capitalise words
	corrected=""

	for word in words:
		if word[0]==1:
			corrected+=word[1].title()+" "
		else:
			corrected+=word[1]+" "

	return corrected

#Uses USA census data to determine if unknown words are names
def nameSearch(words):
	for count,word in enumerate(words):
		if word[0]==2:

			file_location = os.path.join(CUR_DIR, "CensusNames/"+word[1][0]+".txt") #Open data from: https://www.drupal.org/project/namedb
			names = open(file_location)

			while True:

				line = names.readline()
				if line=="":
					break;

				if line.lower()[:-1]==word[1]:
					words[count][0]=1
					break;

#Searches online dictionary to find out if the word begins with a lowercase letter
def onlineDictSearch(words):
	for count,word in enumerate(words):
		if word[0]==2:
			try:

				response = urllib.request.urlopen("http://www.oxforddictionaries.com/definition/english/"+word[1])
				html = str(response.read())

				index = html.index('<h2 class="pageTitle">')+22
				end = index

				found_end=False
				while not found_end:
					if html[end]=='<':
						found_end=True
					else:
						end+=1

				if word[1]==html[index:end]:
					words[count][0]=0
				elif not html[index]==html[index].lower:
					words[count][0] = 1

			except:
				pass

def punctuationSearch(words):
	

def commonWordSearch(words):
	common = []
	commonFile = open("CommonWords.txt") #Open data from: http://www.englishclub.com/vocabulary/common-words-5000.htm
	while True:
		line = commonFile.readline()
		if line=="":
			break;

	for count,word in enumerate(words):
		if word in commonFile:
			words[count] = [0,word] #0=Word begins with lowercase
		elif word=="i":
			words[count] = [1,word] #1=Word begins with uppercase
		else:
			words[count] = [2,word] #2=Unknown


print(webFilter("hello alexander taylor it is a nice day"))
