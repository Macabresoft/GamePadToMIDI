namespace Macabresoft.Macabre2D.Project.Gameplay;

using Macabresoft.Macabre2D.Framework;
using Macabresoft.Macabre2D.Project.Common;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

public class SettingsSubMenu : SavableVerticalMenu {
    public override void Initialize(IScene scene, IEntity parent) {
        if (!BaseGame.IsDesignMode) {
            this.ClearChildren();

            var menuItemHeight = this.GetMenuItemHeight(scene.Project);
            var header = this.AddHeader(nameof(Resources.Menu_Settings));

            var device = this.AddSpinnerMenuItemWithText<MidiDeviceMenuItem>(header.LocalPosition.Y - menuItemHeight - SeparatorHeight);
            var gamePad = this.AddSpinnerMenuItemWithText<GamePadDisplayMenuItem>(device.LocalPosition.Y - menuItemHeight);

            var currentPosition = gamePad.LocalPosition.Y - menuItemHeight - SeparatorHeight;
            foreach (var button in MidiNoteBindingHelper.AvailableButtons) {
                var midiNote = this.GetMidiNote(button);
                var noteHeader = this.AddNoteHeaderMenuItem(button, currentPosition);
                var noteMenuItem = this.AddNoteMenuItem(button, midiNote, noteHeader.LocalPosition.Y - menuItemHeight);
                var velocity = this.AddVelocityMenuItem(button, midiNote, noteMenuItem.LocalPosition.Y - menuItemHeight);
                currentPosition = velocity.LocalPosition.Y - menuItemHeight - SeparatorHeight;
            }
        }

        base.Initialize(scene, parent);
    }

    protected override void OnSave() {
        base.OnSave();
        this.Game.State.Save();
    }

    private GamePadButtonRenderer AddNoteHeaderMenuItem(Buttons button, float yPosition) {
        var renderer = this.AddChild<GamePadButtonRenderer>();
        renderer.LocalPosition = new Vector2(0f, yPosition);
        renderer.RenderOptions.OffsetType = PixelOffsetType.Center;
        renderer.Button = button;
        return renderer;
    }

    private MenuItem AddNoteMenuItem(Buttons button, MidiNote midiNote, float yPosition) {
        var menuItem = new NoteMenuItem(button, midiNote);
        this.AddChild(menuItem);
        menuItem.LocalPosition = new Vector2(0f, yPosition);
        this.ApplyTextToMenuItem(menuItem, menuItem.ResourceName);
        this.ApplySpinnerToMenuItem(menuItem);
        return menuItem;
    }

    private MenuItem AddVelocityMenuItem(Buttons button, MidiNote midiNote, float yPosition) {
        var menuItem = new VelocityMenuItem(button, midiNote);
        this.AddChild(menuItem);
        menuItem.LocalPosition = new Vector2(0f, yPosition);
        this.ApplyTextToMenuItem(menuItem, menuItem.ResourceName);
        this.ApplySpinnerToMenuItem(menuItem);
        return menuItem;
    }

    private MidiNote GetMidiNote(Buttons button) {
        if (this.Game.State.CurrentSave.TryGetMidiNote(button, out var midiNote)) {
            return midiNote.Value;
        }

        var newMidiNote = new MidiNote(0, 100);
        this.Game.State.CurrentSave.SetNote(button, newMidiNote);
        return newMidiNote;
    }
}