namespace Macabresoft.GamepadToMidi.UI.Editor;

using Avalonia.Input;
using global::GamepadToMidi.UI.Common;
using Macabresoft.AvaloniaEx;

public partial class MainWindow : BaseDialog {
    private readonly MainWindowViewModel _viewModel;

    public MainWindow() {
        this.InitializeComponent();
        this._viewModel = Resolver.Resolve<MainWindowViewModel>();
        this.DataContext = this._viewModel;
    }

    private void InputElement_OnKeyDown(object? sender, KeyEventArgs e) {
        if (e.Key == Key.Space) {
            this._viewModel.SendNote();
        }
    }

    private void InputElement_OnKeyUp(object? sender, KeyEventArgs e) {
        if (e.Key == Key.Space) {
            this._viewModel.StopNote();
        }
    }
}