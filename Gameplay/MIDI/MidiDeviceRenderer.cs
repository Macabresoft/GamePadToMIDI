namespace Macabresoft.Macabre2D.Project.Gameplay;

using Macabresoft.Macabre2D.Framework;
using Macabresoft.Macabre2D.Project.Common;

/// <summary>
/// Renders the name of the selected MIDI device.
/// </summary>
public class MidiDeviceRenderer : TextLineRenderer {
    private MidiSystem? _midiSystem;

    /// <inheritdoc />
    public override void Deinitialize() {
        base.Deinitialize();
        
        if (this._midiSystem != null) {
            this._midiSystem.DeviceChanged -= this.MidiSystem_DeviceChanged;
            this._midiSystem = null;
        }
    }

    /// <inheritdoc />
    public override void Initialize(IScene scene, IEntity parent) {
        base.Initialize(scene, parent);

        this.FontCategory = FontCategory.Normal;
        this._midiSystem = this.Scene.GetSystem<MidiSystem>();
        if (this._midiSystem != null) {
            this._midiSystem.DeviceChanged += this.MidiSystem_DeviceChanged;
        }
    }

    private void MidiSystem_DeviceChanged(object? sender, EventArgs e) {
        this.ResetSize();
    }

    /// <inheritdoc />
    public override string GetFullText() {
        var text = "N/A";
        if (this._midiSystem is { Selected.IsEmpty: false }) {
            text = $"Device: {this._midiSystem.Selected.Name}";
        }
        
        return text;
    }
}