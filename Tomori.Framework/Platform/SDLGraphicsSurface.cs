// This code is part of the Tomori framework project. Licensed under the MIT License.
// See the LICENSE file for full license text.

using System;
using System.Diagnostics.CodeAnalysis;

namespace Tomori.Framework.Platform;

[SuppressMessage("ReSharper", "InconsistentNaming")]
public class SDLGraphicsSurface : IGraphicsSurface
{
    private Func<string, IntPtr> GetFunctionAddress { get; set; }

    Func<string, IntPtr> IGraphicsSurface.GetFunctionAddress
    {
        get => GetFunctionAddress;
        set => GetFunctionAddress = value;
    }
}
