namespace ConsoleTest02;

[Description("Display a Boggle board")]
public sealed class BoggleCommand : Command<BoggleCommand.Settings> {
	public override int Execute([NotNull] CommandContext context, [NotNull] Settings settings) {

		BoggleDice.BoggleType type = settings.Type.ToLower() switch {
			"classic"  => BoggleDice.BoggleType.Classic4x4,
			"big"      => BoggleDice.BoggleType.BigBoggleOriginal,
			"deluxe"   => BoggleDice.BoggleType.BigBoggleDeluxe,
			"superbig" => BoggleDice.BoggleType.SuperBigBoggle2012,
			_ => throw new NotImplementedException(),
		};

		BoggleDice boggle = new(type);
		boggle.ShakeAndFillBoard();

		for (int i = 0; i < boggle.BoardSize; i++) {
			Console.WriteLine();
			Console.WriteLine();
			Console.WriteLine();
			Console.WriteLine();
			(int _, int cursorRow) = Console.GetCursorPosition();
			cursorRow -= 3;
			for (int j = 0; j < boggle.BoardSize; j++) {
				DisplayDie(boggle.Board[j + (i * boggle.BoardSize)], null, cursorRow);
			}
		}

		return 0;
	}

	static void DisplayDie(LetterDie die, int? col = null, int? row = null) {
		(int cursorCol, int cursorRow) = Console.GetCursorPosition();
		col ??= cursorCol;
		row ??= cursorRow;
		Console.SetCursorPosition((int)col, (int)row);

		Console.Write($"┌───┐");
		Console.SetCursorPosition((int)col, (int)row + 1);
		Console.Write($"│ {die.FaceValue.Display, -2}│");
		Console.SetCursorPosition((int)col, (int)row + 2);
		Console.Write($"└───┘");
	}

	public sealed class Settings : CommandSettings {
		[Description("Boggle Type - classic, deluxe, big or superbig")]
		[CommandArgument(0, "[TYPE]")]
		public required string Type { get; init; } = "classic";

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
