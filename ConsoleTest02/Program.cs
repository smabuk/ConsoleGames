CommandApp app = new();

app.Configure(config => {

	config.AddCommand<QLessCommand>("qless")
		.WithDescription("Display a Q-Less rack")
		.WithExample(new[] { "qless", })
		.WithExample(new[] { "qless", "-v" });

	config.AddCommand<BoggleCommand>("boggle")
		.WithDescription("Display a Boggle board")
		.WithExample(new[] { "boggle", })
		.WithExample(new[] { "boggle", "classic" })
		.WithExample(new[] { "boggle", "big" })
		.WithExample(new[] { "boggle", "deluxe" })
		.WithExample(new[] { "boggle", "superbig" });
});

return app.Run(args);
