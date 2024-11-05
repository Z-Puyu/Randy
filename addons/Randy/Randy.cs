#if TOOLS
using Godot;
using System.Collections.Generic;
using System.Linq;

namespace RandyPlugin;
[Tool]
public partial class Randy : EditorPlugin {
    private static RandomNumberGenerator Rand { get; } = new RandomNumberGenerator();
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
	public override void _EnterTree() {
        AddCustomType("Selector", "Node", GD.Load<Script>("res://addons/Randy/Randy.cs"), null);
	}

	public override void _ExitTree() {
        RemoveCustomType("Selector");
	}

    public static uint Randi() {
        return Randy.Rand.Randi();
    }

    public static int Randi(int a, int b) {
        return a <= b ? Randy.Rand.RandiRange(a, b) : Randy.Rand.RandiRange(b, a);
    }

    public static int Randi((int a, int b) range) {
        return Randy.Randi(range.a, range.b);
    }

    public static int Randi(Vector2I range) {
        return Randy.Randi(range.X, range.Y);
    }

    public static double Randf() {
        return Randy.Rand.Randf();
    }

    public static double Randf(float a, float b) {
        return a <= b ? Randy.Rand.RandfRange(a, b) : Randy.Rand.RandfRange(b, a);
    }

    public static double Randf((float a, float b) range) {
        return Randy.Randf(range.a, range.b);
    }

    public static double Randf(Vector2 range) {
        return Randy.Randf(range.X, range.Y);
    }

    public static bool FairCoin() {
        return Randy.Randi(0, 1) == 0;
    }

    public static Vector2I RandomDirection(bool allowDiagonals = false) {
        int idx = Randy.Randi(0, Directions.Length - 1);
        return allowDiagonals ? Directions[idx] : Directions[idx / 2 * 2];
    }

    public static IEnumerable<Vector2I> RandomWalk(int length, bool allowDiagonals = false) {
        List<Vector2I> walk = new List<Vector2I>(length);
        for (int i = 0; i < length; i += 1) {
            walk.Add(Randy.RandomDirection(allowDiagonals));
        }
        return walk.AsEnumerable();
    }
}
#endif
