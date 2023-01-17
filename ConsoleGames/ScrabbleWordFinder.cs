namespace ConsoleGames;

internal sealed class ScrabbleWordFinder {
	private readonly List<ScrabbleTile>    _board;
	private readonly DictionaryOfWords?    _dictionary;
	private readonly HashSet<string>       _visited;

	public ScrabbleWordFinder(List<ScrabbleTile> board, DictionaryOfWords? dictionary = null) {
		_board      = board;
		_dictionary = dictionary;
		_visited    = new();
	}

	private enum Direction {
		Horizontal,
		Vertical
	}

	public List<List<ScrabbleTile>>        WordsAsTiles { get; private set; } = new();

	public List<string> FindWords() {
		List<string> foundWords = new();
		WordsAsTiles = new();

		foreach (ScrabbleTile currentTile in _board) {
			_visited.Add(GetKey(currentTile.Col, currentTile.Row));
			List<ScrabbleTile> currentWord = new() { currentTile };
			if (IsStartOfWord(currentTile, Direction.Horizontal)) {
				FindWords(currentTile, currentWord, foundWords, Direction.Horizontal);
			}
			if (IsStartOfWord(currentTile, Direction.Vertical)) {
				FindWords(currentTile, currentWord, foundWords, Direction.Vertical);
			}
			_visited.Remove(GetKey(currentTile.Col, currentTile.Row));
		}
		return foundWords;
	}

	private void FindWords(ScrabbleTile currentTile, List<ScrabbleTile> currentWord, List<string> foundWords, Direction direction) {
		string currentWordString = CreateWord(currentWord);
		if (currentWordString.Length > 1 && IsEndOfWord(currentTile, direction)) {
			//if (_dictionary.IsWord(currentWordString)) {
			foundWords.Add(currentWordString);
			WordsAsTiles.Add(new(currentWord));
			//}
		}

		var neighbours = GetNeighbours(currentTile, direction);
		for (int i = 0; i < neighbours.Count; i++) {
			var nextTile = neighbours[i];
			if (!currentWord.Contains(nextTile)) {
				currentWord.Add(nextTile);
				FindWords(nextTile, currentWord, foundWords, direction);
				currentWord.RemoveAt(currentWord.Count - 1);
			}
		}
	}

	private static string GetKey(int col, int row) => $"{col}-{row}";

	private static string CreateWord(List<ScrabbleTile> tiles) => string.Join("", tiles.Select(t => t.Letter));

	private List<ScrabbleTile> GetNeighbours(ScrabbleTile tile, Direction direction) {
		List<ScrabbleTile> neighbours = new();

		for (int i = 0; i < _board.Count; i++) {
			var nextTile = _board[i];
			if (IsAdjacentAndInLine(tile, nextTile, direction)) {
				neighbours.Add(nextTile);
			}
		}
		return neighbours;
	}

	private static bool IsAdjacentAndInLine(ScrabbleTile tile1, ScrabbleTile tile2, Direction direction) {
		return direction switch {
			Direction.Vertical   => tile1.Col == tile2.Col && (tile1.Row == tile2.Row - 1 || tile1.Row == tile2.Row + 1),
			Direction.Horizontal => tile1.Row == tile2.Row && (tile1.Col == tile2.Col - 1 || tile1.Col == tile2.Col + 1),
			_ => throw new NotImplementedException()
		};
	}

	private bool IsEndOfWord(ScrabbleTile currentTile, Direction direction) {
		return direction switch {
			Direction.Horizontal => !_board.Any(x => x.Col == currentTile.Col + 1 && x.Row == currentTile.Row),
			Direction.Vertical => !_board.Any(x => x.Row == currentTile.Row + 1 && x.Col == currentTile.Col),
			_ => throw new NotImplementedException(),
		};
	}

	private bool IsStartOfWord(ScrabbleTile currentTile, Direction direction) {
		return direction switch {
			Direction.Horizontal => !_board.Any(x => x.Col == currentTile.Col - 1 && x.Row == currentTile.Row),
			Direction.Vertical => !_board.Any(x => x.Row == currentTile.Row - 1 && x.Col == currentTile.Col),
			_ => throw new NotImplementedException(),
		};
	}

	public record ScrabbleTile(char Letter, int Col, int Row);
}
