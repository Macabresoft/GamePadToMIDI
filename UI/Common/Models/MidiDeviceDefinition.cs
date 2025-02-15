namespace GamepadToMidi.UI.Common;

public record struct MidiDeviceDefinition(int Index, string Name) {
    public static MidiDeviceDefinition Empty { get; } = new(-1, "<< NOTHING >>");
}