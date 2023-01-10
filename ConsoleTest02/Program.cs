using Smab.DiceAndTiles;

QLessDice qlessDice = new();
qlessDice.ShakeAndFillRack();

Console.Clear();
Console.WriteLine("Q-Less dice rack");
for (int i = 0; i < qlessDice.Rack.Count; i++) {
	Console.Write($"┌───┐ ");
}
Console.WriteLine();
foreach (LetterDie die in qlessDice.Rack) {
	Console.Write($"│ {die.FaceValue.Display} │ ");
}
Console.WriteLine();
for (int i = 0; i < qlessDice.Rack.Count; i++) {
	Console.Write($"└───┘ ");
}
Console.WriteLine();

(int cursorCol, int cursorRow) = Console.GetCursorPosition();
Console.WriteLine();

Console.Write("    Vowels: ");
qlessDice.Rack
	.Where(d => "AEIOU".Contains(d.FaceValue.Value!))
	.OrderBy(d => d.FaceValue.Value)
	.ToList()
	.ForEach(d => { DisplayDie(d, null, cursorRow); } );
Console.WriteLine();

(cursorCol, cursorRow) = Console.GetCursorPosition();
Console.WriteLine();

Console.Write("Consonants: ");
qlessDice.Rack
	.Where(d => "AEIOU".Contains(d.FaceValue.Value!) == false)
	.OrderBy(d => d.FaceValue.Value)
	.ToList()
	.ForEach(d => { DisplayDie(d, null, cursorRow); } );

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