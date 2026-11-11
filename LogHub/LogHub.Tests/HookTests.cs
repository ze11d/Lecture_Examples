using LogHub;
using LogHub.Filtering;
using LogHub.Formatting;
using LogHub.Importing;
using LogHub.Models;
using LogHub.Parsing;
using LogHub.Sinks;
using LogHub.Observers;
using System.Linq;


namespace LogHub.Tests;

public class HookTests
{
    [Fact]
    public void ReplacingImporter_ChangesProcessingResult()
    {
        var record = CreateRecord();

        var firstImporter = new TestImporter(new[] { record });
        var secondImporter = new TestImporter(Array.Empty<LogRecord>());

        var firstSink = new InMemoryLogSink();
        var secondSink = new InMemoryLogSink();

        var firstProcessor = CreateProcessor(firstImporter, firstSink);
        var secondProcessor = CreateProcessor(secondImporter, secondSink);

        var firstResult = firstProcessor.Process("source");
        var secondResult = secondProcessor.Process("source");

        Assert.Equal(1, firstResult.Written);
        Assert.Equal(0, secondResult.Written);
    }

    [Fact]
    public void TemplateMethod_ExecutesStepsInCorrectOrder()
    {
        var importer = new OrderedImporter();

        foreach (var record in importer.Import("source"))
        {
        }

        Assert.Equal(
            new[] { "Open", "Read", "Parse", "Validate", "Close", "Dispose" },
            importer.Steps);
    }

    [Fact]
    public void DefaultValidation_DoesNotRejectRecords()
    {
        var importer = new DefaultValidationImporter();

        var records = importer.Import("source").ToList();

        Assert.Single(records);
    }

    [Fact]
    public void OverriddenValidation_FiltersInvalidRecords()
    {
        var importer = new CustomValidationImporter();

        var records = importer.Import("source").ToList();

        Assert.Empty(records);
    }

    private static LogProcessor CreateProcessor(
        ILogImporter importer,
        InMemoryLogSink sink)
    {
        var filterChain = new LogFilterChain(
            new ILogRecordFilter[]
            {
                new SeverityRecordFilter(LogSeverity.Trace)
            });

        return new LogProcessor(
            importer,
            filterChain,
            new PlainTextFormatter(),
            new ILogSink[] { sink },
            new LogPipelineSubject(),
            new LogNotificationSource(),
            new PushLogObserver(),
            new PipelineMediator());
    }

    private static LogRecord CreateRecord()
    {
        return new LogRecord
        {
            IpAddress = "127.0.0.1",
            UserIdentifier = "-",
            UserName = "-",
            Date = DateTime.Now,
            Request = "GET /index.html",
            StatusCode = 200,
            ResponseSize = 123,
            UserAgent = "TestAgent"
        };
    }

    private class TestImporter : ILogImporter
    {
        private readonly IEnumerable<LogRecord> _records;

        public TestImporter(IEnumerable<LogRecord> records)
        {
            _records = records;
        }

        public IEnumerable<LogRecord> Import(string source)
        {
            return _records;
        }
    }

    private class OrderedImporter : LogImporterBase
    {
        public List<string> Steps { get; } = new();

        protected override void Open(string source)
        {
            Steps.Add("Open");
        }

        protected override IEnumerable<string> Read(string source)
        {
            Steps.Add("Read");
            return new[] { "test" };
        }

        protected override LogRecord? Parse(string line)
        {
            Steps.Add("Parse");
            return CreateRecord();
        }

        protected override bool Validate(LogRecord record)
        {
            Steps.Add("Validate");
            return true;
        }

        protected override void Close()
        {
            Steps.Add("Close");
        }

        protected override void Dispose()
        {
            Steps.Add("Dispose");
        }
    }

    private class DefaultValidationImporter : LogImporterBase
    {
        protected override IEnumerable<string> Read(string source)
        {
            return new[] { "test" };
        }

        protected override LogRecord? Parse(string line)
        {
            return CreateRecord();
        }
    }

    private class CustomValidationImporter : LogImporterBase
    {
        protected override IEnumerable<string> Read(string source)
        {
            return new[] { "test" };
        }

        protected override LogRecord? Parse(string line)
        {
            return CreateRecord();
        }

        protected override bool Validate(LogRecord record)
        {
            return false;
        }
    }
}
