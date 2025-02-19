namespace Macabresoft.Macabre2D.Project.Gameplay;

using Macabresoft.Macabre2D.Framework;
using Macabresoft.Macabre2D.Project.Common;
using Microsoft.Xna.Framework.Input;

public class NoteMenuItem : SelectionMenuItem {
    private readonly Buttons _button;
    private readonly List<SelectionOption> _selectionOptions = [];
    private MidiNote _midiNote;

    public NoteMenuItem(Buttons button, MidiNote midiNote) : base() {
        this._button = button;
        this._midiNote = midiNote;

        for (var i = 0; i <= MidiNote.MaxNote; i++) {
            var note = i;
            this._selectionOptions.Add(new SelectionOption(note.ToString(), () => this.SetValue(note)));
        }
    }

    public override string ResourceName => nameof(Resources.Menu_Settings_Note);

    protected override List<SelectionOption> AvailableOptions => this._selectionOptions;

    public override void Initialize(IScene scene, IEntity parent) {
        base.Initialize(scene, parent);
        this.SetInitialValue();
    }

    private void SetInitialValue() {
        this.InitializeSelection(this._selectionOptions[this._midiNote.Note]);
    }

    private void SetValue(int note) {
        this._midiNote = this._midiNote.WithNote(note);
        this.Game.State.CurrentSave.SetNote(this._button, this._midiNote);
        this.SetHasChanges();
    }
}