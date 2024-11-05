using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using C5;
using Godot;

namespace RandyPlugin.Generic;

/// <summary>
/// Encapsulates a selector that randomly chooses an item from a collection
/// based on weighted probabilities. The selector permits duplicates.
/// </summary>
/// <typeparam name="T">The type of the items in the selector.</typeparam>
public partial class Selector<T> : Node,
    System.Collections.Generic.ICollection<(T item, int weight)> where T : notnull {
    /// <summary>
    /// Whether the selector is weighted or not.
    /// </summary>
    [Export] protected bool IsWeighted { get; set; } = true;
    /// <summary>
    /// The collections of all items.
    /// </summary>
    protected TreeDictionary<int, (T item, int index)> Items { get; init; } = [];
    /// <summary>
    /// A lookup table for the weight and cumulative weight of each item.
    /// </summary>
    protected Dictionary<T, List<(int weight, int cumulative)>> Weights { get; init; } = [];
    /// <summary>
    /// A lookup table for the items with a given weight.
    /// </summary>
    protected TreeDictionary<int, List<(T item, int index)>> LookUp { get; init; } = [];
    /// <summary>
    /// The sum of weights of all items.
    /// </summary>
    protected int TotalWeight { get; set; } = 0;
    /// <summary>
    /// Whether the selector is empty or not.
    /// </summary>
    public bool IsEmpty => this.Items.Count == 0;
    /// <summary>
    /// The number of items in the selector.
    /// </summary>
    public int Count => this.Items.Count;
    /// <summary>
    /// Whether the selector is read-only or not.
    /// </summary>
    public bool IsReadOnly => false;

    /// <summary>
    /// Creates a new weighted selector.
    /// </summary>
    /// <returns></returns>
    public static Selector<T> Weighted() {
        return new Selector<T>() { IsWeighted = true };
    }

    /// <summary>
    /// Creates a new unweighted selector.
    /// </summary>
    /// <returns></returns>
    public static Selector<T> Unweighted() {
        return new Selector<T>() { IsWeighted = false };
    }

    /// <summary>
    /// Adds an item to the selector with a given weight.
    /// </summary>
    /// <param name="item">The item to be added.</param>
    /// <param name="weight">The weight of the item.</param>
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

    /// <summary>
    /// Adds a collection of items to the selector with given weights.
    /// </summary>
    /// <param name="items">The items to be added.</param>
    public void Add(params (T item, int weight)[] items) {
        foreach ((T item, int weight) in items) {
            this.Add(item, weight);
        }
    }

    /// <summary>
    /// Adds a collection of items to the selector with given weights.
    /// </summary>
    /// <param name="items">The items to be added.</param>
    public void Add(IEnumerable<(T item, int weight)> items) {
        foreach ((T item, int weight) in items) {
            this.Add(item, weight);
        }
    }

    /// <summary>
    /// Adds a collection of key-value pair items to the selector with given weights.
    /// </summary>
    /// <param name="items">The items to be added.</param>
    public void Add(params KeyValuePair<T, int>[] items) {
        foreach (KeyValuePair<T, int> pair in items) {
            this.Add(pair.Key, pair.Value);
        }
    }

    /// <summary>
    /// Adds a collection of key-value pair items to the selector with given weights.
    /// </summary>
    /// <param name="items">The items to be added.</param>
    public void Add(IEnumerable<KeyValuePair<T, int>> items) {
        foreach (KeyValuePair<T, int> pair in items) {
            this.Add(pair.Key, pair.Value);
        }
    }

    /// <summary>
    /// Selects a random item from the selector based on weighted probabilities.
    /// </summary>
    /// <returns>The randomly selected item.</returns>
    public T Select() {
        return this.Items.WeakPredecessor((int)(Randy.Randf() * this.TotalWeight)).Value.item;
    }

    /// <summary>
    /// Removes a specified number of copies of an item from the selector.
    /// </summary>
    /// <param name="item">The item to be removed.</param>
    /// <param name="k">The number of copies to remove.</param>
    /// <returns>True if the item was removed, false otherwise.</returns>
    /// <exception cref="IndexOutOfRangeException">There are fewer than
    /// <paramref name="k"/> copies of the target item.</exception>
    public virtual bool Remove(ref T item, int k = 0) {
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

    /// <summary>
    /// Retrieves the weight and cumulative weight of a specified copy of an item.
    /// </summary>
    /// <param name="item">The item to lookup.</param>
    /// <param name="k">The index of the copy to retrieve.</param>
    /// <returns>A tuple containing weight and cumulative weight.</returns>
    /// <exception cref="ArgumentException">The target item does not exist
    /// in the selector.</exception>
    /// <exception cref="IndexOutOfRangeException">There are fewer than
    /// <paramref name="k"/> copies of the target item.</exception>
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

    /// <summary>
    /// Retrieves the weight of the <paramref name="k"/>-th copy of an item.
    /// </summary>
    /// <param name="item">The item to lookup.</param>
    /// <param name="k">The index of the copy.</param>
    /// <returns>The weight of the specified copy.</returns> <summary>
    /// <exception cref="ArgumentException">The target item does not exist
    /// in the selector.</exception>
    /// <exception cref="IndexOutOfRangeException">There are fewer than
    /// <paramref name="k"/> copies of the target item.</exception>
    public int WeightOf(T item, int k = 1) {
        return this.WeightRecordOf(item, k).weight;
    }

    /// <summary>
    /// Retrieves the cumulative weight of the <paramref name="k"/>-th copy of an item.
    /// </summary>
    /// <param name="item">The item to lookup.</param>
    /// <param name="k">The index of the copy.</param>
    /// <returns>The cumulative weight of the specified copy.</returns>
    /// <exception cref="ArgumentException">The target item does not exist
    /// in the selector.</exception>
    /// <exception cref="IndexOutOfRangeException">There are fewer than
    /// <paramref name="k"/> copies of the target item.</exception>
    public int CumulativeWeightOf(T item, int k = 1) {
        return this.WeightRecordOf(item, k).cumulative;
    }

    /// <summary>
    /// Counts the number of copies of a specified item in the selector.
    /// </summary>
    /// <param name="item">The item to count.</param>
    /// <returns>The number of copies of the specified item.</returns>
    public int CountOf(T item) {
        return this.Weights.TryGetValue(item, out List<(int, int)>? weights) ? weights.Count : 0;
    }

    /// <summary>
    /// Retrieves the list of items with a specific weight.
    /// </summary>
    /// <param name="weight">The weight to filter items by.</param>
    /// <returns>A list of items with the specified weight.</returns>
    public List<(T item, int index)> WhereWeightIs(int weight) {
        if (this.LookUp.Contains(weight)) {
            return this.LookUp[weight];
        }
        return [];
    }

    /// <summary>
    /// Retrieves the list of items that are the heaviest in the selector.
    /// </summary>
    /// <returns>A list of items that are the heaviest.</returns>
    public List<(T item, int index)> Top() {
        return this.LookUp.First().Value;
    }

    /// <summary>
    /// Retrieves the list of items that are the lightest in the selector.
    /// </summary>
    /// <returns>A list of items that are the lightest.</returns>
    public List<(T item, int index)> Bottom() {
        return this.LookUp.Last().Value;
    }

    /// <summary>
    /// Adds a single item to the selector with a specified weight.
    /// </summary>
    /// <param name="item">The item-weight pair to be added.</param> <summary>
    public void Add((T item, int weight) item) {
        this.Add(item.item, item.weight);
    }

    /// <summary>
    /// Checks if the item exists in the selector with a specific weight.
    /// </summary>
    /// <param name="item">The item-weight pair to check for.</param>
    /// <returns>True if the item with the specified weight exists, false otherwise.</returns>
    public bool Contains((T item, int weight) item) {
        return this.Weights.TryGetValue(
            item.item, out List<(int weight, int _)>? weights
        ) && weights.Any(w => w.weight == item.weight);
    }

    /// <summary>
    /// Copies the contents of the selector to an array, starting at a specified index.
    /// </summary>
    /// <param name="array">The array to copy to.</param>
    /// <param name="arrayIndex">The index in the array to start copying.</param>
    public void CopyTo((T item, int weight)[] array, int arrayIndex) {
        foreach (KeyValuePair<int, (T item, int)> pair in this.Items) {
            if (arrayIndex >= array.Length) {
                break;
            }
            array[arrayIndex] = (pair.Value.item, this.WeightOf(pair.Value.item));
        }
    }

    /// <summary>
    /// Removes a specified item from the selector based on its weight.
    /// </summary>
    /// <param name="item">The item to be removed.</param>
    /// <returns>True if the item was removed, false otherwise.</returns>
    public bool Remove((T item, int weight) item) {
        if (this.Weights.TryGetValue(item.item, out List<(int weight, int)>? weights)) {
            return this.Remove(ref item.item, weights.FindIndex(w => w.weight == item.weight) + 1);
        }
        return false;
    }

    /// <summary>
    /// Clears all items from the selector.
    /// </summary>
    public virtual void Clear() {
        this.Items.Clear();
        this.Weights.Clear();
        this.TotalWeight = 0;
    }

    /// <summary>
    /// Returns an enumerator that iterates through the items in the selector.
    /// </summary>
    /// <returns>An enumerator for the items in the selector.</returns>
    public IEnumerator GetEnumerator() {
        return this.Items.GetEnumerator();
    }

    /// <summary>
    /// Returns an enumerator that iterates through the items in the selector
    /// with their weights.
    /// </summary>
    /// <returns>An enumerator for the items and weights.</returns>
    IEnumerator<(T item, int weight)> IEnumerable<(T item, int weight)>.GetEnumerator() {
        return this.Items.Select(entry => (entry.Value.item, this.WeightOf(
            entry.Value.item, entry.Value.index + 1
        ))).GetEnumerator();
    }

    /// <summary>
    /// Returns a string representation of the selector's contents, including weights.
    /// </summary>
    /// <returns>A string representation of the selector.</returns>
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