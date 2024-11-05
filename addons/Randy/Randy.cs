#if TOOLS
using Godot;
using System.Collections.Generic;
using System.Linq;

namespace RandyPlugin;
[Tool]
public partial class Randy : EditorPlugin {
    /// <summary>
    /// A Godot RandomNumberGenerator instance for generating random numbers.
    /// </summary>
    private static RandomNumberGenerator Rand { get; } = new RandomNumberGenerator();
    /// <summary>
    /// An array of all possible directions in a 2D grid.
    /// </summary>
    private static Vector2I[] Directions = [
        new Vector2I(1, 0),
        new Vector2I(1, 1),
        new Vector2I(0, 1),
        new Vector2I(-1, 1),
        new Vector2I(-1, 0),
        new Vector2I(-1, -1),
        new Vector2I(0, -1),
        new Vector2I(1, -1)
    ];

    /// <summary>
    /// Called when the node is added to the scene tree, used to register custom types.
    /// </summary>
 public override void _EnterTree() {
        AddCustomType("Selector", "Node", GD.Load<Script>("res://addons/Randy/Randy.cs"), null);
 }

    /// <summary>
    /// Called when the node is removed from the scene tree, used to unregister custom types.
    /// </summary>
 public override void _ExitTree() {
        RemoveCustomType("Selector");
 }

    /// <summary>
    /// Returns a pseudo-random 32-bit unsigned integer between 0 and 4294967295 (inclusive).
    /// </summary>
    /// <returns>A pseudo-random 32-bit unsigned integer between 0 and 4294967295 (inclusive).</returns>
    public static uint Randi() {
        return Randy.Rand.Randi();
    }

    /// <summary>
    /// Returns a pseudo-random 32-bit signed integer between
    /// <paramref name="a"/> and <paramref name="b"/> (inclusive).
    /// This method does not care about the numerical relationship between
    /// <paramref name="a"/> and <paramref name="b"/>.
    /// </summary>
    /// <param name="a">One of the end points of the range.</param>
    /// <param name="b">The other end point of the range.</param>
    /// <returns>A pseudo-random 32-bit signed integer between
    /// <paramref name="a"/> and <paramref name="b"/> (inclusive).</returns>
    public static int Randi(int a, int b) {
        return a <= b ? Randy.Rand.RandiRange(a, b) : Randy.Rand.RandiRange(b, a);
    }

    /// <summary>
    /// Returns a pseudo-random 32-bit signed integer within the
    /// <paramref name="range"/> (end points included).
    /// </summary>
    /// <param name="range">A 2-tuple representing the range of the integer.</param>
    /// <returns>A pseudo-random 32-bit signed integer within the
    /// <paramref name="range"/> (end points included).</returns>
    public static int Randi((int a, int b) range) {
        return Randy.Randi(range.a, range.b);
    }

    /// <summary>
    /// Returns a pseudo-random 32-bit signed integer within the range represented by a
    /// 2D vector (inclusive of the end points).
    /// </summary>
    /// <param name="range">A 2D integral vector representing the range of the integer.</param>
    /// <returns>A pseudo-random 32-bit signed integer within the range represented by a
    /// 2D vector (inclusive of the end points).</returns>
    public static int Randi(Vector2I range) {
        return Randy.Randi(range.X, range.Y);
    }

    /// <summary>
    /// Returns a pseudo-random float between 0.0 and 1.0 (inclusive).
    /// </summary>
    /// <returns>A pseudo-random float between 0.0 and 1.0 (inclusive).</returns>
    public static double Randf() {
        return Randy.Rand.Randf();
    }

    /// <summary>
    /// Returns a pseudo-random float within the range
    /// [<paramref name="a"/>, <paramref name="b"/>] (inclusive of the end points).
    /// </summary>
    /// <param name="a">One of the end points of the range.</param>
    /// <param name="b">The other end point of the range.</param>
    /// <returns>A pseudo-random float within the range
    /// [<paramref name="a"/>, <paramref name="b"/>] (inclusive of the end points).</returns>
    public static double Randf(float a, float b) {
        return a <= b ? Randy.Rand.RandfRange(a, b) : Randy.Rand.RandfRange(b, a);
    }

    /// <summary>
    /// Returns a pseudo-random float within a specified range represented as a tuple.
    /// </summary>
    /// <param name="range">A 2-tuple representing the range of the float.</param>
    /// <returns>A pseudo-random float within the range represented by a tuple.</returns>
    public static double Randf((float a, float b) range) {
        return Randy.Randf(range.a, range.b);
    }

    /// <summary>
    /// Returns a pseudo-random float within the range represented by a 2D vector.
    /// </summary>
    /// <param name="range">A 2D vector representing the range of the float.</param>
    /// <returns>A pseudo-random float within the range represented by a 2D vector.</returns>
    public static double Randf(Vector2 range) {
        return Randy.Randf(range.X, range.Y);
    }

    /// <summary>
    /// Simulates a fair coin toss, returning true or false with equal probability.
    /// </summary>
    /// <returns>A boolean result of a fair coin toss.</returns>
    public static bool FairCoin() {
        return Randy.Randi(0, 1) == 0;
    }

    /// <summary>
    /// Returns a random direction from the defined 2D grid directions.
    /// Optionally allows diagonal directions.
    /// </summary>
    /// <param name="allowDiagonals">Boolean indicating whether to include diagonal directions.</param>
    /// <returns>A random direction as a Vector2I.</returns>
    public static Vector2I RandomDirection(bool allowDiagonals = false) {
        int idx = Randy.Randi(0, Directions.Length - 1);
        return allowDiagonals ? Directions[idx] : Directions[idx / 2 * 2];
    }

    /// <summary>
    /// Generates a random walk represented as a sequence of directions.
    /// </summary>
    /// <param name="length">The length of the random walk.</param>
    /// <param name="allowDiagonals">Boolean indicating whether to include diagonal movements.</param>
    /// <returns>An enumerable collection of Vector2I representing the random walk.</returns>
    public static IEnumerable<Vector2I> RandomWalk(int length, bool allowDiagonals = false) {
        List<Vector2I> walk = new List<Vector2I>(length);
        for (int i = 0; i < length; i += 1) {
            walk.Add(Randy.RandomDirection(allowDiagonals));
        }
        return walk.AsEnumerable();
    }
}
#endif