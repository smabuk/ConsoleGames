using static Smab.DiceAndTiles.BoggleDice.BoggleType;

namespace ConsoleTest02;

public sealed class Boggle {
	private const int DieDisplayWidth  = 5;
	private const int DieDisplayHeight = 3;
	private const int OneSecond = 1000;

	private static readonly TimeSpan GameLength = new(0, 3,  0);
	private static readonly TimeSpan RedZone    = new(0, 0, 10);

	private List<Slot> Slots { get; set; } = new();
	private readonly BoggleDice m_BoggleDice;
	private long _timerStart;
	private int _topRow = int.MinValue;
	private int _bottomRow;

	public Boggle(BoggleDice.BoggleType type) {
		Type = type;

		m_BoggleDice = new(Type);
		m_BoggleDice.ShakeAndFillBoard();

		Slots = m_BoggleDice
			.Board
			.Select((die, index) => new Slot(die, die.FaceValue.Display, index % m_BoggleDice.BoardSize, index / m_BoggleDice.BoardSize))
			.ToList();
	}

	TimeSpan TimeRemaining => GameLength.Subtract(Stopwatch.GetElapsedTime(_timerStart));

	record Slot(LetterDie Die, string FaceValue, int Col, int Row);

	public List<string> Words { get; set; } = new(); 
	public BoggleDice.BoggleType Type { get; set; } = Classic4x4;

	
	public void DisplayBoggle(string word = "") {
		if (_topRow == int.MinValue) {
			for (int i = 0; i < (m_BoggleDice.BoardSize * DieDisplayHeight); i++) {
				Console.WriteLine();
			}

			(int _, _topRow) = Console.GetCursorPosition();
			_topRow -= (m_BoggleDice.BoardSize * DieDisplayHeight);
		}

		foreach (Slot slot in Slots) {
			DisplayDie(slot.Die, slot.Col * DieDisplayWidth, _topRow + (slot.Row * DieDisplayHeight));
		}

		if (word.Length > 0) {
			IEnumerable<Slot> slots = SlotsIfWordIsPossible(word);
			foreach (Slot slot in slots) {
				Console.ForegroundColor = ConsoleColor.Green;
				DisplayDie(slot.Die, slot.Col * DieDisplayWidth, _topRow + (slot.Row * DieDisplayHeight));
			}
			Console.ResetColor();
		}
	}

	private IEnumerable<Slot> SlotsIfWordIsPossible(string word) {
		if (string.IsNullOrEmpty(word)) {
			yield break;
		}

		List <Slot> startSlots = Slots
			.Where(die => word.StartsWith(die.FaceValue.ToUpperInvariant()))
			.ToList();

		List<Slot> slots = new();
		foreach (var start in startSlots) {

		}
		
		foreach (Slot slot in slots) {
			yield return slot;
		}
	}

	public void Play() {
		_timerStart = Stopwatch.GetTimestamp();
		Console.WriteLine();
		(int _, _bottomRow) = Console.GetCursorPosition();
		string currentWord = "";
		while (TimeRemaining.Seconds > 0) {
			DisplayBoggle(currentWord);
			ConsoleKey key = DisplayAndGetInput(_bottomRow, currentWord);
			if (key == ConsoleKey.Escape) {
				break;
			} else if (key == ConsoleKey.Enter && currentWord.Length >= 1) {
				Words.Add(currentWord);
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
		foreach (string word in Words.Order()) {
			int score = ScoreWord(word, Type);
			totalScore += score;
			Console.WriteLine($"{score,4}  {word}");
		}
		Console.WriteLine();
		Console.WriteLine($"{totalScore,4}  Total Score ");
	}

	private static void DisplayDie(LetterDie die, int? col = null, int? row = null) {
		(int cursorCol, int cursorRow) = Console.GetCursorPosition();
		col ??= cursorCol;
		row ??= cursorRow;
		Console.SetCursorPosition((int)col, (int)row);

		Console.Write($"┌───┐");
		Console.SetCursorPosition((int)col, (int)row + 1);
		Console.Write($"│ {die.FaceValue.Display, -2}│");
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

}
