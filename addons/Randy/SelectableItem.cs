using Godot;
using MonoCustomResourceRegistry;

namespace RandyPlugin;

[RegisteredType(nameof(SelectableItem), "", nameof(Resource)), GlobalClass]
public partial class SelectableItem() : Resource() {
    [Export] public int Weight { get; private set; }
    [Export] public GodotObject? Item { get; private set; }

    public override string ToString() {
        return $"[Item: {this.Item}, Weight: {this.Weight}]";
    }
}
