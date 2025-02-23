﻿namespace Macabresoft.Macabre2D.Project.Gameplay;

using Macabresoft.Macabre2D.Framework;
using Macabresoft.Macabre2D.Project.Common;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

public class SettingsSubMenu : BaseMenu {
    private bool _isLoaded;

    public override void Initialize(IScene scene, IEntity parent) {
        if (!BaseGame.IsDesignMode && !this._isLoaded) {
            this.ClearChildren();

            var menuItemHeight = this.GetMenuItemHeight(scene.Project);
            var header = this.AddHeader(nameof(Resources.Menu_Settings));

            var device = this.AddSpinnerMenuItemWithText<MidiDeviceMenuItem>(header.LocalPosition.Y - menuItemHeight - SeparatorHeight);
            var gamePad = this.AddSpinnerMenuItemWithText<GamePadDisplayMenuItem>(device.LocalPosition.Y - menuItemHeight);

            var currentPosition = gamePad.LocalPosition.Y - menuItemHeight - SeparatorHeight;
            foreach (var button in MidiNoteBindingHelper.AvailableButtons) {
                var midiNote = this.GetMidiNote(button, scene.Game);
                this.AddNoteHeaderMenuItem(button, currentPosition - (menuItemHeight * 1.5f));
                var noteMenuItem = this.AddNoteMenuItem(button, midiNote, currentPosition - menuItemHeight);
                var velocity = this.AddVelocityMenuItem(button, midiNote, noteMenuItem.LocalPosition.Y - menuItemHeight);
                currentPosition = velocity.LocalPosition.Y - menuItemHeight - SeparatorHeight;
            }
            
            this.AddReturnMenuItem(currentPosition);
            this._isLoaded = true;
        }

        base.Initialize(scene, parent);
    }

    protected override void OnSave() {
        this.Game.State.Save();
        this.Game.UserSettings.Custom.CurrentSave = this.Game.State.CurrentSave.Id;
        base.OnSave();
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

    private MidiNote GetMidiNote(Buttons button, IGame game) {
        if (game.State.CurrentSave.TryGetMidiNote(button, out var midiNote)) {
            return midiNote.Value;
        }

        var newMidiNote = new MidiNote(0, 100);
        game.State.CurrentSave.SetNote(button, newMidiNote);
        this.HasChanges = true;
        return newMidiNote;
    }
}