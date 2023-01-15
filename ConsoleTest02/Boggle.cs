﻿using static Smab.DiceAndTiles.BoggleDice.BoggleType;

namespace ConsoleTest02;

public sealed class Boggle {
	private const int DieDisplayWidth  = 5;
	private const int DieDisplayHeight = 3;
	private const int OneSecond = 1000;

	private static readonly TimeSpan RedZone = new(0, 0, 10);

	private readonly BoggleDice _boggleDice;
	private List<Slot> _board = new();
	private List<string> _words = new(); 
	private long _timerStart;
	private int _topRow = int.MinValue;
	private int _bottomRow;

	public Boggle(BoggleDice.BoggleType type) {
		Type = type;

		_boggleDice = new(Type);
		_boggleDice.ShakeAndFillBoard();

		_board = _boggleDice
			.Board
			.Select((die, index) => new Slot(die.FaceValue.Display, index % _boggleDice.BoardSize, index / _boggleDice.BoardSize))
			.ToList();
	}

	private record Slot(string Letter, int Col, int Row);

	public TimeSpan GameLength { get; set; } = new(0, 3, 0);
	public BoggleDice.BoggleType Type { get; set; } = Classic4x4;
	public bool Verbose { get; set; } = false;
	
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
			List<Slot> validSlots = BoggleSearch(word);
			foreach (Slot slot in validSlots) {
				Console.ForegroundColor = ConsoleColor.Green;
				DisplayDie(slot.Letter, slot.Col * DieDisplayWidth, _topRow + (slot.Row * DieDisplayHeight));
			}
			Console.ResetColor();
		}
	}

	public void Play() {
		_timerStart = Stopwatch.GetTimestamp();
		Console.WriteLine();
		(int _, _bottomRow) = Console.GetCursorPosition();
		string currentWord = "";
		while (TimeRemaining.Seconds > 0) {
			DisplayBoard(currentWord);
			ConsoleKey key = DisplayAndGetInput(_bottomRow, currentWord);
			if (key == ConsoleKey.Escape) {
				break;
			} else if (key == ConsoleKey.Enter && currentWord.Length >= 1) {
				_words.Add(currentWord);
				currentWord = "";
				//ValidateAndScoreWord(word);
			} else if (key == ConsoleKey.Backspace && currentWord.Length >= 1) {
				currentWord = currentWord[..^1];
			} else if (key >= ConsoleKey.A && key <= ConsoleKey.Z) {
				currentWord += key;
			} 
		}

		FinalSummary(_bottomRow);
	}

	private ConsoleKey DisplayAndGetInput(int row, string word) {
		Console.SetCursorPosition(0, row);
		Console.Write("                                                                                      ");
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

	private void FinalSummary(int row) {
		Console.SetCursorPosition(0, row);
		Console.Write("                                                                                      ");
		Console.WriteLine();
		Console.WriteLine("Score Word");
		int totalScore = 0;
		foreach (string word in _words.Order().Distinct()) {
			List<Slot> validSlots = BoggleSearch(word);
			int score = 0;
			if (validSlots.Count != 0) {
				score = ScoreWord(word, Type);
				totalScore += score;
			} else {
				Console.ForegroundColor = ConsoleColor.Red;
			}
			Console.WriteLine($"{score,4}  {word}");
			Console.ResetColor();
		}
		Console.WriteLine();
		Console.WriteLine($"{totalScore,4}  Total Score ");
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
	private List<Slot> BoggleSearch(string word) {
		List<Slot> result = new();
		int cols = _board.Max(x => x.Col) + 1;
		int rows = _board.Max(x => x.Row) + 1;
		bool[,] visited = new bool[rows, cols];
		for (int i = 0; i < _board.Count; i++) {
			if (BoggleDFS(word, 0, _board[i].Col, _board[i].Row, visited, result)) {
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
	private bool BoggleDFS(string word, int index, int col, int row, bool[,] visited, List<Slot> result) {
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
		if (current.Letter.Length == 1) {
			if (current.Letter[0] != word[index]) {
				return false;
			}
		} else {
			if (current.Letter.ToUpperInvariant() != word[index..newIndex]) {
				return false;
			}
		}

		result.Add(current);
		visited[col, row] = true;
		bool found = BoggleDFS(word, newIndex, col - 1, row - 1, visited, result) ||
					 BoggleDFS(word, newIndex, col    , row - 1, visited, result) ||
					 BoggleDFS(word, newIndex, col + 1, row - 1, visited, result) ||
					 BoggleDFS(word, newIndex, col - 1, row    , visited, result) ||
					 BoggleDFS(word, newIndex, col + 1, row    , visited, result) ||
					 BoggleDFS(word, newIndex, col - 1, row + 1, visited, result) ||
					 BoggleDFS(word, newIndex, col    , row + 1, visited, result) ||
					 BoggleDFS(word, newIndex, col + 1, row + 1, visited, result);
		if (!found) {
			result.Remove(current);
		}

		visited[col, row] = false;
		return found;
	}

	private TimeSpan TimeRemaining => GameLength.Subtract(Stopwatch.GetElapsedTime(_timerStart));

}
