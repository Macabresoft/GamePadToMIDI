namespace GamepadToMidi.UI.Common;

using Unity;
using Unity.Extension;
using Unity.Lifetime;

/// <summary>
/// Registers services and instances to a <see cref="IUnityContainer" />.
/// </summary>
public class CommonContainerExtension : UnityContainerExtension {

    /// <inheritdoc />
    protected override void Initialize() {
        this.Container.RegisterType<IMidiDeviceService, MidiDeviceService>(new SingletonLifetimeManager());
    }
}