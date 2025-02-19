namespace Macabresoft.Macabre2D.Project.Gameplay;

public class SelectionOption {
    private readonly Action? _action;

    public SelectionOption(string text, Action? selectedAction) {
        this.Text = text;
        this._action = selectedAction;
    }

    public string Text { get; }

    public void Select() {
        this._action?.Invoke();
    }
}