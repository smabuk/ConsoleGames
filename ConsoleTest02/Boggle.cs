namespace ConsoleTest02;

public sealed class Boggle {
	private const int DieDisplayWidth  = 5;
	private const int DieDisplayHeight = 3;
	private static readonly TimeSpan s_GameLength = new(0, 3, 0); 
	private long m_TimerStart;
	
	TimeSpan TimeRemaining => s_GameLength.Subtract(Stopwatch.GetElapsedTime(m_TimerStart));

	record Slot(LetterDie Die, string FaceValue, int Col, int Row);

	public List<string> Words { get; set; } = new(); 

	public void DisplayBoggle(BoggleDice.BoggleType type, bool play = false) {

		BoggleDice boggle = new(type);
		boggle.ShakeAndFillBoard();
		List<Slot> slots = boggle
			.Board
			.Select((die, index) => new Slot(die, die.FaceValue.Display, index % boggle.BoardSize, index / boggle.BoardSize))
			.ToList();

		for (int i = 0; i < (boggle.BoardSize * DieDisplayHeight) ; i++) {
			Console.WriteLine();
		}
		
		(int _, int topRow) = Console.GetCursorPosition();
		topRow -= (boggle.BoardSize * DieDisplayHeight);

		foreach (Slot slot in slots) {
			DisplayDie(slot.Die, slot.Col * DieDisplayWidth, topRow + (slot.Row * DieDisplayHeight));
		}

		if (play) {
			Play(slots, topRow);
		}
	}

	private void Play(List<Slot> slots, int topRow) {
		m_TimerStart = Stopwatch.GetTimestamp();
		Console.WriteLine();
		(int _, int bottomRow) = Console.GetCursorPosition();
		string wordTemp = "";
		while (TimeRemaining <= s_GameLength) {
			ConsoleKey key = DisplayAndGetInput(bottomRow, wordTemp);
			if (key == ConsoleKey.Escape) {
				break;
			} else if (key == ConsoleKey.Enter && wordTemp.Length >= 1) {
				Words.Add(wordTemp);
				wordTemp = "";
				//ValidateAndScoreWord(word);
			} else if (key == ConsoleKey.Backspace && wordTemp.Length >= 1) {
				wordTemp = wordTemp[..^1];
			} else if (key >= ConsoleKey.A && key <= ConsoleKey.Z) {
				wordTemp += key;
			} 

			IEnumerable<Slot> x = slots.Where(die => die.FaceValue.StartsWith($"{key}"));
			foreach (Slot slot in x) {
				Console.ForegroundColor = ConsoleColor.Green;
				DisplayDie(slot.Die, slot.Col * DieDisplayWidth, topRow + (slot.Row * DieDisplayHeight));
			}
			Console.ResetColor();
		}

		FinalSummary(bottomRow);
	}

	private ConsoleKey DisplayAndGetInput(int row, string word) {
		Console.SetCursorPosition(0, row);
		Console.Write("                                                                                      ");
		Console.SetCursorPosition(0, row);
		Console.Write($"Time remaining: {TimeRemaining:m':'ss} Press <Esc> to exit... {word}");
		return Console.ReadKey(true).Key;
	}

	private void FinalSummary(int row) {
		Console.SetCursorPosition(0, row);
		Console.Write("                                                                                      ");
		Console.WriteLine();
		Console.WriteLine("Words:");
		foreach (string word in Words.Order()) {
			Console.WriteLine(word);
		}
		Console.WriteLine();
		Console.WriteLine("Total Score: ");
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
}
