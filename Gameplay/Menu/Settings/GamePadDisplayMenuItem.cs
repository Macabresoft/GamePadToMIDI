namespace Macabresoft.Macabre2D.Project.Gameplay;

using Macabresoft.Macabre2D.Framework;
using Macabresoft.Macabre2D.Project.Common;

public class GamePadDisplayMenuItem : SelectionMenuItem {
    private readonly Dictionary<GamePadDisplay, SelectionOption> _inputDisplayToOptions;

    public GamePadDisplayMenuItem() : base() {
        this._inputDisplayToOptions = new Dictionary<GamePadDisplay, SelectionOption> {
            { GamePadDisplay.X, new SelectionOption(Resources.Menu_Settings_GamePadSelection_X, () => this.SetDisplay(GamePadDisplay.X)) },
            { GamePadDisplay.N, new SelectionOption(Resources.Menu_Settings_GamePadSelection_N, () => this.SetDisplay(GamePadDisplay.N)) },
            { GamePadDisplay.S, new SelectionOption(Resources.Menu_Settings_GamePadSelection_S, () => this.SetDisplay(GamePadDisplay.S)) }
        };
    }

    public override bool CanFocus {
        get => this.Game.InputBindings.DesiredInputDevice != InputDevice.KeyboardMouse;
    }

    public override string ResourceName => nameof(Resources.Menu_Settings_GamePadSelection);

    protected override List<SelectionOption> AvailableOptions { get; } = new();

    public override void Activate() {
        base.Activate();
        this.SetInitialValue();
    }

    public override void Initialize(IScene scene, IEntity parent) {
        this.AvailableOptions.Clear();
        this.AvailableOptions.AddRange(this._inputDisplayToOptions.Values);

        base.Initialize(scene, parent);
        this.SetInitialValue();
    }

    private void SetDisplay(GamePadDisplay display) {
        this.Game.InputBindings.DesiredGamePad = display;
        this.SetHasChanges();
    }

    private void SetInitialValue() {
        if (this._inputDisplayToOptions.TryGetValue(this.Game.InputBindings.DesiredGamePad, out var option)) {
            this.InitializeSelection(option);
        }
    }
}