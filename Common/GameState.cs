namespace Macabresoft.Macabre2D.Project.Common;

using Macabresoft.Core;
using Macabresoft.Macabre2D.Common;

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
    private IDataManager _dataManager = EmptyDataManager.Instance;
    private bool _isBusy;

    /// <summary>
    /// An event that is called when it is requested that the game should perform a specific action.
    /// </summary>
    public event EventHandler<GameAction>? ActionRequested;

    /// <summary>
    /// Gets the current save data.
    /// </summary>
    public SaveData CurrentSave { get; private set; } = new();

    /// <summary>
    /// Gets existing <see cref="SaveData" />.
    /// </summary>
    public IReadOnlyCollection<SaveData> ExistingSaves => this._existingSaves;

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
            currentSave = this._existingSaves.FirstOrDefault(x => x.Id == customSettings.CurrentSave);

            if (currentSave != null) {
                this.CurrentSave = currentSave;
            }
            else {
                this._existingSaves.Sort((x, y) => DateTime.Compare(x.LastSaved, y.LastSaved));
                currentSave = this._existingSaves.FirstOrDefault();
            }
        }

        if (currentSave != null) {
            this.CurrentSave = currentSave;
        }
        else {
            this.CreateNew();
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
        if (this.CurrentSave is { } saveData && !this._isBusy) {
            try {
                this._isBusy = true;
                saveData.LastSaved = DateTime.Now;
                Serializer.Instance.Serialize(saveData, this.GetCurrentSaveFilePath(saveData));

                // It is the most recently saved, so put it at the top of the list.
                this._existingSaves.Remove(saveData);
                this._existingSaves.InsertOrAdd(0, saveData);
            }
            finally {
                this._isBusy = false;
            }
        }
    }

    private string GetCurrentSaveFilePath(SaveData saveData) => Path.Combine(this._dataManager.GetPathToDataDirectory(), saveData.GetFileName());
}