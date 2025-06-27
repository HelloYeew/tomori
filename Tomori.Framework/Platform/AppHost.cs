// This code is part of the Tomori framework project. Licensed under the MIT License.
// See the LICENSE file for full license text.

using System;
using System.Collections.Generic;
using System.Threading;
using JetBrains.Annotations;
using Tomori.Framework.Extensions.IEnumerableExtensions;
using Tomori.Framework.Logging;

namespace Tomori.Framework.Platform;

public abstract class AppHost : IDisposable
{
    public IWindow Window { get; private set; }

    [NotNull]
    public HostOptions Options { get; private set; }

    public string Name { get; }

    protected AppHost([NotNull] string appName, [CanBeNull] HostOptions options = null)
    {
        Options = options ?? new HostOptions();

        if (string.IsNullOrEmpty(Options.FriendlyAppName))
        {
            Options.FriendlyAppName = $@"Tomori framework (running {appName})";
        }

        Name = appName;
    }

    /// <summary>
    /// All valid user storage paths in order of usage priority.
    /// </summary>
    public virtual IEnumerable<string> UserStoragePaths
        // This is common to _most_ operating systems, with some specific ones overriding this value where a better option exists.
        => Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData, Environment.SpecialFolderOption.Create).Yield();

    private static readonly SemaphoreSlim host_running_mutex = new SemaphoreSlim(1);

    public void Run(App app)
    {
        Logger.AppIdentifier = Name;
        Logger.Initialize();

        foreach (var path in UserStoragePaths)
        {
            Logger.Verbose($"User storage path: {path}");
        }
    }

    public void Dispose()
    {
        Logger.Shutdown();
    }
}
