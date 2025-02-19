namespace Macabresoft.Macabre2D.Project.Gameplay;

using System.Runtime.Serialization;
using Macabresoft.Core;
using Macabresoft.Macabre2D.Framework;
using Macabresoft.Macabre2D.Project.Common;
using Microsoft.Xna.Framework;

public class SelectionSpinner : RenderableEntity {
    private const char DecreaseCharacter = '<';
    private const char IncreaseCharacter = '>';

    private readonly ResettableLazy<BoundingArea> _boundingArea;
    private readonly Dictionary<string, IReadOnlyCollection<SpriteSheetFontCharacter>> _characterCollections = new();
    private readonly SpriteSheetFontReference _fontReference = new();

    private ResourceCulture _currentCulture;
    private SpriteSheetFontCharacter _decreaseCharacter = new();
    private int _endCapPadding;
    private int _endCapWidth;
    private SpriteSheetFont? _font;
    private SpriteSheetFontCharacter _increaseCharacter = new();
    private SelectionMenuItem? _menuItem;
    private string _resourceName = string.Empty;
    private SpriteSheet? _spriteSheet;
    private float _textStartLocation;


    public override event EventHandler? BoundingAreaChanged;

    public SelectionSpinner() : base() {
        this._boundingArea = new ResettableLazy<BoundingArea>(this.CreateBoundingArea);
    }

    public override BoundingArea BoundingArea => this._boundingArea.Value;

    public float ActualEndCapWidth { get; private set; }


    [DataMember(Order = 1)]
    public Color Color { get; set; } = Color.White;

    [DataMember]
    public int EndCapPadding {
        get => this._endCapPadding;
        set {
            this._endCapPadding = value;
            this.Reset();
        }
    }

    [DataMember]
    public int EndCapWidth {
        get => this._endCapWidth;
        set {
            this._endCapWidth = value;
            this.ResetEndCapWidth();
            this.Reset();
        }
    }

    [DataMember(Order = 4)]
    public RenderOptions RenderOptions { get; private set; } = new();

    /// <summary>
    /// Gets or sets the text.
    /// </summary>
    [DataMember]
    public string ResourceName {
        get => this._resourceName;
        set {
            if (value != this._resourceName) {
                this._resourceName = value;
                this.ResetText();
            }
        }
    }

    /// <inheritdoc />
    public override void Deinitialize() {
        base.Deinitialize();
        this._fontReference.AssetChanged -= this.Font_AssetChanged;
    }

    /// <inheritdoc />
    public override void Initialize(IScene scene, IEntity parent) {
        base.Initialize(scene, parent);

        if (this.TryGetAncestor<SubMenu>(out var subMenu)) {
            this._fontReference.ContentId = subMenu.MenuItemFontReference.ContentId;
            this._fontReference.PackagedAssetId = subMenu.MenuItemFontReference.PackagedAssetId;
        }

        this._menuItem = this.Parent as SelectionMenuItem;
        this._currentCulture = this.Game.DisplaySettings.Culture;
        this.ResetCharacters();
        this.ResetEndCapWidth();
        this.RenderOptions.Initialize(this.CreateSize);
        this.Reset();
        this.ResetText();
        this._fontReference.AssetChanged += this.Font_AssetChanged;
    }

    public override void Render(FrameTime frameTime, BoundingArea viewBoundingArea) {
        this.Render(frameTime, viewBoundingArea, this.GetMenuItemColor());
    }

    public override void Render(FrameTime frameTime, BoundingArea viewBoundingArea, Color colorOverride) {
        if (this.BoundingArea.IsEmpty || this.SpriteBatch == null) {
            return;
        }

        if (this._currentCulture != this.Game.DisplaySettings.Culture) {
            this._currentCulture = this.Game.DisplaySettings.Culture;
            this.ResetCharacters();
            this.Reset();
            this.ResetText();
        }

        if (this._font != null && this._spriteSheet != null && this._characterCollections.TryGetValue(this.ResourceName, out var characters)) {
            this._spriteSheet.Draw(
                this.SpriteBatch,
                this.Project.PixelsPerUnit,
                this._decreaseCharacter.SpriteIndex,
                new Vector2(this.BoundingArea.Minimum.X, this.BoundingArea.Minimum.Y),
                this._menuItem?.IsDecreaseEnabled == true ? colorOverride : PredefinedColors.DeactivatedText,
                this.RenderOptions.Orientation);

            var position = this._textStartLocation;

            foreach (var character in characters) {
                this._spriteSheet.Draw(
                    this.SpriteBatch,
                    this.Project.PixelsPerUnit,
                    character.SpriteIndex,
                    new Vector2(position, this.BoundingArea.Minimum.Y),
                    colorOverride,
                    this.RenderOptions.Orientation);

                position += this._font.GetCharacterWidth(character, 0, this.Project);
            }

            this._spriteSheet.Draw(
                this.SpriteBatch,
                this.Project.PixelsPerUnit,
                this._increaseCharacter.SpriteIndex,
                new Vector2(this.BoundingArea.Maximum.X - this.ActualEndCapWidth, this.BoundingArea.Minimum.Y),
                this._menuItem?.IsIncreaseEnabled == true ? colorOverride : PredefinedColors.DeactivatedText,
                this.RenderOptions.Orientation);
        }
    }

