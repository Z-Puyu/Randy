using Godot;
using MonoCustomResourceRegistry;

namespace RandyPlugin;

/// <summary>
/// A custom resource encapsulating a weighted item.
/// </summary>
[RegisteredType(nameof(SelectableItem), "", nameof(Resource)), GlobalClass]
public partial class SelectableItem() : Resource() {
    /// <summary>
    /// The weight of the item.
    /// </summary>
    [Export] public int Weight { get; private set; }
    /// <summary>
    /// The item data.
    /// </summary>
    [Export] public GodotObject? Item { get; private set; }

    /// <summary>
    /// Returns a string representation of the selectable item.
    /// </summary>
    /// <returns>The string representation of the selectable item.</returns>
    public override string ToString() {
        return $"[Item: {this.Item}, Weight: {this.Weight}]";
    }
}
