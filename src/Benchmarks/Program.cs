var config = ManualConfig.Create(DefaultConfig.Instance)
    .WithOptions(ConfigOptions.JoinSummary)
    .WithOptions(ConfigOptions.DisableLogFile)
    // Suppresses build/run logs
    .AddLogger(NullLogger.Instance)
    // Still outputs the tables;
    .AddExporter(MarkdownExporter.Console);
BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args,config);