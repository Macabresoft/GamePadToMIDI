namespace Macabresoft.Macabre2D.Project.Gameplay;

using System.Runtime.Serialization;
using Macabresoft.Macabre2D.Framework;
using Macabresoft.Macabre2D.Project.Common;
using Microsoft.Xna.Framework;

/// <summary>
/// Mouse cursor for the MIDI controller interface.
/// </summary>
public class MidiMouseCursor : MouseCursorRenderer {
    private readonly GameTimer _disappearTimer = new(5f);
    private readonly List<MidiPad> _pads = [];
    private readonly GameTimer _viewportSizeChangedTimer = new(0.1f);
    private MidiSystem? _midiSystem;
    private bool _viewportSizeChanged;

    /// <summary>
    /// Gets a value indicating whether this is clicking.
    /// </summary>
    public bool IsClicking { get; private set; }

    /// <summary>
    /// Gets the settings button reference.
    /// </summary>
    [DataMember]
    public EntityReference<IBoundableEntity> SettingsButtonReference { get; } = new();

    /// <summary>
    /// Deactivates this instance.
    /// </summary>
    public void Deactivate() {
        this._disappearTimer.Complete();
        this.IsHoveringOverActivatableElement = false;
        this.ShouldRender = false;
    }

    /// <inheritdoc />
    public override void Deinitialize() {
        this.Game.ViewportSizeChanged -= this.Game_ViewportSizeChanged;

        base.Deinitialize();

        this._midiSystem = null;
        this._pads.Clear();
    }

    /// <inheritdoc />
    public override void Initialize(IScene scene, IEntity parent) {
        base.Initialize(scene, parent);

        this._midiSystem = this.Scene.GetSystem<MidiSystem>();
        this.Game.ViewportSizeChanged += this.Game_ViewportSizeChanged;

        if (this.TryGetAncestor<ICamera>(out var camera)) {
            this.CameraReference.EntityId = camera.Id;
        }

        this._pads.Clear();
        this._pads.AddRange(this.Scene.GetDescendants<MidiPad>());

        this.IsClicking = false;
        this.RenderOptions.OffsetType = PixelOffsetType.TopLeft;
        this.Deactivate();
    }

    /// <inheritdoc />
    public override void Update(FrameTime frameTime, InputState inputState) {
        // Behavior around the mouse when the viewport changes is complicated.
        // Essentially, the view port changing in size triggers the mouse to
        // think it moved and reappear. It can then select a different menu
        // item, which is confusing to users (me). This is a hacky way to
        // prevent that.
        if (this._viewportSizeChanged) {
            if (this._viewportSizeChangedTimer.State == TimerState.Finished) {
                this._viewportSizeChanged = false;
            }
            else {
                this._viewportSizeChangedTimer.Increment(frameTime);
            }

            return;
        }

        if (!this.Game.InputBindings.TryGetBindings(InputAction.Confirm, out _, out _, out _, out var mouseButton) || mouseButton == MouseButton.None) {
            mouseButton = MouseButton.Left;
        }

        var isHeld = inputState.IsMouseButtonHeld(mouseButton);
        var hadInput = isHeld ||
                       inputState.PreviousMouseState.Position != inputState.CurrentMouseState.Position ||
                       inputState.PreviousMouseState.ScrollWheelValue != inputState.CurrentMouseState.ScrollWheelValue;

        if (hadInput) {
            this._disappearTimer.Restart();
        }
        else {
            this._disappearTimer.Increment(frameTime);
            this.IsClicking = false;
        }

        if (this._disappearTimer.State == TimerState.Running) {
            base.Update(frameTime, inputState);

            if (hadInput) {
                this.ResetFocus();
            }

            this.ShouldRender = true;

            if (inputState.IsMouseButtonNewlyPressed(mouseButton)) {
                this.IsClicking = true;
            }
            else if (this.IsClicking && inputState.IsMouseButtonNewlyReleased(mouseButton)) {
                this.IsClicking = false;

                if (this.CheckIsOverSettingsButton()) {
                    this._midiSystem?.OpenSettings();
                }
            }
        }
        else {
            this.Deactivate();
        }
    }

    /// <inheritdoc />
    protected override IEnumerable<IAssetReference> GetAssetReferences() {
        yield return this.MouseCursorIconSet;
    }

    /// <inheritdoc />
    protected override IEnumerable<IEntityReference> GetEntityReferences() {
        foreach (var reference in base.GetEntityReferences()) {
            yield return reference;
        }

        yield return this.SettingsButtonReference;
    }

    private bool CheckIfOverPad() {
        var position = this.WorldPosition;
        return this._pads.Any(x => x.BoundingArea.Contains(position));
    }

    private bool CheckIsOverSettingsButton() => this.SettingsButtonReference.Entity != null && this.SettingsButtonReference.Entity.BoundingArea.Contains(this.WorldPosition);

    private void Game_ViewportSizeChanged(object? sender, Point e) {
        this._viewportSizeChanged = true;
        this._viewportSizeChangedTimer.Restart();
    }

    private void ResetFocus() {
        this.IsHoveringOverActivatableElement = this.CheckIsOverSettingsButton() || this.CheckIfOverPad();
    }
}