# Console Games

Emulates some games played with various letter dice

Currently supported games:

---
## Boggle

### Syntax
	boggle [Type] [Options]

### Type
	- classic (default)
	- big
	- deluxe
	- superbig
	- new
	- challenge

### Options
	-p | --play              Play the game
	-d | --dict  <filename>  Specify a dictionary
	                         <filename> should be a file with valid words - 1 per line
	-t | --timer <secs>      Set the countdown timer length in seconds (default 180)
	-v | --verbose           Display valid paths as you type the words

### Gameplay
Type the word you want to add and then press the `Enter` key.

If you have the `verbose` option set the dice will light up on the board if a word can be made.

When the timer runs out your score will be tallied.

You can exit the game at any time by pressing `Escape` and your score will still be tallied.

---
## Q-Less

### Syntax
	qless [Options]

### Options
	-p | --play          Play the game
	-d | --dict  <filename>  Specify a dictionary
	                         <filename> should be a file with valid words - 1 per line
	-v | --verbose       Show the vowels and consonants

### Gameplay
Whilst playing the game you use the following keys

`A` - `Z` select a die from the rack. Repeated presses choose the next letter on the rack that is the same.

Move the dice around the board using the arrow keys.

When you think you have solved the puzzle, press the `Enter` key.

You can exit the puzzle at any time by pressing `Escape`.

### To be implemented
- The board needs to be checked to ensure that all dice are in 1 contiguous block.

---