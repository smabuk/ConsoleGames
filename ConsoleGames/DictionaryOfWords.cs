namespace ConsoleGames;

internal sealed class DictionaryOfWords {
	
	private Trie _trie = new();

	public DictionaryOfWords(string filename) {

		if (!File.Exists(filename)) {
			throw new FileNotFoundException(nameof(filename));
		}
		
		File.ReadAllLines(filename).ToList().ForEach(_trie.Insert);
	}

	public bool IsWord(string word) => _trie.Search(word.ToUpperInvariant());
}

internal sealed class Trie {
	public TrieNode Root { get; set; }

	public Trie() {
		Root = new TrieNode();
	}

	public void Insert(string word) {
		TrieNode current = Root;

		for (int i = 0; i < word.Length; i++) {
			char c = word[i];

			if (!current.Children.ContainsKey(c)) {
				current.Children.Add(c, new TrieNode());
			}

			current = current.Children[c];
		}

		current.IsWord = true;
	}

	public bool Search(string word) {
		TrieNode current = Root;

		for (int i = 0; i < word.Length; i++) {
			char c = word[i];

			if (!current.Children.ContainsKey(c)) {
				return false;
			}

			current = current.Children[c];
		}

		return current.IsWord;
	}

	public class TrieNode {
		public Dictionary<char, TrieNode> Children { get; set; }
		public bool IsWord { get; set; }

		public TrieNode() {
			Children = new Dictionary<char, TrieNode>();
		}
	}
}

