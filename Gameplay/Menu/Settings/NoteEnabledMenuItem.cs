namespace Macabresoft.Macabre2D.Project.Gameplay;

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
    protected override bool GetValue() => this.Game.State.CurrentSave.TryGetMidiNote(this._button, out var midiNote) && midiNote.Value.IsEnabled;

    /// <inheritdoc />
    protected override void SetValue(bool value) {
        this.Game.State.CurrentSave.SetEnabled(this._button, value);

        base.SetValue(value);
    }
}