namespace Macabresoft.Macabre2D.Project.Common;

using System.Runtime.Serialization;

/// <summary>
/// Settings custom to this specific project.
/// </summary>
[DataContract]
public class CustomSettings {
    /// <summary>
    /// Gets or sets the user who owns these settings.
    /// </summary>
    /// <remarks>
    /// This is an example setting and may be removed without consequence.
    /// </remarks>
    [DataMember]
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    /// Clones this instance.
    /// </summary>
    /// <remarks>This method is required by Macabre2D.</remarks>
    /// <returns>The cloned settings.</returns>
    public CustomSettings Clone() {
        return new CustomSettings {
            UserName = this.UserName
        };
    }

    /// <summary>
    /// Copies settings to another instance.
    /// </summary>
    /// <remarks>This method is required by Macabre2D.</remarks>
    /// <param name="other">The other instance.</param>
    public void CopyTo(CustomSettings other) {
        other.UserName = this.UserName;
    }
}