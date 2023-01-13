namespace ConsoleTest02;

[Description("Display a Q-Less rack")]
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
	}

}

[Description("Display a Boggle board")]
public sealed class BoggleCommand : Command<BoggleCommand.Settings> {
	public override int Execute([NotNull] CommandContext context, [NotNull] Settings settings) {
		Boggle.DisplayBoggle(settings.BoggleType, settings.Play);
		return 0;
	}

	public sealed class Settings : CommandSettings {
		[Description("Boggle Type - classic, deluxe, big or superbig")]
		[CommandArgument(0, "[TYPE]")]
		public string Type { get; init; } = "classic";

		public BoggleDice.BoggleType BoggleType => Type.ToLower() switch {
			"classic" => BoggleDice.BoggleType.Classic4x4,
			"big" => BoggleDice.BoggleType.BigBoggleOriginal,
			"deluxe" => BoggleDice.BoggleType.BigBoggleDeluxe,
			"superbig" => BoggleDice.BoggleType.SuperBigBoggle2012,
			_ => throw new NotImplementedException(),
		};

		[Description("Play")]
		[CommandOption("-p|--play")]
		[DefaultValue(false)]
		public bool Play { get; init; }

		public override ValidationResult Validate() {
			string[] validTypes = {
				"classic",
				"big",
				"deluxe",
				"superbig",
			};

			if (!validTypes.Contains(Type.ToLower())) {
				return ValidationResult.Error("Type must be one of classic, deluxe, big or superbig");
			}

			return base.Validate();
		}
	}
}
