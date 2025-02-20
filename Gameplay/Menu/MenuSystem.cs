﻿namespace Macabresoft.Macabre2D.Project.Gameplay;

using System.ComponentModel;
using System.Runtime.Serialization;
using Macabresoft.Core;
using Macabresoft.Macabre2D.Framework;
using Microsoft.Xna.Framework;

public interface IMenuSystem {
    event EventHandler? MenuItemChanged;

    ISubMenu FocusedMenu { get; }

    void PopMenu();

    void RaiseMenuItemChanged();

    void TryClick(Vector2 clickStart, Vector2 clickEnd);
}

public class MenuSystem : GameSystem, IMenuSystem {
    private ISubMenu _focusedMenu = SubMenu.EmptyInstance;

    public event EventHandler? MenuItemChanged;

    public static IMenuSystem Empty { get; } = new EmptyMenuSystem();

    [DataMember]
    public EntityReference<ISubMenu> FirstMenu { get; } = new();

    public ISubMenu FocusedMenu {
        get => this._focusedMenu;
        private set {
            this._focusedMenu.PropertyChanged -= this.FocusedMenu_PropertyChanged;
            this._focusedMenu = value;

            if (this.FocusedMenu != SubMenu.EmptyInstance) {
                this._focusedMenu.PropertyChanged += this.FocusedMenu_PropertyChanged;
            }

            this.RaisePropertyChanged();
            this.RaiseMenuItemChanged();
            this.FocusedMenu.OnPush();
        }
    }

    public override GameSystemKind Kind => GameSystemKind.Update;

    public override void Initialize(IScene scene) {
        this.Scene.Activated -= this.Scene_Activated;

        base.Initialize(scene);

        this.Scene.Activated += this.Scene_Activated;
        this.FirstMenu.Initialize(this.Scene);
        this.Reset();
    }

    public void PopMenu() {
        this.FocusedMenu.OnPop();

        if (this.Game.TryPopScene(out _)) {
            this.FocusedMenu = SubMenu.EmptyInstance;
            this.Unpause();
        }
    }

    public void PushMenu(ISubMenu menu) {
        this.FocusedMenu = menu;
    }

    public void RaiseMenuItemChanged() {
        this.MenuItemChanged.SafeInvoke(this);
    }

    public void TryClick(Vector2 clickStart, Vector2 clickEnd) {
        if (this.FocusedMenu.FocusedMenuItem.BoundingArea.Contains(clickEnd)) {
            this.FocusedMenu.FocusedMenuItem.Click(clickStart, clickEnd, false);
        }
        else {
            foreach (var menuItem in this.FocusedMenu.GetDescendants<IMenuItem>()) {
                if (menuItem.BoundingArea.Contains(clickEnd)) {
                    this.FocusedMenu.FocusedMenuItem = menuItem;
                    menuItem.Click(clickStart, clickEnd, false);
                    break;
                }
            }
        }
    }

    public void TryHold(Vector2 clickStart, Vector2 clickEnd) {
        if (this.FocusedMenu.FocusedMenuItem is BaseSelectionMenuItem && this.FocusedMenu.FocusedMenuItem.BoundingArea.Contains(clickEnd)) {
            this.FocusedMenu.FocusedMenuItem.Click(clickStart, clickEnd, true);
        }
    }

    public override void Update(FrameTime frameTime, InputState inputState) {
        this.FocusedMenu.HandleInput(frameTime, inputState);
    }

    private void FocusedMenu_PropertyChanged(object? sender, PropertyChangedEventArgs e) {
        if (e.PropertyName == nameof(ISubMenu.FocusedMenuItem)) {
            this.RaiseMenuItemChanged();
        }
    }

    private void Reset() {
        this.FocusedMenu = this.FirstMenu.Entity ?? SubMenu.EmptyInstance;
    }

    private void Scene_Activated(object? sender, EventArgs e) {
        this.Reset();
    }

    private void Unpause() {
        this.FocusedMenu = SubMenu.EmptyInstance;
    }

    private class EmptyMenuSystem : IMenuSystem {
        public event EventHandler? MenuItemChanged;

        public ISubMenu FocusedMenu => SubMenu.EmptyInstance;

        public void PopMenu() {
        }

        public void RaiseMenuItemChanged() {
        }


        public void TryClick(Vector2 clickStart, Vector2 clickEnd) {
        }
    }
}