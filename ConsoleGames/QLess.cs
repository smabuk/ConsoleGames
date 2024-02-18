using Smab.DiceAndTiles.Games.QLess;

namespace ConsoleGames;

public sealed class QLess {
	private const int BOARD_HEIGHT = 10;
	private const int BOARD_WIDTH  = 12;
	private const int BOARD_DISPLAY_HEIGHT = (BOARD_HEIGHT * (DIE_DISPLAY_HEIGHT - 1)) + 1;
	private const int BOARD_DISPLAY_INDENT = 21;
	private const int DIE_DISPLAY_HEIGHT   =  3;
	
	private QLessDice _qLessDice = null!;

	private long _timerStart;
	private int  _bottomRow;
	private int  _rackRow;
	private int  _topRow = int.MinValue;

	public bool Verbose { get; set; } = false;

	public void Play(string? dictionaryFilename)
	{
		_qLessDice = new(string.IsNullOrWhiteSpace(dictionaryFilename) ? new CSW21Dictionary() : new DictionaryService(dictionaryFilename));
		List<PositionedDie> localRack = [.. _qLessDice.Rack.OrderBy(r => r.Col)];

		DisplayInit();

		_timerStart = Stopwatch.GetTimestamp();
		string currentKey = "";
		int currentRackIndex = 0;
		DieId? currentRackDieId = null;

		while (true)
		{
			DisplayBoard(_qLessDice.Board, highlightId: currentRackDieId);
			Console.SetCursorPosition(0, _rackRow);
			DisplayRack(localRack, "Rack", board: [.._qLessDice.Board], highlightId: currentRackDieId);

			DisplayBottomRow($" Press A-Z to select, arrow keys to place, <Enter> to check, and <Esc> to quit... ");
			ConsoleKey key = Console.ReadKey(true).Key;

			if (key == ConsoleKey.Escape)
			{
				break;
			}
			else if (key == ConsoleKey.Enter)
			{
				QLessDice.Status gameStatus = _qLessDice.GameStatus();
				if (gameStatus is QLessDice.Win) {
					if (_qLessDice.HasDictionary) {
						Console.ForegroundColor = ConsoleColor.Yellow;
						DisplayBoard(_qLessDice.Board);
						Console.SetCursorPosition(0, _rackRow);
						DisplayRack(_qLessDice.Rack, "Success", board: [.._qLessDice.Board]);
						Console.ResetColor();
						break;
					} else {
						DisplayBottomRow($" No dictionary specified, so if you know you have this correct press Y now ... ");
						if (Console.ReadKey(true).Key == ConsoleKey.Y) {
							break;
						}
					}
				} else if (gameStatus is QLessDice.Errors errorsStatus) {
					if (errorsStatus.ErrorReasons.HasFlag(QLessDice.ErrorReasons.MissingDice)) {
						DisplayBoard(_qLessDice.Board, [.. errorsStatus.DiceWithErrors]);
						DisplayBottomRow($" You haven't used all of the dice to make words (press a key to continue)... ", ConsoleColor.Red);
						_ = Console.ReadKey(true).Key;
					} else if (errorsStatus.ErrorReasons.HasFlag(QLessDice.ErrorReasons.TwoLetterWords)) {
						DisplayBoard(_qLessDice.Board, [.. errorsStatus.DiceWithErrors]);
						DisplayBottomRow($" Words must be at least 3 letters long (press a key to continue)... ", ConsoleColor.Red);
						_ = Console.ReadKey(true).Key;
					} else if (errorsStatus.ErrorReasons.HasFlag(QLessDice.ErrorReasons.MultipleBlocks)) {
						DisplayBoard(_qLessDice.Board, [.. errorsStatus.DiceWithErrors]);
						DisplayBottomRow($" The dice are not joined into 1 block (press a key to continue)... ", ConsoleColor.Red);
						_ = Console.ReadKey(true).Key;
					} else if (errorsStatus.ErrorReasons.HasFlag(QLessDice.ErrorReasons.Misspelt)) {
						DisplayBoard(_qLessDice.Board, [.. errorsStatus.DiceWithErrors]);
						DisplayBottomRow($" Some of the words are not spelt correctly (press a key to continue)... ", ConsoleColor.Red);
						_ = Console.ReadKey(true).Key;
					}
				} 
			}
			else if (key is >= ConsoleKey.A and <= ConsoleKey.Z)
			{
				currentKey = key.ToString();

				for (int i = currentRackIndex + 1; i < localRack.Count * 2; i++)
				{
					if (localRack[i % localRack.Count].Die.Display == currentKey)
					{
						currentRackIndex = i % localRack.Count;
						currentRackDieId = localRack[currentRackIndex].Die.Id;
						break;
					}
				}
			}
			else if (key is ConsoleKey.LeftArrow
						 or ConsoleKey.RightArrow
						 or ConsoleKey.UpArrow
						 or ConsoleKey.DownArrow)
			{
				if (currentRackDieId is not null)
				{
					MoveDie((DieId)currentRackDieId, key);
				}
			}
		}

		DisplayBottomRow($"Time elapsed: {Stopwatch.GetElapsedTime(_timerStart):mm\\:ss}");
	}

