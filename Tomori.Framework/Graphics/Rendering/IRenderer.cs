// This code is part of the Tomori framework project. Licensed under the MIT License.
// See the LICENSE file for full license text.

using Tomori.Framework.Platform;

namespace Tomori.Framework.Graphics.Rendering;

public interface IRenderer
{
    /// <summary>
    /// Initializes the renderer to be used with the specified window.
    /// </summary>
    protected internal void Initialize(IGraphicsSurface graphicsSurface);
}
