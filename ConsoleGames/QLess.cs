namespace ConsoleGames;

internal sealed class QLess {
	private const int DieDisplayHeight = 3;
	private const int RackSize = 12;


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
		int currentRackIndex = -1;
		List<Slot> board = _rack.Select((d, index) => new Slot(index, d.FaceValue.Display, 6, -1)).ToList();
		while (true) {
			DisplayBoard(board, highlightIndex: currentRackIndex);
			Console.SetCursorPosition(0, _bottomRow);
			DisplayRack(_rack, "Rack", board: board, highlightIndex: currentRackIndex);
			Console.WriteLine();
			Console.SetCursorPosition(0, _bottomRow + 3);
			Console.Write(new string(' ', Console.WindowWidth - 2));
			Console.SetCursorPosition(0, _bottomRow + 3);
			Console.Write($" Press <Esc> to exit... ");
			ConsoleKey key = Console.ReadKey(true).Key;
			if (key == ConsoleKey.Escape) {
				break;
			} else if (key == ConsoleKey.Enter) {
				List<ScrabbleWordFinder.ScrabbleTile> scrabbleBoard = 
					board
					.Select(d => new ScrabbleWordFinder.ScrabbleTile(d.Letter[0], d.Col, d.Row))
					.ToList();
				ScrabbleWordFinder swf = new(scrabbleBoard, _dictionary!);
				List<string> words = swf.FindWords();
				if (swf.TilesUsed != 12) {
					Console.SetCursorPosition(0, _bottomRow + 3);
					Console.ForegroundColor = ConsoleColor.Red;
					Console.Write($" Not all of the dice make words (press a key to continue)... ");
					Console.ResetColor();
					_ = Console.ReadKey(true).Key;
				} else {
					List<Slot> errors = new();
					if (_dictionary is not null) {
						foreach (List<ScrabbleWordFinder.ScrabbleTile> tile in swf.WordsAsTiles) {
							if (_dictionary.IsWord(string.Join("", tile.Select(t => t.Letter))) is false) {
								tile.ForEach(t => errors.Add( new Slot(99, t.Letter.ToString(), t.Col, t.Row)));
							}
						}
						if (errors.Count != 0) {
							DisplayBoard(board, errors);
							Console.SetCursorPosition(0, _bottomRow + 3);
							Console.ForegroundColor = ConsoleColor.Red;
							Console.Write($" You have some words spelt incorrectly (press a key to continue)... ");
							Console.ResetColor();
							_ = Console.ReadKey(true).Key;
						} else {
							currentRackIndex = -1;
							Console.ForegroundColor= ConsoleColor.Yellow;
							DisplayBoard(board);
							Console.SetCursorPosition(0, _bottomRow + 3);
							Console.ResetColor();
							break;
						}
					}
					Console.SetCursorPosition(0, _bottomRow + 3);
					Console.Write($" If you think you have this correct press Y now ... ");
					if (Console.ReadKey(true).Key == ConsoleKey.Y) {
						break;
					}
				}
			} else if (key >= ConsoleKey.A && key <= ConsoleKey.Z) {
				currentKey = key.ToString();
				for (int i = currentRackIndex + 1; i < RackSize * 2; i++) {
					if (board[i % RackSize].Letter == currentKey) {
						currentRackIndex = i % RackSize;
						break;
					}
				}
			} else if (key is ConsoleKey.LeftArrow or ConsoleKey.RightArrow or ConsoleKey.UpArrow or ConsoleKey.DownArrow ) {
				if (currentRackIndex >= 0) {
					board[currentRackIndex] = MoveDie(board[currentRackIndex], board, key);
				}
			}
		}

