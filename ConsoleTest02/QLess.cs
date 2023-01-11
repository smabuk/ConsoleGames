using System.ComponentModel;

namespace ConsoleTest02;

public sealed class QLessCommand : Command<QLessCommand.Settings> {

	public override int Execute([NotNull] CommandContext context, [NotNull] Settings settings) {
		QLessDice qlessDice = new();
		qlessDice.ShakeAndFillRack();

		IEnumerable<LetterDie> rack = qlessDice.Rack;

		DisplayRack(rack, "Rack");
		if (settings.Verbose) {
			DisplayRack(rack.Where(d =>  "AEIOU".Contains(d.FaceValue.Value!)), "Vowels"    , true);
			DisplayRack(rack.Where(d => !"AEIOU".Contains(d.FaceValue.Value!)), "Consonants", true);
		}
		return 0;
	}

	static void DisplayRack(IEnumerable<LetterDie> dice, string name, bool sort = false) {

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

	public sealed class Settings : CommandSettings {
		[Description("Display rack breakdown")]
		[CommandOption("-v|--verbose")]
		[DefaultValue(false)]
		public bool Verbose { get; init; }
	}

}
