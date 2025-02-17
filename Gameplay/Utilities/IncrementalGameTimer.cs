namespace Macabresoft.Macabre2D.Project.Gameplay;

using System.Runtime.Serialization;
using Macabresoft.Macabre2D.Framework;

[DataContract]
public sealed class IncrementalGameTimer {
    private const float HoldTimerMultiplierIncrement = 0.1f;
    private const float MaximumHoldTimerMultiplier = 5f;
    private const float MinimumHoldTimerMultiplier = 1f;
    private float _holdTimeMultiplier = MinimumHoldTimerMultiplier;

    [DataMember]
    public GameTimer Timer { get; } = new() { TimeLimit = 0.25f };

    private float HoldTimerMultiplier {
        get => this._holdTimeMultiplier;
        set => this._holdTimeMultiplier = Math.Clamp(value, MinimumHoldTimerMultiplier, MaximumHoldTimerMultiplier);
    }

    public bool CanExecute() {
        var result = false;
        if (this.Timer.State == TimerState.Finished) {
            result = true;
            this.Timer.Restart();
            this.HoldTimerMultiplier += HoldTimerMultiplierIncrement;
        }

        return result;
    }

    public bool CanExecute(Func<bool> canExecute) {
        var result = false;

        if (canExecute.Invoke()) {
            result = this.CanExecute();
        }

        return result;
    }

    public bool CanExecute(FrameTime frameTime, Func<bool> canExecute) {
        var result = false;
        if (canExecute.Invoke()) {
            if (this.CanExecute()) {
                result = true;
            }
            else {
                this.Timer.Increment(frameTime, this.HoldTimerMultiplier);
            }
        }
        else {
            this.Complete();
        }

        return result;
    }

    public void Complete() {
        this.Timer.Complete();
        this.HoldTimerMultiplier = MinimumHoldTimerMultiplier;
    }

    public void Increment(FrameTime frameTime) {
        this.Timer.Increment(frameTime, this.HoldTimerMultiplier);
    }
}