namespace Macabresoft.Macabre2D.Project.Gameplay;

using Macabresoft.Macabre2D.Framework;
using Macabresoft.Macabre2D.Project.Common;
using Microsoft.Xna.Framework;

public interface IMenuItem : IDockable, IEntity {
    bool CanFocus { get; }
    string ConfirmPromptResourceName { get; }
    string ExtraPromptResourceName { get; }
    bool IsFocused { get; }
    bool IsSecondaryExecuteAvailable { get; }
    void Activate();
    void Click(Vector2 startPosition, Vector2 endPosition, bool isHold);
    void Focus();
    bool HandleInput(FrameTime frameTime, IInputSystem input);
    bool IsHoveringOver(Vector2 position);
    void RemoveFocus();
}

public abstract class MenuItem : DockableWrapper, IMenuItem {
    public static readonly IMenuItem EmptyInstance = new EmptyMenuItem();

    public virtual bool CanFocus => true;

    public virtual string ConfirmPromptResourceName => nameof(Resources.Menu_Prompts_Confirm);

    public virtual string ExtraPromptResourceName => string.Empty;

    public virtual bool IsSecondaryExecuteAvailable => false;

    public abstract string ResourceName { get; }

    public bool IsFocused { get; private set; }

    public virtual void Activate() {
    }

    public virtual void Click(Vector2 startPosition, Vector2 endPosition, bool isHold) {
        if (this.BoundingArea.Contains(startPosition) && this.BoundingArea.Contains(endPosition)) {
            this.Execute();
        }
    }

    public virtual void Focus() {
        this.IsFocused = true;
    }

    public virtual bool HandleInput(FrameTime frameTime, IInputSystem input) {
        if (input.IsPressed(InputAction.Confirm, InputKind.GamePad | InputKind.Keyboard)) {
            this.Execute();
            return true;
        }

        return false;
    }

    public override void Initialize(IScene scene, IEntity parent) {
        base.Initialize(scene, parent);

        foreach (var textLine in this.GetDescendants<MenuTextLineRenderer>()) {
            textLine.Color = Color.White;
        }
    }

    public virtual bool IsHoveringOver(Vector2 position) => this.BoundingArea.Contains(position);

    public virtual void RemoveFocus() {
        this.IsFocused = false;
    }

    protected abstract void Execute();

    protected virtual void SecondaryExecute() {
    }

    private class EmptyMenuItem : EmptyObject, IMenuItem {
        public bool CanFocus => false;
        public string ConfirmPromptResourceName => string.Empty;
        public string ExtraPromptResourceName => string.Empty;
        public bool IsFocused => false;
        public bool IsSecondaryExecuteAvailable => false;

        public DockLocation Location {
            get => DockLocation.None;
            set { }
        }

        public void Activate() {
        }

        public void Click(Vector2 startPosition, Vector2 endPosition, bool isHold) {
        }

        public void Focus() {
        }

        public bool HandleInput(FrameTime frameTime, IInputSystem input) => false;

        public bool IsHoveringOver(Vector2 position) => false;

        public void RemoveFocus() {
        }
    }
}