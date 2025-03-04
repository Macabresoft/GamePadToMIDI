namespace Macabresoft.Macabre2D.Project.Gameplay;

using System.ComponentModel;
using Macabresoft.Macabre2D.Framework;
using Macabresoft.Macabre2D.Project.Common;

/// <summary>
/// A menu item for changing the MIDI channel on the current configuration.
/// </summary>
public class ChannelMenuItem : SelectionMenuItem {
    private readonly List<SelectionOption> _selectionOptions = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="ChannelMenuItem" /> class.
    /// </summary>
    public ChannelMenuItem() : base() {
        for (var i = CustomSettings.MinimumChannel; i <= CustomSettings.MaximumChannel; i++) {
            var channel = i;
            this._selectionOptions.Add(new SelectionOption(i.ToString(), () => this.SetValue(channel)));
        }
    }

    /// <inheritdoc />
    public override string ResourceName => nameof(Resources.Menu_Settings_Channel);

    /// <inheritdoc />
    protected override List<SelectionOption> AvailableOptions => this._selectionOptions;

    /// <inheritdoc />
    public override void Deinitialize() {
        this.Game.State.PropertyChanged -= this.GameState_PropertyChanged;
        base.Deinitialize();
    }

    /// <inheritdoc />
    public override void Initialize(IScene scene, IEntity parent) {
        base.Initialize(scene, parent);
        this.Game.State.PropertyChanged += this.GameState_PropertyChanged;
        this.SetInitialValue();
    }

    private void GameState_PropertyChanged(object? sender, PropertyChangedEventArgs e) {
        if (e.PropertyName == nameof(GameState.CurrentSave)) {
            this.SetInitialValue();
        }
    }

    private void SetInitialValue() {
        this.InitializeSelection(this._selectionOptions[this.Game.UserSettings.Custom.Channel - 1]);
    }

    private void SetValue(int channel) {
        this.Game.UserSettings.Custom.Channel = channel;
        this.SetHasChanges();
    }
}