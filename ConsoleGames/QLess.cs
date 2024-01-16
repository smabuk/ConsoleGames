using static Smab.DiceAndTiles.ScrabbleWordFinder;

namespace ConsoleGames;

public sealed class QLess {
	private const int BoardHeight      = 21;
	private const int BoardIndent      = 21;
	private const int DieDisplayHeight =  3;
	private const int RackSize         = 12;

	private readonly QLessDice       _qLessDice = new();

	private DictionaryOfWords? _dictionary;
	private long _timerStart;
	private int  _bottomRow;
	private int  _rackRow;
	private int  _topRow = int.MinValue;

	public bool Verbose { get; set; } = false;

	public void Play(string? dictionaryFilename)
	{

		if (string.IsNullOrWhiteSpace(dictionaryFilename) is false)
		{
			_dictionary = new DictionaryOfWords(dictionaryFilename);
		}

		DisplayInit();

		_timerStart = Stopwatch.GetTimestamp();
		string currentKey = "";
		int currentRackIndex = -1;

		while (true)
		{
			DisplayBoard(_qLessDice.Board, highlightIndex: currentRackIndex);
			Console.SetCursorPosition(0, _rackRow);
			DisplayRack(_qLessDice.Rack, "Rack", board: _qLessDice.Board, highlightIndex: currentRackIndex);

			DisplayBottomRow($" Press A-Z to select, arrow keys to place, <Enter> to check, and <Esc> to quit... ");
			ConsoleKey key = Console.ReadKey(true).Key;

			if (key == ConsoleKey.Escape)
			{
				break;
			}
			else if (key == ConsoleKey.Enter)
			{
				//IEnumerable<PositionedLetter> scrabbleBoard =
				//	_qLessDice.Board.Select(d => new PositionedLetter(d.Die.Display[0], d.Col, d.Row));
				ScrabbleWordFinder swf = new(_qLessDice.Board, _dictionary);
				List<string> words = swf.FindWords();
				List<PositionedDie> errors = [];
				if (swf.ValidWordsAsTiles.Concat(swf.InvalidWordsAsTiles).SelectMany(t => t).Distinct().Count() != RackSize) {
					DisplayBottomRow($" You haven't used all of the dice to make words (press a key to continue)... ", ConsoleColor.Red);
					_ = Console.ReadKey(true).Key;
				} else if (swf.ValidWordsAsTiles.Concat(swf.InvalidWordsAsTiles).Where(t => t.Count == 2).Any()) {
					errors = swf
						.ValidWordsAsTiles
						.Concat(swf.InvalidWordsAsTiles)
						.Where(t => t.Count == 2)
						.SelectMany(t => t)
						.Distinct()
						.Select(t => new PositionedDie(_qLessDice.Board.Where(d => d.Col == t.Col && d.Row == t.Row).Single().Die, t.Col, t.Row))
						.ToList();
					DisplayBoard(_qLessDice.Board, errors);
					DisplayBottomRow($" Words must be at least 3 letters long (press a key to continue)... ", ConsoleColor.Red);
					_ = Console.ReadKey(true).Key;
				} else if (swf.IsBlockInMoreThanOnePiece()) {
					errors = swf
						.Islands
						.OrderByDescending(i => i.Count)
						.Skip(1)
						.SelectMany(t => t)
						.Select(t => new PositionedDie(_qLessDice.Board.Where(d => d.Col == t.Col && d.Row == t.Row).Single().Die, t.Col, t.Row))
						.ToList();
					DisplayBoard(_qLessDice.Board, errors);
					DisplayBottomRow($" The dice are not joined into 1 block (press a key to continue)... ", ConsoleColor.Red);
					_ = Console.ReadKey(true).Key;
				} else {
					if (_dictionary is not null) {
						if (swf.InvalidWordsAsTiles.Count == 0) {
							Console.ForegroundColor = ConsoleColor.Yellow;
							DisplayBoard(_qLessDice.Board);
							Console.SetCursorPosition(0, _rackRow);
							DisplayRack(_qLessDice.Rack, "Success", board: _qLessDice.Board);
							Console.ResetColor();
							break;
						}

						foreach (List<PositionedTile> tile in swf.InvalidWordsAsTiles) {
							tile.ForEach(t => errors.Add(new PositionedDie(_qLessDice.Board.Where(d => d.Col == t.Col && d.Row == t.Row).Single().Die, t.Col, t.Row)));
						}

						DisplayBoard(_qLessDice.Board, errors);
						DisplayBottomRow($" You have some words spelt incorrectly (press a key to continue)... ", ConsoleColor.Red);
						_ = Console.ReadKey(true).Key;
					} else {
						DisplayBottomRow($" No dictionary specified, so if you know you have this correct press Y now ... ");
						if (Console.ReadKey(true).Key == ConsoleKey.Y) {
							break;
						}
					}
				}
			}
			else if (key is >= ConsoleKey.A and <= ConsoleKey.Z)
			{
				currentKey = key.ToString();
				for (int i = currentRackIndex + 1; i < RackSize * 2; i++)
				{
					if (_qLessDice.Board[i % RackSize].Die.Display == currentKey)
					{
						currentRackIndex = i % RackSize;
						break;
					}
				}
			}
			else if (key is ConsoleKey.LeftArrow
						 or ConsoleKey.RightArrow
						 or ConsoleKey.UpArrow
						 or ConsoleKey.DownArrow)
			{
				if (currentRackIndex >= 0)
				{
					_qLessDice.Board[currentRackIndex] = MoveDie(_qLessDice.Board[currentRackIndex], _qLessDice.Board, key);
				}
			}
		}

		DisplayBottomRow($"Time elapsed: {Stopwatch.GetElapsedTime(_timerStart):mm\\:ss}");
	}

