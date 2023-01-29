CommandApp app = new();

app.Configure(config => {

	config.AddCommand<BoggleCommand>("boggle")
		.WithExample(new[] { "boggle", })
		.WithExample(new[] { "boggle", "classic" })
		.WithExample(new[] { "boggle", "big" })
		.WithExample(new[] { "boggle", "deluxe" })
		.WithExample(new[] { "boggle", "superbig" });

	config.AddCommand<QLessCommand>("qless")
		.WithExample(new[] { "qless", })
		.WithExample(new[] { "qless", "-p" })
		.WithExample(new[] { "qless", "-v" });

	config.AddCommand<ScrabbleDiceCommand>("scrabbledice")
		.WithExample(new[] { "scrabbledice", })
		.WithExample(new[] { "scrabbledice", "-v" });

});

return app.Run(args);
