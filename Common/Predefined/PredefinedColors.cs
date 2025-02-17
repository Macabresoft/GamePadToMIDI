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
    /// Static constructor for <see cref="PredefinedColors" />.
    /// </summary>
    static PredefinedColors() {
        Colors = [
            MacabrePurple,
            MacabreLightPurple,
            MacabreWhite,
            MacabreGrey,
            MacabreYellow,
            MacabreRed,
            MacabreBlue,
            MacabreLightBlue,
            Color.White,
            Color.Black,
            Color.Transparent
        ];
    }

    /// <summary>
    /// The colors of the cosmos background fade.
    /// </summary>
    public static Color BackgroundFadeColor { get; } = new(63, 63, 63);

    /// <summary>
    /// Gets the colors available to the Macabre2D editor.
    /// </summary>
    public static IReadOnlyCollection<Color> Colors { get; }

    /// <summary>
    /// Gets the combo score color.
    /// </summary>
    public static Color ComboScoreColor => MacabreYellow;

    /// <summary>
    /// Gets the combo success color.
    /// </summary>
    public static Color ComboSuccessColor => MacabreYellow;

    /// <summary>
    /// The colors of the cosmos background fade.
    /// </summary>
    public static Color CosmosFadeColor { get; } = new(159, 159, 159);

    /// <summary>
    /// Gets the color of deactivated text.
    /// </summary>
    public static Color DeactivatedText => MacabreGrey;

    /// <summary>
    /// Gets the blue color in the palette.
    /// </summary>
    public static Color MacabreBlue { get; } = new(30, 100, 162);

    /// <summary>
    /// Gets the color of grey.
    /// </summary>
    public static Color MacabreGrey { get; } = new(91, 80, 73);

    /// <summary>
    /// Gets the blue color in the palette.
    /// </summary>
    public static Color MacabreLightBlue { get; } = new(24, 146, 160);

    /// <summary>
    /// Gets the combo gap color.
    /// </summary>
    public static Color MacabreLightPurple { get; } = new(139, 124, 185);

    /// <summary>
    /// The purple seen in Macabresoft's logo.
    /// </summary>
    public static Color MacabrePurple { get; } = new(99, 73, 96);

    /// <summary>
    /// Gets the combo bail color.
    /// </summary>
    public static Color MacabreRed { get; } = new(203, 0, 29);

    /// <summary>
    /// Gets the off-white used in this game.
    /// </summary>
    public static Color MacabreWhite { get; } = new(227, 218, 201);

    /// <summary>
    /// Gets the yellow used in this game.
    /// </summary>
    public static Color MacabreYellow { get; } = new(255, 172, 10);

    /// <summary>
    /// Gets the text highlight color.
    /// </summary>
    public static Color TextHighlightColor => MacabreYellow;
}