using Smab.DiceAndTiles;
Console.Clear();

QLessDice qlessDice = new();
qlessDice.ShakeAndFillRack();

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

Console.Write("Die 0: ");
DisplayDie(qlessDice.Rack[0], 12);

Console.SetCursorPosition(20, cursorRow);
Console.Write("Die 1: ");
DisplayDie(qlessDice.Rack[1], 30);


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