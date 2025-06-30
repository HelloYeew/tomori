// This code is part of the Tomori framework project. Licensed under the MIT License.
// See the LICENSE file for full license text.

using System;
using System.Collections.Concurrent;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Tomori.Framework.Development;

namespace Tomori.Framework.Logging;

public class Logger
{
    private static readonly ConcurrentQueue<LogMessage> message_queue = new ConcurrentQueue<LogMessage>();
    private static readonly ConcurrentDictionary<LoggingTarget, StreamWriter> file_writers = new ConcurrentDictionary<LoggingTarget, StreamWriter>();
    private static readonly Lock console_lock = new Lock();  // Lock for console output to not mutate concurrently

    private static Task? processingTask;
    private static CancellationTokenSource? cancellationTokenSource;
    private static string logDirectory = "logs";
    private static long startupTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

    private readonly record struct LogMessage(DateTime Timestamp, LogLevel Level, LoggingTarget Target, string Message);

    #region Public Properties

    /// <summary>
    /// Gets or sets the minimum log level to be processed. Messages below this level will be ignored.
    /// Default is <see cref="LogLevel.Debug"/>.
    /// </summary>
    public static LogLevel MinimumLogLevel { get; set; } = LogLevel.Debug;

    /// <summary>
    /// Gets or sets whether to log messages to the console.
    /// Default is <c>true</c>.
    /// </summary>
    public static bool LogToConsole { get; set; } = true;

    /// <summary>
    /// Gets or sets the application name. Must be set before Initialize().
    /// If not set, it will be inferred from the entry assembly.
    /// </summary>
    public static string? AppIdentifier { get; set; }

    /// <summary>
    /// Gets or sets the application version. Must be set before Initialize().
    /// If not set, it will be inferred from the entry assembly.
    /// </summary>
    public static string? VersionIdentifier { get; set; }

    #endregion

    #region Public Methods

    /// <summary>
    /// Log a <see cref="LogLevel.Debug"/> message to the logger.
    /// </summary>
    /// <param name="message">The message to log.</param>
    /// <param name="target">The target to write the log message to</param>
    public static void Debug(string message, LoggingTarget target = LoggingTarget.Runtime) => log(message, LogLevel.Debug, target);

    /// <summary>
    /// Log a <see cref="LogLevel.Verbose"/> message to the logger.
    /// </summary>
    /// <param name="message">The message to log.</param>
    /// <param name="target">The target to write the log message to</param>
    public static void Verbose(string message, LoggingTarget target = LoggingTarget.Runtime) => log(message, LogLevel.Verbose, target);

    /// <summary>
    /// Log a <see cref="LogLevel.Warning"/> message to the logger.
    /// </summary>
    /// <param name="message">The message to log.</param>
    /// <param name="target">The target to write the log message to</param>
    public static void Warning(string message, LoggingTarget target = LoggingTarget.Runtime) => log(message, LogLevel.Warning, target);

    /// <summary>
    /// Log a <see cref="LogLevel.Error"/> message to the logger.
    /// </summary>
    /// <param name="message">The message to log.</param>
    /// <param name="target">The target to write the log message to</param>
    public static void Error(string message, LoggingTarget target = LoggingTarget.Runtime) => log(message, LogLevel.Error, target);

    /// <summary>
    /// Log an <see cref="Exception"/> to the logger with a <see cref="LogLevel.Error"/> level.
    /// </summary>
    /// <param name="ex">The exception to log.</param>
    /// <param name="target">The target to write the log message to</param>
    public static void Error(Exception ex, LoggingTarget target = LoggingTarget.Runtime) => log(ex.ToString(), LogLevel.Error, target);

    /// <summary>
    /// Log a message with an <see cref="Exception"/> to the logger with a <see cref="LogLevel.Error"/> level.
    /// </summary>
    /// <param name="message">The message to log.</param>
    /// <param name="ex">The exception to log.</param>
    /// <param name="target">The target to write the log message to</param>
    public static void Error(string message, Exception ex, LoggingTarget target = LoggingTarget.Runtime) => log($"{message}\n{ex}", LogLevel.Error, target);

    /// <summary>
    /// Log a <see cref="LogLevel.Fatal"/> message to the logger.
    /// </summary>
    /// <param name="message">The message to log.</param>
    /// <param name="target">The target to write the log message to</param>
    public static void Fatal(string message, LoggingTarget target = LoggingTarget.Runtime) => log(message, LogLevel.Fatal, target);

