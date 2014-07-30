import re, collections, json


#ONLY NEEDS TO RUN ONCE IN ORDER TO UPDATE WORD LIST 


#trains the spell checker by counting occurrences of words in text
def words(text): return re.findall('[a-z]+', text.lower()) 

def train(features): #generates probability data from a large text sample
    model = collections.defaultdict(lambda: 1) 
    #default dict means that any novel word has been seen once
    for f in features:
        model[f] += 1
    return model #model[f] contains a count of how many times the word f has been seen

NWORDS = train(words(file('big.txt').read())) #big.txt contains a large number of works from project gutenburg, war and peace, wiktionary and other sources (~1.5 million words)
spell_check_model = open("spell_check_model.json","w")
spell_check_model.write(json.dumps(NWORDS))
spell_check_model.close()