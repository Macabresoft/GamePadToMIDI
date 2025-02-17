namespace Macabresoft.Macabre2D.Project.Gameplay;

using System.Runtime.Serialization;
using Macabresoft.Macabre2D.Framework;
using Macabresoft.Macabre2D.Project.Common;

public enum PromptType {
    Confirm,
    Cancel
}

public class MenuPrompt : DockableWrapper {
    private ISpriteEntity _inputActionRenderer = EmptyObject.Instance;
    private IMenuSystem _menuSystem = MenuSystem.Empty;
    private PromptType _promptType = PromptType.Confirm;
    private ITextRenderer _textLineRenderer = EmptyObject.Instance;

    [DataMember]
    public PromptType PromptType {
        get => this._promptType;
        set {
            if (this.Set(ref this._promptType, value)) {
                this.UpdateInputAction();
            }
        }
    }

    public override void Deinitialize() {
        this._menuSystem.MenuItemChanged -= this.MenuSystem_MenuItemChanged;
        this.Game.InputDeviceChanged -= this.GameOnInputDeviceChanged;

        base.Deinitialize();

        this._textLineRenderer = EmptyObject.Instance;
        this._inputActionRenderer = EmptyObject.Instance;
        this._menuSystem = MenuSystem.Empty;
    }

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
        bool result;
        resourceName = string.Empty;
        if (this.PromptType == PromptType.Cancel) {
            result = this._menuSystem.FocusedMenu.ShowReturnPrompt;
            resourceName = nameof(Resources.Menu_Return);
        }
        else {
            resourceName = this._menuSystem.FocusedMenu.FocusedMenuItem.ConfirmPromptResourceName;
            result = !string.IsNullOrEmpty(resourceName);
        }

        return result;
    }

    private void UpdateInputAction() {
        if (this._inputActionRenderer is InputActionRenderer renderer) {
            renderer.Action = this.PromptType switch {
                PromptType.Confirm => InputAction.Confirm,
                PromptType.Cancel => InputAction.Cancel,
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }
}