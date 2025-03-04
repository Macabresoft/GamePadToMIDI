namespace Macabresoft.Macabre2D.Project.Gameplay;

using System.ComponentModel;
using Macabresoft.Macabre2D.Framework;
using Macabresoft.Macabre2D.Project.Common;

/// <summary>
/// Renders the name of the selected MIDI device.
/// </summary>
public class MidiDeviceRenderer : TextLineRenderer {

    /// <inheritdoc />
    public override void Deinitialize() {
        this.Game.State.PropertyChanged -= this.GameState_PropertyChanged;
        base.Deinitialize();
    }

    /// <inheritdoc />
    public override string GetFullText() {
        var text = "Please Select a Device in Settings";
        if (this.Game.State.SelectedMidiDevice is { IsEmpty: false } midiDevice && !string.IsNullOrEmpty(midiDevice.Name)) {
            text = $"Device: {midiDevice.Name}";
        }

        return text;
    }

    /// <inheritdoc />
    public override void Initialize(IScene scene, IEntity parent) {
        base.Initialize(scene, parent);

        this.FontCategory = FontCategory.Normal;
        this.Game.State.PropertyChanged += this.GameState_PropertyChanged;
    }

    private void GameState_PropertyChanged(object? sender, PropertyChangedEventArgs e) {
        if (e.PropertyName == nameof(GameState.SelectedMidiDevice)) {
            this.OnTransformChanged();
        }
    }
}