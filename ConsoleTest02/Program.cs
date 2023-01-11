CommandApp app = new();

app.Configure(config => {
	config.AddCommand<QLessCommand>("qless");
});

return app.Run(args);
