namespace Macabresoft.Macabre2D.Project.Gameplay;

using Macabresoft.Macabre2D.Framework;
using Macabresoft.Macabre2D.Project.Common;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

/// <summary>
/// The settings menu.
/// </summary>
public class SettingsSubMenu : BaseMenu {
    private bool _isLoaded;

    /// <inheritdoc />
    public override void Initialize(IScene scene, IEntity parent) {
        if (!BaseGame.IsDesignMode && !this._isLoaded) {
            this.ClearChildren();

            var menuItemHeight = this.GetMenuItemHeight(scene.Project);
            var header = this.AddHeader(nameof(Resources.Menu_Settings), 0f);

            var device = this.AddSpinnerMenuItemWithText<MidiDeviceMenuItem>(header.LocalPosition.Y - menuItemHeight - SeparatorHeight);
            var gamePad = this.AddSpinnerMenuItemWithText<GamePadDisplayMenuItem>(device.LocalPosition.Y - menuItemHeight);
            var channel = this.AddSpinnerMenuItemWithText<ChannelMenuItem>(gamePad.LocalPosition.Y - menuItemHeight);

            var configurationHeader = this.AddHeader(nameof(Resources.Menu_Settings_Configuration), channel.LocalPosition.Y - menuItemHeight - SeparatorHeight);
            // TODO: select configuration menu item

            var createNew = this.AddMenuItemWithText<CreateSaveMenuItem>(configurationHeader.LocalPosition.Y - menuItemHeight - SeparatorHeight, 0f, PixelOffsetType.Center);
            var delete = this.AddMenuItemWithText<DeleteSaveMenuItem>(createNew.LocalPosition.Y - menuItemHeight, 0f, PixelOffsetType.Center);

            var currentPosition = delete.LocalPosition.Y - menuItemHeight - SeparatorHeight;
            foreach (var button in MidiNoteBindingHelper.AvailableButtons) {
                this.AddNoteHeaderMenuItem(button, currentPosition - menuItemHeight * 1.5f);
                var enabledMenuItem = this.AddEnabledMenuItem(button, currentPosition - menuItemHeight);
                var noteMenuItem = this.AddNoteMenuItem(button, enabledMenuItem.LocalPosition.Y - menuItemHeight);
                var velocity = this.AddVelocityMenuItem(button, noteMenuItem.LocalPosition.Y - menuItemHeight);
                currentPosition = velocity.LocalPosition.Y - menuItemHeight - SeparatorHeight;
            }

            this.AddReturnMenuItem(currentPosition);
            this._isLoaded = true;
        }

        base.Initialize(scene, parent);
    }

    /// <inheritdoc />
    protected override void OnSave() {
        this.Game.State.Save();
        this.Game.UserSettings.Custom.CurrentSave = this.Game.State.CurrentSave.Id;
        base.OnSave();
    }

    private NoteEnabledMenuItem AddEnabledMenuItem(Buttons button, float yPosition) {
        var menuItem = new NoteEnabledMenuItem(button);
        this.AddChild(menuItem);
        menuItem.LocalPosition = new Vector2(0f, yPosition);
        this.ApplyTextToMenuItem(menuItem, menuItem.ResourceName, -MenuItemDistanceFromCenter, PixelOffsetType.Left);
        this.ApplySpinnerToMenuItem(menuItem);
        return menuItem;
    }

    private GamePadButtonRenderer AddNoteHeaderMenuItem(Buttons button, float yPosition) {
        var renderer = this.AddChild<GamePadButtonRenderer>();
        renderer.LocalPosition = new Vector2(0f, yPosition);
        renderer.RenderOptions.OffsetType = PixelOffsetType.Center;
        renderer.Button = button;
        return renderer;
    }

    private NoteMenuItem AddNoteMenuItem(Buttons button, float yPosition) {
        var menuItem = new NoteMenuItem(button);
        this.AddChild(menuItem);
        menuItem.LocalPosition = new Vector2(0f, yPosition);
        this.ApplyTextToMenuItem(menuItem, menuItem.ResourceName, -MenuItemDistanceFromCenter, PixelOffsetType.Left);
        this.ApplySpinnerToMenuItem(menuItem);
        return menuItem;
    }

    private VelocityMenuItem AddVelocityMenuItem(Buttons button, float yPosition) {
        var menuItem = new VelocityMenuItem(button);
        this.AddChild(menuItem);
        menuItem.LocalPosition = new Vector2(0f, yPosition);
        this.ApplyTextToMenuItem(menuItem, menuItem.ResourceName, -MenuItemDistanceFromCenter, PixelOffsetType.Left);
        this.ApplySpinnerToMenuItem(menuItem);
        return menuItem;
    }
}