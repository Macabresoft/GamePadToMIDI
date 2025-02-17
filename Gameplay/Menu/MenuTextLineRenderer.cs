namespace Macabresoft.Macabre2D.Project.Gameplay;

using Macabresoft.Macabre2D.Framework;

public class MenuTextLineRenderer : TextLineRenderer {
    public override void Initialize(IScene scene, IEntity parent) {
        base.Initialize(scene, parent);
        
        // Might be a bug, but we need to initialize the font reference again.
        // Need to make font category more robust to handle this.
        this.FontReference.Initialize(this.Scene.Assets, this.Game);
    }

    public override void Render(FrameTime frameTime, BoundingArea viewBoundingArea) {
        this.Render(frameTime, viewBoundingArea, this.GetMenuItemColor());
    }
}