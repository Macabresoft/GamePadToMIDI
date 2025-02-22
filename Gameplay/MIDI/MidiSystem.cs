namespace Macabresoft.Macabre2D.Project.Gameplay;

using Macabresoft.Macabre2D.Framework;
using Macabresoft.Macabre2D.Project.Common;
using Microsoft.Xna.Framework.Input;
using NAudio.Midi;

public class MidiSystem : GameSystem {
    private MidiOut? _midiOut;
    private IScene? _pauseScene;


    public override GameSystemKind Kind => GameSystemKind.Update;


    public override void Deinitialize() {
        base.Deinitialize();
        this._midiOut = null;
        this._pauseScene = null;
    }

    public override void Initialize(IScene scene) {
        base.Initialize(scene);

        this._midiOut = this.Game.State.SelectedMidiDevice.Index >= 0 ? new MidiOut(this.Game.State.SelectedMidiDevice.Index) : null;

        if (this.Scene.Assets.TryLoadContent<Scene>(this.Scene.Project.AdditionalConfiguration.PauseSceneId, out var pauseScene)) {
            this._pauseScene = pauseScene;
        }
    }

    public override void Update(FrameTime frameTime, InputState inputState) {
        if (this._midiOut != null) {
            if (inputState.IsGamePadButtonNewlyPressed(Buttons.Start) && this._pauseScene != null) {
                this.Game.PushScene(this._pauseScene);
            }

            foreach (var button in MidiNoteBindingHelper.AvailableButtons) {
                if (inputState.IsGamePadButtonNewlyPressed(button) && this.Game.State.CurrentSave.TryGetMidiNote(button, out var midiNote) && midiNote.Value.IsEnabled) {
                    var noteOnEvent = new NoteOnEvent(0, 1, midiNote.Value.Note, midiNote.Value.Velocity, 50);
                    this._midiOut.Send(noteOnEvent.GetAsShortMessage());
                }
            }
        }
    }
}