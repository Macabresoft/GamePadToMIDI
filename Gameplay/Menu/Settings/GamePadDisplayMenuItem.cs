namespace Macabresoft.Macabre2D.Project.Gameplay;

using Macabresoft.Macabre2D.Framework;
using Macabresoft.Macabre2D.Project.Common;

/// <summary>
/// A menu item for changing the style of game pad to display.
/// </summary>
public class GamePadDisplayMenuItem : SelectionMenuItem {
    private readonly Dictionary<GamePadDisplay, SelectionOption> _inputDisplayToOptions;

    /// <summary>
    /// Initializes a new instance of the <see cref="GamePadDisplayMenuItem" /> class.
    /// </summary>
    public GamePadDisplayMenuItem() : base() {
        this._inputDisplayToOptions = new Dictionary<GamePadDisplay, SelectionOption> {
            { GamePadDisplay.X, new SelectionOption(Resources.Menu_Settings_GamePadSelection_X, () => this.SetDisplay(GamePadDisplay.X)) },
            { GamePadDisplay.N, new SelectionOption(Resources.Menu_Settings_GamePadSelection_N, () => this.SetDisplay(GamePadDisplay.N)) },
            { GamePadDisplay.S, new SelectionOption(Resources.Menu_Settings_GamePadSelection_S, () => this.SetDisplay(GamePadDisplay.S)) }
        };
    }

    /// <inheritdoc />
    public override bool CanFocus => this.Game.InputBindings.DesiredInputDevice != InputDevice.KeyboardMouse;

    /// <inheritdoc />
    public override string ResourceName => nameof(Resources.Menu_Settings_GamePadSelection);

    /// <inheritdoc />
    protected override List<SelectionOption> AvailableOptions { get; } = new();

    /// <inheritdoc />
    public override void Activate() {
        base.Activate();
        this.SetInitialValue();
    }

    /// <inheritdoc />
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