    /// <summary>
    /// Log an <see cref="Exception"/> to the logger with a <see cref="LogLevel.Fatal"/> level.
    /// </summary>
    /// <param name="ex">The exception to log.</param>
    /// <param name="target">The target to write the log message to</param>
    public static void Fatal(Exception ex, LoggingTarget target = LoggingTarget.Runtime) => log(ex.ToString(), LogLevel.Fatal, target);

    /// <summary>
    /// Log a message with an <see cref="Exception"/> to the logger with a <see cref="LogLevel.Fatal"/> level.
    /// </summary>
    /// <param name="message">The message to log.</param>
    /// <param name="ex">The exception to log.</param>
    /// <param name="target">The target to write the log message to</param>
    public static void Fatal(string message, Exception ex, LoggingTarget target = LoggingTarget.Runtime) => log($"{message}\n{ex}", LogLevel.Fatal, target);

    /// <summary>
    /// Log a message to the logger with a specified <see cref="LogLevel"/> and <see cref="LoggingTarget"/>, also prints it to the debug output if in a debug build.
    /// </summary>
    /// <param name="message"> The message to log.</param>
    /// <param name="level"> The log level of the message. Default is <see cref="LogLevel.Verbose"/>.</param>
    /// <param name="target"> The target to write the log message to. Default is <see cref="LoggingTarget.Runtime"/>.</param>
    public static void LogPrint(string message, LogLevel level = LogLevel.Verbose, LoggingTarget target = LoggingTarget.Runtime)
    {
        if (DebugUtils.IsDebugBuild)
            System.Diagnostics.Debug.Print(message);
        log(message, level, target);
    }

    #endregion

    /// <summary>
    /// Initialize the logger, creates the log directory if it does not exist, and starts the background processing task.
    /// <remarks>This method should be called once at application startup. <see cref="AppIdentifier"/> and <see cref="VersionIdentifier"/> should be set before calling this.</remarks>
    /// </summary>
    /// <param name="logPath">The base directory where log files will be stored. If null or empty, defaults to "logs".</param>
    /// <param name="minimumLogLevel">The minimum log level to be processed. Messages below this level will be ignored. Default is <see cref="LogLevel.Debug"/>.</param>
    /// <param name="logToConsole">Whether to log messages to the console. Default is <c>true</c>.</param>
    public static void Initialize(string? logPath = null, LogLevel minimumLogLevel = LogLevel.Debug, bool logToConsole = true)
    {
        if (processingTask != null && !processingTask.IsCompleted)
        {
            Warning("Logger is already initialized.");
            return;
        }

        logDirectory = string.IsNullOrWhiteSpace(logPath) ? "logs" : logPath;
        MinimumLogLevel = minimumLogLevel;
        startupTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        if (!Directory.Exists(logDirectory))
        {
            Directory.CreateDirectory(logDirectory);
        }

        clearOldLogs();

        cancellationTokenSource = new CancellationTokenSource();

        processingTask = Task.Run(() => processLogQueue(cancellationTokenSource.Token));
    }

    /// <summary>
    /// Clears old log files that are older than the specified retention days.
    /// </summary>
    /// <param name="retentionDays"></param>
    private static void clearOldLogs(int retentionDays = 7)
    {
        if (!Directory.Exists(logDirectory)) return;

        string[] files = Directory.GetFiles(logDirectory, "*.log");
        var threshold = DateTime.UtcNow.AddDays(-retentionDays);

        foreach (string file in files)
        {
            try
            {
                var lastWriteTime = File.GetLastWriteTimeUtc(file);
                if (lastWriteTime < threshold)
                {
                    File.Delete(file);
                }
            }
            catch (Exception ex)
            {
                Error($"Failed to delete log file {file}", ex);
            }
        }
    }

    /// <summary>
    /// Signals the logger to shut down gracefully.
    /// Before completing, it will wait for all queued messages to be processed before stopping and closing all file writers.
    /// </summary>
    public static void Shutdown()
    {
        if (cancellationTokenSource == null || processingTask == null) return;

        Verbose("Logger is shutting down...");

        cancellationTokenSource.Cancel();

        try
        {
            processingTask.Wait();
        }
        catch (OperationCanceledException)
        {

        }
        catch (AggregateException ex)
        {
            ex.Handle(e => e is OperationCanceledException);
        }

        cancellationTokenSource.Dispose();

        foreach (var writer in file_writers.Values)
        {
            writer.Flush();
            writer.Dispose();
        }

        file_writers.Clear();
    }

