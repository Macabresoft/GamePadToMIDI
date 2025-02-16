namespace Macabresoft.Macabre2D.Project.Common;

using Microsoft.Xna.Framework;

/// <summary>
/// Predefined colors that can be accessed by the editor and the game.
/// </summary>
/// <remarks>
/// This existing as a prerequisite to using the editor is for selecting predefined colors from the editor.
/// </remarks>
public static class PredefinedColors {
    /// <summary>
    /// Gets the colors available to the Macabre2D editor.
    /// </summary>
    public static IReadOnlyCollection<Color> Colors { get; } = new[] {
        Color.Transparent,
        Color.White,
        Color.Black
    };
}