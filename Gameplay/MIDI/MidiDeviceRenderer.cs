namespace Macabresoft.Macabre2D.Project.Gameplay;

using Macabresoft.Macabre2D.Framework;
using Macabresoft.Macabre2D.Project.Common;

/// <summary>
/// Renders the name of the selected MIDI device.
/// </summary>
public class MidiDeviceRenderer : TextLineRenderer {

    /// <inheritdoc />
    public override void Deinitialize() {
        this.Game.State.MidiDeviceChanged -= this.GameState_MidiDeviceChanged;
        base.Deinitialize();
    }

    /// <inheritdoc />
    public override string GetFullText() {
        var text = "Please Select a Device in Settings";
        if (this.Game.State.SelectedMidiDevice is { IsEmpty: false } midiDevice) {
            text = $"Device: {midiDevice.Name}";
        }

        return text;
    }

    /// <inheritdoc />
    public override void Initialize(IScene scene, IEntity parent) {
        base.Initialize(scene, parent);

        this.FontCategory = FontCategory.Normal;
        this.Game.State.MidiDeviceChanged += this.GameState_MidiDeviceChanged;
    }

    private void GameState_MidiDeviceChanged(object? sender, EventArgs e) {
        this.OnTransformChanged();
    }
}