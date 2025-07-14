// This code is part of the Tomori framework project. Licensed under the MIT License.
// See the LICENSE file for full license text.

#nullable disable

using System;
using Tomori.Framework.Platform;

namespace Tomori.Framework;

public class App : IDisposable
{
    public IWindow Window => Host?.Window;

    protected AppHost Host { get; private set; }

    /// <summary>
    /// Create a default <see cref="Storage"/> that
    /// </summary>
    /// <param name="host"></param>
    /// <param name="defaultStorage"></param>
    /// <returns></returns>
    protected internal virtual Storage CreateStorage(AppHost host, Storage defaultStorage) => defaultStorage;

    public void Dispose()
    {
        // TODO release managed resources here
    }
}
