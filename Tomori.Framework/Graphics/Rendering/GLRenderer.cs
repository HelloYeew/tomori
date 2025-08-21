// This code is part of the Tomori framework project. Licensed under the MIT License.
// See the LICENSE file for full license text.

#nullable disable

using System;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Text;
using Silk.NET.OpenGL;
using Tomori.Framework.Logging;
using Tomori.Framework.Platform;

namespace Tomori.Framework.Graphics.Rendering;

[SuppressMessage("ReSharper", "InconsistentNaming")]
public class GLRenderer : IRenderer
{
    private static GL gl;

    public unsafe void Initialize(IGraphicsSurface graphicsSurface)
    {
        gl = GL.GetApi(graphicsSurface.GetFunctionAddress);
        if (gl == null)
        {
            throw new InvalidOperationException("Failed to initialize OpenGL context.");
        }

        Logger.Verbose("🖼️ OpenGL renderer initialized");
        byte* glInfo = gl.GetString(StringName.Version);
        Logger.Verbose($"GL Version: {new string((sbyte*)glInfo)}");
        glInfo = gl.GetString(StringName.Renderer);
        Logger.Verbose($"GL Renderer: {new string((sbyte*)glInfo)}");
        glInfo = gl.GetString(StringName.ShadingLanguageVersion);
        Logger.Verbose($"GL Shading Language Version: {new string((sbyte*)glInfo)}");
        glInfo = gl.GetString(StringName.Vendor);
        Logger.Verbose($"GL Vendor: {new string((sbyte*)glInfo)}");
        Logger.Verbose($"GL Extensions: {GetExtensions()}");
        gl.ClearColor(Color.DarkBlue);
    }

    /// <summary>
    /// Retrieves the list of OpenGL extensions supported by the current context.
    /// </summary>
    /// <returns>A string containing the names of all supported OpenGL extensions, separated by spaces.</returns>
    private unsafe string GetExtensions()
    {
        gl.GetInteger(GetPName.NumExtensions, out int numExtensions);
        var extensionStringBuilder = new StringBuilder();
        for (uint i = 0; i < numExtensions; i++)
        {
            byte* extension = gl.GetString(StringName.Extensions, i);
            if (extension != null)
            {
                extensionStringBuilder.Append($"{new string((sbyte*)extension)} ");
            }
        }
        return extensionStringBuilder.ToString().TrimEnd();
    }
}
