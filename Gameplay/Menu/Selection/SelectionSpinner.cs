namespace Macabresoft.Macabre2D.Project.Gameplay;

using System.Runtime.Serialization;
using Macabresoft.Core;
using Macabresoft.Macabre2D.Framework;
using Macabresoft.Macabre2D.Project.Common;
using Microsoft.Xna.Framework;

/// <summary>
/// A selection spinner that can be applied to a <see cref="MenuItem" />.
/// </summary>
public class SelectionSpinner : RenderableEntity {
    private const char DecreaseCharacter = '<';
    private const char IncreaseCharacter = '>';

    private readonly ResettableLazy<BoundingArea> _boundingArea;
    private readonly Dictionary<string, IReadOnlyCollection<SpriteSheetFontCharacter>> _characterCollections = new();
    private ResourceCulture _currentCulture;
    private SpriteSheetFontCharacter _decreaseCharacter = new();
    private int _endCapPadding;
    private int _endCapWidth;
    private SpriteSheetFont? _font;
    private SpriteSheetFontCharacter _increaseCharacter = new();
    private SelectionMenuItem? _menuItem;
    private SpriteSheet? _spriteSheet;
    private string _text = string.Empty;
    private TextLineRenderer? _textRenderer;

    /// <inheritdoc />
    public override event EventHandler? BoundingAreaChanged;

    /// <summary>
    /// Initializes a new instance of the <see cref="SelectionSpinner" /> class.
    /// </summary>
    public SelectionSpinner() : base() {
        this._boundingArea = new ResettableLazy<BoundingArea>(this.CreateBoundingArea);
    }

    /// <summary>
    /// Gets the actual end cap width in units. This includes padding.
    /// </summary>
    public float ActualEndCapWidth { get; private set; }

    /// <inheritdoc />
    public override BoundingArea BoundingArea => this._boundingArea.Value;

    /// <summary>
    /// Gets or sets the padding for the end caps.
    /// </summary>
    [DataMember]
    public int EndCapPadding {
        get => this._endCapPadding;
        set {
            this._endCapPadding = value;
            this.Reset();
        }
    }

    /// <summary>
    /// Gets or sets the width of the end caps.
    /// </summary>
    [DataMember]
    public int EndCapWidth {
        get => this._endCapWidth;
        set {
            this._endCapWidth = value;
            this.ResetEndCapWidth();
            this.Reset();
        }
    }

    /// <summary>
    /// Gets the render options.
    /// </summary>
    [DataMember(Order = 4)]
    public RenderOptions RenderOptions { get; } = new();

    /// <summary>
    /// Gets or sets the text.
    /// </summary>
    [DataMember]
    public string Text {
        get => this._text;
        set {
            if (value != this._text) {
                this._text = value;
                this.ResetText();
            }
        }
    }

    /// <inheritdoc />
    public override void Deinitialize() {
        base.Deinitialize();
        this._textRenderer = null;
    }

    /// <inheritdoc />
    public override void Initialize(IScene scene, IEntity parent) {
        base.Initialize(scene, parent);

        this._textRenderer = this.GetOrAddChild<MenuTextLineRenderer>();
        this._textRenderer.RenderOptions.OffsetType = PixelOffsetType.BottomLeft;
        this._textRenderer.FontCategory = FontCategory.Normal;
        this._menuItem = this.Parent as SelectionMenuItem;
        this._currentCulture = this.Game.DisplaySettings.Culture;
        this.ResetCharacters();
        this.ResetEndCapWidth();
        this.Scene.Invoke(() =>
        {
            this.RenderOptions.Initialize(this.CreateSize);
            this.Reset();
            this.ResetText();
        });
    }

    /// <inheritdoc />
    public override void Render(FrameTime frameTime, BoundingArea viewBoundingArea) {
        this.Render(frameTime, viewBoundingArea, this.GetMenuItemColor());
    }

    /// <inheritdoc />
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

        if (this._spriteSheet != null) {
            this._spriteSheet.Draw(
                this.SpriteBatch,
                this.Project.PixelsPerUnit,
                this._decreaseCharacter.SpriteIndex,
                new Vector2(this.BoundingArea.Minimum.X, this.BoundingArea.Minimum.Y),
                this._menuItem?.IsDecreaseEnabled == true ? colorOverride : PredefinedColors.DeactivatedText,
                this.RenderOptions.Orientation);

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
    protected override void OnTransformChanged() {
        base.OnTransformChanged();
        this.Reset();
        this.ResetText();
    }

    private BoundingArea CreateBoundingArea() => this.RenderOptions.CreateBoundingArea(this);

    private Vector2 CreateSize() {
        if (this._font != null && this._spriteSheet != null) {
            var longestTextWidth = this.GetLongestOptionWidth(this._font).ToPixelSnappedValue(this.Project);
            var actualWidth = MathF.Min(longestTextWidth, BaseMenu.SpinnerWidth);
            return new Vector2((this.EndCapWidth + this.EndCapPadding) * 2 + actualWidth * this.Project.PixelsPerUnit, this._spriteSheet.SpriteSize.Y);
        }

        return Vector2.Zero;
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
        if (this._textRenderer?.FontReference is { PackagedAsset: not null, Asset: not null }) {
            this._font = this._textRenderer.FontReference.PackagedAsset;
            this._spriteSheet = this._textRenderer.FontReference.Asset;
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
                foreach (var character in option.Text) {
                    if (this._font.TryGetSpriteCharacter(character, out var spriteCharacter)) {
                        spriteCharacters.Add(spriteCharacter);
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
        if (this._font != null && this._textRenderer != null && this._characterCollections.TryGetValue(this.Text, out var characters)) {
            var totalWidth = characters.Sum(character => this._font.GetCharacterWidth(character, 0, this.Project));
            if (totalWidth > BaseMenu.SpinnerWidth) {
                this._textRenderer.SetWorldPosition(new Vector2(this.BoundingArea.Minimum.X + this.ActualEndCapWidth, this.BoundingArea.Minimum.Y));
                this._textRenderer.ShouldScroll = true;
                this._textRenderer.WidthOverride.IsEnabled = true;
                this._textRenderer.WidthOverride.Value = BaseMenu.SpinnerWidth;
            }
            else {
                var halfWidth = totalWidth * 0.5f;
                var boundingAreaMidPoint = this.BoundingArea.Minimum.X + 0.5f * this.BoundingArea.Width;
                this._textRenderer.SetWorldPosition(new Vector2(boundingAreaMidPoint - halfWidth, this.BoundingArea.Minimum.Y));
                this._textRenderer.ShouldScroll = false;
                this._textRenderer.WidthOverride.IsEnabled = false;
            }

            this._textRenderer.Text = this.Text;
        }
    }
}