		Console.SetCursorPosition(0, _bottomRow + 3);
		Console.Write(new string(' ', Console.WindowWidth - 2));
		Console.SetCursorPosition(0, _bottomRow + 3);
		Console.WriteLine($"Time elapsed: {Stopwatch.GetElapsedTime(_timerStart):mm\\:ss}");
	}

	private void DisplayBoard(IReadOnlyCollection<Slot>? board = null, IReadOnlyCollection<Slot>? errors = null, int? highlightIndex = -1) {
		const int BoardHeight = 21;
		const int BoardIndent = 21;
		const string boardTemplate = """
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
			Console.WriteLine(boardTemplate);
			Console.WriteLine();
			Console.WriteLine();
			Console.WriteLine();

			(int _, _topRow) = Console.GetCursorPosition();
			Console.SetCursorPosition(0, _topRow - 3);
			_topRow -= (BoardHeight + 3);
		} else {
			Console.SetCursorPosition(0, _topRow);
			Console.WriteLine(boardTemplate);
		}

		if (board is not null) {
			foreach (Slot slot in board) {
				if (slot.Row >= 0) {
					int row = _topRow + 1 + (slot.Row * 2);
					int col = BoardIndent + (slot.Col * 4);
					Console.SetCursorPosition(col, row);
					if (highlightIndex == slot.RackIndex) {
						Console.ForegroundColor = ConsoleColor.Green;
					}
					if (errors?.Any(d => d.Col == slot.Col && d.Row == slot.Row) ?? false) {
						Console.ForegroundColor = ConsoleColor.Red;
					}
					Console.Write(slot.Letter);
					Console.ResetColor();
				}
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

	private static void DisplayRack(IEnumerable<LetterDie> dice, string name, bool sort = false, List<Slot>? board = null, int? highlightIndex = -1) {
		List<LetterDie> orderedDice = sort switch {
			true => dice.OrderBy(d => d.FaceValue.Value).ToList(),
			false => dice.ToList()
		};

		for (int i = 0; i < DieDisplayHeight; i++) {
			Console.WriteLine();
		}

		(int cursorCol, int cursorRow) = Console.GetCursorPosition();
		cursorRow -= DieDisplayHeight;

		Console.SetCursorPosition((int)cursorCol, (int)cursorRow + 1);
		Console.Write($"{name,12}: ");
		for (int i = 0; i < orderedDice.Count; i++) {
			if (highlightIndex == i) {
				Console.ForegroundColor = ConsoleColor.Green;
			} else if (board is not null && board[i].Row >= 0) {
				Console.ForegroundColor = ConsoleColor.DarkGray;
			}
			LetterDie die = orderedDice[i];
			DisplayDie(die, null, cursorRow);
			Console.ResetColor();
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

	private static Slot MoveDie(Slot slot, List<Slot> board, ConsoleKey key) {
		int slotRow = (slot.Row < 0 ? 9 : slot.Row);
		switch (key) {
			case ConsoleKey.LeftArrow:
				for (int col = slot.Col - 1; col >= 0; col--) {
					if (!board.Any(d => d.Row == slot.Row && d.Col == col)) {
						return slot with { Col = col, Row = slotRow };
					}
				}
				break;
			case ConsoleKey.RightArrow:
				for (int col = slot.Col + 1; col < 12; col++) {
					if (!board.Any(d => d.Row == slot.Row && d.Col == col)) {
						return slot with { Col = col, Row = slotRow };
					}
				}
				break;
			case ConsoleKey.DownArrow:
				for (int row = slot.Row + 1; row < 10; row++) {
					if (!board.Any(d => d.Col == slot.Col && d.Row == row)) {
						return slot with { Row = row };
					}
				}
				break;
			case ConsoleKey.UpArrow:
				slotRow = (slot.Row < 0 ? 10 : slot.Row);
				for (int row = slotRow - 1; row >= 0; row--) {
					if (!board.Any(d => d.Col == slot.Col && d.Row == row)) {
						return slot with { Row = row };
					}
				}
				break;
			default:
				break;
		}

		return slot;
	}

	private record Slot(int RackIndex, string Letter, int Col, int Row);
}
