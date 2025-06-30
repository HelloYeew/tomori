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

    public void Dispose()
    {
        // TODO release managed resources here
    }
}
