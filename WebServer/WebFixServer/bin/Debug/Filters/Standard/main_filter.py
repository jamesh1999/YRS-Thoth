#IMPORTS
import sqlite3,sys,re
import spellcheck



CONN = sqlite3.connect('dictionary.db')
CURSOR = CONN.cursor() #Data from SCOWL http://wordlist.aspell.net/



#Removes the punctuation from the start and the end of the word
def removePunctuation(word_old):
	return [word_old[0],re.sub('(?<![a-zA-Z])[^a-zA-Z](?![a-zA-Z])','',word_old[1])]

#Capitalises words at start of sentence
#TODO: different cases involving speech
def punctuationSearch(words):
	try:
		words[0][0] = 1
	except IndexError: #For lists with no words
		pass

	for count,word in enumerate(words):

		if word[0]==0 or word[0]==2:
			
			if words[count-1][1][-1] in ['.','!','?'] or (words[count-1][1][-1]=='"' and words[count-1][1][-2] in ['.','!','?']):
				words[count][0] = 1


#Searches local database for instances of the word
#ALSO:
#-Runs spellcheck
#-Translates internet slang
def localDictSearch(words):

	for count,word_old in enumerate(words):

		word = removePunctuation(word_old)

		if word[1]=="": #As this is the first step of the filter we have to check for words with no letters

			CURSOR.execute('select data from slang where key=?', (word_old[1],)) #Checks for emoticons

			for i in CURSOR:
				words[count][1]=i[0] #Set the word to be the first match in dictionary
				break;

			words[count][0] = 0 #Guarantees that letterless words/emoticons are ignored in future except for punctuation
			continue;

		elif word[0]==2:

			found=False #Used to tell if word was found in dictionary

			CURSOR.execute('select data from dictionary where key=?', (word[1],))

			for i in CURSOR:

				found=True

				if i[0].islower(): #Dictionary returns proper nouns that match the word
					words[count][0] = 0 #Not a proper noun
				else:
					words[count][0] = 1 #Proper noun

				break;

			if not found: #First check if it is an abbreviation/internet slang

				CURSOR.execute('select data from slang where key=?', (word[1],))

				for i in CURSOR:

					found=True

					punc=words[count][1].partition(word[1]) #Add punctuation to new word
					words[count][1] = punc[0]+i[0]+punc[2] #Replace word with translation
					words[count][0] = 0 #Guarantees that it is ignored in future except for punctuation

					break;

			if not found: #Second spellcheck the word

				word_new = spellcheck.correct(word[1])

				CURSOR.execute('select data from dictionary where key=?', (word_new,))

				for i in CURSOR:

					if i[0].islower(): #Dictionary returns proper nouns that match the word
						words[count][0] = 0 #Not a proper noun
					else:
						words[count][0] = 1 #Proper noun

					punc=words[count][1].partition(word[1]) #Add punctuation to new word
					words[count][1] = punc[0]+word_new+punc[2] #"

					break;


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

		elif not word.isupper(): #If word contains some capitals split along those capitals

			temp=""
			for letter in word:
				if letter.isupper() and not temp=="":
					words.append([2,temp.lower()])
					temp=""
				temp+=letter

			words.append([2,temp.lower()])

		else:
			words.append([2,word.lower()]) #2 denotes that it may or may not need capitalising

	#Call individual searches for the words
	localDictSearch(words)
	punctuationSearch(words)

	#Merge list and capitalise words
	new=""

	for word in words:
		if word[0]==1:
			new+=word[1].capitalize()+" "
		else:
			new+=word[1]+" "

	return new




#--------MAIN--------#

spellcheck.init() #Initialize the spellcheck .json file

#Handle any unexpected exceptions
try:

	line = sys.stdin.readline()
	sys.stdout.write(webFilter(line))
	#print("")  #<-- Uncomment to view output

except:

	print("\nUnexpected error:\n",sys.exc_info()[0],"\n")
	raise

finally:
	#Close database cursor if there is an unexpected exception
	CURSOR.close()