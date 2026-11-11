using Microsoft.Extensions.DependencyInjection;
using LogHub;
using LogHub.Filtering;
using LogHub.Formatting;
using LogHub.Importing;
using LogHub.Parsing;
using LogHub.Sinks;
using LogHub.Graphs;
using LogHub.Configuration;
using LogHub.Observers;

var profilePath = Path.Combine(AppContext.BaseDirectory, "profile.txt");

var profile = File.Exists(profilePath)
    ? File.ReadAllText(profilePath).Trim().ToLowerInvariant()
    : "console";

var formatter = new PlainTextFormatter();
var memorySink = new InMemoryLogSink();

ILogSink outputSink = profile switch
{
    "console" => new ConsoleLogSink(formatter),
    "file" => new FileLogSink("logs.txt", formatter),
    _ => throw new InvalidOperationException(
        $"Неизвестный профиль: {profile}")
};

var services = new ServiceCollection();
services.AddSingleton<LogNotificationSource>();
services.AddSingleton<PushLogObserver>();

services.AddSingleton<PullLogObserver>(sp =>
{
    var source = sp.GetRequiredService<LogNotificationSource>();
    var observer = new PullLogObserver(source.GetRecord);

    source.RecordProcessed += observer.OnRecordProcessed;

    return observer;
});
services.AddSingleton<LogPipelineSubject>(_ =>
{
    var subject = new LogPipelineSubject();

    subject.Subscribe(new LogRecordCounter());
    subject.Subscribe(new CriticalAlertObserver());
    subject.Subscribe(new ConsoleLogObserver());

    return subject;
});
var config = new ConfigService();
config.SetValue("FilterOrder", "severity,age,duplicates");

services.AddSingleton<IConfigService>(config);
services.AddSingleton<LogFilterChainFactory>();

services.AddSingleton<LogFilterChain>(sp =>
    sp.GetRequiredService<LogFilterChainFactory>().Create());

services.AddSingleton<PlainTextFormatter>(formatter);
services.AddSingleton<InMemoryLogSink>(memorySink);

services.AddSingleton<ILogSink>(memorySink);
services.AddSingleton<ILogSink>(outputSink);

services.AddSingleton<ApacheLogParser>();
services.AddSingleton<PipelineMediator>();
services.AddSingleton<IMediator>(sp =>
    sp.GetRequiredService<PipelineMediator>());
services.AddSingleton<ApacheLogImporter>();
services.AddSingleton<CsvLogImporter>();

services.AddSingleton<InMemoryLogImporter>(_ =>
    new InMemoryLogImporter(memorySink.Records));

var importerName = args.Length > 1
    ? args[1].Trim().ToLowerInvariant()
    : "apache";

var importerFactories =
    new Dictionary<string, Func<IServiceProvider, ILogImporter>>
    {
        ["apache"] = sp =>
            sp.GetRequiredService<ApacheLogImporter>(),

        ["csv"] = sp =>
            sp.GetRequiredService<CsvLogImporter>(),

        ["inmemory"] = sp =>
            sp.GetRequiredService<InMemoryLogImporter>()
    };

services.AddSingleton<ILogImporter>(sp =>
{
    var importer = importerFactories.TryGetValue(importerName, out var factory)
        ? factory(sp)
        : throw new InvalidOperationException(
            $"Неизвестный импортёр: {importerName}");

    if (importer is IMediatorAwareImporter awareImporter)
        awareImporter.SetMediator(sp.GetRequiredService<IMediator>());

    return importer;
});

services.AddSingleton<LogProcessor>();

using var provider = services.BuildServiceProvider();

var processor = provider.GetRequiredService<LogProcessor>();

var inputFile = args.Length > 0 ? args[0] : "access.log";

var pullObserver = provider.GetRequiredService<PullLogObserver>();

var result = processor.Process(inputFile);

var pushObserver = provider.GetRequiredService<PushLogObserver>();

Console.WriteLine($"Push: {pushObserver.Count}");
Console.WriteLine($"Pull: {pullObserver.Count}");

Console.WriteLine($"Импортёр: {importerName}");
Console.WriteLine($"Записано: {result.Written}");
Console.WriteLine($"Пропущено: {result.Skipped}");
Console.WriteLine($"В памяти сохранено: {memorySink.Records.Count}");

var serviceA = new Node { Name = "ServiceA" };
var serviceB = new Node { Name = "ServiceB" };
var serviceC = new Node { Name = "ServiceC" };

serviceA.Dependencies.Add(serviceB);
serviceA.Dependencies.Add(serviceC);

var graph = new Graph();

graph.PrintCycles(serviceA);

foreach (var service in graph.DepthFirst(serviceA))
{
    foreach (var dependency in graph.BreadthFirst(service))
    {
        Console.WriteLine($"{service} → {dependency}");
    }
}

Console.WriteLine("Ручной DFS:");

using var enumerator = graph.ManualDepthFirst(serviceA);

while (enumerator.MoveNext())
{
    Console.WriteLine(enumerator.Current.Name);
}

Console.WriteLine("DFS через yield:");

foreach (var node in graph.DepthFirst(serviceA))
{
    Console.WriteLine(node.Name);
}
