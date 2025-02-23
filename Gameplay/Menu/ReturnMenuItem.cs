namespace Macabresoft.Macabre2D.Project.Gameplay;

using Macabresoft.Macabre2D.Project.Common;

/// <summary>
/// A menu item that returns to the previous screen.
/// </summary>
public class ReturnMenuItem : MenuItem {
    /// <inheritdoc />
    public override string ResourceName => nameof(Resources.Menu_Return);

    /// <inheritdoc />
    protected override void Execute() {
        if (this.Scene.GetSystem<MenuSystem>() is { } menuSystem) {
            menuSystem.PopMenu();
        }
    }
}