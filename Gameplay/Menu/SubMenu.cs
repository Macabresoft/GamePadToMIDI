namespace Macabresoft.Macabre2D.Project.Gameplay;

using System.Runtime.Serialization;
using Macabresoft.Macabre2D.Framework;
using Macabresoft.Macabre2D.Project.Common;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

public interface ISubMenu : IEntity, IActivatableMenuElement, IBoundable {
    float AdornmentWidth { get; }
    IMenuItem FocusedMenuItem { get; set; }
    bool ShowDirectionActions { get; }
    bool ShowReturnPrompt { get; }
    bool ShowUpDownOnLeft { get; }
    void HandleInput(FrameTime frameTime, InputState inputState);
    void OnPop();
    void OnPush();
}

public abstract class SubMenu : ActivatableMenuElement, ISubMenu, IRenderableEntity {
    internal const float MenuItemDistanceFromCenter = 6.5f;
    internal const float SeparatorHeight = 0.5f;
    private const char LeftAdornment = '[';
    private const char RightAdornment = ']';

    public static readonly ISubMenu EmptyInstance = new EmptySubMenu();
    private static readonly Color HeaderColor = PredefinedColors.MacabreLightPurple;

    private IMenuItem _focusedItem = MenuItem.EmptyInstance;
    private SpriteSheetFont? _font;

    private FontCategory _fontCategory;
    private SpriteSheetFontCharacter? _leftAdornmentCharacter;
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

    public SpriteSheetFontReference MenuItemFontReference { get; } = new();

    [DataMember]
    public PixelSnap PixelSnap { get; set; } = PixelSnap.Inherit;

    [DataMember]
    public bool RenderOutOfBounds { get; set; } = true;

    [DataMember]
    public bool ShouldRender { get; set; } = true;

    public virtual bool ShowDirectionActions => true;
    public virtual bool ShowReturnPrompt => true;
    public virtual bool ShowUpDownOnLeft => true;

    protected List<IMenuItem> MenuItems { get; } = new();

    public override void Activate() {
        base.Activate();

        if (!BaseGame.IsDesignMode) {
            foreach (var menuItem in this.MenuItems) {
                menuItem.Activate();
            }
        }
    }

    public abstract void HandleInput(FrameTime frameTime, InputState inputState);

    public override void Initialize(IScene scene, IEntity parent) {
        base.Initialize(scene, parent);
        this.MenuItems.Clear();
        this.MenuItems.AddRange(this.Children.OfType<IMenuItem>());

        if (this.MenuItems.FirstOrDefault() is { } menuItem) {
            this.FocusedMenuItem = menuItem;
        }

        this.ResetAdornments();
    }

    public override void LoadAssets(IAssetManager assets, IGame game) {
        this.ResetFont(game.Project);
        base.LoadAssets(assets, game);
    }

    public virtual void OnPop() {
    }

    public virtual void OnPush() {
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

    protected void ApplySpinnerToMenuItem(MenuItem menuItem) {
        var spinner = menuItem.AddChild<SelectionSpinner>();
        spinner.LocalPosition = new Vector2(MenuItemDistanceFromCenter, 0f);
        spinner.EndCapPadding = 4;
        spinner.EndCapWidth = 7;
        spinner.RenderOptions.OffsetType = PixelOffsetType.Right;
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

    private class EmptySubMenu : EmptyObject, ISubMenu {
        public float AdornmentWidth => 0f;

        public IMenuItem FocusedMenuItem {
            get => MenuItem.EmptyInstance;
            set { }
        }

        public bool ShowDirectionActions => false;
        public bool ShowReturnPrompt => false;
        public bool ShowUpDownOnLeft => false;

        public void Activate() {
        }

        public void Deactivate() {
        }

        public void HandleInput(FrameTime frameTime, InputState inputState) {
        }

        public void OnPop() {
        }

        public void OnPush() {
        }
    }
}