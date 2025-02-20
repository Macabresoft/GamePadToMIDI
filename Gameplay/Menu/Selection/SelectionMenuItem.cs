namespace Macabresoft.Macabre2D.Project.Gameplay;

using System.Runtime.Serialization;
using Macabresoft.Macabre2D.Framework;
using Microsoft.Xna.Framework;

public abstract class SelectionMenuItem : BaseSelectionMenuItem {
    private bool _canWrap;
    private SelectionOption _selectedOption = new(string.Empty, null);
    private SelectionSpinner? _spinner;

    [DataMember]
    public bool CanWrap {
        get => this._canWrap;
        set {
            if (value != this._canWrap) {
                this._canWrap = value;
                this.ResetCanIncreaseDecrease();
            }
        }
    }

    public IReadOnlyCollection<SelectionOption> Options => this.AvailableOptions;

    protected abstract List<SelectionOption> AvailableOptions { get; }

    private SelectionOption SelectedOption {
        get => this._selectedOption;
        set {
            if (this._selectedOption != value) {
                this.InitializeSelection(value);
                this._selectedOption.Select();
            }
        }
    }

    public override void Initialize(IScene scene, IEntity parent) {
        base.Initialize(scene, parent);

        this._spinner = this.GetOrAddChild<SelectionSpinner>();
    }

    protected override void Decrease() {
        this.SelectPrevious();
    }

    protected override void Execute() {
        if (this.Options.Count == 2) {
            if (this.IsDecreaseEnabled) {
                this.SelectedOption = this.Options.FirstOrDefault() ?? this.SelectedOption;
                this.DecreaseTimer.Timer.Restart();
            }
            else {
                this.SelectedOption = this.Options.LastOrDefault() ?? this.SelectedOption;
                this.IncreaseTimer.Timer.Restart();
            }
        }
    }

    protected override void Increase() {
        this.SelectNext();
    }

    protected void InitializeSelection(SelectionOption option) {
        this._selectedOption = option;

        if (this._spinner != null) {
            this._spinner.Text = this._selectedOption.Text;
        }

        this.ResetCanIncreaseDecrease();
    }

    protected override bool IsClickDecrease(Vector2 clickPosition) =>
        this._spinner != null &&
        this._spinner.BoundingArea.Contains(clickPosition) &&
        this._spinner.BoundingArea.Minimum.X + this._spinner.ActualEndCapWidth >= clickPosition.X;


    protected override bool IsClickIncrease(Vector2 clickPosition) =>
        this._spinner != null &&
        this._spinner.BoundingArea.Contains(clickPosition) &&
        this._spinner.BoundingArea.Maximum.X - this._spinner.ActualEndCapWidth <= clickPosition.X;

    protected void SetHasChanges() {
        if (this.TryGetAncestor<BaseMenu>(out var menu)) {
            menu.HasChanges = true;
        }
    }

    private void ResetCanIncreaseDecrease() {
        if (this.CanWrap) {
            this.IsIncreaseEnabled = this.AvailableOptions.Count > 1;
            this.IsDecreaseEnabled = this.IsIncreaseEnabled;
        }
        else {
            this.IsIncreaseEnabled = this._selectedOption != this.AvailableOptions.LastOrDefault();
            this.IsDecreaseEnabled = this._selectedOption != this.AvailableOptions.FirstOrDefault();
        }
    }

    private void SelectNext() {
        if (this.AvailableOptions.Count > 1) {
            var newIndex = this.AvailableOptions.IndexOf(this.SelectedOption) + 1;
            if (this.CanWrap && newIndex >= this.AvailableOptions.Count) {
                this.SelectedOption = this.AvailableOptions[0];
            }
            else if (newIndex >= 0 && newIndex < this.AvailableOptions.Count) {
                this.SelectedOption = this.AvailableOptions[newIndex];
            }
        }
    }

    private void SelectPrevious() {
        if (this.AvailableOptions.Count > 1) {
            var newIndex = this.AvailableOptions.IndexOf(this.SelectedOption) - 1;
            if (this.CanWrap && newIndex < 0) {
                this.SelectedOption = this.AvailableOptions[^1];
            }
            else if (newIndex >= 0 && newIndex < this.AvailableOptions.Count) {
                this.SelectedOption = this.AvailableOptions[newIndex];
            }
        }
    }
}