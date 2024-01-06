namespace ConsoleGames;

public partial class ScrabbleDice2
{
	public static readonly int BoardHeight = 9;
	public static readonly int BoardWidth  = 9;
	public static readonly int BoardSize   = 9;
	public static readonly int RackSize    = 7;

	private static readonly List<LetterDie> s_letterDice =
	[
		new LetterDie(new (string, int)[] { ("A", 1), ("E", 1), ("I",  1), ("O", 1), ("U",  1), ("Y", 4) }) { Name = "AEIOUY1" },
		new LetterDie(new (string, int)[] { ("A", 1), ("E", 1), ("I",  1), ("O", 1), ("U",  1), ("Y", 4) }) { Name = "AEIOUY2" },
		new LetterDie(new (string, int)[] { ("A", 1), ("E", 1), ("I",  1), ("L", 1), ("O",  1), ("#", 0) }) { Name = "AEILO#"  },
		new LetterDie(new (string, int)[] { ("B", 3), ("F", 4), ("H",  4), ("N", 1), ("W",  4), ("#", 0) }) { Name = "BFHNW#"  },
		new LetterDie(new (string, int)[] { ("C", 3), ("D", 2), ("G",  2), ("T", 1), ("V",  4), ("#", 0) }) { Name = "CDGTV#"  },
		new LetterDie(new (string, int)[] { ("J", 8), ("K", 5), ("Q", 10), ("X", 8), ("Z", 10), ("#", 0) }) { Name = "JKQXZ#"  },
		new LetterDie(new (string, int)[] { ("M", 3), ("N", 1), ("P",  3), ("R", 1), ("S",  1), ("#", 0) }) { Name = "MNPRS#"  },
	];

	public List<PositionedDie> Board { get; set; } = [];
	public List<LetterDie>     Dice  { get; set; } = new(s_letterDice);

	public int NoOfDice        => Dice.Count;
	public int NoOfDiceOnBoard => Board.Count;

	public List<LetterDie> Rack { get; set; } = [];

	public void ShakeAndFillRack()
	{
		List<LetterDie> bag = new(Dice);

		Rack = [];
		Random rnd = new();

		do
		{
			int i = rnd.Next(0, bag.Count);
			bag[i].Roll();
			bag[i].Orientation = rnd.Next(0, 4) * 90;
			if (bag[i].FaceValue.Name == "#")
			{
				bag[i].Faces[bag[i].UpperFace] = bag[i].FaceValue with { Display = "■" };
			}
			Rack.Add(bag[i]);
			_ = bag.Remove(bag[i]);
		} while (bag.Count > 0);

	}

	public void ReRollAndFillRack(IEnumerable<LetterDie> dice)
	{
		List<LetterDie> bag = new(dice);
		Random rnd = new();

		do
		{
			int i = rnd.Next(0, bag.Count);
			_ = Rack.Remove(bag[i]);
			bag[i].Roll();
			bag[i].Orientation = rnd.Next(0, 4) * 90;
			if (bag[i].FaceValue.Name == "#")
			{
				bag[i].Faces[bag[i].UpperFace] = bag[i].FaceValue with { Display = "■" };
			}
			Rack.Add(bag[i]);
			_ = bag.Remove(bag[i]);
		} while (bag.Count > 0);

	}


	public static int ScoreWordByLength(string word)
	{
		return (word.Length) switch
		{
			2 =>  2,
			3 =>  5,
			4 => 10,
			5 => 18,
			6 => 25,
			7 => 50,
			_ =>  0
		};
	}

}
