namespace Macabresoft.Macabre2D.Project.Gameplay;

using System.Runtime.Serialization;
using Macabresoft.Macabre2D.Framework;
using Macabresoft.Macabre2D.Project.Common;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

public interface IBaseMenu : IEntity, IBoundable {
    IMenuItem FocusedMenuItem { get; set; }
    bool ShowDirectionActions { get; }
    bool ShowReturnPrompt { get; }
    bool ShowUpDownOnLeft { get; }
    void HandleInput(FrameTime frameTime, InputState inputState);
    void OnPop();
    void OnPush();
}

public abstract class BaseMenu : DockableWrapper, IBaseMenu, IRenderableEntity {
    internal const float SeparatorHeight = 0.2f;
    private const char LeftAdornment = '[';
    private const float MenuItemDistanceFromCenter = 3.75f;
    private const char RightAdornment = ']';

    private const float ScrollVelocity = 20f;

    public static readonly IBaseMenu EmptyInstance = new EmptyBaseMenu();
    private static readonly Color HeaderColor = PredefinedColors.MacabreLightPurple;
    private int _currentIndex = -1;
    private MenuMouseCursor? _cursor;

    private IMenuItem _focusedItem = MenuItem.EmptyInstance;
    private SpriteSheetFont? _font;

    private FontCategory _fontCategory;
    private IInputSystem _inputSystem = InputSystem.Empty;
    private SpriteSheetFontCharacter? _leftAdornmentCharacter;
    private IMenuSystem _menuSystem = MenuSystem.Empty;
    private float _moveTo;
    private SpriteSheetFontCharacter? _rightAdornmentCharacter;
    private SpriteSheet? _spriteSheet;

    public float AdornmentWidth { get; private set; }

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

    public bool HasChanges { get; set; }

    [DataMember]
    public IncrementalGameTimer HoldDownTimer { get; } = new();

    [DataMember]
    public IncrementalGameTimer HoldUpTimer { get; } = new();

    public SpriteSheetFontReference MenuItemFontReference { get; } = new();

    [DataMember]
    public PixelSnap PixelSnap { get; set; } = PixelSnap.Inherit;

    [DataMember]
    public bool RenderOutOfBounds { get; set; } = true;

    [DataMember]
    public bool ResetFocusedMenuItemOnEnable { get; set; }

    [DataMember]
    public bool ShouldRender { get; set; } = true;

    [DataMember]
    public bool ShouldScroll { get; set; }

    public virtual bool ShowDirectionActions => true;
    public virtual bool ShowReturnPrompt => true;
    public virtual bool ShowUpDownOnLeft => true;

    protected List<IMenuItem> MenuItems { get; } = new();

    public override void Deinitialize() {
        base.Deinitialize();

        this._cursor = null;
        this._inputSystem = InputSystem.Empty;
        this._menuSystem = MenuSystem.Empty;
    }

