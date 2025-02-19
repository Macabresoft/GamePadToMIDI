namespace Macabresoft.Macabre2D.Project.Gameplay;

using Macabresoft.Core;
using Macabresoft.Macabre2D.Framework;
using Microsoft.Xna.Framework.Input;
using NAudio.Midi;

public class MidiSystem : GameSystem {
    private readonly List<MidiDeviceDefinition> _midiDevices = new();
    private MidiOut? _midiOut;
    private IScene? _pauseScene;
    private MidiDeviceDefinition _selected = MidiDeviceDefinition.Empty;

    public event EventHandler? DeviceChanged;

    public override GameSystemKind Kind => GameSystemKind.Update;

    public IReadOnlyCollection<MidiDeviceDefinition> MidiDevices => this._midiDevices;

    public MidiDeviceDefinition Selected {
        get => this._selected;
        set {
            this._selected = value;
            this.DeviceChanged?.SafeInvoke(this);
        }
    }

    public override void Deinitialize() {
        base.Deinitialize();
        this._midiOut = null;
        this._pauseScene = null;
    }

    public override void Initialize(IScene scene) {
        base.Initialize(scene);
        this._midiDevices.Clear();
        this._midiDevices.Add(MidiDeviceDefinition.Empty);

        if (MidiOut.NumberOfDevices > 0 && !BaseGame.IsDesignMode) {
            for (var device = 0; device < MidiOut.NumberOfDevices; device++) {
                this._midiDevices.Add(new MidiDeviceDefinition(device, MidiOut.DeviceInfo(device).ProductName));
            }

            this._selected = this._midiDevices.Last();
            this._midiOut = this._selected.Index >= 0 ? new MidiOut(this._selected.Index) : null;
        }

        if (this.Scene.Assets.TryLoadContent<Scene>(this.Scene.Project.AdditionalConfiguration.PauseSceneId, out var pauseScene)) {
            this._pauseScene = pauseScene;
        }
    }

    public override void Update(FrameTime frameTime, InputState inputState) {
        if (this._midiOut != null) {
            if (inputState.IsGamePadButtonNewlyPressed(Buttons.Start) && this._pauseScene != null) {
                this.Game.PushScene(this._pauseScene);
            }

            if (inputState.IsGamePadButtonNewlyPressed(Buttons.A)) {
                var noteOnEvent = new NoteOnEvent(0, 1, 36, 100, 50);
                this._midiOut.Send(noteOnEvent.GetAsShortMessage());
            }

            if (inputState.IsGamePadButtonNewlyPressed(Buttons.X)) {
                var noteOnEvent = new NoteOnEvent(0, 1, 38, 100, 50);
                this._midiOut.Send(noteOnEvent.GetAsShortMessage());
            }
        }
    }
}