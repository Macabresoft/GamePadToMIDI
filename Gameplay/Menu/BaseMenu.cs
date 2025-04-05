namespace Macabresoft.Macabre2D.Project.Gameplay;

using System.Runtime.Serialization;
using Macabresoft.Macabre2D.Framework;
using Macabresoft.Macabre2D.Project.Common;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

/// <summary>
/// Interface for a menu.
/// </summary>
public interface IBaseMenu : IEntity, IBoundable {

    /// <summary>
    /// Gets or sets the focused menu item.
    /// </summary>
    IMenuItem FocusedMenuItem { get; set; }

    /// <summary>
    /// Handles input for this menu.
    /// </summary>
    /// <param name="frameTime">The frame time.</param>
    /// <param name="inputState">The input state.</param>
    void HandleInput(FrameTime frameTime, InputState inputState);

    /// <summary>
    /// Called when a menu is popped from the stack.
    /// </summary>
    void OnPop();

    /// <summary>
    /// Called when a menu is pushed to the stack.
    /// </summary>
    void OnPush();
}

/// <summary>
/// Base implementation of menu functionality.
/// </summary>
public abstract class BaseMenu : DockableWrapper, IBaseMenu, IRenderableEntity {
    internal const float MenuItemDistanceFromCenter = 3.75f;
    internal const float SeparatorHeight = 0.2f;
    internal const float SpinnerWidth = 3f;
    private const char LeftAdornment = '[';
    private const char RightAdornment = ']';
    private const float ScrollVelocity = 20f;

    private static readonly Color HeaderColor = PredefinedColors.MacabreLightPurple;

    private readonly List<IMenuItem> _menuItems = [];
    private float _adornmentWidth;
    private int _currentIndex = -1;
    private MenuMouseCursor? _cursor;
    private IMenuItem _focusedItem = MenuItem.EmptyInstance;
    private SpriteSheetFont? _font;
    private FontCategory _fontCategory;
    private IInputSystem _inputSystem = InputSystem.Empty;
    private SpriteSheetFontCharacter? _leftAdornmentCharacter;
    private IMenuSystem _menuSystem = MenuSystem.Empty;
    private float _moveTo;
    private int _renderOrder;
    private SpriteSheetFontCharacter? _rightAdornmentCharacter;
    private SpriteSheet? _spriteSheet;

    /// <summary>
    /// Gets an empty menu instance.
    /// </summary>
    public static IBaseMenu EmptyInstance { get; } = new EmptyBaseMenu();

    /// <inheritdoc />
    public IMenuItem FocusedMenuItem {
        get => this._focusedItem;
        set {
            if (value.Id != this._focusedItem.Id) {
                foreach (var menuItem in this.GetDescendants<IMenuItem>()) {
                    menuItem.RemoveFocus();
                }

                this._focusedItem = value;
                this._focusedItem.Focus();
                this.ResetAdornments();
                this.RaisePropertyChanged();
            }
        }
    }

