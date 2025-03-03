﻿namespace Macabresoft.Macabre2D.Project.Gameplay;

using System.ComponentModel;
using Macabresoft.Macabre2D.Framework;
using Macabresoft.Macabre2D.Project.Common;
using Microsoft.Xna.Framework.Input;

/// <summary>
/// A menu item for the <see cref="MidiNote.IsEnabled" /> property.
/// </summary>
public class NoteEnabledMenuItem : OnOffMenuItem {
    private readonly Buttons _button;

    /// <summary>
    /// Initializes a new instance of the <see cref="NoteEnabledMenuItem" /> class.
    /// </summary>
    /// <param name="button">The button associated with this note.</param>
    public NoteEnabledMenuItem(Buttons button) : base() {
        this._button = button;
    }

    /// <inheritdoc />
    public override string ResourceName => nameof(Resources.Menu_Settings_NoteEnabled);

    /// <inheritdoc />
    public override void Deinitialize() {
        this.Game.State.PropertyChanged -= this.GameState_PropertyChanged;
        base.Deinitialize();
    }

    /// <inheritdoc />
    public override void Initialize(IScene scene, IEntity parent) {
        base.Initialize(scene, parent);
        this.Game.State.PropertyChanged += this.GameState_PropertyChanged;
    }

    /// <inheritdoc />
    protected override bool GetValue() => this.Game.State.CurrentSave.TryGetMidiNote(this._button, out var midiNote) && midiNote.Value.IsEnabled;

    /// <inheritdoc />
    protected override void SetValue(bool value) {
        if (value) {
            this.Game.State.CurrentSave.Enable(this._button);
        }
        else {
            this.Game.State.CurrentSave.Disable(this._button);
        }

        base.SetValue(value);
    }

    private void GameState_PropertyChanged(object? sender, PropertyChangedEventArgs e) {
        if (e.PropertyName == nameof(GameState.CurrentSave)) {
            this.SetInitialValue();
        }
    }
}