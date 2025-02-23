namespace Macabresoft.Macabre2D.Project.Gameplay;

using System.Runtime.Serialization;
using Macabresoft.Macabre2D.Framework;
using Macabresoft.Macabre2D.Project.Common;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

/// <summary>
/// Represents a virtual MIDI pad that can be "pressed" with a game pad button.
/// </summary>
public class MidiPad : BaseSpriteEntity, IUpdateableEntity {
    private Buttons _button = Buttons.None;
    private GamePadButtonRenderer? _gamePadButtonRenderer;
    private bool _isOn;
    private MidiNote _midiNote = MidiNote.Empty;
    private MidiSystem? _midiSystem;
    private ITextRenderer _noteIndexRenderer = EmptyObject.Instance;

    /// <summary>
    /// Gets or sets the button for this MIDI pad.
    /// </summary>
    [DataMember]
    public Buttons Button {
        get => this._button;
        set {
            if (this._button != value) {
                this._button = value;
                this.ResetMidiNote();
                this.RaisePropertyChanged(nameof(this.ShouldUpdate));
            }
        }
    }

    /// <inheritdoc />
    public bool ShouldUpdate => this._midiNote.IsEnabled;

    /// <inheritdoc />
    public override byte? SpriteIndex => this._isOn ? this._midiSystem?.PadOnSprite.SpriteIndex : this._midiSystem?.PadOffSprite.SpriteIndex;

    /// <inheritdoc />
    protected override SpriteSheet? SpriteSheet => this._isOn ? this._midiSystem?.PadOnSprite.Asset : this._midiSystem?.PadOffSprite.Asset;

    /// <inheritdoc />
    public override void Deinitialize() {
        base.Deinitialize();
        this._gamePadButtonRenderer = null;
        this._noteIndexRenderer = EmptyObject.Instance;
    }

    /// <inheritdoc />
    public override void Initialize(IScene scene, IEntity parent) {
        base.Initialize(scene, parent);
        this._midiSystem = this.Scene.GetSystem<MidiSystem>();
        this.RenderOptions.OffsetType = PixelOffsetType.Center;

        this._gamePadButtonRenderer = this.GetOrAddChild<GamePadButtonRenderer>();
        this._gamePadButtonRenderer.LocalPosition = new Vector2(-0.6f, 0.65f);
        this._gamePadButtonRenderer.RenderOptions.OffsetType = PixelOffsetType.Center;

        this._noteIndexRenderer = this.GetOrAddChild<TextLineRenderer>();
        this._noteIndexRenderer.RenderOptions.OffsetType = PixelOffsetType.BottomRight;
        this._noteIndexRenderer.LocalPosition = new Vector2(0.8f, -0.9f);

        this.ResetMidiNote();
    }

    /// <inheritdoc />
    public void Update(FrameTime frameTime, InputState inputState) {
        if (inputState.IsGamePadButtonNewlyPressed(this.Button)) {
            this._midiSystem?.PlayMidiNote(this._midiNote);
            this._isOn = true;
        }
        else if (!inputState.IsGamePadButtonHeld(this.Button)) {
            this._isOn = false;
        }

        this._noteIndexRenderer.Text = this._midiNote.Note.ToString();
    }

    private void ResetMidiNote() {
        if (this._gamePadButtonRenderer != null) {
            this._gamePadButtonRenderer.Button = this.Button;
        }

        this._midiNote = this._midiSystem?.GetMidiNote(this.Button) ?? MidiNote.Empty;
    }
}