	private void DisplayBoard(IReadOnlyCollection<PositionedDie>? board = null, IReadOnlyCollection<PositionedDie>? errors = null, string? highlightId = null) {
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

			for (int i = 0; i < DIE_DISPLAY_HEIGHT; i++) {
				Console.WriteLine();
			}

			(int _, _topRow) = Console.GetCursorPosition();
			Console.SetCursorPosition(0, _topRow - DIE_DISPLAY_HEIGHT);
			_topRow -= (BOARD_DISPLAY_HEIGHT + DIE_DISPLAY_HEIGHT);
		} else {
			Console.SetCursorPosition(0, _topRow);
			Console.WriteLine(boardTemplate);
		}

		if (board is not null) {
			foreach (PositionedDie slot in board) {
				if (slot.Row >= 0) {
					Console.ResetColor();
					int row = _topRow + 1 + (slot.Row * RowHeight);
					int col = BOARD_DISPLAY_INDENT + (slot.Col * ColWidth);
					Console.SetCursorPosition(col, row);
					if (highlightId == slot.Die.Id) {
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

		for (int i = 0; i < DIE_DISPLAY_HEIGHT; i++)
		{
			Console.WriteLine();
		}

		(int _, _bottomRow) = Console.GetCursorPosition();
		_rackRow = _bottomRow - DIE_DISPLAY_HEIGHT;
	}

	public void DisplayQLess(bool verbose) {
		DisplayRack(_qLessDice.Rack, "Rack");
		if (verbose) {
			Console.WriteLine();
			DisplayRack(_qLessDice.Rack.Where(d =>  "AEIOU".Contains(d.Die.Display)), "Vowels"    , true);
			Console.WriteLine();
			DisplayRack(_qLessDice.Rack.Where(d => !"AEIOU".Contains(d.Die.Display)), "Consonants", true);
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

	private static void DisplayRack(IEnumerable<PositionedDie> localRack, string name, bool sort = false, List<PositionedDie>? board = null, string? highlightId = null) {
		List<PositionedDie> orderedRack = sort switch {
			true  => [.. localRack.OrderBy(r => r.Die.Display)],
			false => [.. localRack],
		};

		for (int i = 0; i < DIE_DISPLAY_HEIGHT; i++) {
			Console.WriteLine();
		}

		(int cursorCol, int cursorRow) = Console.GetCursorPosition();
		cursorRow -= DIE_DISPLAY_HEIGHT;

		Console.SetCursorPosition((int)cursorCol, (int)cursorRow + 1);
		Console.Write($"{name,12}: ");
		(cursorCol, _) = Console.GetCursorPosition();
		for (int i = 0; i < orderedRack.Count; i++) {
			if (highlightId == (orderedRack[i].Die).Id) {
				Console.ForegroundColor = ConsoleColor.Green;
			} else if (board is not null && board.Any(pd => pd.Die.Id == orderedRack[i].Die.Id)) {
				Console.ForegroundColor = ConsoleColor.DarkGray;
			}

			Die die = orderedRack[i].Die;
			DisplayDie(die, cursorCol + (orderedRack[i].Col * 5), cursorRow);
			Console.ResetColor();
		}
	}

	private static void DisplayDie(Die die, int? col = null, int? row = null) {
		(int cursorCol, int cursorRow) = Console.GetCursorPosition();
		col ??= cursorCol;
		row ??= cursorRow;
		Console.SetCursorPosition((int)col, (int)row);

		Console.Write($"┌───┐");
		Console.SetCursorPosition((int)col, (int)row + 1);
		Console.Write($"│ {die.Display} │");
		Console.SetCursorPosition((int)col, (int)row + 2);
		Console.Write($"└───┘");
	}

	private void MoveDie(DieId dieId, ConsoleKey key) {
		PositionedDie slot = _qLessDice.Board.SingleOrDefault(pd => pd.Die.Id == dieId) ?? _qLessDice.Rack.Single(pd => pd.Die.Id == dieId);
		int slotRow = (slot.Row < 0 ? BOARD_HEIGHT - 1 : slot.Row);
		switch (key) {
			case ConsoleKey.LeftArrow:
				for (int col = slot.Col - 1; col >= 0; col--) {
					if (_qLessDice.PlaceOnBoard(dieId, col, slotRow)) {
						break;
					}
				}

				break;
			case ConsoleKey.RightArrow:
				for (int col = slot.Col + 1; col < BOARD_WIDTH; col++) {
					if (_qLessDice.PlaceOnBoard(dieId, col, slotRow)) {
						break;
					}
				}

				break;
			case ConsoleKey.DownArrow:
				for (int row = slot.Row + 1; row < BOARD_HEIGHT; row++) {
					if (_qLessDice.PlaceOnBoard(dieId, slot.Col, row)) {
						break;
					}
				}

				break;
			case ConsoleKey.UpArrow:
				slotRow = (slot.Row < 0 ? BOARD_HEIGHT : slot.Row);
				for (int row = slotRow - 1; row >= 0; row--) {
					if (_qLessDice.PlaceOnBoard(dieId, slot.Col, row)) {
						break;
					}
				}

				break;
			default:
				break;
		}
	}
}
