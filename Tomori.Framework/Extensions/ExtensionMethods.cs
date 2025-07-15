// This code is part of the Tomori framework project. Licensed under the MIT License.
// See the LICENSE file for full license text.

using System.IO;

namespace Tomori.Framework.Extensions;

public static class ExtensionMethods
{
    /// <summary>
    /// Trim DirectorySeparatorChar from the end of the path.
    /// </summary>
    /// <remarks>
    /// Trims both <see cref="Path.DirectorySeparatorChar"/> and <see cref="Path.AltDirectorySeparatorChar"/>.
    /// </remarks>
    /// <param name="path">The path string to trim.</param>
    /// <returns>The path with DirectorySeparatorChar trimmed.</returns>
    public static string TrimDirectorySeparator(this string path)
        => path.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
}
