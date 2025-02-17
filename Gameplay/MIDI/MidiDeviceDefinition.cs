namespace Macabresoft.Macabre2D.Project.Gameplay;

public readonly record struct MidiDeviceDefinition(int Index, string Name) {
    public static MidiDeviceDefinition Empty { get; } = new(-1, "--");
    public bool IsEmpty => this.Index < 0;
}