namespace ConsoleGames;

internal sealed class ScrabbleWordFinder {
	private readonly List<ScrabbleTile>    _board;
	private readonly DictionaryOfWords     _dictionary;
	private readonly HashSet<string>       _visited;
	private HashSet<ScrabbleTile>          _usedTiles;
	public List<List<ScrabbleTile>> WordsAsTiles { get; private set; } = new();

	public ScrabbleWordFinder(List<ScrabbleTile> board, DictionaryOfWords dictionary) {
		_board      = board;
		_dictionary = dictionary;
		_visited    = new();
		_usedTiles  = new();
	}

	public List<string> FindWords() {
		List<string> foundWords = new();
		_usedTiles = new();
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
			currentWord.ForEach(i => _usedTiles.Add(i));
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

	public int TilesUsed => _usedTiles.Count;

	private bool IsStartOfWord(ScrabbleTile currentTile, Direction direction) {
		return direction switch {
			Direction.Horizontal => !_board.Any(x => x.Col == currentTile.Col - 1 && x.Row == currentTile.Row),
			Direction.Vertical => !_board.Any(x => x.Row == currentTile.Row - 1 && x.Col == currentTile.Col),
			_ => throw new NotImplementedException(),
		};
	}

	private bool IsEndOfWord(ScrabbleTile currentTile, Direction direction) {
		return direction switch {
			Direction.Horizontal => !_board.Any(x => x.Col == currentTile.Col + 1 && x.Row == currentTile.Row),
			Direction.Vertical => !_board.Any(x => x.Row == currentTile.Row + 1 && x.Col == currentTile.Col),
			_ => throw new NotImplementedException(),
		};
	}
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

	private bool IsAdjacentAndInLine(ScrabbleTile tile1, ScrabbleTile tile2, Direction direction) {
		if (direction == Direction.Vertical)
			return tile1.Col == tile2.Col && (tile1.Row == tile2.Row - 1 || tile1.Row == tile2.Row + 1);
		else
			return tile1.Row == tile2.Row && (tile1.Col == tile2.Col - 1 || tile1.Col == tile2.Col + 1);
	}

	private bool IsEndOfLine(ScrabbleTile tile1, ScrabbleTile tile2, Direction direction) {
		if (direction == Direction.Vertical)
			return tile1.Col == tile2.Col && (tile1.Row == tile2.Row - 1 || tile1.Row == tile2.Row + 1) && HasSpaceAtEnds(tile1.Col, tile1.Row, tile2.Row);
		else
			return tile1.Row == tile2.Row && (tile1.Col == tile2.Col - 1 || tile1.Col == tile2.Col + 1) && HasSpaceAtEnds(tile1.Row, tile1.Col, tile2.Col);
	}

	private bool HasSpaceAtEnds(int line, int start, int end) {
		return !_board.Any(x => x.Col == line && x.Row == start - 1) && !_board.Any(x => x.Col == line && x.Row == end + 1);
	}

	private bool IsNeighbour(ScrabbleTile tile1, ScrabbleTile tile2) {
		return (tile1.Col == tile2.Col && (tile1.Row == tile2.Row - 1 || tile1.Row == tile2.Row + 1))
			|| (tile1.Row == tile2.Row && (tile1.Col == tile2.Col - 1 || tile1.Col == tile2.Col + 1));
	}

	private string GetKey(int col, int row) {
		return $"{col}-{row}";
	}

	private static string CreateWord(List<ScrabbleTile> tiles) {
		return string.Join("", tiles.Select(t => t.Letter));
	}

	private enum Direction {
		Horizontal,
		Vertical
	}

	public record ScrabbleTile(char Letter, int Col, int Row);
}
