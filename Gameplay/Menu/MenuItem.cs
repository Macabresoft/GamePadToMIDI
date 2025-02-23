namespace Macabresoft.Macabre2D.Project.Gameplay;

using Macabresoft.Macabre2D.Framework;
using Macabresoft.Macabre2D.Project.Common;
using Microsoft.Xna.Framework;

/// <summary>
/// Interface for a menu item.
/// </summary>
public interface IMenuItem : IDockable {
    /// <summary>
    /// Gets a value indicating whether this can be focused on.
    /// </summary>
    bool CanFocus { get; }

    /// <summary>
    /// Gets the resource name for the confirm prompt.
    /// </summary>
    string ConfirmPromptResourceName { get; }

    /// <summary>
    /// Gets a value indicating whether this is focused.
    /// </summary>
    bool IsFocused { get; }

    /// <summary>
    /// Activates this menu item.
    /// </summary>
    void Activate();

    /// <summary>
    /// Clicks on this menu item.
    /// </summary>
    /// <param name="startPosition">The position where the mouse button was initially pressed.</param>
    /// <param name="endPosition">The position where the mouse button was released.</param>
    /// <param name="isHold">A value indicating whether the mouse button is being held.</param>
    void Click(Vector2 startPosition, Vector2 endPosition, bool isHold);

    /// <summary>
    /// Focuses this menu item.
    /// </summary>
    void Focus();

    /// <summary>
    /// Handles input.
    /// </summary>
    /// <param name="frameTime">The frame time.</param>
    /// <param name="input">The input system.</param>
    /// <returns>A value indicating whether any input was processed.</returns>
    bool HandleInput(FrameTime frameTime, IInputSystem input);

    /// <summary>
    /// Determines whether a position is hovering over this menu item.
    /// </summary>
    /// <param name="position">The position to check.</param>
    /// <returns>A value indicating whether a position is hovering over this menu item.</returns>
    bool IsHoveringOver(Vector2 position);

    /// <summary>
    /// Removes focus from this menu item.
    /// </summary>
    void RemoveFocus();
}

/// <summary>
/// A base class for menu items.
/// </summary>
public abstract class MenuItem : DockableWrapper, IMenuItem {
    public static readonly IMenuItem EmptyInstance = new EmptyMenuItem();

    /// <inheritdoc />
    public virtual bool CanFocus => true;

    /// <inheritdoc />
    public virtual string ConfirmPromptResourceName => nameof(Resources.Menu_Prompts_Confirm);

    /// <inheritdoc />
    public bool IsFocused { get; private set; }

    /// <summary>
    /// Gets the resource name.
    /// </summary>
    public abstract string ResourceName { get; }

    /// <inheritdoc />
    public virtual void Activate() {
    }

    /// <inheritdoc />
    public virtual void Click(Vector2 startPosition, Vector2 endPosition, bool isHold) {
        if (this.BoundingArea.Contains(startPosition) && this.BoundingArea.Contains(endPosition)) {
            this.Execute();
        }
    }

    /// <inheritdoc />
    public virtual void Focus() {
        this.IsFocused = true;
    }

    /// <inheritdoc />
    public virtual bool HandleInput(FrameTime frameTime, IInputSystem input) {
        if (input.IsPressed(InputAction.Confirm, InputKind.GamePad | InputKind.Keyboard)) {
            this.Execute();
            return true;
        }

        return false;
    }

    /// <inheritdoc />
    public override void Initialize(IScene scene, IEntity parent) {
        base.Initialize(scene, parent);

        foreach (var textLine in this.GetDescendants<MenuTextLineRenderer>()) {
            textLine.Color = Color.White;
        }
    }

    /// <inheritdoc />
    public virtual bool IsHoveringOver(Vector2 position) => this.BoundingArea.Contains(position);

    /// <inheritdoc />
    public virtual void RemoveFocus() {
        this.IsFocused = false;
    }

    /// <summary>
    /// Executes this menu item.
    /// </summary>
    protected abstract void Execute();

    private class EmptyMenuItem : EmptyObject, IMenuItem {
        public bool CanFocus => false;
        public string ConfirmPromptResourceName => string.Empty;
        public bool IsFocused => false;

        public DockLocation Location {
            get => DockLocation.None;
            set { }
        }

        public void Activate() {
        }

        public void Click(Vector2 startPosition, Vector2 endPosition, bool isHold) {
        }

        public void Focus() {
        }

        public bool HandleInput(FrameTime frameTime, IInputSystem input) => false;

        public bool IsHoveringOver(Vector2 position) => false;

        public void RemoveFocus() {
        }
    }
}