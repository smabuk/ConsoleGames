using Smab.DiceAndTiles;

QLessDice qlessDice = new();
qlessDice.ShakeAndFillRack();

Console.Clear();
Console.WriteLine("Q-Less");

IEnumerable<LetterDie> rack = qlessDice.Rack;

DisplayRack(rack, "Rack");
DisplayRack(rack.Where(d =>  "AEIOU".Contains(d.FaceValue.Value!)), "Vowels", true);
DisplayRack(rack.Where(d => !"AEIOU".Contains(d.FaceValue.Value!)), "Consonants", true);

static void DisplayDie(LetterDie die, int? col = null, int? row = null, bool as3d = false) {
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

static void DisplayRack(IEnumerable<LetterDie> dice, string name, bool sort = false) {
	(int cursorCol, int cursorRow) = Console.GetCursorPosition();

	Console.WriteLine();

	IEnumerable<LetterDie> orderedDice = sort switch {
		true => dice.OrderBy(d => d.FaceValue.Value),
		false => dice
	};

	Console.Write($"{name,12}: ");
	foreach (var die in orderedDice) {
		DisplayDie(die, null, cursorRow);
	}

	Console.WriteLine();
}