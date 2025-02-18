namespace Macabresoft.Macabre2D.Project.Common;

using System.Runtime.Serialization;

/// <summary>
/// Represents a MIDI note definition with both the note and velocity.
/// </summary>
[DataContract]
public readonly struct MidiNote {
    /// <summary>
    /// Initializes a new instance of the <see cref="MidiNote" /> class.
    /// </summary>
    /// <param name="note">The note.</param>
    /// <param name="velocity">The velocity</param>
    public MidiNote(int note, int velocity) {
        this.Note = Math.Clamp(note, 0, 127);
        this.Velocity = Math.Clamp(velocity, 0, 100);
        this.IsEnabled = this.Velocity > 0;
    }

    /// <summary>
    /// The note.
    /// </summary>
    [DataMember]
    public readonly int Note;

    /// <summary>
    /// A value indicating whether this is enabled.
    /// </summary>
    [DataMember]
    public readonly bool IsEnabled;

    /// <summary>
    /// The velocity.
    /// </summary>
    [DataMember]
    public readonly int Velocity;

    /// <summary>
    /// Returns a new <see cref="MidiNote" /> with the current note, but a new velocity.
    /// </summary>
    /// <param name="velocity">The velocity.</param>
    /// <returns>A new <see cref="MidiNote" />.</returns>
    public MidiNote WithVelocity(int velocity) => new(this.Note, velocity);

    /// <summary>
    /// Returns a new <see cref="MidiNote" /> with the current velocity, but a new note.
    /// </summary>
    /// <param name="note">The note.</param>
    /// <returns>A new <see cref="MidiNote" />.</returns>
    public MidiNote WithNote(int note) => new(note, this.Velocity);
}