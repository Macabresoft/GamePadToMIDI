namespace Macabresoft.Macabre2D.Project.Gameplay.MIDI;

using Macabresoft.Core;
using Macabresoft.Macabre2D.Framework;
using Microsoft.Xna.Framework.Input;
using NAudio.Midi;

public class MidiSystem : GameSystem {
    private readonly List<MidiDeviceDefinition> _midiDevices = new();
    private MidiOut? _midiOut;
    private MidiDeviceDefinition _selected = MidiDeviceDefinition.Empty;

    public override GameSystemKind Kind => GameSystemKind.Update;

    public event EventHandler? DeviceChanged;

    public override void Deinitialize() {
        base.Deinitialize();
        this._midiOut = null;
    }

    public MidiDeviceDefinition Selected {
        get => this._selected;
        set {
            this._selected = value;
            this.DeviceChanged?.SafeInvoke(this);
        }
    }

    public override void Initialize(IScene scene) {
        base.Initialize(scene);
        this._midiDevices.Clear();
        this._midiDevices.Add(MidiDeviceDefinition.Empty);
        for (var device = 0; device < MidiOut.NumberOfDevices; device++) {
            this._midiDevices.Add(new MidiDeviceDefinition(device, MidiOut.DeviceInfo(device).ProductName));
        }

        this._selected = this._midiDevices.Last();
        this._midiOut = this._selected.Index >= 0 ? new MidiOut(this._selected.Index) : null;
    }

    public override void Update(FrameTime frameTime, InputState inputState) {
        if (this._midiOut != null) {
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