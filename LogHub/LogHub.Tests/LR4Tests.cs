using System;
using System.Collections.Generic;
using LogHub;
using LogHub.Filtering;
using LogHub.Models;
using LogHub.Observers;
using Xunit;
using LogHub.Formatting;

namespace LogHub.Tests;

public class Lab7Tests
{
    [Fact]
    public void Filters_RejectRecordsIndividually()
    {
        var oldRecord = CreateRecord(date: DateTime.UtcNow.AddDays(-30));
        var lowSeverityRecord = CreateRecord(severity: LogSeverity.Trace);
        var duplicateRecord = CreateRecord();

        Assert.False(new LogFilterChain(new ILogRecordFilter[]
        {
            new SeverityRecordFilter(LogSeverity.Warning)
        }).IsAllowed(lowSeverityRecord));

        Assert.False(new LogFilterChain(new ILogRecordFilter[]
        {
            new RecordAgeFilter(TimeSpan.FromDays(7))
        }).IsAllowed(oldRecord));

        var duplicateFilter = new DuplicateRecordFilter();
        Assert.True(duplicateFilter.IsAllowed(duplicateRecord));
        Assert.False(duplicateFilter.IsAllowed(duplicateRecord));
    }

    [Fact]
    public void EmptyChain_AllowsAllRecords()
    {
        var chain = new LogFilterChain(Array.Empty<ILogRecordFilter>());

        Assert.True(chain.IsAllowed(CreateRecord()));
        Assert.True(chain.IsAllowed(CreateRecord(severity: LogSeverity.Trace)));
        Assert.True(chain.IsAllowed(CreateRecord(date: DateTime.UtcNow.AddYears(-1))));
    }

    [Fact]
    public void FilterOrder_ChangesDuplicateFilterState()
    {
        var oldRecord = CreateRecord(date: DateTime.UtcNow.AddDays(-30));

        var ageFirstDuplicateFilter = new DuplicateRecordFilter();
        var ageFirstChain = new LogFilterChain(new ILogRecordFilter[]
        {
            new RecordAgeFilter(TimeSpan.FromDays(7)),
            ageFirstDuplicateFilter
        });

        Assert.False(ageFirstChain.IsAllowed(oldRecord));
        Assert.True(ageFirstDuplicateFilter.IsAllowed(oldRecord));

        var duplicateFirstFilter = new DuplicateRecordFilter();
        var duplicateFirstChain = new LogFilterChain(new ILogRecordFilter[]
        {
            duplicateFirstFilter,
            new RecordAgeFilter(TimeSpan.FromDays(7))
        });

        Assert.False(duplicateFirstChain.IsAllowed(oldRecord));
        Assert.False(duplicateFirstFilter.IsAllowed(oldRecord));
    }

    [Fact]
    public void NewVisitor_CanVisitEntriesWithoutChangingEntryClasses()
    {
        var visitor = new RecordingVisitor();

        new SimpleLogEntry { Message = "simple" }.Accept(visitor);
        new ExceptionLogEntry { Message = "exception", StackTrace = "stack" }.Accept(visitor);
        new MetricLogEntry { Value = 12.5 }.Accept(visitor);

        Assert.Equal(3, visitor.VisitedCount);
    }

    [Fact]
    public void Visitor_DispatchesToCorrectVisitOverloads()
    {
        var visitor = new RecordingVisitor();

        new SimpleLogEntry { Message = "message" }.Accept(visitor);
        new ExceptionLogEntry { Message = "failure", StackTrace = "trace" }.Accept(visitor);
        new MetricLogEntry { Value = 3.14 }.Accept(visitor);

        Assert.Equal(1, visitor.SimpleCount);
        Assert.Equal(1, visitor.ExceptionCount);
        Assert.Equal(1, visitor.MetricCount);
    }

    [Fact]
    public void UnsubscribedObserver_DoesNotReceiveEvents()
    {
        var subject = new LogPipelineSubject();
        var observer = new CountingObserver();

        subject.Subscribe(observer);
        subject.Unsubscribe(observer);
        subject.Notify(CreateRecord());

        Assert.Equal(0, observer.Count);
    }

