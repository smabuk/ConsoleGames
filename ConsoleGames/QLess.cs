namespace ConsoleGames;

internal sealed class QLess {
	private const int DieDisplayWidth = 5;
	private const int DieDisplayHeight = 3;


	private readonly QLessDice _qLessDice = new();
	private readonly List<LetterDie> _rack = new();

	private DictionaryOfWords? _dictionary;
	private long _timerStart;
	private int _bottomRow;
	private int _topRow = int.MinValue;

	internal QLess() {
		_qLessDice.ShakeAndFillRack();
		_rack = _qLessDice.Rack;
	}

	public bool Verbose { get; set; } = false;

	internal void Play(string filename) {
		if (string.IsNullOrWhiteSpace(filename) is false) {
			_dictionary = new DictionaryOfWords(filename);
		}

		DisplayBoard();
		Console.WriteLine();
		Console.WriteLine();
		Console.WriteLine();
		(int _, _bottomRow) = Console.GetCursorPosition();
		_bottomRow -= 3;
		_timerStart = Stopwatch.GetTimestamp();
		string currentKey = "";
		List<Slot> testSlots = new() {
			new("T", 4, 4),
			new("E", 5, 4),
			new("S", 6, 4),
			new("T", 7, 4)
		};
		while (true) {
			DisplayBoard(testSlots);
			Console.SetCursorPosition(0, _bottomRow);
			DisplayRack(_rack, "Rack");
			Console.WriteLine();
			Console.Write($" Press <Esc> to exit... ");
			ConsoleKey key = Console.ReadKey(true).Key;
			if (key == ConsoleKey.Escape) {
				break;
			} else if (key >= ConsoleKey.A && key <= ConsoleKey.Z) {
				currentKey = key.ToString();
			}
		}

		Console.WriteLine();
		Console.WriteLine($"Time elapsed: {Stopwatch.GetElapsedTime(_timerStart).TotalSeconds}s");

	}

	private void DisplayBoard(IEnumerable<Slot>? slots = null) {
		const int BoardHeight = 21;
		const int BoardIndent = 21;
		const string board = """
			                   ┌───┬───┬───┬───┬───┬───┬───┬───┬───┬───┬───┬───┐
			                   │   │   │   │   │   │   │   │   │   │   │   │   │
			                   ├───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┤
			                   │   │   │   │   │   │   │   │   │   │   │   │   │
			                   ├───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┤
			                   │   │   │   │   │   │   │   │   │   │   │   │   │
			                   ├───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┤
			                   │   │   │   │   │   │   │   │   │   │   │   │   │
			                   ├───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┤
			                   │   │   │   │   │   │   │   │   │   │   │   │   │
			                   ├───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┤
			                   │   │   │   │   │   │   │   │   │   │   │   │   │
			                   ├───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┤
			                   │   │   │   │   │   │   │   │   │   │   │   │   │
			                   ├───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┤
			                   │   │   │   │   │   │   │   │   │   │   │   │   │
			                   ├───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┤
			                   │   │   │   │   │   │   │   │   │   │   │   │   │
			                   ├───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┼───┤
			                   │   │   │   │   │   │   │   │   │   │   │   │   │
			                   └───┴───┴───┴───┴───┴───┴───┴───┴───┴───┴───┴───┘
			""";

		if (_topRow == int.MinValue) {
			Console.WriteLine(board);
			Console.WriteLine();
			Console.WriteLine();
			Console.WriteLine();

			(int _, _topRow) = Console.GetCursorPosition();
			Console.SetCursorPosition(0, _topRow - 3);
			_topRow -= (BoardHeight + 3);
		}

		if (slots is not null) {
			foreach (Slot slot in slots) {
				int row = _topRow + 1 + (slot.Row * 2);
				int col = BoardIndent + (slot.Col * 4);
				Console.SetCursorPosition(col, row);
				Console.Write(slot.Letter);
			}
		}
	}

	internal void DisplayQLess(bool verbose) {
		DisplayRack(_rack, "Rack");
		if (verbose) {
			Console.WriteLine();
			DisplayRack(_rack.Where(d =>  "AEIOU".Contains(d.FaceValue.Value!)), "Vowels"    , true);
			Console.WriteLine();
			DisplayRack(_rack.Where(d => !"AEIOU".Contains(d.FaceValue.Value!)), "Consonants", true);
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
		cursorRow -= DieDisplayHeight;

		Console.SetCursorPosition((int)cursorCol, (int)cursorRow + 1);
		Console.Write($"{name,12}: ");
		foreach (var die in orderedDice) {
			DisplayDie(die, null, cursorRow);
		}
	}

	private static void DisplayDie(LetterDie die, int? col = null, int? row = null) {
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

	private record Slot(string Letter, int Col, int Row);
}
