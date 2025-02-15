using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;

namespace Macabresoft.GamepadToMidi.UI.Editor;

using global::GamepadToMidi.UI.Common;
using Macabresoft.AvaloniaEx;
using Unity;

public partial class App : Application {
    public override void Initialize() {
        Resolver.Container
            .AddNewExtension<AvaloniaUnityContainerExtension>()
            .AddNewExtension<CommonContainerExtension>();
        
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted() {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop) {
            desktop.MainWindow = new MainWindow();
        }

        base.OnFrameworkInitializationCompleted();
    }
}