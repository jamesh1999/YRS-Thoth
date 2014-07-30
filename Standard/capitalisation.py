#IMPORTS
import os.path,sqlite3,sys



CUR_DIR = os.path.dirname(__file__) #Directory of the python filter

CONN = sqlite3.connect('dictionary.db')
CURSOR = CONN.cursor() #Data from SCOWL http://wordlist.aspell.net/



#Removes the punctuation from the start and the end of the word
def removePunctuation(word_old):
	try:

		word=list(word_old) #Word_old is a list so a copy must be created to prevent editing the origional

		#Remove preceeding punctuation
		removed_punctuation=False
		while not removed_punctuation:

			if ord(word[1][0])<=64 or ord(word[1][0])>122:
				word[1] = word[1][1:]
			else:
				removed_punctuation=True

		#Remove trailing punctuation
		removed_punctuation=False
		while not removed_punctuation:

			if ord(word[1][-1])<=64 or ord(word[1][-1])>122:
				word[1] = word[1][:-1]
			else:
				removed_punctuation=True
		return word

	except IndexError: #Error thrown if the word contains no letters
		return ""


#Uses USA census data to determine if unknown words are names
#TODO: Merge with main database to speed up
def nameSearch(words):
	for count,word in enumerate(words):
		if word[0]==2:

			word = removePunctuation(word)

			file_location = os.path.join(CURSOR_DIR, "CensusNames/"+word[1][0]+".txt") #Open data from: https://www.drupal.org/project/namedb

			with open(file_location) as names:

				for line in names.readlines():

					if line.lower()[:-1]==word[1]:
						words[count][0] = 1
						break;


#Capitalises words at start of sentence
#TODO: different cases involving speech
def punctuationSearch(words):
	words[0][0] = 1

	for i,word in enumerate(words):
		if word[0]==0 or word[0]==2:
			if words[i-1][1][-1] in ['.','!','?'] or (words[i-1][1][-1]=='"' and words[i-1][1][-2] in ['.','!','?']):
				words[i][0] = 1


#Searches local database for instances of the word
def localDictSearch(words):

	for count,word in enumerate(words):

		word = removePunctuation(word)

		if word=="": #As this is the first step of the filter we have to check for words with no letters
			words[count][0] = 1 #Guarantees that letterless words are ignored in future
			continue;

		if word[0]==2:

			try:
				CURSOR.execute('select data from dictionary where key=?', (word[1],))
				for i in CURSOR:
					correct = i[0]
					break;

				if correct.islower(): #Dictionary returns proper nouns that match the word
					words[count][0] = 0 #Not a proper noun
				else:
					words[count][0] = 1 #Proper noun

			except UnboundLocalError:
				pass


#Calls all of the steps required to capitalise the text
#Breaks down the text and rebuilds it
def webFilter(text):

	words = []

	#Split text into a list of words
	for word in text.split():

		if word.istitle(): #If word already begins with a capital it is probably correct
			words.append([0,word])

		elif word=="i":
			words.append([1,word]) #Special case: only single letter word that needs capitalising

		else:
			words.append([2,word.lower()]) #2 denotes that it may or may not need capitalising

	#Call individual searches for the words
	localDictSearch(words)
	punctuationSearch(words)
	#nameSearch(words) All names from census data are covered by SCOWL

	#Merge list and capitalise words
	corrected=""

	for word in words:
		if word[0]==1:
			corrected+=word[1].capitalize()+" "
		else:
			corrected+=word[1]+" "

	return corrected



#Handle any unexpected exceptions
try:

	#Main loop reads and writes to standard io
	while True:
		line = sys.stdin.readline()
		sys.stdout.write(webFilter(line))
		print("")  #<-- Uncomment to view output

except:
	print("Unexpected error: "+sys.exc_info()[0])

	#Close database cursor if there is an unexpected exception
	CURSOR.close()