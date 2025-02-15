namespace GamepadToMidi.UI.Common;

using Macabresoft.Core;
using NAudio.Midi;

/// <summary>
/// Interface for interacting with the virtual midi device.
/// </summary>
public interface IMidiDeviceService {

    /// <summary>
    /// Gets the available devices.
    /// </summary>
    IReadOnlyCollection<MidiDeviceDefinition> MidiDevices { get; }

    /// <summary>
    /// Gets or sets the selected device.
    /// </summary>
    MidiDeviceDefinition Selected { get; set;  }

    /// <summary>
    /// Sends a note.
    /// </summary>
    void SendNote();

    /// <summary>
    /// Stops a note.
    /// </summary>
    void StopNote();
}

/// <summary>
/// Service for interacting with the virtual midi device.
/// </summary>
public class MidiDeviceService : PropertyChangedNotifier, IMidiDeviceService {
    private readonly ObservableCollectionExtended<MidiDeviceDefinition> _midiDevices = new();
    private MidiDeviceDefinition _selected = MidiDeviceDefinition.Empty;
    private MidiOut? _midiOut;

    public MidiDeviceService() {
        this._midiDevices.Add(MidiDeviceDefinition.Empty);
        for (var device = 0; device < MidiOut.NumberOfDevices; device++) {
            this._midiDevices.Add(new MidiDeviceDefinition(device, MidiOut.DeviceInfo(device).ProductName));
        }
    }

    /// <inheritdoc />
    public IReadOnlyCollection<MidiDeviceDefinition> MidiDevices => this._midiDevices;

    /// <inheritdoc />
    public MidiDeviceDefinition Selected {
        get => this._selected;
        set {
            if (this.Set(ref this._selected, value)) {
                this._midiOut?.Dispose();
                this._midiOut = this._selected.Index >= 0 ? new MidiOut(this._selected.Index) : null;
            }
        }
    }

    public void SendNote() {
        if (this._midiOut != null) {
            var noteOnEvent = new NoteOnEvent(0, 1, 36, 100, 50);
            this._midiOut.Send(noteOnEvent.GetAsShortMessage());
        }

    }

    public void StopNote() {
        //throw new NotImplementedException();
    }
}