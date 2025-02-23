namespace Macabresoft.Macabre2D.Project.Gameplay;

using Macabresoft.Macabre2D.Framework;
using Macabresoft.Macabre2D.Project.Common;

public class ChannelMenuItem : SelectionMenuItem {
    private readonly List<SelectionOption> _selectionOptions = [];

    public ChannelMenuItem() : base() {
        for (var i = SaveData.MinimumChannel; i <= SaveData.MaximumChannel; i++) {
            var channel = i;
            this._selectionOptions.Add(new SelectionOption(i.ToString(), () => this.SetValue(channel)));
        }
    }

    public override string ResourceName => nameof(Resources.Menu_Settings_Channel);

    protected override List<SelectionOption> AvailableOptions => this._selectionOptions;

    public override void Initialize(IScene scene, IEntity parent) {
        base.Initialize(scene, parent);
        this.SetInitialValue();
    }

    private void SetInitialValue() {
        this.InitializeSelection(this._selectionOptions[this.Game.State.CurrentSave.Channel - 1]);
    }

    private void SetValue(int channel) {
        this.Game.State.CurrentSave.Channel = channel;
        this.SetHasChanges();
    }
}