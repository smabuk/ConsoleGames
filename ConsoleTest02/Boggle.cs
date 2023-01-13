namespace ConsoleTest02;

public sealed class Boggle {
	private const int DieDisplayWidth  = 5;
	private const int DieDisplayHeight = 3;

	record Slot(LetterDie Die, string FaceValue, int Col, int Row);
	public static void DisplayBoggle(BoggleDice.BoggleType type, bool play = false) {

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
			Console.WriteLine();
			(int _, int bottomRow) = Console.GetCursorPosition();
			Console.Write("Press <Esc> to exit... ");
			while (true) {
				ConsoleKey key = Console.ReadKey(true).Key;
				if (key == ConsoleKey.Escape) {
					break;
				}
				IEnumerable<Slot> x = slots.Where(die => die.FaceValue.StartsWith($"{key}"));
				foreach (Slot slot in x) {
					Console.ForegroundColor = ConsoleColor.Green;
					DisplayDie(slot.Die, slot.Col * DieDisplayWidth, topRow + (slot.Row * DieDisplayHeight));
				}
				Console.ResetColor();
				Console.SetCursorPosition(23, bottomRow);

			}
		}


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
