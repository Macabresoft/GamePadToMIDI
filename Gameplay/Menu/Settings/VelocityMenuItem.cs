namespace Macabresoft.Macabre2D.Project.Gameplay;

using Macabresoft.Macabre2D.Framework;
using Macabresoft.Macabre2D.Project.Common;
using Microsoft.Xna.Framework.Input;

public class VelocityMenuItem : SelectionMenuItem {
    private readonly Buttons _button;
    private readonly List<SelectionOption> _selectionOptions = [];
    private MidiNote _midiNote;

    public VelocityMenuItem(Buttons button, MidiNote midiNote) : base() {
        this._button = button;
        this._midiNote = midiNote;

        for (var i = 0; i <= MidiNote.MaxVelocity; i++) {
            var note = i;
            this._selectionOptions.Add(new SelectionOption(note.ToString(), () => this.SetValue(note)));
        }
    }

    public override string ResourceName => nameof(Resources.Menu_Settings_Velocity);

    protected override List<SelectionOption> AvailableOptions => this._selectionOptions;

    public override void Initialize(IScene scene, IEntity parent) {
        base.Initialize(scene, parent);
        this.SetInitialValue();
    }

    private void SetInitialValue() {
        this.InitializeSelection(this._selectionOptions[this._midiNote.Velocity]);
    }

    private void SetValue(int note) {
        this._midiNote = this._midiNote.WithVelocity(note);
        this.Game.State.CurrentSave.SetNote(this._button, this._midiNote);
        this.SetHasChanges();
    }
}