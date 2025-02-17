namespace Macabresoft.Macabre2D.Project.Gameplay;

using System.ComponentModel;
using System.Runtime.Serialization;
using Macabresoft.Core;
using Macabresoft.Macabre2D.Framework;
using Microsoft.Xna.Framework;

public interface IMenuSystem {
    event EventHandler? MenuItemChanged;

    bool CanBackOut { get; set; }

    ISubMenu FocusedMenu { get; }

    void PopMenu();

    void PushMenu(ISubMenu menu);

    void RaiseMenuItemChanged();

    void TryClick(Vector2 clickStart, Vector2 clickEnd);
}

public class MenuSystem : GameSystem, IMenuSystem {
    private readonly Stack<ISubMenu> _menuStack = new();
    private ISubMenu _focusedMenu = SubMenu.EmptyInstance;

    public event EventHandler? MenuItemChanged;

    [DataMember]
    public bool CanBackOut { get; set; }

    public static IMenuSystem Empty { get; } = new EmptyMenuSystem();

    [DataMember]
    public EntityReference<ISubMenu> FirstMenu { get; } = new();

    public ISubMenu FocusedMenu {
        get => this._focusedMenu;
        private set {
            if (value != this._focusedMenu) {
                this._focusedMenu.PropertyChanged -= this.FocusedMenu_PropertyChanged;
                this._focusedMenu.Deactivate();

                this._focusedMenu = value;

                if (this.FocusedMenu != SubMenu.EmptyInstance) {
                    this._focusedMenu.Activate();
                    this._focusedMenu.PropertyChanged += this.FocusedMenu_PropertyChanged;
                }

                this.RaisePropertyChanged();
                this.RaiseMenuItemChanged();
            }
        }
    }

    public override GameSystemKind Kind => GameSystemKind.Update;

    public bool LastChangeWasPush { get; private set; }


    public override void Initialize(IScene scene) {
        this.Scene.Activated -= this.Scene_Activated;

        base.Initialize(scene);

        this.Scene.Activated += this.Scene_Activated;
        this.FirstMenu.Initialize(this.Scene);
        this.Reset();
    }

    public void PopMenu() {
        this.LastChangeWasPush = false;
        this.FocusedMenu.OnPop();
        if (this._menuStack.TryPop(out var menu)) {
            this.FocusedMenu = menu;
        }
        else if (this.CanBackOut || this.Game.TryPopScene(out _)) {
            this.Unpause();
        }
    }

    public void PushMenu(ISubMenu menu) {
        if (menu != SubMenu.EmptyInstance && this.FocusedMenu.Id != menu.Id) {
            if (this.FocusedMenu != SubMenu.EmptyInstance) {
                this._menuStack.Push(this.FocusedMenu);
            }

            this.FocusedMenu = menu;
            this.FocusedMenu.OnPush();
        }
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
        foreach (var menu in this.Scene.GetDescendants<ISubMenu>().Where(x => x.Id != this.FocusedMenu.Id)) {
            menu.Deactivate();
        }

        this.PushMenu(this.FirstMenu.Entity ?? SubMenu.EmptyInstance);
    }

    private void Scene_Activated(object? sender, EventArgs e) {
        this.Reset();
    }

    private void Unpause() {
        this.FocusedMenu = SubMenu.EmptyInstance;
        this._menuStack.Clear();
    }

    private class EmptyMenuSystem : IMenuSystem {
        public event EventHandler? MenuItemChanged;

        public bool CanBackOut {
            get => false;
            set { }
        }

        public ISubMenu FocusedMenu => SubMenu.EmptyInstance;

        public void PopMenu() {
        }

        public void PushMenu(ISubMenu menu) {
        }

        public void RaiseMenuItemChanged() {
        }


        public void TryClick(Vector2 clickStart, Vector2 clickEnd) {
        }
    }
}