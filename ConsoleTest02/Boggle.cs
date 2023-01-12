namespace ConsoleTest02;

public sealed class Boggle {
	public static void DisplayBoggle(string boggleType) {

		BoggleDice.BoggleType type = boggleType.ToLower() switch {
			"classic"  => BoggleDice.BoggleType.Classic4x4,
			"big"      => BoggleDice.BoggleType.BigBoggleOriginal,
			"deluxe"   => BoggleDice.BoggleType.BigBoggleDeluxe,
			"superbig" => BoggleDice.BoggleType.SuperBigBoggle2012,
			_ => throw new NotImplementedException(),
		};

		BoggleDice boggle = new(type);
		boggle.ShakeAndFillBoard();

		for (int i = 0; i < boggle.BoardSize; i++) {
			Console.WriteLine();
			Console.WriteLine();
			Console.WriteLine();
			(int _, int cursorRow) = Console.GetCursorPosition();
			cursorRow -= 3;
			for (int j = 0; j < boggle.BoardSize; j++) {
				DisplayDie(boggle.Board[j + (i * boggle.BoardSize)], null, cursorRow);
			}
			Console.WriteLine();
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
