// This code is part of the Tomori framework project. Licensed under the MIT License.
// See the LICENSE file for full license text.

using System;

namespace Tomori.Framework.Platform;

public interface IWindow : IDisposable
{
    /// <summary>
    /// The window's title.
    /// </summary>
    string Title { get; set; }

    /// <summary>
    /// Whether the window can be resizable by the user.
    /// </summary>
    bool Resizable { get; set; }

    /// <summary>
    /// Initialize all necessary parts for the window.
    /// This needs to be called before any function that interacts with the window.
    /// </summary>
    void Initialize();

    /// <summary>
    /// Create the window.
    /// </summary>
    void Create();

    /// <summary>
    /// Start the window's main loop.
    /// </summary>
    void Run();

    event Action Update;

    event Action Suspended;

    event Action Resumed;

    event Action ExitRequested;

    event Action Exited;

    /// <summary>
    /// Close the window peacefully.
    /// </summary>
    void Close();
}
