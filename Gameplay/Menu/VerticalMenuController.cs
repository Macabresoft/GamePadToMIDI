namespace Macabresoft.Macabre2D.Project.Gameplay;

using System.Runtime.Serialization;
using Macabresoft.Macabre2D.Framework;
using Macabresoft.Macabre2D.Project.Common;
using Microsoft.Xna.Framework;

public class VerticalMenuController : SubMenu {
    private const float ScrollVelocity = 20f;
    private int _currentIndex = -1;
    private MenuMouseCursor? _cursor;
    private IInputSystem _inputSystem = InputSystem.Empty;
    private IMenuSystem _menuSystem = MenuSystem.Empty;
    private float _moveTo;

    [DataMember]
    public IncrementalGameTimer HoldDownTimer { get; } = new();

    [DataMember]
    public IncrementalGameTimer HoldUpTimer { get; } = new();

    [DataMember]
    public bool ResetFocusedMenuItemOnEnable { get; set; }

    [DataMember]
    public bool ShouldScroll { get; set; }

    public override void Deinitialize() {
        base.Deinitialize();

        this._cursor = null;
        this._inputSystem = InputSystem.Empty;
        this._menuSystem = MenuSystem.Empty;
    }

    public override void HandleInput(FrameTime frameTime, InputState inputState) {
        if (this._inputSystem.IsPressed(InputAction.Cancel) || this._inputSystem.IsPressed(InputAction.Settings)) {
            this._menuSystem.PopMenu();
        }
        else if (this.MenuItems.Any()) {
            var currentIndex = this.FocusedMenuItem == MenuItem.EmptyInstance ? -1 : Math.Max(this.MenuItems.IndexOf(this.FocusedMenuItem), 0);
            var originalIndex = currentIndex;

            if (this.HoldDownTimer.CanExecute(frameTime, () => this._inputSystem.IsHeld(InputAction.Down))) {
                currentIndex = this.GetNextIndex(currentIndex);
                this._cursor?.Deactivate();
            }
            else if (this.HoldUpTimer.CanExecute(frameTime, () => this._inputSystem.IsHeld(InputAction.Up))) {
                currentIndex = this.GetPreviousIndex(currentIndex);
                this._cursor?.Deactivate();
            }

            currentIndex = Math.Clamp(currentIndex, 0, this.MenuItems.Count - 1);
            var currentMenuItem = this.MenuItems[currentIndex];

            if (currentIndex != originalIndex) {
                this.FocusedMenuItem = currentMenuItem;
                this._currentIndex = currentIndex;
                this.ResetScroll();
            }
            else if (currentMenuItem.HandleInput(frameTime, this._inputSystem)) {
                this._cursor?.Deactivate();
            }

            if (this.Parent is IBoundableEntity parent &&
                this.BoundingArea.Height > parent.BoundingArea.Height &&
                inputState.CurrentMouseState.ScrollWheelValue != inputState.PreviousMouseState.ScrollWheelValue) {
                var amount = (float)frameTime.SecondsPassed * SeparatorHeight * (inputState.PreviousMouseState.ScrollWheelValue - inputState.CurrentMouseState.ScrollWheelValue);
                this._moveTo = Math.Clamp(this._moveTo + amount, this.Parent.WorldPosition.Y, this.Parent.WorldPosition.Y + this.BoundingArea.Height);
                this.LocalPosition = new Vector2(this.LocalPosition.X, this._moveTo);
            }

            if (this.ShouldScroll && Math.Abs(this.LocalPosition.Y - this._moveTo) > 0.01f) {
                if (this.LocalPosition.Y < this._moveTo) {
                    this.Move(new Vector2(0f, (float)(ScrollVelocity * frameTime.SecondsPassed)));

                    if (this.LocalPosition.Y >= this._moveTo) {
                        this.LocalPosition = new Vector2(this.LocalPosition.X, this._moveTo);
                    }
                }
                else {
                    this.Move(new Vector2(0f, (float)(-ScrollVelocity * frameTime.SecondsPassed)));

                    if (this.LocalPosition.Y <= this._moveTo) {
                        this.LocalPosition = new Vector2(this.LocalPosition.X, this._moveTo);
                    }
                }

                // This call assures menu item adornments are updated after a scroll
                this.RaisePropertyChanged(nameof(this.FocusedMenuItem));
            }
        }
    }

