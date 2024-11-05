using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using C5;
using Godot;

namespace RandyPlugin.Generic;

public partial class Selector<T> : Node,
    System.Collections.Generic.ICollection<(T item, int weight)> where T : notnull {
    [Export] protected bool IsWeighted { get; set; } = true;
    protected TreeDictionary<int, (T item, int index)> Items { get; init; } = [];
    protected Dictionary<T, List<(int weight, int cumulative)>> Weights { get; init; } = [];
    protected TreeDictionary<int, List<(T item, int index)>> LookUp { get; init; } = [];
    protected int TotalWeight { get; set; } = 0;
    public bool IsEmpty => this.Items.Count == 0;
    public int Count => this.Items.Count;
    public bool IsReadOnly => false;

    public static Selector<T> Weighted() {
        return new Selector<T>() { IsWeighted = true };
    }

    public static Selector<T> Unweighted() {
        return new Selector<T>() { IsWeighted = false };
    }

    public virtual void Add(T item, int weight) {
        weight = this.IsWeighted ? weight : 1;
        if (!this.Weights.TryAdd(item, [(weight, this.TotalWeight + weight)])) {
            this.Weights[item].Add((weight, this.TotalWeight + weight));
        }
        (T, int) entry = (item, this.Weights[item].Count - 1);
        this.Items.Add(this.TotalWeight, entry);
        this.TotalWeight += weight;
        if (this.LookUp.Contains(weight)) {
            this.LookUp[weight].Add(entry);
        } else {
            this.LookUp.Add(weight, [entry]);
        }
    }

    public void Add(params (T item, int weight)[] items) {
        foreach ((T item, int weight) in items) {
            this.Add(item, weight);
        }
    }

    public void Add(IEnumerable<(T item, int weight)> items) {
        foreach ((T item, int weight) in items) {
            this.Add(item, weight);
        }
    }

    public void Add(params KeyValuePair<T, int>[] items) {
        foreach (KeyValuePair<T, int> pair in items) {
            this.Add(pair.Key, pair.Value);
        }
    }

    public void Add(IEnumerable<KeyValuePair<T, int>> items) {
        foreach (KeyValuePair<T, int> pair in items) {
            this.Add(pair.Key, pair.Value);
        }
    }

    public T Select() {
        return this.Items.WeakPredecessor((int)(Randy.Randf() * this.TotalWeight)).Value.item;
    }

    public bool Remove(ref T item, int k = 0) {
        if (this.Weights.TryGetValue(item, out List<(int, int)>? weights)) {
            k = k <= 0 ? weights.Count : k;
            if (k > weights.Count) {
                throw new IndexOutOfRangeException(
                    $"The selector only contains {weights.Count} < {k} copies of {item}."
                );
            }
            // Get the cumulative weight of the k-th copy of the item.
            (int weight, int cumulative) = this.WeightRecordOf(item, k);
            int currKey = cumulative - weight;
            if (this.Items.Remove(currKey, out (T item, int index) entry)) {
                item = entry.item;
                this.LookUp[weight].Remove(entry);
            }
            weights.RemoveAt(k - 1);
            if (this.Items.TrySuccessor(
                currKey, out KeyValuePair<int, (T item, int index)> next
            )) {
                // Shift down the heavier items.
                int gap = next.Key - currKey;
                this.TotalWeight -= gap;
                do {
                    // Remove the successor.
                    this.Items.Remove(next.Key);
                    // Shift the successor to the current position.
                    this.Items.Add(currKey, next.Value);
                    // Obtain the weight of the successor item.
                    int w = this.Weights[next.Value.item][next.Value.index].weight;
                    // Update the cumulative weight of the successor item.
                    this.Weights[next.Value.item][next.Value.index] = (w, currKey + weight);
                    currKey = next.Key;
                } while (this.Items.TrySuccessor(currKey, out next));
            } else {
                this.TotalWeight = currKey;
            }
        }
        return false;
    }

    public (int weight, int cumulative) WeightRecordOf(T item, int k = 1) {
        if (this.Weights.TryGetValue(item, out List<(int, int)>? weights)) {
            if (weights.Count >= k) {
                return weights[k - 1];
            }
            throw new IndexOutOfRangeException(
                $"The selector only contains {weights.Count} < {k} copies of {item}."
            );
        }
        throw new ArgumentException($"The item {item} does not exist in the selector.");
    }

    public int WeightOf(T item, int k = 1) {
        return this.WeightRecordOf(item, k).weight;
    }

    public int CumulativeWeightOf(T item, int k = 1) {
        return this.WeightRecordOf(item, k).cumulative;
    }

    public int CountOf(T item) {
        return this.Weights.TryGetValue(item, out List<(int, int)>? weights) ? weights.Count : 0;
    }

    public List<(T item, int index)> WhereWeightIs(int weight) {
        if (this.LookUp.Contains(weight)) {
            return this.LookUp[weight];
        }
        return [];
    }

    public List<(T item, int index)> Top() {
        return this.LookUp.First().Value;
    }

    public List<(T item, int index)> Bottom() {
        return this.LookUp.Last().Value;
    }

    public void Add((T item, int weight) item) {
        this.Add(item.item, item.weight);
    }
    public bool Contains((T item, int weight) item) {
        return this.Weights.TryGetValue(
            item.item, out List<(int weight, int _)>? weights
        ) && weights.Any(w => w.weight == item.weight);
    }

    public void CopyTo((T item, int weight)[] array, int arrayIndex) {
        foreach (KeyValuePair<int, (T item, int)> pair in this.Items) {
            if (arrayIndex >= array.Length) {
                break;
            }
            array[arrayIndex] = (pair.Value.item, this.WeightOf(pair.Value.item));
        }
    }

    public bool Remove((T item, int weight) item) {
        if (this.Weights.TryGetValue(item.item, out List<(int weight, int)>? weights)) {
            return this.Remove(ref item.item, weights.FindIndex(w => w.weight == item.weight) + 1);
        }
        return false;
    }

    public virtual void Clear() {
        this.Items.Clear();
        this.Weights.Clear();
        this.TotalWeight = 0;
    }

    public IEnumerator GetEnumerator() {
        return this.Items.GetEnumerator();
    }

    IEnumerator<(T item, int weight)> IEnumerable<(T item, int weight)>.GetEnumerator() {
        return this.Items.Select(entry => (entry.Value.item, this.WeightOf(
            entry.Value.item, entry.Value.index + 1
        ))).GetEnumerator();
    }

    public override string ToString() {
        StringBuilder sb = new StringBuilder();
        foreach (KeyValuePair<int, (T item, int index)> entry in this.Items) {
            sb.AppendLine($"----- {entry.Key} -----");
            sb.AppendLine($"[weight: {this.WeightOf(entry.Value.item, entry.Value.index + 1)}]");
            sb.AppendLine($"{entry.Value.item}({entry.Value.index})");
        }
        return sb.ToString();
    }
}
