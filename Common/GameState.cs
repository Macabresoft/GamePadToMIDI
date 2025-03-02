namespace Macabresoft.Macabre2D.Project.Common;

using Macabresoft.Core;
using Macabresoft.Macabre2D.Common;
using NAudio.Midi;

/// <summary>
/// Events that can be raised to the game.
/// </summary>
public enum GameAction {
    Shutdown,
    SaveSettings,
    SaveAndApplySettings
}

/// <summary>
/// A class accessible via the containing game that can include any information about the currently running game that might be relevant in the current scene or across scenes.
/// </summary>
public class GameState {
    private readonly List<SaveData> _existingSaves = new();

    private readonly List<MidiDeviceDefinition> _midiDevices = new();
    private IDataManager _dataManager = EmptyDataManager.Instance;
    private bool _isBusy;
    private MidiDeviceDefinition _selectedMidiDevice = MidiDeviceDefinition.Empty;

    /// <summary>
    /// An event that is called when it is requested that the game should perform a specific action.
    /// </summary>
    public event EventHandler<GameAction>? ActionRequested;

    /// <summary>
    /// Raised when <see cref="SelectedMidiDevice" /> changes.
    /// </summary>
    public event EventHandler? MidiDeviceChanged;

    /// <summary>
    /// Gets the current save data.
    /// </summary>
    public SaveData CurrentSave { get; private set; } = new();

    /// <summary>
    /// Gets existing <see cref="SaveData" />.
    /// </summary>
    public IReadOnlyCollection<SaveData> ExistingSaves => this._existingSaves;

    /// <summary>
    /// Gets the available MIDI devices.
    /// </summary>
    public IReadOnlyCollection<MidiDeviceDefinition> MidiDevices => this._midiDevices;

    /// <summary>
    /// Gets or sets the selected MIDI device.
    /// </summary>
    public MidiDeviceDefinition SelectedMidiDevice {
        get => this._selectedMidiDevice;
        set {
            this._selectedMidiDevice = value;
            this.MidiDeviceChanged.SafeInvoke(this);
        }
    }

    /// <summary>
    /// Creates a new save file and saves it.
    /// </summary>
    public void CreateNew() {
        this.Load(new SaveData());
        this.Save();
    }

    /// <summary>
    /// Initializes this instance.
    /// </summary>
    /// <param name="dataManager">The data manager.</param>
    /// <param name="customSettings">The custom settings.</param>
    public void Initialize(IDataManager dataManager, CustomSettings customSettings) {
        this._dataManager = dataManager;

        try {
            this._isBusy = true;
            foreach (var file in dataManager.GetFiles(SaveData.FileExtension)) {
                try {
                    var saveData = Serializer.Instance.Deserialize<SaveData>(file);
                    this._existingSaves.Add(saveData);
                }
                catch {
                    // TODO: Log this somehow?
                }
            }
        }
        finally {
            this._isBusy = false;
        }

        SaveData? currentSave = null;
        if (this._existingSaves.Any()) {
            this._existingSaves.Sort((x, y) => -DateTime.Compare(x.CreatedDate, y.CreatedDate));
            currentSave = this._existingSaves.FirstOrDefault(x => x.Id == customSettings.CurrentSave);

            if (currentSave != null) {
                this.CurrentSave = currentSave;
            }
            else {
                currentSave = this._existingSaves.OrderByDescending(x => x.LastSaved).FirstOrDefault();
            }
        }

        if (currentSave != null) {
            this.CurrentSave = currentSave;
        }
        else {
            this.CreateNew();
        }

        this._midiDevices.Clear();

        if (MidiOut.NumberOfDevices > 0) {
            for (var device = 0; device < MidiOut.NumberOfDevices; device++) {
                this._midiDevices.Add(new MidiDeviceDefinition(device, MidiOut.DeviceInfo(device).ProductName));
            }

            if (this._midiDevices.FirstOrDefault(x => string.Equals(customSettings.DeviceName, x.Name)) is { IsEmpty: false } midiDevice) {
                this.SelectedMidiDevice = midiDevice;
            }
            else {
                this.SelectedMidiDevice = this._midiDevices.FirstOrDefault();
            }
        }
    }

    /// <summary>
    /// Loads the specified save data.
    /// </summary>
    /// <param name="saveData">The save data.</param>
    public void Load(SaveData saveData) {
        if (this.CurrentSave != saveData) {
            this.CurrentSave = saveData;

            if (!this._existingSaves.Contains(saveData)) {
                this._existingSaves.Add(saveData);
            }
        }
    }

    /// <summary>
    /// Raises an event which requests an action be taken by the game.
    /// </summary>
    /// <param name="action">The action.</param>
    public void RaiseActionRequested(GameAction action) {
        this.ActionRequested.SafeInvoke(this, action);
    }

    /// <summary>
    /// Saves the current save data, provided it isn't busy doing a save or a load already.
    /// </summary>
    public void Save() {
        if (!this._isBusy) {
            try {
                this._isBusy = true;
                foreach (var saveData in this._existingSaves.Where(x => x.HasChanges)) {
                    saveData.LastSaved = DateTime.Now;
                    saveData.HasChanges = false;
                    Serializer.Instance.Serialize(saveData, this.GetCurrentSaveFilePath(saveData));
                }
            }
            finally {
                this._isBusy = false;
            }
        }
    }

    private string GetCurrentSaveFilePath(SaveData saveData) => Path.Combine(this._dataManager.GetPathToDataDirectory(), saveData.GetFileName());
}