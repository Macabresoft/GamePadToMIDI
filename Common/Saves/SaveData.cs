namespace Macabresoft.Macabre2D.Project.Common;

using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using Microsoft.Xna.Framework.Input;
using Newtonsoft.Json;

[DataContract]
public class SaveData {
    /// <summary>
    /// The minimum MIDI channel.
    /// </summary>
    public const int MinimumChannel = 1;
    
    /// <summary>
    /// The maximum MIDI channel.
    /// </summary>
    public const int MaximumChannel = 16;

    public const string FileExtension = ".gptomidi";

    [JsonProperty(ObjectCreationHandling = ObjectCreationHandling.Replace)]
    private readonly Dictionary<Buttons, MidiNote> _buttonsToMidiNote = [];

    [DataMember]
    public Guid Id { get; private set; } = Guid.NewGuid();

    [DataMember]
    public DateTime LastSaved { get; set; }

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

    public string GetFileName() => $"Game Pad to MIDI Save Data - ({this.Id}){FileExtension}";

    public void SetNote(Buttons button, MidiNote note) {
        if (MidiNoteBindingHelper.AvailableButtons.Contains(button)) {
            this._buttonsToMidiNote[button] = note;
            this.HasChanges = true;
        }
    }

    public bool TryGetMidiNote(Buttons button, [NotNullWhen(true)] out MidiNote? note) {
        if (this._buttonsToMidiNote.TryGetValue(button, out var foundNote)) {
            note = foundNote;
            return true;
        }

        note = null;
        return false;
    }
}