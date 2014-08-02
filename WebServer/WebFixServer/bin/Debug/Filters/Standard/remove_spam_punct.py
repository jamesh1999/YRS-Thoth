#IMPORTS
import sys



#Removes any repetative ?s,!s and .s
def removeRepetativePunct(text):
	start=0
	erase=False

	for count,char in enumerate(text):

		if char in ['.','!','?']:

			if not erase: #Start selecting texxt to be erased
				erase=True
				start=count

		elif erase: #Erase selected punctuation except for the first one
			erase=False
			text=text[:start+1]+text[count:]

	return text



#--------MAIN--------#

#Handle any unexpected exceptions
try:

	line = sys.stdin.readline()
	sys.stdout.write(removeRepetativePunct(line))
	#print("")  #<-- Uncomment to view output

except:

	print("\nUnexpected error:\n",sys.exc_info()[0],"\n")
	raise