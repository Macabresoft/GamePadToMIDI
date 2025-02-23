namespace Macabresoft.Macabre2D.Project.Gameplay;

using Macabresoft.Macabre2D.Framework;
using Macabresoft.Macabre2D.Project.Common;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

public abstract class BaseSelectionMenuItem : MenuItem {
    private readonly IncrementalGameTimer _clickDecreaseTimer = new();
    private readonly IncrementalGameTimer _clickIncreaseTimer = new();
    private readonly GameTimer _clickResetTimer = new(0.75f);
    
    public bool IsDecreaseEnabled { get; protected set; }

    public bool IsIncreaseEnabled { get; protected set; }

    protected IncrementalGameTimer DecreaseTimer { get; } = new();
    protected IncrementalGameTimer IncreaseTimer { get; } = new();

    public override void Click(Vector2 startPosition, Vector2 endPosition, bool isHold) {
        if (this.BoundingArea.Contains(startPosition) && this.BoundingArea.Contains(endPosition)) {
            if (this.CanDecreaseViaClick(startPosition, endPosition)) {
                this._clickResetTimer.Restart();

                if (this._clickDecreaseTimer.CanExecute()) {
                    this.Decrease();
                }
            }
            else if (this.CanIncreaseViaClick(startPosition, endPosition)) {
                this._clickResetTimer.Restart();

                if (this._clickIncreaseTimer.CanExecute()) {
                    this.Increase();
                }
            }
            else {
                this.TryRogueClick(endPosition, isHold);
            }
        }
    }

    public override void Focus() {
        base.Focus();
        this.ResetTimers();
    }

    public override bool HandleInput(FrameTime frameTime, IInputSystem input) {
        var result = base.HandleInput(frameTime, input);

        if (!result) {
            if (this.DecreaseTimer.CanExecute(frameTime, () => this.CanDecrease(input))) {
                this.Decrease();
                result = true;
            }
            else if (this.IncreaseTimer.CanExecute(frameTime, () => this.CanIncrease(input))) {
                this.Increase();
                result = true;
            }

            this._clickDecreaseTimer.Increment(frameTime);
            this._clickIncreaseTimer.Increment(frameTime);
            this._clickResetTimer.Increment(frameTime);

            if (this._clickResetTimer.State == TimerState.Finished) {
                this._clickDecreaseTimer.Complete();
                this._clickIncreaseTimer.Complete();
                this._clickResetTimer.Restart();
            }
        }

        return result;
    }

    public override void Initialize(IScene scene, IEntity parent) {
        base.Initialize(scene, parent);

        this._clickIncreaseTimer.Timer.TimeLimit = 0.2f;
        this._clickDecreaseTimer.Timer.TimeLimit = 0.2f;

        this.IncreaseTimer.Timer.TimeLimit = 0.25f;
        this.DecreaseTimer.Timer.TimeLimit = 0.25f;

        this.ResetTimers();
    }

    public override bool IsHoveringOver(Vector2 position) => this.CanDecreaseViaClick(position, position) || this.CanIncreaseViaClick(position, position);

    public override void RemoveFocus() {
        base.RemoveFocus();
        this.ResetTimers();
    }

    protected abstract void Decrease();

    protected abstract void Increase();

    protected abstract bool IsClickDecrease(Vector2 clickPosition);

    protected abstract bool IsClickIncrease(Vector2 clickPosition);

    /// <summary>
    /// Tries to click somewhere in the menu item that isn't at the start or end position.
    /// </summary>
    /// <remarks>
    /// This was mostly made for <see cref="AudioMenuItem" />.
    /// </remarks>
    /// <param name="clickPosition">The ending click position. We already know the start is the same menu item at this point.</param>
    /// <param name="isHold">A value indicating whether the click is a hold.</param>
    protected virtual void TryRogueClick(Vector2 clickPosition, bool isHold) {
    }

    private bool CanDecrease(IInputSystem input) => this.IsDecreaseEnabled && input.IsHeld(InputAction.Left);

    private bool CanDecreaseViaClick(Vector2 startPosition, Vector2 endPosition) => this.IsDecreaseEnabled && this.IsClickDecrease(startPosition) && this.IsClickDecrease(endPosition);

    private bool CanIncrease(IInputSystem input) => this.IsIncreaseEnabled && input.IsHeld(InputAction.Right);

    private bool CanIncreaseViaClick(Vector2 startPosition, Vector2 endPosition) => this.IsIncreaseEnabled && this.IsClickIncrease(startPosition) && this.IsClickIncrease(endPosition);

    private void ResetTimers() {
        this.DecreaseTimer.Complete();
        this.IncreaseTimer.Complete();
        this._clickDecreaseTimer.Complete();
        this._clickIncreaseTimer.Complete();
        this._clickResetTimer.Restart();
    }
}