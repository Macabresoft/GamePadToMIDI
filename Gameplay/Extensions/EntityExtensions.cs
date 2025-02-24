namespace Macabresoft.Macabre2D.Project.Gameplay;

using Macabresoft.Macabre2D.Framework;
using Macabresoft.Macabre2D.Project.Common;
using Microsoft.Xna.Framework;

public static class EntityExtensions {
    public static Color GetMenuItemColor(this IEntity entity) {
        if (entity.TryGetAncestor<IMenuItem>(out var menuItem)) {
            if (!menuItem.CanFocus) {
                return PredefinedColors.DeactivatedText;
            }

            if (menuItem.IsFocused) {
                return PredefinedColors.TextHighlightColor;
            }
        }

        return Color.White;
    }
}