    private static void log(string message, LogLevel level, LoggingTarget target)
    {
        if (cancellationTokenSource == null || cancellationTokenSource.IsCancellationRequested || level < MinimumLogLevel)
            return;

        message_queue.Enqueue(new LogMessage(DateTime.UtcNow, level, target, message));
    }

    private static async Task processLogQueue(CancellationToken token)
    {
        while (!token.IsCancellationRequested || !message_queue.IsEmpty)
        {
            if (message_queue.TryDequeue(out var logMessage))
            {
                try
                {
                    await writeLogMessage(logMessage);
                }
                catch (Exception ex)
                {
                    await Console.Error.WriteLineAsync($"Failed to write log message: {ex}");
                }
            }
            else
            {
                try
                {
                    await Task.Delay(10, token).ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    // This is expected when shutdown is called and the queue is empty.
                    // Exit the loop.
                    break;
                }
            }
        }
    }

    private static async Task writeLogMessage(LogMessage logMessage)
    {
        string formattedMessage = $"{logMessage.Timestamp:yyyy-MM-dd HH:mm:ss} [{getLogLevelPrefix(logMessage.Level)}]: {logMessage.Message}";

        var writer = getFileWriter(logMessage.Target);
        await writer.WriteLineAsync(formattedMessage);

        formattedMessage = $"[{logMessage.Target.ToString().ToLower()}] " + formattedMessage;

        if (LogToConsole)
        {
            lock (console_lock)
            {
                // For debug listeners
                System.Diagnostics.Debug.Print(formattedMessage);
                // For console output
                Console.WriteLine(formattedMessage);
            }
        }
    }

    private static StreamWriter getFileWriter(LoggingTarget target)
    {
        return file_writers.GetOrAdd(target, (key) =>
        {
            string filePath = Path.Combine(logDirectory, $"{startupTimestamp}.{key.ToString().ToLower()}.log");
            bool fileExists = File.Exists(filePath);

            var stream = new FileStream(filePath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
            var writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true };

            if (!fileExists)
            {
                writer.WriteLine(generateHeader(key));
            }

            return writer;
        });
    }

    private static string generateHeader(LoggingTarget target)
    {
        var entryAssembly = Assembly.GetEntryAssembly();
        string appName = AppIdentifier ?? entryAssembly?.GetName().Name ?? "UnknownApp";
        string appVersion = VersionIdentifier ?? entryAssembly?.GetName().Version?.ToString() ?? "0.0.0";

        string frameworkVersion = typeof(Logger).Assembly.GetName().Version?.ToString() ?? "0.0.0";

        var stringBuilder = new StringBuilder();
        stringBuilder.AppendLine("----------------------------------------------------------");
        stringBuilder.AppendLine($"{target} Log (LogLevel: {MinimumLogLevel})");
        stringBuilder.AppendLine($"Running {appName} {appVersion} on Tomori Framework {frameworkVersion} using .NET {Environment.Version}");
        stringBuilder.AppendLine($"Environment: {Environment.OSVersion} ({Environment.ProcessorCount} cores) (Debug: {DebugUtils.IsDebugBuild})");
        stringBuilder.AppendLine("----------------------------------------------------------");
        return stringBuilder.ToString();
    }

    private static string getLogLevelPrefix(LogLevel level) => level == LogLevel.Debug ? "verbose" : level.ToString().ToLower();
}

/// <summary>
/// Defines the severity levels for log messages.
/// </summary>
public enum LogLevel
{
    /// <summary>
    /// Detailed information, typically of interest only when diagnosing problems.
    /// </summary>
    Debug,

    /// <summary>
    /// Most log messages that are not errors or warnings, such as informational messages.
    /// </summary>
    Verbose,

    /// <summary>
    /// Indicate some potential problem or important situation that is not an error.
    /// </summary>
    Warning,

    /// <summary>
    /// Indicates a failure in the application that is not critical but should be addressed and can be recovered from.
    /// </summary>
    Error,

    /// <summary>
    /// Indicates a critical error that causes the application to stop functioning properly
    /// </summary>
    Fatal
}

public enum LoggingTarget
{
    /// <summary>
    /// General runtime information.
    /// </summary>
    Runtime,

    /// <summary>
    /// Performance-related metrics and information.
    /// </summary>
    Performance,

    /// <summary>
    /// Network activities, such as requests and responses.
    /// </summary>
    Network,

    /// <summary>
    /// Graphics-related information, such as rendering details or graphics errors.
    /// </summary>
    Graphics,

    /// <summary>
    /// Database-related information, such as queries and transactions.
    /// </summary>
    Database
}
