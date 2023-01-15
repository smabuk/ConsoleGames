namespace ConsoleGames;

internal sealed class DictionaryOfWords {

	public DictionaryOfWords(string filename) {

		if (!File.Exists(filename)) {
			throw new FileNotFoundException(nameof(filename));
		}
		
		Words = File.ReadAllLines(filename).Select(i => i.ToUpperInvariant()).ToList();

	}

	public bool IsWord(string word) => Words.Contains(word.ToUpperInvariant());

	private List<string> Words { get; set; } = new();
}
