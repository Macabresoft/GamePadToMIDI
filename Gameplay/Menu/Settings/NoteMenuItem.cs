namespace Macabresoft.Macabre2D.Project.Gameplay;

using System.ComponentModel;
using Macabresoft.Macabre2D.Framework;
using Macabresoft.Macabre2D.Project.Common;
using Microsoft.Xna.Framework.Input;

/// <summary>
/// A menu item for the <see cref="MidiNote.Note" /> property.
/// </summary>
public class NoteMenuItem : SelectionMenuItem {
    private readonly Buttons _button;
    private readonly List<SelectionOption> _selectionOptions = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="NoteMenuItem" /> class.
    /// </summary>
    /// <param name="button">The button associated with this note.</param>
    public NoteMenuItem(Buttons button) : base() {
        this._button = button;

        for (var i = 0; i <= MidiNote.MaxNote; i++) {
            var note = i;
            this._selectionOptions.Add(new SelectionOption(note.ToString(), () => this.SetValue(note)));
        }
    }

    /// <inheritdoc />
    public override bool CanFocus => this.Game.State.CurrentSave.TryGetMidiNote(this._button, out var midiNote) && midiNote.Value.IsEnabled;

    /// <inheritdoc />
    public override string ResourceName => nameof(Resources.Menu_Settings_Note);

    /// <inheritdoc />
    protected override List<SelectionOption> AvailableOptions => this._selectionOptions;

    /// <inheritdoc />
    public override void Deinitialize() {
        this.Game.State.PropertyChanged -= this.GameState_PropertyChanged;
        base.Deinitialize();
    }

    /// <inheritdoc />
    public override void Initialize(IScene scene, IEntity parent) {
        base.Initialize(scene, parent);
        this.Game.State.PropertyChanged += this.GameState_PropertyChanged;
        this.SetInitialValue();
    }

    private void GameState_PropertyChanged(object? sender, PropertyChangedEventArgs e) {
        if (e.PropertyName == nameof(GameState.CurrentSave)) {
            this.SetInitialValue();
        }
    }

    private void SetInitialValue() {
        if (this.Game.State.CurrentSave.TryGetMidiNote(this._button, out var midiNote)) {
            this.InitializeSelection(this._selectionOptions[midiNote.Value.Note]);
        }
    }

    private void SetValue(int note) {
        this.Game.State.CurrentSave.SetNote(this._button, note);
        this.SetHasChanges();
    }
}