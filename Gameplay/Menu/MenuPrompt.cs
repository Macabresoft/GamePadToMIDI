namespace Macabresoft.Macabre2D.Project.Gameplay;

using System.Runtime.Serialization;
using Macabresoft.Macabre2D.Framework;
using Macabresoft.Macabre2D.Project.Common;

/// <summary>
/// The type of prompt.
/// </summary>
public enum PromptType {
    Confirm,
    Cancel,
    Settings
}

/// <summary>
/// A prompt which shows an input and its description.
/// </summary>
public class MenuPrompt : DockableWrapper {
    private ISpriteEntity _inputActionRenderer = EmptyObject.Instance;
    private IMenuSystem _menuSystem = MenuSystem.Empty;
    private PromptType _promptType = PromptType.Confirm;
    private ITextRenderer _textLineRenderer = EmptyObject.Instance;

    /// <summary>
    /// Gets or sets the prompt type.
    /// </summary>
    [DataMember]
    public PromptType PromptType {
        get => this._promptType;
        set {
            if (this.Set(ref this._promptType, value)) {
                this.UpdateInputAction();
            }
        }
    }

    /// <inheritdoc />
    public override void Deinitialize() {
        this._menuSystem.MenuItemChanged -= this.MenuSystem_MenuItemChanged;
        this.Game.InputDeviceChanged -= this.GameOnInputDeviceChanged;

        base.Deinitialize();

        this._textLineRenderer = EmptyObject.Instance;
        this._inputActionRenderer = EmptyObject.Instance;
        this._menuSystem = MenuSystem.Empty;
    }

    /// <inheritdoc />
    public override void Initialize(IScene scene, IEntity parent) {
        base.Initialize(scene, parent);

        this._menuSystem = this.Scene.GetSystem<MenuSystem>() ?? this._menuSystem;
        this._menuSystem.MenuItemChanged += this.MenuSystem_MenuItemChanged;

        this._inputActionRenderer = this.GetOrAddChild<InputActionRenderer>();
        this._inputActionRenderer.RenderOptions.OffsetType = PixelOffsetType.Right;

        this._textLineRenderer = this.GetOrAddChild<TextLineRenderer>();
        this._textLineRenderer.RenderOptions.OffsetType = PixelOffsetType.Left;

        if (this.Project.Fonts.TryGetFont(FontCategory.Normal, this.Game.DisplaySettings.Culture, out var fontDefinition)) {
            this._textLineRenderer.FontReference.LoadAsset(fontDefinition.SpriteSheetId, fontDefinition.FontId);
        }

        this.Game.InputDeviceChanged += this.GameOnInputDeviceChanged;
        this.Reset();
    }

    private void GameOnInputDeviceChanged(object? sender, InputDevice e) {
        this.Reset();
    }

    private void MenuSystem_MenuItemChanged(object? sender, EventArgs e) {
        this.Reset();
    }

    private void Reset() {
        if (this.TryGetPromptResource(out var resourceName)) {
            this._textLineRenderer.ResourceName = resourceName;
            this.UpdateInputAction();

            this._textLineRenderer.ShouldRender = true;
            this._inputActionRenderer.ShouldRender = true;
            this.IsCollapsed = false;
        }
        else {
            this._textLineRenderer.ShouldRender = false;
            this._inputActionRenderer.ShouldRender = false;
            this.IsCollapsed = true;
        }

        this.RequestRearrangeFromParent();
    }

    private bool TryGetPromptResource(out string resourceName) {
        resourceName = this.PromptType switch {
            PromptType.Cancel => nameof(Resources.Menu_Return),
            PromptType.Confirm => this._menuSystem.FocusedMenu.FocusedMenuItem.ConfirmPromptResourceName,
            PromptType.Settings => nameof(Resources.Menu_Settings),
            _ => string.Empty
        };

        return !string.IsNullOrEmpty(resourceName);
    }

    private void UpdateInputAction() {
        if (this._inputActionRenderer is InputActionRenderer renderer) {
            renderer.Action = this.PromptType switch {
                PromptType.Confirm => InputAction.Confirm,
                PromptType.Cancel => InputAction.Cancel,
                PromptType.Settings => InputAction.Settings,
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }
}