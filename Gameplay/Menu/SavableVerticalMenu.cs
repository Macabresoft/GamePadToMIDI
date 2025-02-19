﻿namespace Macabresoft.Macabre2D.Project.Gameplay;

public class SavableVerticalMenu : VerticalMenuController {
    public bool HasChanges { get; set; }

    public override void Deactivate() {
        base.Deactivate();

        if (this.HasChanges) {
            this.OnSave();
        }
    }

    public override void OnPush() {
        base.OnPush();
        this.HasChanges = false;
    }

    protected virtual void OnSave() {
        this.Game.SaveUserSettings();
    }
}