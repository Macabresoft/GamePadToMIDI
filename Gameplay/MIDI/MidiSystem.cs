namespace Macabresoft.Macabre2D.Project.Gameplay;

using System.Runtime.Serialization;
using Macabresoft.Macabre2D.Framework;
using Macabresoft.Macabre2D.Project.Common;
using Microsoft.Xna.Framework.Input;
using NAudio.Midi;

public class MidiSystem : GameSystem {
    private MidiOut? _midiOut;
    private IScene? _pauseScene;
    
    public override GameSystemKind Kind => GameSystemKind.Update;

    [DataMember]
    public SpriteReference PadOffSprite { get; } = new();

    [DataMember]
    public SpriteReference PadOnSprite { get; } = new();

    public override void Deinitialize() {
        base.Deinitialize();
        this._midiOut = null;
        this._pauseScene = null;

        this.PadOffSprite.Deinitialize();
        this.PadOnSprite.Deinitialize();
    }

    public MidiNote GetMidiNote(Buttons button) => this.Game.State.CurrentSave.TryGetMidiNote(button, out var midiNote) ? midiNote.Value : MidiNote.Empty;

    public override void Initialize(IScene scene) {
        base.Initialize(scene);

        this.PadOffSprite.Initialize(this.Scene.Assets, this.Game);
        this.PadOnSprite.Initialize(this.Scene.Assets, this.Game);

        this._midiOut = this.Game.State.SelectedMidiDevice.Index >= 0 ? new MidiOut(this.Game.State.SelectedMidiDevice.Index) : null;

        if (this.Scene.Assets.TryLoadContent<Scene>(this.Scene.Project.AdditionalConfiguration.PauseSceneId, out var pauseScene)) {
            this._pauseScene = pauseScene;
        }
    }

    public void PlayMidiNote(MidiNote midiNote) {
        if (this._midiOut != null) {
            var noteOnEvent = new NoteOnEvent(0, 1, midiNote.Note, midiNote.Velocity, 50);
            this._midiOut.Send(noteOnEvent.GetAsShortMessage());
        }
    }

    public override void Update(FrameTime frameTime, InputState inputState) {
        if (this._pauseScene != null && inputState.IsGamePadButtonNewlyPressed(Buttons.Start)) {
            this.Game.PushScene(this._pauseScene);
        }
    }
}