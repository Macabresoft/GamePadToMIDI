namespace Macabresoft.Macabre2D.Project.Common;

using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using Microsoft.Xna.Framework.Input;
using Newtonsoft.Json;

[DataContract]
public class SaveData {

    public const string FileExtension = ".gptomidi";

    /// <summary>
    /// The maximum MIDI channel.
    /// </summary>
    public const int MaximumChannel = 16;

    /// <summary>
    /// The minimum MIDI channel.
    /// </summary>
    public const int MinimumChannel = 1;

    [JsonProperty(ObjectCreationHandling = ObjectCreationHandling.Replace)]
    private readonly Dictionary<Buttons, MidiNote> _buttonsToMidiNote = [];

    private int _channel = 1;

    [DataMember]
    public int Channel {
        get => this._channel;
        set {
            if (this._channel != value) {
                this._channel = Math.Clamp(value, MinimumChannel, MaximumChannel);
                this.HasChanges = true;
            }
        }
    }

    public bool HasChanges { get; set; }

    [DataMember]
    public Guid Id { get; private set; } = Guid.NewGuid();

    [DataMember]
    public DateTime LastSaved { get; set; }

    public string GetFileName() => $"Game Pad to MIDI Save Data - ({this.Id}){FileExtension}";

    public void SetEnabled(Buttons button, bool isEnabled) {
        if (MidiNoteBindingHelper.AvailableButtons.Contains(button) && this.TryGetMidiNote(button, out var midiNote)) {
            this._buttonsToMidiNote[button] = midiNote.Value.WithEnabled(isEnabled);
            this.HasChanges = true;
        }
    }

    public void SetNote(Buttons button, int note) {
        if (MidiNoteBindingHelper.AvailableButtons.Contains(button) && this.TryGetMidiNote(button, out var midiNote)) {
            this._buttonsToMidiNote[button] = midiNote.Value.WithNote(note);
            this.HasChanges = true;
        }
    }

    public void SetVelocity(Buttons button, int velocity) {
        if (MidiNoteBindingHelper.AvailableButtons.Contains(button) && this.TryGetMidiNote(button, out var midiNote)) {
            this._buttonsToMidiNote[button] = midiNote.Value.WithVelocity(velocity);
            this.HasChanges = true;
        }
    }

    public bool TryGetMidiNote(Buttons button, [NotNullWhen(true)] out MidiNote? note) {
        if (MidiNoteBindingHelper.AvailableButtons.Contains(button)) {
            if (this._buttonsToMidiNote.TryGetValue(button, out var foundNote)) {
                note = foundNote;
            }
            else {
                note = new MidiNote(0, 100, true);
                this._buttonsToMidiNote[button] = note.Value;
                this.HasChanges = true;
            }

            return true;
        }

        note = null;
        return false;
    }
}