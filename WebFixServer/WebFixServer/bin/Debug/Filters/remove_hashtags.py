#IMPORTS
import sys



#Removes any repetative ?s,!s and .s
def removeHashtags(text):
	words = text.split()

	new = []
	for word in words:
		if not word[0]=="#":
			new.append(word)

	new_text = ' '.join(new)

	return new_text



#--------MAIN--------#

#Handle any unexpected exceptions
try:

	line = sys.stdin.readline()
	sys.stdout.write(removeHashtags(line))
	#print("")  #<-- Uncomment to view output

except:

	print("\nUnexpected error:\n",sys.exc_info()[0],"\n")
	raise