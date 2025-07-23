// This code is part of the Tomori framework project. Licensed under the MIT License.
// See the LICENSE file for full license text.

using Silk.NET.SDL;
using Tomori.Framework.Logging;

namespace Tomori.Framework.Platform;

public class SDLWindow : IWindow
{
    private static Sdl _sdl;
    private static unsafe Window* window;

    private string title = "Window";
    private bool resizable = true;

    private WindowFlags windowFlags = WindowFlags.Opengl | WindowFlags.AllowHighdpi;

    public string Title
    {
        get => title;
        set => setTitle(value);
    }

    public bool Resizable
    {
        get => resizable;
        set => setResizable(value);
    }

    public void Initialize()
    {
        _sdl = Sdl.GetApi();

        Version sdlVersion = new Version();

        Logger.Verbose("SDL initialized");
        Logger.Verbose($"SDL Version: {sdlVersion.Major}.{sdlVersion.Minor}.{sdlVersion.Patch}");

        windowFlags = WindowFlags.Opengl | WindowFlags.AllowHighdpi;
    }

    public unsafe void Create()
    {
        if (Resizable)
        {
            windowFlags |= WindowFlags.Resizable;
        }

        window = _sdl.CreateWindow(
            title,
            Sdl.WindowposCentered, // X position
            Sdl.WindowposCentered, // Y position
            800, // Width
            600, // Height
            (uint)windowFlags
        );
    }

    public void Run()
    {
        throw new System.NotImplementedException();
    }

    public void Close()
    {
        throw new System.NotImplementedException();
    }

    public void Dispose()
    {
        throw new System.NotImplementedException();
    }

    private unsafe void setTitle(string newTitle)
    {
        if (window == null)
            return;

        _sdl.SetWindowTitle(window, newTitle);
        title = newTitle;
    }

    private unsafe void setResizable(bool isResizable)
    {
        if (window == null)
            return;

        if (isResizable)
            windowFlags |= WindowFlags.Resizable;
        else
            windowFlags &= ~WindowFlags.Resizable;

        _sdl.SetWindowResizable(window, (SdlBool)(isResizable ? 1 : 0));
        resizable = isResizable;
    }
}
