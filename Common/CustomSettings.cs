namespace Macabresoft.Macabre2D.Project.Common;

using System.Runtime.Serialization;

/// <summary>
/// Settings custom to this specific project.
/// </summary>
[DataContract]
public class CustomSettings {
    /// <summary>
    /// The maximum MIDI channel.
    /// </summary>
    public const int MaximumChannel = 16;

    /// <summary>
    /// The minimum MIDI channel.
    /// </summary>
    public const int MinimumChannel = 1;

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
            }
        }
    }

    /// <summary>
    /// Gets or sets the identifier of the current save.
    /// </summary>
    [DataMember]
    public Guid CurrentSave { get; set; }

    /// <summary>
    /// Gets or sets the name of the MIDI device being used.
    /// </summary>
    [DataMember]
    public string DeviceName { get; set; } = string.Empty;

    /// <summary>
    /// Clones this instance.
    /// </summary>
    /// <remarks>This method is required by Macabre2D.</remarks>
    /// <returns>The cloned settings.</returns>
    public CustomSettings Clone() =>
        new() {
            DeviceName = this.DeviceName
        };

    /// <summary>
    /// Copies settings to another instance.
    /// </summary>
    /// <remarks>This method is required by Macabre2D.</remarks>
    /// <param name="other">The other instance.</param>
    public void CopyTo(CustomSettings other) {
        other.DeviceName = this.DeviceName;
    }
}