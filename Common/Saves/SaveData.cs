namespace Macabresoft.Macabre2D.Project.Common;

using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using Microsoft.Xna.Framework.Input;
using Newtonsoft.Json;

/// <summary>
/// Save data (represents a single configuration).
/// </summary>
[DataContract]
public class SaveData {

    /// <summary>
    /// The file extension for serialized <see cref="SaveData" />.
    /// </summary>
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

    /// <summary>
    /// Gets or sets the channel.
    /// </summary>
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

    /// <summary>
    /// Gets the date this save file was created.
    /// </summary>
    [DataMember]
    public DateTime CreatedDate { get; private set; } = DateTime.Now;

    public bool HasChanges { get; set; }

    /// <summary>
    /// Gets the identifier.
    /// </summary>
    [DataMember]
    public Guid Id { get; private set; } = Guid.NewGuid();

    /// <summary>
    /// Gets or sets the last date and time this was saved.
    /// </summary>
    [DataMember]
    public DateTime LastSaved { get; set; }

    /// <summary>
    /// Disables the specified button.
    /// </summary>
    /// <param name="button">The button.</param>
    public void Disable(Buttons button) {
        this.SetEnabled(button, false);
    }

    /// <summary>
    /// Enables the specified button.
    /// </summary>
    /// <param name="button">The button.</param>
    public void Enable(Buttons button) {
        this.SetEnabled(button, true);
    }

    /// <summary>
    /// Gets the file name.
    /// </summary>
    /// <returns>The file name.</returns>
    public string GetFileName() => $"Game Pad to MIDI Save Data - ({this.Id}){FileExtension}";

    /// <summary>
    /// Sets the note for a specified button.
    /// </summary>
    /// <param name="button">The button.</param>
    /// <param name="note">The note.</param>
    public void SetNote(Buttons button, int note) {
        if (MidiNoteBindingHelper.AvailableButtons.Contains(button) && this.TryGetMidiNote(button, out var midiNote)) {
            this._buttonsToMidiNote[button] = midiNote.Value.WithNote(note);
            this.HasChanges = true;
        }
    }

    /// <summary>
    /// Sets the velocity for a specified button.
    /// </summary>
    /// <param name="button">The button.</param>
    /// <param name="velocity">The velocity.</param>
    public void SetVelocity(Buttons button, int velocity) {
        if (MidiNoteBindingHelper.AvailableButtons.Contains(button) && this.TryGetMidiNote(button, out var midiNote)) {
            this._buttonsToMidiNote[button] = midiNote.Value.WithVelocity(velocity);
            this.HasChanges = true;
        }
    }

    /// <summary>
    /// Tries to get the <see cref="MidiNote" /> associated with the specified button.
    /// </summary>
    /// <param name="button">The button.</param>
    /// <param name="note">The note.</param>
    /// <returns>A value indicating whether a note was found.</returns>
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

    private void SetEnabled(Buttons button, bool isEnabled) {
        if (MidiNoteBindingHelper.AvailableButtons.Contains(button) && this.TryGetMidiNote(button, out var midiNote)) {
            this._buttonsToMidiNote[button] = midiNote.Value.WithEnabled(isEnabled);
            this.HasChanges = true;
        }
    }
}