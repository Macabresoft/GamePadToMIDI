namespace Macabresoft.Macabre2D.Project.Gameplay;

using Macabresoft.Macabre2D.Project.Common;

/// <summary>
/// A menu item to delete the current save.
/// </summary>
public class DeleteSaveMenuItem : MenuItem {
    /// <inheritdoc />
    public override bool CanFocus => this.Game.State.CanDeleteCurrentSave;

    /// <inheritdoc />
    public override string ResourceName => nameof(Resources.Menu_Settings_Delete);

    /// <inheritdoc />
    protected override void Execute() {
        // TODO: dialog
        this.Game.State.DeleteCurrent();
    }
}