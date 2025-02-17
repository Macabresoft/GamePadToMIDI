namespace Macabresoft.Macabre2D.Project.Common;

using System.Runtime.Serialization;

[DataContract]
public class SaveData {
    public const string FileExtension = ".gptomidi";

    [DataMember]
    public Guid Id { get; set; } = Guid.NewGuid();

    [DataMember]
    public DateTime LastSaved { get; set; }

    public string GetFileName() => $"Game Pad to MIDI Save Data - ({this.Id}){FileExtension}";
}