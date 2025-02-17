namespace Macabresoft.Macabre2D.Project.Gameplay;

public class SelectionOption {
    private readonly Action? _action;

    public SelectionOption(string resourceName, Action? selectedAction) {
        this.ResourceName = resourceName;
        this._action = selectedAction;
    }

    public string ResourceName { get; }

    public void Select() {
        this._action?.Invoke();
    }
}