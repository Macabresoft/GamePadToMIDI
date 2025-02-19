namespace Macabresoft.Macabre2D.Project.Common;

using System.Runtime.Serialization;
using Macabresoft.Macabre2D.Common.Attributes;

[DataContract]
public class ProjectConfiguration {
    /// <summary>
    /// Gets the scene identifier for the pause screen.
    /// </summary>
    [DataMember(Order = 10)]
    [SceneGuid]
    public Guid PauseSceneId { get; private set; }
}