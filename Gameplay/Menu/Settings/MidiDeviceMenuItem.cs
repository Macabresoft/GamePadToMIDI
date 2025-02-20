namespace Macabresoft.Macabre2D.Project.Gameplay;

using Macabresoft.Macabre2D.Framework;
using Macabresoft.Macabre2D.Project.Common;

public class MidiDeviceMenuItem : SelectionMenuItem {
    private readonly SelectionOption _noSelection;

    public MidiDeviceMenuItem() : base() {
        this._noSelection = new SelectionOption(MidiDeviceDefinition.Empty.Name, () => this.SetMidiDevice(MidiDeviceDefinition.Empty));
    }

    public override string ResourceName => nameof(Resources.Menu_Settings_MidiDevice);

    protected override List<SelectionOption> AvailableOptions { get; } = [];


    public override void Initialize(IScene scene, IEntity parent) {
        this.AvailableOptions.Clear();
        this.AvailableOptions.Add(this._noSelection);

        foreach (var midiDevice in scene.Game.State.MidiDevices) {
            this.AvailableOptions.Add(new SelectionOption(midiDevice.Name, () => this.SetMidiDevice(midiDevice)));
        }
        

        base.Initialize(scene, parent);

        this.SetInitialValue();
    }

    private void SetInitialValue() {
        if (this.AvailableOptions.FirstOrDefault(x => string.Equals(x.Text, this.Game.UserSettings.Custom.DeviceName)) is { } option) {
            this.InitializeSelection(option);
        }
        else {
            this.InitializeSelection(this._noSelection);
        }
    }

    private void SetMidiDevice(MidiDeviceDefinition midiDevice) {
        this.Game.UserSettings.Custom.DeviceName = midiDevice.Name;
        this.Game.State.SelectedMidiDevice = midiDevice;
        this.SetHasChanges();
    }
}