	private void DisplayBoard(IReadOnlyCollection<PositionedDie>? board = null, IReadOnlyCollection<PositionedDie>? errors = null, int? highlightIndex = -1) {
		const int RowHeight = 2;
		const int ColWidth = 4;
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

			for (int i = 0; i < DieDisplayHeight; i++) {
				Console.WriteLine();
			}

			(int _, _topRow) = Console.GetCursorPosition();
			Console.SetCursorPosition(0, _topRow - DieDisplayHeight);
			_topRow -= (BoardHeight + DieDisplayHeight);
		} else {
			Console.SetCursorPosition(0, _topRow);
			Console.WriteLine(boardTemplate);
		}

		if (board is not null) {
			foreach (PositionedDie slot in board) {
				if (slot.Row >= 0) {
					Console.ResetColor();
					int row = _topRow + 1 + (slot.Row * RowHeight);
					int col = BoardIndent + (slot.Col * ColWidth);
					Console.SetCursorPosition(col, row);
					if (highlightIndex == slot.Index) {
						Console.ForegroundColor = ConsoleColor.Green;
					}

					if (errors?.Any(d => d.Col == slot.Col && d.Row == slot.Row) ?? false) {
						Console.ForegroundColor = ConsoleColor.Red;
					}

					Console.Write(slot.Die.Display);
					Console.ResetColor();
				}
			}
		}
	}

	/// <summary>
	/// The first time we run, we have to make sure that we know where the bottom and rack rows will be
	/// If we have to scroll the cursor numbers change
	/// </summary>
	private void DisplayInit()
	{
		DisplayBoard();

		for (int i = 0; i < DieDisplayHeight; i++)
		{
			Console.WriteLine();
		}

		(int _, _bottomRow) = Console.GetCursorPosition();
		_rackRow = _bottomRow - DieDisplayHeight;
	}

	public void DisplayQLess(bool verbose) {
		DisplayRack(_qLessDice.Rack, "Rack");
		if (verbose) {
			Console.WriteLine();
			DisplayRack(_qLessDice.Rack.Where(d =>  "AEIOU".Contains(d.FaceValue.Value!)), "Vowels"    , true);
			Console.WriteLine();
			DisplayRack(_qLessDice.Rack.Where(d => !"AEIOU".Contains(d.FaceValue.Value!)), "Consonants", true);
		}
	}

	private void DisplayBottomRow(string message, ConsoleColor? colour = null) {
		Console.SetCursorPosition(0, _bottomRow);
		Console.Write(new string(' ', Console.WindowWidth - 2));

		if (colour is not null) {
			Console.ForegroundColor = (ConsoleColor)colour;
		}

		Console.SetCursorPosition(0, _bottomRow);
		Console.Write(message);

		if (colour is not null) {
			Console.ResetColor();
		}
	}

	private static void DisplayRack(IEnumerable<LetterDie> dice, string name, bool sort = false, List<PositionedDie>? board = null, int? highlightIndex = -1) {
		List<LetterDie> orderedDice = sort switch {
			true  => [.. dice.OrderBy(d => d.FaceValue.Value)],
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

	private static PositionedDie MoveDie(PositionedDie slot, List<PositionedDie> board, ConsoleKey key) {
		int slotRow = (slot.Row < 0 ? QLessDice.BoardHeight - 1 : slot.Row);
		switch (key) {
			case ConsoleKey.LeftArrow:
				for (int col = slot.Col - 1; col >= 0; col--) {
					if (!board.Any(d => d.Row == slot.Row && d.Col == col)) {
						return slot with { Col = col, Row = slotRow };
					}
				}

				break;
			case ConsoleKey.RightArrow:
				for (int col = slot.Col + 1; col < QLessDice.BoardWidth; col++) {
					if (!board.Any(d => d.Row == slot.Row && d.Col == col)) {
						return slot with { Col = col, Row = slotRow };
					}
				}

				break;
			case ConsoleKey.DownArrow:
				for (int row = slot.Row + 1; row < QLessDice.BoardHeight; row++) {
					if (!board.Any(d => d.Col == slot.Col && d.Row == row)) {
						return slot with { Row = row };
					}
				}

				break;
			case ConsoleKey.UpArrow:
				slotRow = (slot.Row < 0 ? QLessDice.BoardHeight : slot.Row);
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
}
