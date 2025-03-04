namespace Macabresoft.Macabre2D.Project.Gameplay;

using Macabresoft.Macabre2D.Project.Common;

/// <summary>
/// A menu item to create a new save configuration.
/// </summary>
public class CreateSaveMenuItem : MenuItem {

    /// <inheritdoc />
    public override string ResourceName => nameof(Resources.Menu_Settings_NewConfiguration);

    /// <inheritdoc />
    protected override void Execute() {
        this.Game.State.CreateNew();
    }
}