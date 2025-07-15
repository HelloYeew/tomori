// This code is part of the Tomori framework project. Licensed under the MIT License.
// See the LICENSE file for full license text.

using System;

namespace Tomori.Framework.Platform;

public interface IWindow : IDisposable
{
    /// <summary>
    /// Create the window.
    /// </summary>
    void Create();

    /// <summary>
    /// Start the window's main loop.
    /// </summary>
    void Run();

    /// <summary>
    /// Close the window peacefully.
    /// </summary>
    void Close();
}
