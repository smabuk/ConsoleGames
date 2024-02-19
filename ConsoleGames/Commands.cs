using Smab.DiceAndTiles.Games.Boggle;

namespace ConsoleGames;

[Description("The game of Q-Less")]
public sealed class QLessCommand : Command<QLessCommand.Settings> {

	public override int Execute([NotNull] CommandContext context, [NotNull] Settings settings) {
		QLess qLess = new() {
			Verbose = settings.Verbose
		};

		if (settings.Play) {
			qLess.Play(settings.Filename);
		} else {
			qLess.DisplayQLess(settings.Verbose);
		}

		return 0;
	}

	public sealed class Settings : CommandSettings {
		[Description("Display rack breakdown")]
		[CommandOption("-v|--verbose")]
		[DefaultValue(false)]
		public bool Verbose { get; init; }

		[Description("Play")]
		[CommandOption("-p|--play")]
		[DefaultValue(false)]
		public bool Play { get; init; }

		[Description("Filename of the valid list of words to check against")]
		[CommandOption("-d|--dict|--dictionary")]
		public required string Filename { get; init; } = "";
	}
}

[Description("The game of Boggle")]
public sealed class BoggleCommand : Command<BoggleCommand.Settings> {
	public override int Execute([NotNull] CommandContext context, [NotNull] Settings settings) {
		Boggle boggle = new(settings.BoggleType, settings.Filename) {
			Verbose            = settings.Verbose,
			GameLength         = new(0, 0, settings.TimerLength),
		};
		boggle.DisplayBoard();

		if (settings.Play) {
			boggle.Play();
		}

		return 0;
	}

	public sealed class Settings : CommandSettings {
		[Description("Boggle Type - classic, deluxe, big, superbig or new")]
		[CommandArgument(0, "[TYPE]")]
		public string Type { get; init; } = "classic";

		public BoggleDice.BoggleType BoggleType => Type.ToLower() switch {
			"classic"   => BoggleDice.BoggleType.Classic4x4,
			"big"       => BoggleDice.BoggleType.BigBoggleOriginal,
			"deluxe"    => BoggleDice.BoggleType.BigBoggleDeluxe,
			"superbig"  => BoggleDice.BoggleType.SuperBigBoggle2012,
			"new"       => BoggleDice.BoggleType.New4x4,
			"challenge" => throw new NotImplementedException(),
			_ => throw new NotImplementedException(),
		};

		[Description("Play")]
		[CommandOption("-p|--play")]
		[DefaultValue(false)]
		public bool Play { get; init; }

		[Description("Display valid paths as you type the words")]
		[CommandOption("-v|--verbose")]
		[DefaultValue(false)]
		public bool Verbose { get; init; }

		[Description("Set the countdown timer length in seconds")]
		[CommandOption("-t|--time")]
		[DefaultValue(180)]
		public int TimerLength { get; init; }

		[Description("Filename of the valid list of words to check against")]
		[CommandOption("-d|--dict|--dictionary")]
		public required string Filename { get; init; } = "";

		public override ValidationResult Validate() {
			string[] validTypes = [
				"classic",
				"big",
				"deluxe",
				"superbig",
				"new",
				//"challenge",
			];

			return validTypes.Contains(Type.ToLowerInvariant())
				? base.Validate()
				: ValidationResult.Error("Type must be one of classic, deluxe, big, superbig or new");
		}
	}
}

//[Description("The game of Scrabble Dice")]
//public sealed class ScrabbleDiceCommand : Command<ScrabbleDiceCommand.Settings>
//{

//	public override int Execute([NotNull] CommandContext context, [NotNull] Settings settings)
//	{
//		ScrabbleDiceGame scrabbleDice = new()
//		{
//			Verbose = settings.Verbose
//		};

//		if (settings.Play)
//		{
//			scrabbleDice.Play(settings.Filename);
//		}

//		return 0;
//	}

//	public sealed class Settings : CommandSettings
//	{
//		[Description("")]
//		[CommandOption("-v|--verbose")]
//		[DefaultValue(false)]
//		public bool Verbose { get; init; }

//		[Description("Play")]
//		[CommandOption("-p|--play")]
//		[DefaultValue(false)]
//		public bool Play { get; init; }

//		[Description("Set the countdown timer length in seconds")]
//		[CommandOption("-t|--time")]
//		[DefaultValue(180)]
//		public int TimerLength { get; init; }

//		[Description("Filename of the valid list of words to check against")]
//		[CommandOption("-d|--dict|--dictionary")]
//		public required string Filename { get; init; } = "";
//	}
//}


