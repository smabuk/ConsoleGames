using static Smab.DiceAndTiles.BoggleDice.BoggleType;

namespace ConsoleGames;

public sealed class Boggle {
	private const int DieDisplayWidth  =    5;
	private const int DieDisplayHeight =    3;
	private const int OneSecond        = 1000;

	private static readonly TimeSpan RedZone = new(0, 0, 10);

	private readonly BoggleDice   _boggleDice;
	private          List<Slot>   _board       = new();
	private        List<string>   _words       = new(); 
	private  DictionaryOfWords?   _dictionary;
	private long                  _timerStart;
	private int                   _bottomRow;
	private int                   _topRow      = int.MinValue;

	public Boggle(BoggleDice.BoggleType type, string? filename) {
		Type = type;

		_boggleDice = new(Type);
		_boggleDice.ShakeAndFillBoard();

		_board = _boggleDice
			.Board
			.Select((die, index) => new Slot(die.FaceValue.Display, index % _boggleDice.BoardSize, index / _boggleDice.BoardSize))
			.ToList();

		if (string.IsNullOrWhiteSpace(filename) is false) {
			_dictionary = new DictionaryOfWords(filename);
		}
	}

	public  TimeSpan              GameLength { get; set; }  = new(0, 3, 0);
	public  BoggleDice.BoggleType Type       { get; init; } = Classic4x4;
	public  bool                  Verbose    { get; set; }  = false;
	private TimeSpan              TimeRemaining => GameLength.Subtract(Stopwatch.GetElapsedTime(_timerStart));

	public void Play() {
		string currentWord = "";
		
		Console.WriteLine();
		(int _, _bottomRow) = Console.GetCursorPosition();
		_timerStart = Stopwatch.GetTimestamp();
		
		while (TimeRemaining.Ticks > 0) {
			DisplayBoard(currentWord);
			DisplayWordList();

			ConsoleKey key = DisplayAndGetInput(_bottomRow, currentWord);
			if (key == ConsoleKey.Escape) {
				break;
			} else if (key == ConsoleKey.Enter && currentWord.Length > 0) {
				_words.Add(currentWord);
				currentWord = "";
			} else if (key == ConsoleKey.Backspace && currentWord.Length > 0) {
				currentWord = currentWord[..^1];
			} else if (key >= ConsoleKey.A && key <= ConsoleKey.Z) {
				currentWord += key;
			} 
		}

		FinalSummary(_bottomRow);
	}

	private (int score, string reason, ConsoleColor colour) CheckWord(string word) {
		string reason = "";
		ConsoleColor colour = Console.ForegroundColor;
		List<Slot> validSlots = SearchBoard(word);
		int score = 0;
		if (validSlots.Count != 0) {
			if (_dictionary is not null) {
				if (_dictionary.IsWord(word)) {
					score = ScoreWord(word, Type);
				} else {
					reason = "Not a valid word";
					colour = ConsoleColor.Red;
				}
			} else {
				score = ScoreWord(word, Type);
			}
		} else {
			reason = "Unplayable";
			colour = ConsoleColor.Red;
		}

		return (score, reason, colour);
	}

	private ConsoleKey DisplayAndGetInput(int row, string word) {
		Console.SetCursorPosition(0, row);
		Console.Write(new string(' ', Console.WindowWidth - 2));

		Console.SetCursorPosition(0, row);
		Console.Write($"Time remaining: ");
		if (TimeRemaining < RedZone) {
			Console.ForegroundColor = ConsoleColor.Red;
		}
		Console.Write($"{TimeRemaining:m':'ss}");

		Console.ResetColor();
		Console.Write($" Press <Esc> to exit... {word}");

		return KeyReader.ReadKey(OneSecond);
	}

	public void DisplayBoard(string word = "") {
		if (_topRow == int.MinValue) {
			for (int i = 0; i < (_boggleDice.BoardSize * DieDisplayHeight); i++) {
				Console.WriteLine();
			}

			(int _, _topRow) = Console.GetCursorPosition();
			_topRow -= (_boggleDice.BoardSize * DieDisplayHeight);
		}

		foreach (Slot slot in _board) {
			DisplayDie(slot.Letter, slot.Col * DieDisplayWidth, _topRow + (slot.Row * DieDisplayHeight));
		}

		if (Verbose && word.Length > 0) {
			List<Slot> validSlots = SearchBoard(word);
			Console.ForegroundColor = ConsoleColor.Green;
			foreach (Slot slot in validSlots) {
				DisplayDie(slot.Letter, slot.Col * DieDisplayWidth, _topRow + (slot.Row * DieDisplayHeight));
			}
			Console.ResetColor();
		}
	}

	private static void DisplayDie(string letter, int? col = null, int? row = null) {
		(int cursorCol, int cursorRow) = Console.GetCursorPosition();
		col ??= cursorCol;
		row ??= cursorRow;
		Console.SetCursorPosition((int)col, (int)row);

		Console.Write($"┌───┐");
		Console.SetCursorPosition((int)col, (int)row + 1);
		Console.Write($"│ {letter, -2}│");
		Console.SetCursorPosition((int)col, (int)row + 2);
		Console.Write($"└───┘");
	}

