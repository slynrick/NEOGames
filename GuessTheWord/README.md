# GuessTheWord
This is a game that everyone can register a word in the contract and everyone can randomly choose a word and try to guess with the tip.

## Rules:
- One word to guess per address
 
- SendTheWord() Send a word to the contract
    - word: The word you want to add
    - description: A tip for the word
    - address: The addres that send the word
	
- GetWords() Get a random word from the ones registered in the contract
    - address: The addres that calls for a random word
	
- GuessTheWord() Send a guess to the contract   
    - word: The word you guessed
	- address: The addres that is trying to guess the word
	
- GetAddressPoints() return the point of an address, doens't need to be the one that is calling the method
    - address: The target address