    /// <summary>
    /// Gets or sets the font category for this menu.
    /// </summary>
    [DataMember]
    public FontCategory FontCategory {
        get => this._fontCategory;
        set {
            if (value != this._fontCategory) {
                this._fontCategory = value;
                this.ResetFont(this.Project);
                this.ResetAdornments();
            }
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether this has changes.
    /// </summary>
    public bool HasChanges { get; set; }

    /// <summary>
    /// Gets a timer when holding up delays scrolling for a bit.
    /// </summary>
    [DataMember]
    public IncrementalGameTimer HoldDownTimer { get; } = new();

    /// <summary>
    /// Gets a timer when holding down delays scrolling for a bit.
    /// </summary>
    [DataMember]
    public IncrementalGameTimer HoldUpTimer { get; } = new();

    /// <summary>
    /// Gets the font to be used by menu items under this menu.
    /// </summary>
    public SpriteSheetFontReference MenuItemFontReference { get; } = new();

    /// <inheritdoc />
    [DataMember]
    public PixelSnap PixelSnap { get; set; } = PixelSnap.Inherit;

    /// <inheritdoc />
    [DataMember]
    public int RenderOrder {
        get => this._renderOrder;
        set => this.Set(ref this._renderOrder, value);
    }

    /// <inheritdoc />
    [DataMember]
    public bool RenderOutOfBounds { get; set; } = true;

    /// <summary>
    /// Gets or sets a value which indicates whether the focused menu item should be reset when this is enabled.
    /// </summary>
    [DataMember]
    public bool ResetFocusedMenuItemOnEnable { get; set; }

    /// <inheritdoc />
    [DataMember]
    public bool ShouldRender { get; set; } = true;

    /// <inheritdoc />
    public override void Deinitialize() {
        base.Deinitialize();

        this._cursor = null;
        this._inputSystem = InputSystem.Empty;
        this._menuSystem = MenuSystem.Empty;
    }

    /// <inheritdoc />
    public void HandleInput(FrameTime frameTime, InputState inputState) {
        if (this._inputSystem.IsPressed(InputAction.Cancel) || this._inputSystem.IsPressed(InputAction.Settings)) {
            this._menuSystem.PopMenu();
        }
        else if (this._menuItems.Any()) {
            var currentIndex = this.FocusedMenuItem == MenuItem.EmptyInstance ? -1 : Math.Max(this._menuItems.IndexOf(this.FocusedMenuItem), 0);
            var originalIndex = currentIndex;

            if (this.HoldDownTimer.CanExecute(frameTime, () => this._inputSystem.IsHeld(InputAction.Down))) {
                currentIndex = this.GetNextIndex(currentIndex);
                this._cursor?.Deactivate();
            }
            else if (this.HoldUpTimer.CanExecute(frameTime, () => this._inputSystem.IsHeld(InputAction.Up))) {
                currentIndex = this.GetPreviousIndex(currentIndex);
                this._cursor?.Deactivate();
            }

            currentIndex = Math.Clamp(currentIndex, 0, this._menuItems.Count - 1);
            var currentMenuItem = this._menuItems[currentIndex];

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

            if (Math.Abs(this.LocalPosition.Y - this._moveTo) > 0.01f) {
                var multiplier = MathF.Max(1f, MathF.Abs(this.LocalPosition.Y - this._moveTo));

                if (this.LocalPosition.Y < this._moveTo) {
                    this.Move(new Vector2(0f, (float)(ScrollVelocity * frameTime.SecondsPassed * multiplier)));

                    if (this.LocalPosition.Y >= this._moveTo) {
                        this.LocalPosition = new Vector2(this.LocalPosition.X, this._moveTo);
                    }
                }
                else {
                    this.Move(new Vector2(0f, (float)(-ScrollVelocity * frameTime.SecondsPassed * multiplier)));

                    if (this.LocalPosition.Y <= this._moveTo) {
                        this.LocalPosition = new Vector2(this.LocalPosition.X, this._moveTo);
                    }
                }

                // This call assures menu item adornments are updated after a scroll
                this.RaisePropertyChanged(nameof(this.FocusedMenuItem));
            }
        }
    }

    /// <inheritdoc />
    public override void Initialize(IScene scene, IEntity parent) {
        if (this.TryGetAncestor<IDockingContainer>(out var originalDockingContainer)) {
            originalDockingContainer.BoundingAreaChanged -= this.DockingContainer_BoundingAreaChanged;
        }

        base.Initialize(scene, parent);
        this._menuItems.Clear();
        this._menuItems.AddRange(this.Children.OfType<IMenuItem>());

        if (this._menuItems.FirstOrDefault() is { } menuItem) {
            this.FocusedMenuItem = menuItem;
        }

        this.ResetAdornments();

        this._inputSystem = this.Scene.GetSystem<InputSystem>() ?? this.Scene.AddSystem<InputSystem>();
        this._menuSystem = this.Scene.GetSystem<MenuSystem>() ?? this.Scene.AddSystem<MenuSystem>();
        this._cursor = this.Scene.GetDescendants<MenuMouseCursor>().FirstOrDefault();
        this.HoldDownTimer.Timer.Complete();
        this.HoldUpTimer.Timer.Complete();

        this.Scene.Invoke(() =>
        {
            this.ResetScroll();
            this.LocalPosition = new Vector2(this.LocalPosition.X, this._moveTo);
            this.RequestRearrangeFromParent();
        });

        if (this.TryGetAncestor<IDockingContainer>(out var dockingContainer)) {
            dockingContainer.BoundingAreaChanged += this.DockingContainer_BoundingAreaChanged;
        }
    }

    /// <inheritdoc />
    public override void LoadAssets(IAssetManager assets, IGame game) {
        this.ResetFont(game.Project);
        base.LoadAssets(assets, game);
    }

    /// <inheritdoc />
    public void OnPop() {
        if (this.HasChanges || this.Game.State.ExistingSaves.Any(x => x.HasChanges)) {
            this.OnSave();
            this.HasChanges = false;
        }
    }

    /// <inheritdoc />
    public void OnPush() {
        if (!BaseGame.IsDesignMode) {
            foreach (var menuItem in this._menuItems) {
                menuItem.Activate();
            }
        }

        if (!this.ResetFocusedMenuItemOnEnable && this._currentIndex >= 0 && this._currentIndex < this._menuItems.Count) {
            this.FocusedMenuItem = this._menuItems[this._currentIndex];
        }
        else {
            this.FocusedMenuItem = this._menuItems.FirstOrDefault() ?? MenuItem.EmptyInstance;
        }

        this.HoldDownTimer.Timer.Restart();
        this.HoldUpTimer.Timer.Restart();
        this.FocusedMenuItem.Focus();
        this.ResetScroll();
        this.LocalPosition = new Vector2(this.LocalPosition.X, this._moveTo);
    }

    /// <inheritdoc />
    public void Render(FrameTime frameTime, BoundingArea viewBoundingArea) {
        this.Render(frameTime, viewBoundingArea, PredefinedColors.TextHighlightColor);
    }

    /// <inheritdoc />
    public void Render(FrameTime frameTime, BoundingArea viewBoundingArea, Color colorOverride) {
        if (this.IsEnabled && this.SpriteBatch != null && this.FocusedMenuItem != MenuItem.EmptyInstance && this._spriteSheet != null) {
            if (this._leftAdornmentCharacter != null) {
                this._spriteSheet.Draw(
                    this.SpriteBatch,
                    this.Project.PixelsPerUnit,
                    this._leftAdornmentCharacter.SpriteIndex,
                    new Vector2(this.FocusedMenuItem.BoundingArea.Minimum.X - this._adornmentWidth, this.FocusedMenuItem.BoundingArea.Minimum.Y),
                    colorOverride,
                    SpriteEffects.FlipVertically);
            }

            if (this._rightAdornmentCharacter != null) {
                this._spriteSheet.Draw(
                    this.SpriteBatch,
                    this.Project.PixelsPerUnit,
                    this._rightAdornmentCharacter.SpriteIndex,
                    new Vector2(this.FocusedMenuItem.BoundingArea.Maximum.X, this.FocusedMenuItem.BoundingArea.Minimum.Y),
                    colorOverride,
                    SpriteEffects.FlipVertically);
            }
        }
    }

    /// <summary>
    /// Adds a header.
    /// </summary>
    /// <param name="resourceName">The resource name.</param>
    /// <param name="yPosition">The Y position.</param>
    /// <returns>The header.</returns>
    protected TextLineRenderer AddHeader(string resourceName, float yPosition) {
        var header = this.AddChild<TextLineRenderer>();
        header.RenderOptions.OffsetType = PixelOffsetType.Center;
        header.ResourceName = resourceName;
        header.FontCategory = FontCategory.Normal;
        header.Color = HeaderColor;
        header.LocalPosition = new Vector2(0f, yPosition);
        return header;
    }

    /// <summary>
    /// Adds a menu item.
    /// </summary>
    /// <param name="yPosition">The Y position.</param>
    /// <typeparam name="T">The type.</typeparam>
    /// <returns>The menu item.</returns>
    protected T AddMenuItem<T>(float yPosition) where T : MenuItem, new() {
        var menuItem = this.AddChild<T>();
        menuItem.LocalPosition = new Vector2(0f, yPosition);
        return menuItem;
    }

    /// <summary>
    /// Adds a menu item with text.
    /// </summary>
    /// <param name="yPosition">The Y position.</param>
    /// <param name="distanceFromCenter">The distance from the center.</param>
    /// <param name="offsetType">The offset type.</param>
    /// <typeparam name="T">The type.</typeparam>
    /// <returns>The menu item.</returns>
    protected T AddMenuItemWithText<T>(float yPosition, float distanceFromCenter, PixelOffsetType offsetType) where T : MenuItem, new() {
        var menuItem = this.AddMenuItem<T>(yPosition);
        this.ApplyTextToMenuItem(menuItem, menuItem.ResourceName, distanceFromCenter, offsetType);
        return menuItem;
    }

    /// <summary>
    /// Adds a return menu item.
    /// </summary>
    /// <param name="yPosition">The Y position.</param>
    /// <returns>The return menu item.</returns>
    protected ReturnMenuItem AddReturnMenuItem(float yPosition) {
        var menuItem = this.AddMenuItem<ReturnMenuItem>(yPosition);
        var textLine = menuItem.AddChild<MenuTextLineRenderer>();
        textLine.RenderOptions.OffsetType = PixelOffsetType.Center;
        textLine.ResourceName = menuItem.ResourceName;
        textLine.FontCategory = this.FontCategory;
        return menuItem;
    }

    /// <summary>
    /// Adds a spinner menu item.
    /// </summary>
    /// <param name="yPosition">The Y position.</param>
    /// <typeparam name="T">The type.</typeparam>
    /// <returns>The menu item.</returns>
    protected T AddSpinnerMenuItem<T>(float yPosition) where T : MenuItem, new() {
        var menuItem = this.AddMenuItem<T>(yPosition);
        this.ApplySpinnerToMenuItem(menuItem);
        return menuItem;
    }

    /// <summary>
    /// Adds a spinner menu item with text.
    /// </summary>
    /// <param name="yPosition">The Y position.</param>
    /// <typeparam name="T">The type.</typeparam>
    /// <returns>The menu item.</returns>
    protected T AddSpinnerMenuItemWithText<T>(float yPosition) where T : MenuItem, new() {
        var menuItem = this.AddMenuItemWithText<T>(yPosition, -MenuItemDistanceFromCenter, PixelOffsetType.Left);
        this.ApplySpinnerToMenuItem(menuItem);
        return menuItem;
    }

    /// <summary>
    /// Applies a spinner element to the menu item.
    /// </summary>
    /// <param name="menuItem">The menu item.</param>
    protected void ApplySpinnerToMenuItem(MenuItem menuItem) {
        var spinner = menuItem.AddChild<SelectionSpinner>();
        spinner.LocalPosition = new Vector2(MenuItemDistanceFromCenter, 0f);
        spinner.EndCapPadding = 4;
        spinner.EndCapWidth = 7;
        spinner.RenderOptions.OffsetType = PixelOffsetType.Right;
    }

    /// <summary>
    /// Applies text to a menu item.
    /// </summary>
    /// <param name="menuItem">The menu item.</param>
    /// <param name="resourceName">The resource name.</param>
    /// <param name="distanceFromCenter">The distance from the center.</param>
    /// <param name="offsetType">The offset type.</param>
    protected void ApplyTextToMenuItem(MenuItem menuItem, string resourceName, float distanceFromCenter, PixelOffsetType offsetType) {
        var textLine = menuItem.AddChild<MenuTextLineRenderer>();
        textLine.ResourceName = resourceName;
        textLine.LocalPosition = new Vector2(distanceFromCenter, 0f);
        textLine.RenderOptions.OffsetType = offsetType;
        textLine.FontCategory = this.FontCategory;
    }

    /// <inheritdoc />
    protected override IEnumerable<IAssetReference> GetAssetReferences() {
        yield return this.MenuItemFontReference;
    }

    /// <summary>
    /// Gets the height of menu items.
    /// </summary>
    /// <param name="project">The project.</param>
    /// <returns>The height of a menu item.</returns>
    protected float GetMenuItemHeight(IGameProject project) {
        if (this.MenuItemFontReference.Asset is { } spriteSheet) {
            return spriteSheet.SpriteSize.Y * project.UnitsPerPixel;
        }

        return 0f;
    }

    /// <summary>
    /// Called when this saves.
    /// </summary>
    protected virtual void OnSave() {
        this.Game.SaveUserSettings();
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

        for (var i = currentIndex + 1; i < this._menuItems.Count; i++) {
            var menuItem = this._menuItems[i];
            if (menuItem.CanFocus) {
                nextIndex = i;
                hasResult = true;
                break;
            }
        }

        if (!hasResult && currentIndex != 0) {
            for (var i = 0; i < currentIndex; i++) {
                var menuItem = this._menuItems[i];
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
            var menuItem = this._menuItems[i];
            if (menuItem.CanFocus) {
                previousIndex = i;
                hasResult = true;
                break;
            }
        }

        if (!hasResult && currentIndex != this._menuItems.Count - 1) {
            for (var i = this._menuItems.Count - 1; i > currentIndex; i--) {
                var menuItem = this._menuItems[i];
                if (menuItem.CanFocus) {
                    previousIndex = i;
                    break;
                }
            }
        }

        return previousIndex;
    }

    private void ResetAdornments() {
        if (this.MenuItemFontReference is { PackagedAsset: not null, Asset: not null }) {
            this._font = this.MenuItemFontReference.PackagedAsset;
            this._spriteSheet = this.MenuItemFontReference.Asset;
        }
        else if (this.Project.Fallbacks.Font is { PackagedAsset: not null, Asset: not null }) {
            this._font = this.Project.Fallbacks.Font.PackagedAsset;
            this._spriteSheet = this.Project.Fallbacks.Font.Asset;
            this.MenuItemFontReference.ContentId = this._spriteSheet.ContentId;
            this.MenuItemFontReference.PackagedAssetId = this._font.Id;
        }

        if (this._font != null && this._spriteSheet != null) {
            this._font.TryGetSpriteCharacter(RightAdornment, out this._rightAdornmentCharacter);

            if (this._font.TryGetSpriteCharacter(LeftAdornment, out this._leftAdornmentCharacter)) {
                var characterPixelWidth = this._spriteSheet.SpriteSize.X + this._font.Kerning + this._leftAdornmentCharacter.Kerning;
                this._adornmentWidth = characterPixelWidth * this.Project.UnitsPerPixel;
            }
        }
        else {
            this._leftAdornmentCharacter = null;
            this._rightAdornmentCharacter = null;
        }
    }

    private void ResetFont(IGameProject project) {
        if (project.Fonts.TryGetFont(this.FontCategory, this.Game.DisplaySettings.Culture, out var fontDefinition)) {
            this.MenuItemFontReference.ContentId = fontDefinition.SpriteSheetId;
            this.MenuItemFontReference.PackagedAssetId = fontDefinition.FontId;
        }

        foreach (var textLine in this.GetDescendants<MenuTextLineRenderer>()) {
            textLine.FontCategory = this.FontCategory;
        }
    }

    private void ResetScroll() {
        if (this.Parent is IBoundableEntity parent && this.CheckRequiresScrolling(parent, out var halfHeight)) {
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

    private class EmptyBaseMenu : EmptyObject, IBaseMenu {
        public IMenuItem FocusedMenuItem {
            get => MenuItem.EmptyInstance;
            set { }
        }

        public void HandleInput(FrameTime frameTime, InputState inputState) {
        }

        public void OnPop() {
        }

        public void OnPush() {
        }
    }
}