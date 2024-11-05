# Randy: Empower Randomness in Godot 4
A C# plugin for Godot 4 to provide various utility functionalities related to randomness.

## The Random Selector
The `Selector<T>` class is a custom node that provides a way to randomly select an element from a list of elements based on weighted probabilities. It uses a weighted random algorithm to ensure that elements with higher weights are more likely to be selected. It extends `System.Collections.Generic.ICollection<(T item, int weight)>` to support LINQ.

### Properties

- `IsEmpty`: Returns a boolean value indicating whether the selector is empty or not.

- `Count`: Returns the number of items in the selector.

- `IsReadOnly`: Returns `false`.

### Methods

- `Add(T item, int weight)`: Adds an item to the selector with the given weight.

- `Add((T item, int weight) item)`: Adds an item to the selector with the given weight.

- `Add(params (T item, int weight)[] items)`: Adds multiple items to the selector with their respective weights. The item-weight pairs should be served as 2D-tuples.

- `Add(IEnumerable<(T item, int weight)> items)`: Adds multiple items to the selector with their respective weights. The item-weight pairs should be served as 2D-tuples.

- `Add(params KeyValuePair<T, int>[] items)`: Adds multiple items to the selector with their respective weights. The item-weight pairs should be served as key-value pairs.

- `Add(IEnumerable<KeyValuePair<T, int>> items)`: Adds multiple items to the selector with their respective weights. The item-weight pairs should be served as key-value pairs.

- `Clear()`: Removes all items from the selector.

- `Contains(T item)`: Returns a boolean value indicating whether the selector contains the given item or not.

- `CopyTo(KeyValuePair<T, int>[] array, int arrayIndex)`: Copies the items and their weights to the given array starting at the given index.

- `Remove(ref T item, int k = 0)`: Removes the given item from the selector and store it in the `item` parameter. If $k > 0$, it will remove the $k$-th occurrence of the item. Otherwise, the last occurrence of the item will be removed. This method returns a boolean value indicating whether the removal was successful or not.

- `WeightRecordOf(T item, int k = 1)`: Returns a 2D tuple $(w, c)$ where $w$ is the weight and $c$ is the cumulative weight of the $k$-th occurrence of an item.

- `WeightOf(T item, int k = 1)`: Returns the weight of the $k$-th occurrence of an item.

- `CumulativeWeightOf(T item, int k = 1)`: Returns the cumulative weight of the $k$-th occurrence of an item.

- `Select()`: Returns a randomly selected item from the selector based on weighted probabilities.

- `CountOf(T item)`: Returns the number of occurrences of the given item in the selector.

- `WhereWeightIs(int weight)`: Returns a list of items with the given weight.

- `Top()`: Returns a collection of items with the highest weights.

- `Bottom()`: Returns a collection of items with the lowest weights.

- `Contains((T item, int weight) item)`: Returns a boolean value indicating whether the selector contains the given item-weight pair or not.

- `GetEnumerator()`: Returns an enumerator that iterates through the items and their weights in the selector.

- `IEnumerable<(T item, int weight)>.GetEnumerator()`: Returns an enumerator that iterates through the items and their weights in the selector.

## The Non-generic Random Selector

Since Godot's inspector does not support generic types, the non-generic `Selector` class is provided to extend the `Selector<object>` class. It provides the same functionality as the generic class, but without the type parameter.

### Export Properties

- `IsWeighted`: A boolean value indicating whether the selector should use weighted probabilities or not.

- `IsTyped`: A boolean value indicating whether the selector should only accept items of the same type or not. If `true`, the selector will only accept items if the same type as the first item added to it. This type constraint will be reset when the selector becomes empty.

- `ItemData`: A collection of items to be used to initialise the selector.

## Selectable Item

The `SelectableItem` class is a custom Godot resource that provides a way to represent an item that can be selected by the `Selector` class. The user is free to define the following two properties:

### Export Properties

- `Weight`: An integer value representing the weight of the item.

- `Item`: A `Variant`-compatible object representing an item.

### Utility Methods

The following utility methods are provided as static methods under the `Randy` class:

- Generate random integers:

    - `Randi()`: A 32-bit unsigned integer within $\left[0, 2^{32} - 1\right]$.

    - `Randi(int a, int b)`: A 32-bit signed integer within $\left[a, b\right]$.

    - `Randi((int a, int b) range)`: A 32-bit signed integer within $\left[a, b\right]$.

    - `Randi(Vector2I range)`: A 32-bit signed integer within $\left[x, y\right]$.

- Generate random floating-point numbers:

    - `Randf()`: A floating-point number within $\left[0, 1\right]$.

    - `Randf(float a, float b)`: A floating-point number within $\left[a, b\right]$.

    - `Randf((float a, float b) range)`: A floating-point number within $\left[a, b\right]$.

    - `Randf(Vector2 range)`: A floating-point number within $\left[x, y\right]$.

- Generate random boolean values:

    - `FairCoin()`: A random boolean value with equal probability of `true` and `false`.

- Generate random directions:

    - `RandomDirection(bool allowDiagonals = false)`: A uniformly distributed random direction vector in a 2D grid. If `allowDiagonals` is `true`, the direction can be any of the four cardinal directions or a diagonal direction.

    - `RandomWalk(int length, bool allowDiagonals = false)`: A sequence of uniformly distributed random direction vectors in a 2D grid. If `allowDiagonals` is `true`, the direction can be any of the four cardinal directions or a diagonal direction.