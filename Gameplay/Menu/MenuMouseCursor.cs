namespace Macabresoft.Macabre2D.Project.Gameplay;

using System.ComponentModel;
using Macabresoft.Macabre2D.Framework;
using Macabresoft.Macabre2D.Project.Common;
using Microsoft.Xna.Framework;

/// <summary>
/// Mouse cursor for menus.
/// </summary>
public class MenuMouseCursor : MouseCursorRenderer {
    private readonly GameTimer _disappearTimer = new(5f);
    private readonly GameTimer _viewportSizeChangedTimer = new(0.1f);
    private Vector2 _clickStart;
    private MenuSystem? _menuSystem;
    private bool _viewportSizeChanged;
    private bool _wasPressed;

    /// <summary>
    /// Deactivates this instance.
    /// </summary>
    public void Deactivate() {
        this._disappearTimer.Complete();
        this.IsHoveringOverActivatableElement = false;
        this.ShouldRender = false;
    }

    /// <inheritdoc />
    public override void Deinitialize() {
        this.Game.ViewportSizeChanged -= this.Game_ViewportSizeChanged;

        base.Deinitialize();

        if (this._menuSystem != null) {
            this._menuSystem.PropertyChanged -= this.MenuSystem_PropertyChanged;
        }
    }

    /// <inheritdoc />
    public override void Initialize(IScene scene, IEntity parent) {
        base.Initialize(scene, parent);
        this.Game.ViewportSizeChanged += this.Game_ViewportSizeChanged;
        this._menuSystem = this.Scene.GetSystem<MenuSystem>();

        if (this._menuSystem != null) {
            this._menuSystem.PropertyChanged += this.MenuSystem_PropertyChanged;
        }

        if (this.TryGetAncestor<ICamera>(out var camera)) {
            this.CameraReference.EntityId = camera.Id;
        }

        this._wasPressed = false;
        this.RenderOptions.OffsetType = PixelOffsetType.TopLeft;
        this.Deactivate();
    }

    /// <inheritdoc />
    public override void Update(FrameTime frameTime, InputState inputState) {
        // Behavior around the mouse when the viewport changes is complicated.
        // Essentially, the view port changing in size triggers the mouse to
        // think it moved and reappear. It can then select a different menu
        // item, which is confusing to users (me). This is a hacky way to
        // prevent that.
        if (this._viewportSizeChanged) {
            if (this._viewportSizeChangedTimer.State == TimerState.Finished) {
                this._viewportSizeChanged = false;
            }
            else {
                this._viewportSizeChangedTimer.Increment(frameTime);
            }

            return;
        }

        if (this._menuSystem == null) {
            return;
        }

        if (!this.Game.InputBindings.TryGetBindings(InputAction.Confirm, out _, out _, out _, out var mouseButton) || mouseButton == MouseButton.None) {
            mouseButton = MouseButton.Left;
        }

        var isHeld = inputState.IsMouseButtonHeld(mouseButton);
        var hadInput = isHeld ||
                       inputState.PreviousMouseState.Position != inputState.CurrentMouseState.Position ||
                       inputState.PreviousMouseState.ScrollWheelValue != inputState.CurrentMouseState.ScrollWheelValue;

        if (hadInput) {
            this._disappearTimer.Restart();
        }
        else {
            this._disappearTimer.Increment(frameTime);
        }

        if (this._disappearTimer.State == TimerState.Running) {
            base.Update(frameTime, inputState);

            if (hadInput) {
                this.ResetFocusedItem();
            }

            var worldPosition = this.WorldPosition;
            this.IsHoveringOverActivatableElement = this._menuSystem.FocusedMenu.FocusedMenuItem.IsHoveringOver(worldPosition);
            this.ShouldRender = true;

            if (inputState.IsMouseButtonNewlyPressed(mouseButton)) {
                this._wasPressed = true;
                this._clickStart = worldPosition;
            }
            else if (this._wasPressed) {
                if (isHeld) {
                    this._menuSystem.TryHold(this._clickStart, worldPosition);
                }
                else if (inputState.IsMouseButtonNewlyReleased(mouseButton)) {
                    this._menuSystem.TryClick(this._clickStart, worldPosition);
                    this._wasPressed = false;
                }
            }
        }
        else {
            this.Deactivate();
        }
    }

    /// <inheritdoc />
    protected override IEnumerable<IAssetReference> GetAssetReferences() {
        yield return this.MouseCursorIconSet;
    }

    private void Game_ViewportSizeChanged(object? sender, Point e) {
        this._viewportSizeChanged = true;
        this._viewportSizeChangedTimer.Restart();
    }

    private void MenuSystem_PropertyChanged(object? sender, PropertyChangedEventArgs e) {
        if (e.PropertyName == nameof(MenuSystem.FocusedMenu)) {
            this.ResetFocusedItem();
            this._wasPressed = false;
        }
    }

    private void ResetFocusedItem() {
        if (this._menuSystem != null) {
            foreach (var menuItem in this._menuSystem.FocusedMenu.GetDescendants<IMenuItem>()) {
                if (menuItem.BoundingArea.Contains(this.WorldPosition)) {
                    this._menuSystem.FocusedMenu.FocusedMenuItem = menuItem;
                    break;
                }
            }
        }
    }
}