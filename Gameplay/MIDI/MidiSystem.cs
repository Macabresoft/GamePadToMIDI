namespace Macabresoft.Macabre2D.Project.Gameplay;

using System.Runtime.Serialization;
using Macabresoft.Macabre2D.Framework;
using Macabresoft.Macabre2D.Project.Common;
using Microsoft.Xna.Framework.Input;
using NAudio.Midi;

/// <summary>
/// A system which handles general MIDI
/// </summary>
public class MidiSystem : InputSystem {
    private MidiOut? _midiOut;
    private IScene? _pauseScene;

    /// <inheritdoc />
    public override GameSystemKind Kind => GameSystemKind.Update;

    /// <summary>
    /// Gets the sprite shown when a MIDI pad is not active.
    /// </summary>
    [DataMember]
    public SpriteReference PadOffSprite { get; } = new();

    /// <summary>
    /// Gets the sprite shown when a MIDI pad is active.
    /// </summary>
    [DataMember]
    public SpriteReference PadOnSprite { get; } = new();

    /// <inheritdoc />
    public override void Deinitialize() {
        this.Game.State.MidiDeviceChanged -= this.State_MidiDeviceChanged;

        base.Deinitialize();
        this._midiOut = null;
        this._pauseScene = null;

        this.PadOffSprite.Deinitialize();
        this.PadOnSprite.Deinitialize();
    }

    /// <summary>
    /// Gets the <see cref="MidiNote" /> associated with the provided button.
    /// </summary>
    /// <param name="button">The button.</param>
    /// <returns>The MIDI note definition.</returns>
    public MidiNote GetMidiNote(Buttons button) => this.Game.State.CurrentSave.TryGetMidiNote(button, out var midiNote) ? midiNote.Value : MidiNote.Empty;

    /// <inheritdoc />
    public override void Initialize(IScene scene) {
        base.Initialize(scene);

        this.PadOffSprite.Initialize(this.Scene.Assets, this.Game);
        this.PadOnSprite.Initialize(this.Scene.Assets, this.Game);

        this.ResetMidiOut();
        this.Game.State.MidiDeviceChanged += this.State_MidiDeviceChanged;

        if (this.Scene.Assets.TryLoadContent<Scene>(this.Scene.Project.AdditionalConfiguration.PauseSceneId, out var pauseScene)) {
            this._pauseScene = pauseScene;
        }
    }

    /// <summary>
    /// Opens the settings menu.
    /// </summary>
    public void OpenSettings() {
        if (this._pauseScene != null) {
            this.Game.PushScene(this._pauseScene);
        }
    }

    /// <summary>
    /// Plays the specified <see cref="MidiNote" />.
    /// </summary>
    /// <param name="midiNote">The MIDI note definition.</param>
    public void PlayMidiNote(MidiNote midiNote) {
        if (this._midiOut != null) {
            var noteOnEvent = new NoteOnEvent(0, this.Game.State.CurrentSave.Channel, midiNote.Note, midiNote.Velocity, 50);
            this._midiOut.Send(noteOnEvent.GetAsShortMessage());
        }
    }

    /// <inheritdoc />
    public override void Update(FrameTime frameTime, InputState inputState) {
        base.Update(frameTime, inputState);

        if (this.IsPressed(InputAction.Settings)) {
            this.OpenSettings();
        }
    }

    private void ResetMidiOut() {
        this._midiOut?.Dispose();
        this._midiOut = this.Game.State.SelectedMidiDevice.Index >= 0 ? new MidiOut(this.Game.State.SelectedMidiDevice.Index) : null;
    }

    private void State_MidiDeviceChanged(object? sender, EventArgs e) {
        this.ResetMidiOut();
    }
}