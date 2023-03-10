using System.Collections.Generic;

namespace ConsoleGames;

internal sealed class ScrabbleWordFinder {
	private readonly List<ScrabbleTile>    _board;
	private readonly DictionaryOfWords?    _dictionary;
	private readonly HashSet<string>       _visited;

	public ScrabbleWordFinder(IEnumerable<ScrabbleTile> board, DictionaryOfWords? dictionary = null) {
		_board      = board.ToList();
		_dictionary = dictionary;
		_visited    = new();
	}

	private enum Direction {
		Horizontal,
		Vertical
	}

	public List<List<ScrabbleTile>>        WordsAsTiles { get; private set; } = new();
	public List<List<ScrabbleTile>>        Islands      { get; private set; } = new();

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

		List<ScrabbleTile> neighbours = GetNeighbours(currentTile, direction);
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

	public bool IsBlockInMoreThanOnePiece()
	{
		HashSet<(int Col, int Row)> visited = new();
		List<ScrabbleTile> island = new();

		int noOfIslands = 0;
		foreach (var tile in _board)
		{
			if (visited.Contains((tile.Col, tile.Row)))
			{
				continue;
			}

			noOfIslands++;
			island = new();
			VisitAdjacent(tile.Col, tile.Row);
			Islands.Add(island);
		}

		return noOfIslands != 1;

		void VisitAdjacent(int col, int row)
		{
			if (row < 0 || col < 0)
			{
				return;
			}

			ScrabbleTile? tile = _board.SingleOrDefault(t => t.Col == col && t.Row == row);
			if (visited.Contains((col, row)) || tile is null)
			{
				return;
			}

			visited.Add((col, row));
			island.Add(tile);

			VisitAdjacent(col - 1, row);
			VisitAdjacent(col + 1, row);
			VisitAdjacent(col    , row + 1);
			VisitAdjacent(col    , row - 1);
		}
	}

	private bool IsEndOfWord(ScrabbleTile currentTile, Direction direction) {
		return direction switch {
			Direction.Horizontal => !_board.Any(x => x.Col == currentTile.Col + 1 && x.Row == currentTile.Row),
			Direction.Vertical   => !_board.Any(x => x.Row == currentTile.Row + 1 && x.Col == currentTile.Col),
			_ => throw new NotImplementedException(),
		};
	}

	private bool IsStartOfWord(ScrabbleTile currentTile, Direction direction) {
		return direction switch {
			Direction.Horizontal => !_board.Any(x => x.Col == currentTile.Col - 1 && x.Row == currentTile.Row),
			Direction.Vertical   => !_board.Any(x => x.Row == currentTile.Row - 1 && x.Col == currentTile.Col),
			_ => throw new NotImplementedException(),
		};
	}

	public record ScrabbleTile(char Letter, int Col, int Row);
}
