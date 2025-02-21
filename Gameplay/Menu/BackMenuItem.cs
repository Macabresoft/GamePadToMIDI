namespace Macabresoft.Macabre2D.Project.Gameplay;

using Macabresoft.Macabre2D.Project.Common;

public class BackMenuItem : MenuItem {
    public override string ResourceName => nameof(Resources.Menu_Return);

    protected override void Execute() {
        if (this.Scene.GetSystem<MenuSystem>() is { } menuSystem) {
            menuSystem.PopMenu();
        }
    }
}