	private void DisplayWordList() {
		const int ScoreWidth = 2;

		int startCol     = DieDisplayWidth * _boggleDice.BoardSize + 2;
		int boardHeight  = DieDisplayHeight * _boggleDice.BoardSize;
		int maxWordWidth = 12;

		Console.SetCursorPosition(startCol, _topRow);
		Console.Write($"┌{new string('─', Console.WindowWidth - 4 - startCol)}┐");
		Console.SetCursorPosition(startCol + ((Console.WindowWidth - 4 - startCol) / 2) - 5, _topRow);
		Console.Write($"Word List");

		for (int i = 1; i < boardHeight - 1; i++) {
			Console.SetCursorPosition(startCol, _topRow + i);
			Console.Write($"│{new string(' ', Console.WindowWidth - 4 - startCol)}│");
		}
		Console.SetCursorPosition(startCol, _topRow + boardHeight - 1);
		Console.Write($"└{new string('─', Console.WindowWidth - 4 - startCol)}┘");

		int totalScore = 0;
		int row = 0;
		if (_words.Count > 0) {
			maxWordWidth = _words.Max(w => w.Length);
			int columnWidth = ScoreWidth + maxWordWidth + 3;

			for (int wordIndex = 0; wordIndex < _words.Count; wordIndex++) {
				row = wordIndex % (boardHeight - 2);
				string word = _words[wordIndex];
				(int score, string reason, ConsoleColor colour) = CheckWord(word);
				if (_words.Take(wordIndex).Contains(word)) {
					score = 0;
					reason = "Duplicate word";
					colour = ConsoleColor.DarkGray;
				}
				totalScore += score;
				Console.ForegroundColor = colour;
				Console.SetCursorPosition(startCol + 1 + (columnWidth * (wordIndex / (boardHeight - 2))), _topRow + 1 + row);
				Console.Write($"{score,ScoreWidth} {word}");
				Console.ResetColor();
			}
		}
	}

	private void FinalSummary(int row) {
		Console.SetCursorPosition(0, row);
		Console.Write(new string(' ', Console.WindowWidth - 2));

		Console.WriteLine();
		Console.WriteLine("Score Word            Reason");
		int totalScore = 0;
		foreach (string word in _words.Order().Distinct()) {
			(int score, string reason, ConsoleColor colour) = CheckWord(word);
			totalScore += score;
			Console.ForegroundColor = colour;
			Console.WriteLine($"{score,4}  {word,-15} {reason}");
			Console.ResetColor();
		}
		Console.WriteLine();
		Console.WriteLine($"{totalScore,4}  Total Score ");
	}

	private static int ScoreWord(string word, BoggleDice.BoggleType boggleSetType) {
		return (boggleSetType, word.Length) switch {
			(BigBoggleDeluxe or BigBoggleOriginal or BigBoggleChallenge or SuperBigBoggle2012, <= 3) => 0,
			(SuperBigBoggle2012, >= 9) => word.Length * 2,
			(_, 3)    =>  1,
			(_, 4)    =>  1,
			(_, 5)    =>  2,
			(_, 6)    =>  3,
			(_, 7)    =>  5,
			(_, >= 8) => 11,
			(_, _)    =>  0
		};

		/*
		*     4x4 version            5x5 version           6x6 version
		*
		*    Word                   Word                  Word
		*   length	Points         length	Points       length	Points
		*     3       1              3       0             3       0
		*     4       1              4       1             4       1
		*     5       2              5       2             5       2
		*     6       3              6       3             6       3
		*     7       5              7       5             7       5
		*     8+     11              8+     11             8      11
		*                                                  9+   2 pts per letter
		*/
	}

	/// <summary>
	/// Searches the Board and validates the word.
	/// </summary>
	/// <param name="word"></param>
	/// <returns>Returns the first list of slots that make up the word otherwise returns an empty List</returns>
	private List<Slot> SearchBoard(string word) {
		List<Slot> result = new();
		int cols = _board.Max(x => x.Col) + 1;
		int rows = _board.Max(x => x.Row) + 1;
		bool[,] visited = new bool[rows, cols];
		for (int i = 0; i < _board.Count; i++) {
			if (SearchBoardDFS(word, 0, _board[i].Col, _board[i].Row, visited, result)) {
				return result;
			}
		}
		return result;
	}

	/// <summary>
	/// Uses a Depth First Search algorithm to find the slots that make up the word
	/// </summary>
	/// <param name="word"></param>
	/// <param name="index"></param>
	/// <param name="col"></param>
	/// <param name="row"></param>
	/// <param name="visited"></param>
	/// <param name="result"></param>
	/// <returns></returns>
	private bool SearchBoardDFS(string word, int index, int col, int row, bool[,] visited, List<Slot> result) {
		if (index == word.Length) {
			return true;
		}

		if (col < 0 || col >= visited.GetLength(0) || row < 0 || row >= visited.GetLength(1)) {
			return false;
		}

		if (visited[col, row]) {
			return false;
		}

		Slot current = _board.First(x => x.Col == col && x.Row == row);
		int newIndex = Math.Min(word.Length, index + current.Letter.Length);
		if (current.Letter.ToUpperInvariant() != word[index..newIndex]) {
			return false;
		}

		result.Add(current);
		visited[col, row] = true;
		bool found = SearchBoardDFS(word, newIndex, col - 1, row - 1, visited, result) ||
					 SearchBoardDFS(word, newIndex, col    , row - 1, visited, result) ||
					 SearchBoardDFS(word, newIndex, col + 1, row - 1, visited, result) ||
					 SearchBoardDFS(word, newIndex, col - 1, row    , visited, result) ||
					 SearchBoardDFS(word, newIndex, col + 1, row    , visited, result) ||
					 SearchBoardDFS(word, newIndex, col - 1, row + 1, visited, result) ||
					 SearchBoardDFS(word, newIndex, col    , row + 1, visited, result) ||
					 SearchBoardDFS(word, newIndex, col + 1, row + 1, visited, result);
		if (!found) {
			result.Remove(current);
		}

		visited[col, row] = false;
		return found;
	}

	private record Slot(string Letter, int Col, int Row);
}
