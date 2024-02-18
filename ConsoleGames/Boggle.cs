using Smab.DiceAndTiles.Games.Boggle;

namespace ConsoleGames;

public sealed class Boggle(BoggleDice.BoggleType type, string? filename) {
	private const int DieDisplayWidth  =    5;
	private const int DieDisplayHeight =    3;
	private const int OneSecond        = 1000;

	private static readonly TimeSpan RedZone = new(0, 0, 10);

	private readonly BoggleDice   _boggleDice = new(type, string.IsNullOrWhiteSpace(filename) ? CSW21Dictionary.Create() : new DictionaryService(filename));

	private long _timerStart;
	private int  _bottomRow;
	private int  _topRow      = int.MinValue;

	public  TimeSpan GameLength { get; set; }  = new(0, 3, 0);
	public  bool     Verbose    { get; set; }  = false;
	private TimeSpan TimeRemaining => GameLength.Subtract(Stopwatch.GetElapsedTime(_timerStart));

	public void Play() {
		string currentWord = "";
		
		Console.WriteLine();
		(_, _bottomRow) = Console.GetCursorPosition();
		_timerStart = Stopwatch.GetTimestamp();
		
		while (TimeRemaining.Ticks > 0) {
			DisplayBoard(currentWord);
			DisplayWordList();

			ConsoleKey key = DisplayAndGetInput(_bottomRow, currentWord);
			if (key == ConsoleKey.Escape) {
				break;
			} else if (key == ConsoleKey.Enter && currentWord.Length > 0) {
				_ = _boggleDice.PlayWord(currentWord);
				currentWord = "";
			} else if (key == ConsoleKey.Backspace && currentWord.Length > 0) {
				currentWord = currentWord[..^1];
			} else if (key is >= ConsoleKey.A and <= ConsoleKey.Z) {
				currentWord += key;
			} 
		}

		FinalSummary(_bottomRow);
	}

	private static (string Reason, ConsoleColor Colour) WordScoreReasonAndColour(BoggleDice.WordScore wordScore) {

		return wordScore.Reason switch
		{
			BoggleDice.ScoreReason.Success       => ("",               Console.ForegroundColor),
			BoggleDice.ScoreReason.AlreadyPlayed => ("Duplicate Word", ConsoleColor.DarkGray),
			BoggleDice.ScoreReason.Misspelt      => ("Misspelt",       ConsoleColor.Red),
			BoggleDice.ScoreReason.TooShort      => ("Too short",      ConsoleColor.Red),
			BoggleDice.ScoreReason.Unplayable    => ("Unplayable",     ConsoleColor.Red),
			_ => ("", Console.ForegroundColor)
		};
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

		// If we get a timeout return a key that we don't use (Zoom)
		return KeyReader.ReadKey(OneSecond) ?? ConsoleKey.Zoom;
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

		int startCol     = (DieDisplayWidth  * _boggleDice.BoardWidth) + 2;
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

		int row = 0;
		List<BoggleDice.WordScore> wordScores = [.. _boggleDice.WordScores];
		if (wordScores.Count != 0) {
			maxWordWidth = _boggleDice.WordScores.Max(ws => ws.Word.Length);
			int columnWidth = ScoreWidth + maxWordWidth + 3;

			for (int wordIndex = 0; wordIndex < wordScores.Count; wordIndex++) {
				row = wordIndex % (boardHeight - 2);
				string word = wordScores[wordIndex].Word;
				(string reason, ConsoleColor colour) = WordScoreReasonAndColour(wordScores[wordIndex]);
				Console.ForegroundColor = colour;
				Console.SetCursorPosition(startCol + 1 + (columnWidth * (wordIndex / (boardHeight - 2))), _topRow + 1 + row);
				Console.Write($"{wordScores[wordIndex].Score,ScoreWidth} {word}");
				Console.ResetColor();
			}
		}
	}

	private void FinalSummary(int row) {
		Console.SetCursorPosition(0, row);
		Console.Write(new string(' ', Console.WindowWidth - 2));

		Console.WriteLine();
		Console.WriteLine("Score Word            Reason");
		foreach (BoggleDice.WordScore wordScore in _boggleDice.WordScores.OrderBy(ws => ws.Word)) {
			(string reason, ConsoleColor colour) = WordScoreReasonAndColour(wordScore);
			Console.ForegroundColor = colour;
			Console.WriteLine($"{wordScore.Score,4}  {wordScore.Word,-15} {reason}");
			Console.ResetColor();
		}

		Console.WriteLine();
		Console.WriteLine($"{_boggleDice.Score,4}  Total Score ");
	}
}
