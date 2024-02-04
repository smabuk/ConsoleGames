Console.OutputEncoding = System.Text.Encoding.UTF8;

CommandApp app = new();

app.Configure(config => {

	_ = config.AddCommand<BoggleCommand>("boggle")
		.WithExample(["boggle",])
		.WithExample(["boggle", "classic"])
		.WithExample(["boggle", "big"])
		.WithExample(["boggle", "deluxe"])
		.WithExample(["boggle", "superbig"]);

	_ = config.AddCommand<QLessCommand>("qless")
		.WithExample(["qless",])
		.WithExample(["qless", "-p"])
		.WithExample(["qless", "-v"]);

	//_ = config.AddCommand<ScrabbleDiceCommand>("scrabbledice")
	//	.WithExample(["scrabbledice",])
	//	.WithExample(["scrabbledice", "-v"]);

});

return app.Run(args);
