namespace Macabresoft.Macabre2D.Project.Common;

/// <summary>
/// Different kinds of <see cref="PredefinedInteger" /> collections.
/// </summary>
public enum PredefinedIntegerKind {
    RenderOrder,
    UpdateOrder
}

/// <summary>
/// A class of predefined integer collections.
/// </summary>
public static class PredefinedIntegers {
    /// <summary>
    /// The collection of <see cref="PredefinedInteger" /> for render order.
    /// </summary>
    public static readonly IReadOnlyCollection<PredefinedInteger> RenderOrders = new[] {
        new PredefinedInteger("Default", 0),
        new PredefinedInteger("Early", -1),
        new PredefinedInteger("Late", 1)
    };

    /// <summary>
    /// The collection of <see cref="PredefinedInteger" /> for update order.
    /// </summary>
    public static readonly IReadOnlyCollection<PredefinedInteger> UpdateOrders = new[] {
        new PredefinedInteger("Default", 0),
        new PredefinedInteger("Early", -1),
        new PredefinedInteger("Late", 1)
    };

    /// <summary>
    /// An undefined integer.
    /// </summary>
    public static PredefinedInteger UndefinedValue { get; } = new("UnDeFiNeD!1!1", int.MinValue);

    /// <summary>
    /// Gets the collection of <see cref="PredefinedInteger" /> for the specified <see cref="PredefinedIntegerKind" />.
    /// </summary>
    /// <param name="kind"></param>
    /// <returns></returns>
    public static IReadOnlyCollection<PredefinedInteger> GetCollection(PredefinedIntegerKind kind) {
        return kind switch {
            PredefinedIntegerKind.RenderOrder => RenderOrders,
            PredefinedIntegerKind.UpdateOrder => UpdateOrders,
            _ => Array.Empty<PredefinedInteger>()
        };
    }

    /// <summary>
    /// Gets a value indicating whether the provided <see cref="PredefinedInteger" /> is secretly undefined.
    /// </summary>
    /// <param name="predefinedInteger">The predefined integer.</param>
    /// <returns>A value indicating whether the provided <see cref="PredefinedInteger" /> is secretly undefined.</returns>
    public static bool IsUndefined(this PredefinedInteger predefinedInteger) => predefinedInteger.Name == UndefinedValue.Name;
}

/// <summary>
/// An attribute for predefined integers.
/// </summary>
/// <param name="kind">The predefined integer kind.</param>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public sealed class PredefinedIntegerAttribute(PredefinedIntegerKind kind) : Attribute {
    /// <summary>
    /// Gets the kind of predefined integer.
    /// </summary>
    public PredefinedIntegerKind Kind { get; } = kind;
}

/// <summary>
/// A predefined integer with a name.
/// </summary>
/// <param name="Name">The name.</param>
/// <param name="Value">The value.</param>
public readonly record struct PredefinedInteger(string Name, int Value);