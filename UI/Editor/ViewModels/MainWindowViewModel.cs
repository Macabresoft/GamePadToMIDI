namespace Macabresoft.GamepadToMidi.UI.Editor;

using global::GamepadToMidi.UI.Common;
using Macabresoft.AvaloniaEx;
using Unity;

public class MainWindowViewModel : UndoBaseViewModel {

    public MainWindowViewModel() : this(Resolver.Resolve<IMidiDeviceService>()) {
    }

    [InjectionConstructor]
    public MainWindowViewModel(IMidiDeviceService midiDeviceService) {
        this.MidiService = midiDeviceService;
    }

    public string Greeting => "Hit [space] to send a note!";

    public IMidiDeviceService MidiService { get; }

    public void SendNote() {
        this.MidiService.SendNote();
    }

    public void StopNote() {
        this.MidiService.StopNote();
    }
}