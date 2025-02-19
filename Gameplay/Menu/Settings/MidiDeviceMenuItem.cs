namespace Macabresoft.Macabre2D.Project.Gameplay;

using Macabresoft.Macabre2D.Framework;
using Macabresoft.Macabre2D.Project.Common;

public class MidiDeviceMenuItem : SelectionMenuItem {
    private readonly SelectionOption _noSelection;
    private MidiSystem? _midiSystem;

    public MidiDeviceMenuItem() : base() {
        this._noSelection = new SelectionOption(MidiDeviceDefinition.Empty.Name, () => this.SetMidiDevice(MidiDeviceDefinition.Empty));
    }

    public override string ResourceName => nameof(Resources.Menu_Settings_MidiDevice);

    protected override List<SelectionOption> AvailableOptions { get; } = [];

    public override void Deinitialize() {
        base.Deinitialize();
        this._midiSystem = null;
    }

    public override void Initialize(IScene scene, IEntity parent) {
        this._midiSystem = scene.GetSystem<MidiSystem>();
        this.AvailableOptions.Clear();
        this.AvailableOptions.Add(this._noSelection);

        if (this._midiSystem != null) {
            foreach (var midiDevice in this._midiSystem.MidiDevices) {
                this.AvailableOptions.Add(new SelectionOption(midiDevice.Name, () => this.SetMidiDevice(midiDevice)));
            }
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
        if (this._midiSystem != null) {
            this._midiSystem.Selected = midiDevice;
        }

        this.Game.UserSettings.Custom.DeviceName = midiDevice.Name;
        this.SetHasChanges();
    }
}