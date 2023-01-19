﻿using static Smab.DiceAndTiles.BoggleDice.BoggleType;

namespace ConsoleGames;

public sealed class Boggle {
	private const int DieDisplayWidth  =    5;
	private const int DieDisplayHeight =    3;
	private const int OneSecond        = 1000;

	private static readonly TimeSpan RedZone = new(0, 0, 10);

	private readonly BoggleDice   _boggleDice;
	private        List<string>   _words       = new(); 
	private  DictionaryOfWords?   _dictionary;
	private long                  _timerStart;
	private int                   _bottomRow;
	private int                   _topRow      = int.MinValue;

	public Boggle(BoggleDice.BoggleType type, string? filename) {
		Type = type;

		_boggleDice = new(Type);

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
		(int score, BoggleDice.CheckResult result) = _boggleDice.CheckAndScoreWord(word);
		if (result == BoggleDice.CheckResult.Success) {
			if (_dictionary is not null) {
				if (_dictionary.IsWord(word)) {
					score = _boggleDice.ScoreWord(word);
				} else {
					reason = "Not a valid word";
					colour = ConsoleColor.Red;
				}
			} else {
				score = _boggleDice.ScoreWord(word);
			}
		} else {
			reason = result switch {
				BoggleDice.CheckResult.Unplayable => "Unplayable",
				BoggleDice.CheckResult.Misspelt   => "Misspelt",
				BoggleDice.CheckResult.Success    => "Success",
				_                                 => "Unknown",
			};
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

		foreach (PositionedDie slot in _boggleDice.Board) {
			DisplayDie(slot.Die.Display, slot.Col * DieDisplayWidth, _topRow + (slot.Row * DieDisplayHeight));
		}

		if (Verbose && word.Length > 0) {
			List<PositionedDie> validSlots = _boggleDice.SearchBoard(word);
			Console.ForegroundColor = ConsoleColor.Green;
			foreach (PositionedDie slot in validSlots) {
				DisplayDie(slot.Die.Display, slot.Col * DieDisplayWidth, _topRow + (slot.Row * DieDisplayHeight));
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

		int startCol     = DieDisplayWidth  * _boggleDice.BoardWidth + 2;
		int boardHeight  = DieDisplayHeight * _boggleDice.BoardHeight;
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
}