    [Fact]
    public void ThrowingObserver_DoesNotBlockOtherObservers()
    {
        var subject = new LogPipelineSubject();
        var throwing = new ThrowingObserver();
        var counting = new CountingObserver();

        subject.Subscribe(throwing);
        subject.Subscribe(counting);
        subject.Notify(CreateRecord());

        Assert.Equal(1, counting.Count);
    }

    [Fact]
    public void PushAndPull_ReceiveSameRecord()
    {
        var record = CreateRecord();
        var source = new LogNotificationSource();
        var push = new PushLogObserver();
        var pull = new PullLogObserver(source.GetRecord);

        source.RecordProcessed += pull.OnRecordProcessed;
        source.NotifyPush(record, push);
        source.NotifyPull(record);

        Assert.Same(record, push.LastRecord);
        Assert.Same(record, pull.LastRecord);
        Assert.Equal(push.Count, pull.Count);
    }

    [Fact]
    public void TextVisitor_FormatsExceptionAndStackTrace()
    {
        var visitor = new LogEntryTextVisitor();

        new ExceptionLogEntry
        {
            Message = "Something failed",
            StackTrace = "at Example.Run()"
        }.Accept(visitor);

        Assert.Contains("Something failed", visitor.Result);
        Assert.Contains("at Example.Run()", visitor.Result);
    }

    [Fact]
    public void StatisticsVisitor_CountsAllEntryTypes()
    {
        var visitor = new LogEntryStatisticsVisitor();

        new SimpleLogEntry { Message = "one" }.Accept(visitor);
        new SimpleLogEntry { Message = "two" }.Accept(visitor);
        new ExceptionLogEntry { Message = "error", StackTrace = "trace" }.Accept(visitor);
        new MetricLogEntry { Value = 1 }.Accept(visitor);

        Assert.Equal(2, visitor.SimpleCount);
        Assert.Equal(1, visitor.ExceptionCount);
        Assert.Equal(1, visitor.MetricCount);
    }

    [Fact]
    public void SeverityFilter_AllowsRecordAtMinimumSeverity()
    {
        var chain = new LogFilterChain(new ILogRecordFilter[]
        {
            new SeverityRecordFilter(LogSeverity.Warning)
        });

        Assert.True(chain.IsAllowed(CreateRecord(severity: LogSeverity.Warning)));
    }

    [Fact]
    public void AgeFilter_AllowsRecentRecord()
    {
        var chain = new LogFilterChain(new ILogRecordFilter[]
        {
            new RecordAgeFilter(TimeSpan.FromDays(7))
        });

        Assert.True(chain.IsAllowed(CreateRecord(date: DateTime.UtcNow.AddMinutes(-5))));
    }

    private static LogRecord CreateRecord(
        LogSeverity severity = LogSeverity.Info,
        DateTime? date = null)
    {
        return new LogRecord
        {
            Severity = severity,
            Date = date ?? DateTime.UtcNow,
            IpAddress = "127.0.0.1",
            Request = "GET /test",
            StatusCode = 200
        };
    }

    private sealed class RecordingVisitor : ILogEntryVisitor
    {
        public int VisitedCount => SimpleCount + ExceptionCount + MetricCount;
        public int SimpleCount { get; private set; }
        public int ExceptionCount { get; private set; }
        public int MetricCount { get; private set; }

        public void Visit(SimpleLogEntry entry) => SimpleCount++;
        public void Visit(ExceptionLogEntry entry) => ExceptionCount++;
        public void Visit(MetricLogEntry entry) => MetricCount++;
    }

    private sealed class CountingObserver : ILogPipelineObserver
    {
        public int Count { get; private set; }

        public void OnRecordProcessed(LogRecord record) => Count++;
    }

    private sealed class ThrowingObserver : ILogPipelineObserver
    {
        public void OnRecordProcessed(LogRecord record) =>
            throw new InvalidOperationException("Observer test exception");
    }
}