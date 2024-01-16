﻿namespace ConsoleGames;

public static class KeyReader {
	private static readonly Thread inputThread;
	private static readonly AutoResetEvent getKey;
	private static readonly AutoResetEvent gotKey;
	private static ConsoleKey key;

	static KeyReader() {
		getKey = new AutoResetEvent(false);
		gotKey = new AutoResetEvent(false);
		inputThread = new Thread(Reader) {
			IsBackground = true
		};
		inputThread.Start();
	}

	private static void Reader() {
		while (true) {
			_ = getKey.WaitOne();
			key = Console.ReadKey(true).Key;
			_ = gotKey.Set();
		}
	}

	public static ConsoleKey? ReadKey(int timeOutMillisecs = Timeout.Infinite) {
		_ = getKey.Set();
		bool success = gotKey.WaitOne(timeOutMillisecs);
		return success ? key : null;
	}
}