    public void HandleInput(FrameTime frameTime, InputState inputState) {
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
        this.MenuItems.Clear();
        this.MenuItems.AddRange(this.Children.OfType<IMenuItem>());

        if (this.MenuItems.FirstOrDefault() is { } menuItem) {
            this.FocusedMenuItem = menuItem;
        }

        this.ResetAdornments();

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

    public override void LoadAssets(IAssetManager assets, IGame game) {
        this.ResetFont(game.Project);
        base.LoadAssets(assets, game);
    }

    public void OnPop() {
        if (this.HasChanges) {
            this.OnSave();
        }
    }

    public void OnPush() {
        if (!BaseGame.IsDesignMode) {
            foreach (var menuItem in this.MenuItems) {
                menuItem.Activate();
            }
        }

        this.HasChanges = false;

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

    public void Render(FrameTime frameTime, BoundingArea viewBoundingArea) {
        this.Render(frameTime, viewBoundingArea, PredefinedColors.TextHighlightColor);
    }

    public void Render(FrameTime frameTime, BoundingArea viewBoundingArea, Color colorOverride) {
        if (this.IsEnabled && this.SpriteBatch != null && this.FocusedMenuItem != MenuItem.EmptyInstance && this._spriteSheet != null) {
            if (this._leftAdornmentCharacter != null) {
                this._spriteSheet.Draw(
                    this.SpriteBatch,
                    this.Project.PixelsPerUnit,
                    this._leftAdornmentCharacter.SpriteIndex,
                    new Vector2(this.FocusedMenuItem.BoundingArea.Minimum.X - this.AdornmentWidth, this.FocusedMenuItem.BoundingArea.Minimum.Y),
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

    protected BackMenuItem AddBackMenuItem(float yPosition) {
        var menuItem = this.AddMenuItem<BackMenuItem>(yPosition);
        var textLine = menuItem.AddChild<MenuTextLineRenderer>();
        textLine.RenderOptions.OffsetType = PixelOffsetType.Center;
        textLine.ResourceName = menuItem.ResourceName;
        textLine.FontCategory = this.FontCategory;
        return menuItem;
    }

    protected TextLineRenderer AddHeader(string resourceName) {
        var header = this.AddChild<TextLineRenderer>();
        header.RenderOptions.OffsetType = PixelOffsetType.Center;
        header.ResourceName = resourceName;
        header.FontCategory = FontCategory.Normal;
        header.Color = HeaderColor;
        return header;
    }

    protected T AddMenuItem<T>(float yPosition) where T : MenuItem, new() {
        var menuItem = this.AddChild<T>();
        menuItem.LocalPosition = new Vector2(0f, yPosition);
        return menuItem;
    }

    protected T AddMenuItemWithText<T>(float yPosition) where T : MenuItem, new() {
        var menuItem = this.AddMenuItem<T>(yPosition);
        this.ApplyTextToMenuItem(menuItem, menuItem.ResourceName);
        return menuItem;
    }

    protected T AddMenuItemWithText<T>(PixelOffsetType offsetType, float distanceFromCenter) where T : MenuItem, new() {
        var menuItem = this.AddMenuItem<T>(0f);
        this.ApplyTextToMenuItem(menuItem, menuItem.ResourceName, offsetType, distanceFromCenter);
        return menuItem;
    }

    protected T AddSpinnerMenuItem<T>(float yPosition) where T : MenuItem, new() {
        var menuItem = this.AddMenuItem<T>(yPosition);
        this.ApplySpinnerToMenuItem(menuItem);
        return menuItem;
    }

    protected T AddSpinnerMenuItemWithText<T>(float yPosition) where T : MenuItem, new() {
        var menuItem = this.AddMenuItemWithText<T>(yPosition);
        this.ApplySpinnerToMenuItem(menuItem);
        return menuItem;
    }

    protected MenuItem AddSpinnerMenuItemWithText(MenuItem menuItem, string resourceName, float yPosition) {
        this.AddChild(menuItem);
        menuItem.LocalPosition = new Vector2(0f, yPosition);
        this.ApplyTextToMenuItem(menuItem, resourceName);
        this.ApplySpinnerToMenuItem(menuItem);
        return menuItem;
    }

    protected void ApplySpinnerToMenuItem(MenuItem menuItem) {
        var spinner = menuItem.AddChild<SelectionSpinner>();
        spinner.LocalPosition = new Vector2(MenuItemDistanceFromCenter, 0f);
        spinner.EndCapPadding = 4;
        spinner.EndCapWidth = 7;
        spinner.RenderOptions.OffsetType = PixelOffsetType.Right;
    }

    protected void ApplyTextToMenuItem(MenuItem menuItem, string resourceName) {
        this.ApplyTextToMenuItem(menuItem, resourceName, PixelOffsetType.Left, -MenuItemDistanceFromCenter);
    }

    protected void ApplyTextToMenuItem(MenuItem menuItem, string resourceName, PixelOffsetType offsetType, float distanceFromCenter) {
        var textLine = menuItem.AddChild<MenuTextLineRenderer>();
        textLine.ResourceName = resourceName;
        textLine.LocalPosition = new Vector2(distanceFromCenter, 0f);
        textLine.RenderOptions.OffsetType = offsetType;
        textLine.FontCategory = this.FontCategory;
    }

    protected override IEnumerable<IAssetReference> GetAssetReferences() {
        yield return this.MenuItemFontReference;
    }

    protected float GetMenuItemHeight(IGameProject project) {
        if (this.MenuItemFontReference.Asset is { } spriteSheet) {
            return spriteSheet.SpriteSize.Y * project.UnitsPerPixel;
        }

        return 0f;
    }

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
                this.AdornmentWidth = characterPixelWidth * this.Project.UnitsPerPixel;
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

    private class EmptyBaseMenu : EmptyObject, IBaseMenu {
        public IMenuItem FocusedMenuItem {
            get => MenuItem.EmptyInstance;
            set { }
        }

        public bool ShowDirectionActions => false;
        public bool ShowReturnPrompt => false;
        public bool ShowUpDownOnLeft => false;

        public void HandleInput(FrameTime frameTime, InputState inputState) {
        }

        public void OnPop() {
        }

        public void OnPush() {
        }
    }
}