    public override void Initialize(IScene scene, IEntity parent) {
        if (this.TryGetAncestor<IDockingContainer>(out var originalDockingContainer)) {
            originalDockingContainer.BoundingAreaChanged -= this.DockingContainer_BoundingAreaChanged;
        }

        base.Initialize(scene, parent);
        this._inputSystem = this.Scene.GetSystem<InputSystem>() ?? this.Scene.AddSystem<InputSystem>();
        this._menuSystem = this.Scene.GetSystem<MenuSystem>() ?? this.Scene.AddSystem<MenuSystem>();
        this._cursor = this.Scene.GetDescendants<MenuMouseCursor>().FirstOrDefault();
        this.HoldDownTimer.Timer.Complete();
        this.HoldUpTimer.Timer.Complete();

        this.ResetScroll();
        this.LocalPosition = new Vector2(this.LocalPosition.X, this._moveTo);

        this.Scene.Invoke(() =>
        {
            this.ResetScroll();
            this.LocalPosition = new Vector2(this.LocalPosition.X, this._moveTo);
        });

        if (this.TryGetAncestor<IDockingContainer>(out var dockingContainer)) {
            dockingContainer.BoundingAreaChanged += this.DockingContainer_BoundingAreaChanged;
        }
    }

    public override void OnPush() {
        base.OnPush();

        if (!this.ResetFocusedMenuItemOnEnable && this._currentIndex >= 0 && this._currentIndex < this.MenuItems.Count) {
            this.FocusedMenuItem = this.MenuItems[this._currentIndex];
        }
        else {
            this.FocusedMenuItem = this.MenuItems.FirstOrDefault() ?? MenuItem.EmptyInstance;
        }

        this.HoldDownTimer.Timer.Restart();
        this.HoldUpTimer.Timer.Restart();
        this.FocusedMenuItem.Focus();
        this.ResetScroll();
        this.LocalPosition = new Vector2(this.LocalPosition.X, this._moveTo);
    }

    private bool CheckRequiresScrolling(IBoundableEntity parent, out float halfHeight) {
        halfHeight = 0.5f * parent.BoundingArea.Height;
        return this.BoundingArea.Height > halfHeight;
    }

    private void DockingContainer_BoundingAreaChanged(object? sender, EventArgs e) {
        this.ResetScroll();
    }

    private int GetNextIndex(int currentIndex) {
        var nextIndex = currentIndex;
        var hasResult = false;

        for (var i = currentIndex + 1; i < this.MenuItems.Count; i++) {
            var menuItem = this.MenuItems[i];
            if (menuItem.CanFocus) {
                nextIndex = i;
                hasResult = true;
                break;
            }
        }

        if (!hasResult && currentIndex != 0) {
            for (var i = 0; i < currentIndex; i++) {
                var menuItem = this.MenuItems[i];
                if (menuItem.CanFocus) {
                    nextIndex = i;
                    break;
                }
            }
        }

        return nextIndex;
    }

    private int GetPreviousIndex(int currentIndex) {
        var previousIndex = currentIndex;
        var hasResult = false;

        for (var i = currentIndex - 1; i >= 0; i--) {
            var menuItem = this.MenuItems[i];
            if (menuItem.CanFocus) {
                previousIndex = i;
                hasResult = true;
                break;
            }
        }

        if (!hasResult && currentIndex != this.MenuItems.Count - 1) {
            for (var i = this.MenuItems.Count - 1; i > currentIndex; i--) {
                var menuItem = this.MenuItems[i];
                if (menuItem.CanFocus) {
                    previousIndex = i;
                    break;
                }
            }
        }

        return previousIndex;
    }

    private void ResetScroll() {
        if (this.ShouldScroll && this.Parent is IBoundableEntity parent && this.CheckRequiresScrolling(parent, out var halfHeight)) {
            var yCenter = parent.BoundingArea.Minimum.Y + halfHeight;
            var difference = yCenter - this.FocusedMenuItem.BoundingArea.Minimum.Y;

            if (this.BoundingArea.Maximum.Y + difference < parent.BoundingArea.Maximum.Y) {
                this._moveTo = this.LocalPosition.Y + (parent.BoundingArea.Maximum.Y - this.BoundingArea.Maximum.Y);
            }
            else {
                this._moveTo = this.LocalPosition.Y + difference;
            }
        }
        else {
            this._moveTo = this.LocalPosition.Y;
        }
    }
}