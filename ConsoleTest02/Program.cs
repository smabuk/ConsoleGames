using Smab.DiceAndTiles;

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