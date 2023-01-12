namespace ConsoleTest02;

public sealed class QLess {

	public static void DisplayQLess(bool verbose) {
		QLessDice qlessDice = new();
		qlessDice.ShakeAndFillRack();

		IEnumerable<LetterDie> rack = qlessDice.Rack;

		DisplayRack(rack, "Rack");
		if (verbose) {
			DisplayRack(rack.Where(d =>  "AEIOU".Contains(d.FaceValue.Value!)), "Vowels"    , true);
			DisplayRack(rack.Where(d => !"AEIOU".Contains(d.FaceValue.Value!)), "Consonants", true);
		}
	}

	private static void DisplayRack(IEnumerable<LetterDie> dice, string name, bool sort = false) {

		IEnumerable<LetterDie> orderedDice = sort switch {
			true => dice.OrderBy(d => d.FaceValue.Value),
			false => dice
		};

		Console.WriteLine();
		Console.WriteLine();
		Console.WriteLine();
		(int cursorCol, int cursorRow) = Console.GetCursorPosition();
		cursorRow -= 3;

		Console.SetCursorPosition((int)cursorCol, (int)cursorRow + 1);
		Console.Write($"{name,12}: ");
		foreach (var die in orderedDice) {
			DisplayDie(die, null, cursorRow);
		}

		Console.WriteLine();
	}

	private static void DisplayDie(LetterDie die, int? col = null, int? row = null, bool as3d = false) {
		(int cursorCol, int cursorRow) = Console.GetCursorPosition();
		col ??= cursorCol;
		row ??= cursorRow;
		Console.SetCursorPosition((int)col, (int)row);

		Console.Write($"┌───┐");
		Console.SetCursorPosition((int)col, (int)row + 1);
		Console.Write($"│ {die.FaceValue.Display} │");
		Console.SetCursorPosition((int)col, (int)row + 2);
		Console.Write($"└───┘");
	}
}
