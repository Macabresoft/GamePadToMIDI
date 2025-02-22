namespace Macabresoft.Macabre2D.Project.Common;

using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework.Input;

/// <summary>
/// Helper class for MIDI notes.
/// </summary>
public static class MidiNoteBindingHelper {
    /// <summary>
    /// Gets the available buttons in order.
    /// </summary>
    public static IReadOnlyCollection<Buttons> AvailableButtons { get; } = [
        Buttons.A,
        Buttons.B,
        Buttons.X,
        Buttons.Y,
        Buttons.DPadLeft,
        Buttons.DPadUp,
        Buttons.DPadRight,
        Buttons.DPadDown,
        Buttons.LeftShoulder,
        Buttons.LeftTrigger,
        Buttons.RightShoulder,
        Buttons.RightTrigger,
        Buttons.LeftThumbstickLeft,
        Buttons.LeftThumbstickUp,
        Buttons.LeftThumbstickRight,
        Buttons.LeftThumbstickDown,
        Buttons.RightThumbstickLeft,
        Buttons.RightThumbstickUp,
        Buttons.RightThumbstickRight,
        Buttons.RightThumbstickDown,
    ];
}