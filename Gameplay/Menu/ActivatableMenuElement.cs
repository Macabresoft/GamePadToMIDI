namespace Macabresoft.Macabre2D.Project.Gameplay;

using Macabresoft.Macabre2D.Framework;

public interface IActivatableMenuElement {
    void Activate();
    void Deactivate();
}

public class ActivatableMenuElement : DockableWrapper, IActivatableMenuElement {
    public virtual void Activate() {
        this.IsEnabled = true;

        if (this is IRenderableEntity renderableThis) {
            renderableThis.ShouldRender = true;
        }

        if (!BaseGame.IsDesignMode) {
            foreach (var enableable in this.GetDescendants<IUpdateableEntity>()) {
                enableable.IsEnabled = true;
            }

            foreach (var renderable in this.GetDescendants<IRenderableEntity>()) {
                renderable.ShouldRender = true;
            }
        }
    }

    public virtual void Deactivate() {
        this.IsEnabled = false;

        if (this is IRenderableEntity renderableThis) {
            renderableThis.ShouldRender = false;
        }

        if (!BaseGame.IsDesignMode) {
            foreach (var enableable in this.GetDescendants<IUpdateableEntity>()) {
                enableable.IsEnabled = false;
            }

            foreach (var renderable in this.GetDescendants<IRenderableEntity>()) {
                renderable.ShouldRender = false;
            }
        }
    }
}