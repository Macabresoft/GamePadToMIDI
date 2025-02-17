namespace Macabresoft.Macabre2D.Project.Common;

/// <summary>
/// Unnamed actions that can be performed by user input.
/// </summary>
public enum InputAction : byte {
    None,

    // Menu
    Confirm,
    Cancel,
    Settings,

    // Directions
    Up,
    Down,
    Left,
    Right
}