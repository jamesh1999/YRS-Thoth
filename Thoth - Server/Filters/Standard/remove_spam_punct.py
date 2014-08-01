#IMPORTS
import sys



#Removes any repetative ?s,!s and .s
def removeRepetativePunct(text):
	start=0
	erase=False
	for count,char in enumerate(text):
		if char in ['.','!','?']:
			if not erase:
				erase=True
				start=count
		elif erase:
			erase=False
			text=text[:start+1]+text[count:]
	return text



#--------MAIN--------#

#Handle any unexpected exceptions
try:
	line = sys.stdin.readline()
	sys.stdout.write(webFilter(line))
	sys.stdout.close()
	#print("")  #<-- Uncomment to view output

except:

	print("\nUnexpected error:\n",sys.exc_info()[0],"\n")
	raise

finally:
	#Close database cursor if there is an unexpected exception
	CURSOR.close()