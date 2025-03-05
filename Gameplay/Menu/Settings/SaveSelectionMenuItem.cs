namespace Macabresoft.Macabre2D.Project.Gameplay;

using System.ComponentModel;
using Macabresoft.Macabre2D.Framework;
using Macabresoft.Macabre2D.Project.Common;

/// <summary>
/// Menu item to select a save.
/// </summary>
public class SaveSelectionMenuItem : SelectionMenuItem {

    /// <inheritdoc />
    public override string ResourceName => nameof(Resources.Menu_Settings_Current);

    /// <inheritdoc />
    protected override List<SelectionOption> AvailableOptions { get; } = [];

    /// <inheritdoc />
    public override void Deinitialize() {
        this.Game.State.PropertyChanged -= this.GameState_PropertyChanged;
        base.Deinitialize();
    }

    /// <inheritdoc />
    public override void Initialize(IScene scene, IEntity parent) {
        this.ReinitializeOptions(scene.Game);
        base.Initialize(scene, parent);
        this.SetInitialSelection();
        this.Game.State.PropertyChanged += this.GameState_PropertyChanged;
    }

    private void GameState_PropertyChanged(object? sender, PropertyChangedEventArgs e) {
        if (e.PropertyName == nameof(GameState.ExistingSaves)) {
            this.ReinitializeOptions(this.Game);
        }
    }

    private void ReinitializeOptions(IGame game) {
        this.AvailableOptions.Clear();

        for (var i = 0; i < game.State.ExistingSaves.Count; i++) {
            var save = game.State.ExistingSaves.ElementAt(i);
            var option = new SelectionOption($"{Resources.Menu_Settings_Configuration} {i + 1}", () => this.SetValue(save));
            this.AvailableOptions.Add(option);
        }

        if (this.IsInitialized) {
            this.SetInitialSelection();
        }
    }

    private void SetInitialSelection() {
        for (var i = 0; i < this.Game.State.ExistingSaves.Count; i++) {
            if (this.Game.State.ExistingSaves.ElementAt(i).Id == this.Game.State.CurrentSave.Id) {
                this.InitializeSelection(this.AvailableOptions[i]);
                break;
            }
        }
    }

    private void SetValue(SaveData save) {
        this.Game.State.CurrentSave = save;
    }
}