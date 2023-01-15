namespace ConsoleGames;

[Description("The game of Q-Less")]
public sealed class QLessCommand : Command<QLessCommand.Settings> {

	public override int Execute([NotNull] CommandContext context, [NotNull] Settings settings) {
		QLess.DisplayQLess(settings.Verbose);
		return 0;
	}

	public sealed class Settings : CommandSettings {
		[Description("Display rack breakdown")]
		[CommandOption("-v|--verbose")]
		[DefaultValue(false)]
		public bool Verbose { get; init; }

		[Description("Play (not yet implemented)")]
		[CommandOption("-p|--play")]
		[DefaultValue(false)]
		public bool Play { get; init; }
	}
}

[Description("The game of Boggle")]
public sealed class BoggleCommand : Command<BoggleCommand.Settings> {
	public override int Execute([NotNull] CommandContext context, [NotNull] Settings settings) {
		Boggle boggle = new(settings.BoggleType) {
			Verbose = settings.Verbose,
			GameLength = new(0, 0, settings.TimerLength),
		};
		boggle.DisplayBoard();

		if (settings.Play) {
			boggle.Play();
		}

		return 0;
	}

	public sealed class Settings : CommandSettings {
		[Description("Boggle Type - classic, deluxe, big, superbig, new or challenge")]
		[CommandArgument(0, "[TYPE]")]
		public string Type { get; init; } = "classic";

		public BoggleDice.BoggleType BoggleType => Type.ToLower() switch {
			"classic"   => BoggleDice.BoggleType.Classic4x4,
			"big"       => BoggleDice.BoggleType.BigBoggleOriginal,
			"deluxe"    => BoggleDice.BoggleType.BigBoggleDeluxe,
			"superbig"  => BoggleDice.BoggleType.SuperBigBoggle2012,
			"new"       => BoggleDice.BoggleType.New4x4,
			"challenge" => BoggleDice.BoggleType.BigBoggleChallenge,
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

		public override ValidationResult Validate() {
			string[] validTypes = {
				"classic",
				"big",
				"deluxe",
				"superbig",
				"new",
				"challenge",
			};

			if (!validTypes.Contains(Type.ToLower())) {
				return ValidationResult.Error("Type must be one of classic, deluxe, big, superbig, new or challenge");
			}

			return base.Validate();
		}
	}
}
