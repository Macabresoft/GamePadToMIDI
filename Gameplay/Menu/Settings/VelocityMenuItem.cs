namespace Macabresoft.Macabre2D.Project.Gameplay;

using Macabresoft.Macabre2D.Framework;
using Macabresoft.Macabre2D.Project.Common;
using Microsoft.Xna.Framework.Input;

/// <summary>
/// A menu item for the <see cref="MidiNote.Velocity" /> property.
/// </summary>
public class VelocityMenuItem : SelectionMenuItem {
    private readonly Buttons _button;
    private readonly List<SelectionOption> _selectionOptions = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="NoteEnabledMenuItem" /> class.
    /// </summary>
    /// <param name="button">The button associated with this note.</param>
    public VelocityMenuItem(Buttons button) : base() {
        this._button = button;

        for (var i = 0; i <= MidiNote.MaxVelocity; i++) {
            var note = i;
            this._selectionOptions.Add(new SelectionOption(note.ToString(), () => this.SetValue(note)));
        }
    }

    /// <inheritdoc />
    public override bool CanFocus => this.Game.State.CurrentSave.TryGetMidiNote(this._button, out var midiNote) && midiNote.Value.IsEnabled;

    /// <inheritdoc />
    public override string ResourceName => nameof(Resources.Menu_Settings_Velocity);

    /// <inheritdoc />
    protected override List<SelectionOption> AvailableOptions => this._selectionOptions;

    /// <inheritdoc />
    public override void Initialize(IScene scene, IEntity parent) {
        base.Initialize(scene, parent);
        this.SetInitialValue();
    }

    private void SetInitialValue() {
        if (this.Game.State.CurrentSave.TryGetMidiNote(this._button, out var midiNote)) {
            this.InitializeSelection(this._selectionOptions[midiNote.Value.Velocity]);
        }
    }

    private void SetValue(int velocity) {
        this.Game.State.CurrentSave.SetVelocity(this._button, velocity);
        this.SetHasChanges();
    }
}