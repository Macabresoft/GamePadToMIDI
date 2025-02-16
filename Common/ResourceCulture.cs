namespace Macabresoft.Macabre2D.Project.Common;

/// <summary>
/// Defines a culture to bse used by <see cref="Resources" /> (Resources.resx).
/// </summary>
public enum ResourceCulture {
    Default = 0
}

/// <summary>
/// Extensions for <see cref="ResourceCulture" />.
/// </summary>
public static class ResourceCultureExtensions {
    private const string DefaultCultureName = "default";

    /// <summary>
    /// Converts a <see cref="ResourceCulture" /> into the culture name as used by <see cref="Resources" /> (Resources.resx).
    /// </summary>
    /// <param name="culture">The culture.</param>
    /// <returns>The culture's name.</returns>
    public static string ToCultureName(this ResourceCulture culture) {
        return culture switch {
            _ => "en"
        };
    }

    /// <summary>
    /// Converts a <see cref="ResourceCulture" /> into the culture name as used by <see cref="Resources" /> (Resources.resx).
    /// </summary>
    /// <param name="culture">The culture.</param>
    /// <returns>The culture's display name.</returns>
    public static string ToDisplayName(this ResourceCulture culture) {
        return culture switch {
            _ => "English (US)"
        };
    }
}