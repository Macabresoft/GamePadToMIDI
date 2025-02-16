namespace Macabresoft.Macabre2D.Project.Gameplay.MIDI;

public record struct MidiDeviceDefinition(int Index, string Name) {
    public static MidiDeviceDefinition Empty { get; } = new(-1, "--");
}