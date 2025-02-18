namespace Macabresoft.Macabre2D.Project.Common;

using System.Runtime.Serialization;

/// <summary>
/// Settings custom to this specific project.
/// </summary>
[DataContract]
public class CustomSettings {
    /// <summary>
    /// Gets or sets the name of the MIDI device being used.
    /// </summary>
    [DataMember]
    public string DeviceName { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the identifier of the current save.
    /// </summary>
    [DataMember]
    public Guid CurrentSave { get; set; }

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