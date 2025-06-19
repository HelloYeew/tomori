// This code is part of the Tomori framework project. Licensed under the MIT License.
// See the LICENSE file for full license text.

namespace Tomori.Framework.Platform;

public class DesktopAppHost : AppHost
{
    public bool IsPortableInstallation { get; }

    public DesktopAppHost(string appName, HostOptions options = null) : base(appName, options)
    {
        IsPortableInstallation = Options.PortableInstallation;
    }
}
