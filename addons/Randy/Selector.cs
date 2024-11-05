using System;
using System.Linq;
using Godot;
using RandyPlugin.Generic;

namespace RandyPlugin;

[GlobalClass]
public partial class Selector : Selector<object> {
    [Export] private bool IsTyped { get; set; } = true;
    [Export] private SelectableItem[] ItemData { get; set; } = [];
    private Type? DataType { get; set; }

    public override void _Ready() {
        foreach (SelectableItem item in this.ItemData.Where(data => data.Item is not null)) {
            this.Add(item.Item!, item.Weight);
        }
    }

    public override void Add(object item, int weight) {
        if (this.IsTyped) {
            Type type = item.GetType();
            this.DataType ??= type;
            if (type != this.DataType) {
                throw new ArgumentException(
                    $"Cannot add an item of type {type} to a selector for {this.DataType} objects."
                );
            }
        }
        base.Add(item, weight);
    }

    public override void Clear() {
        base.Clear();
        this.DataType = null;
    }
}
