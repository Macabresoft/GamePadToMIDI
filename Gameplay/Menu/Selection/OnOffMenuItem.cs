namespace Macabresoft.Macabre2D.Project.Gameplay;

using Macabresoft.Macabre2D.Framework;
using Macabresoft.Macabre2D.Project.Common;
using Microsoft.Xna.Framework;

public abstract class OnOffMenuItem : SelectionMenuItem {
    private readonly SelectionOption _offOption;
    private readonly SelectionOption _onOption;
    private IMenuSystem _menuSystem = MenuSystem.Empty;

    protected OnOffMenuItem() : base() {
        this._offOption = new SelectionOption(Resources.Menu_Off, () => this.SetValue(false));
        this._onOption = new SelectionOption(Resources.Menu_On, () => this.SetValue(true));
        this.AvailableOptions = [
            this._offOption,
            this._onOption
        ];
    }

    public override string ConfirmPromptResourceName => this.GetValue() ? nameof(Resources.Menu_Prompts_TurnOff) : nameof(Resources.Menu_Prompts_TurnOn);

    protected override List<SelectionOption> AvailableOptions { get; }

    public override void Activate() {
        base.Activate();
        this.SetInitialValue();
    }

    public override void Deinitialize() {
        base.Deinitialize();
        this._menuSystem = MenuSystem.Empty;
    }

    public override void Initialize(IScene scene, IEntity parent) {
        base.Initialize(scene, parent);
        this._menuSystem = this.Scene.GetSystem<MenuSystem>() ?? MenuSystem.Empty;
        this.SetInitialValue();
    }

    protected abstract bool GetValue();

    protected void SetInitialValue() {
        this.InitializeSelection(this.GetValue() ? this._onOption : this._offOption);
    }

    protected virtual void SetValue(bool value) {
        this._menuSystem.RaiseMenuItemChanged();
    }

    protected override void TryRogueClick(Vector2 clickPosition, bool isHold) {
        if (!isHold && this.BoundingArea.Contains(clickPosition) && !this.IsClickDecrease(clickPosition) && !this.IsClickIncrease(clickPosition)) {
            this.Execute();
        }
    }
}