    /// <inheritdoc />
    protected override IEnumerable<IAssetReference> GetAssetReferences() {
        yield return this._fontReference;
    }

    protected override void OnTransformChanged() {
        base.OnTransformChanged();
        this.Reset();
        this.ResetText();
    }

    private BoundingArea CreateBoundingArea() => this.RenderOptions.CreateBoundingArea(this);

    private Vector2 CreateSize() {
        if (this._font != null && this._spriteSheet != null) {
            var longestTextWidth = this.GetLongestOptionWidth(this._font).ToPixelSnappedValue(this.Project);
            return new Vector2((this.EndCapWidth + this.EndCapPadding) * 2 + longestTextWidth * this.Project.PixelsPerUnit, this._spriteSheet.SpriteSize.Y);
        }

        return Vector2.Zero;
    }

    private void Font_AssetChanged(object? sender, bool hasAsset) {
        this.ResetCharacters();
    }

    private float GetLongestOptionWidth(SpriteSheetFont font) {
        if (this._characterCollections.Any()) {
            return this._characterCollections.Values.Max(x => x.Sum(character => font.GetCharacterWidth(character, 0, this.Project)));
        }

        return 0f;
    }

    private IEnumerable<SelectionOption> GetOptions() {
        if (this.Parent is SelectionMenuItem menu) {
            return menu.Options;
        }

        return [];
    }

    private void Reset() {
        this.RenderOptions.ResetOffset();
        this._boundingArea.Reset();
        this.BoundingAreaChanged.SafeInvoke(this);
    }

    private void ResetCharacters() {
        if (this._fontReference is { PackagedAsset: not null, Asset: not null }) {
            this._font = this._fontReference.PackagedAsset;
            this._spriteSheet = this._fontReference.Asset;
        }
        else if (this.Project.Fallbacks.Font is { PackagedAsset: not null, Asset: not null }) {
            this._font = this.Project.Fallbacks.Font.PackagedAsset;
            this._spriteSheet = this.Project.Fallbacks.Font.Asset;
        }

        if (this._font != null) {
            this._characterCollections.Clear();
            var options = this.GetOptions();
            foreach (var option in options) {
                var spriteCharacters = new List<SpriteSheetFontCharacter>();
                if (Resources.ResourceManager.TryGetString(option.Text, out var text)) {
                    foreach (var character in text) {
                        if (this._font.TryGetSpriteCharacter(character, out var spriteCharacter)) {
                            spriteCharacters.Add(spriteCharacter);
                        }
                    }
                }

                this._characterCollections.TryAdd(option.Text, spriteCharacters);
            }

            if (this._font.TryGetSpriteCharacter(DecreaseCharacter, out var decreaseCharacter)) {
                this._decreaseCharacter = decreaseCharacter;
            }

            if (this._font.TryGetSpriteCharacter(IncreaseCharacter, out var increaseCharacter)) {
                this._increaseCharacter = increaseCharacter;
            }
        }
    }

    private void ResetEndCapWidth() {
        this.ActualEndCapWidth = this._endCapWidth * this.Project.UnitsPerPixel;
    }

    private void ResetText() {
        if (this._font != null && this._characterCollections.TryGetValue(this.ResourceName, out var characters)) {
            var totalWidth = characters.Sum(character => this._font.GetCharacterWidth(character, 0, this.Project));
            var halfWidth = totalWidth * 0.5f;
            var boundingAreaMidPoint = this.BoundingArea.Minimum.X + 0.5f * this.BoundingArea.Width;
            this._textStartLocation = (boundingAreaMidPoint - halfWidth).ToPixelSnappedValue(this.Project);
        }
    }
}