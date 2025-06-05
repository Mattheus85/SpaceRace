using Godot;
using System;
using System.Collections.Generic;

public partial class SpawnManager : Node
{
    public static SpawnManager Instance { get; private set; }

    [Export] private float spawnRadius = 1.5f; // Spawn within 1.5x viewport
    [Export] private float cullRadius = 1.25f; // Cull beyond 2x viewport
    private float InnerRadiusBufferFactor = 1.5f;
    private float DefaultOuterRadiusFactor = 2.5f;
    private int firstFewObjects = 25;
    private const int FIRST_FEW_OBJECTS_RESET = 25;

    private Camera2D camera;
    private Vector2 viewportSize;
    private Dictionary
        <string,
        (
            List<Node> objects,
            int maxCount,
            PackedScene providedPackedScene,
            Node parent,
            Action<Node2D> configure
        )
        > spawnedObjects = new();

    public override void _Ready()
    {
        if (Instance == null)
        {
            Instance = this;
            viewportSize = GetViewport().GetVisibleRect().Size;
            camera = GetViewport().GetCamera2D();
        }
        else
        {
            QueueFree();
        }
    }

    public override void _Process(double delta)
    {
        if (camera == null) return;
        var player = GetNode<CharacterBody2D>("/root/Main/Player");
    }

    private void RemoveOutOfSightObjects(List<Node> objects)
    {
        objects.RemoveAll(node =>
        {
            if (node is Node2D node2D && node2D.GlobalPosition.DistanceTo(camera.GlobalPosition) > viewportSize.Length() * cullRadius)
            {
                // GD.Print($"DEAD {node2D.GetChild(0).Name} at {node2D.GlobalPosition}");
                node2D.QueueFree();
                return true;
            }
            if (node is Control control && control.GlobalPosition.DistanceTo(camera.GlobalPosition) > viewportSize.Length())
            {
                // GD.Print($"Culled Control at {control.GlobalPosition}");
                control.QueueFree();
                return true;
            }
            return false;
        });
    }

    private void InternalSpawnObjects(CharacterBody2D player, List<Node> objects, int maxCount, PackedScene providedPackedScene, Node parent, Action<Node2D> configure)
    {
        while (firstFewObjects > 0/* || objects.Count < maxCount*/)
        {
            var obj = providedPackedScene.Instantiate<Node2D>();
            Vector2 velocity = player.Velocity.Normalized();
            float angle = velocity.Angle() + Mathf.DegToRad(GD.Randf() * 90f - 45f);
            float distance = Mathf.Lerp(1200f, 2000f, Mathf.Abs(Mathf.Cos(angle - velocity.Angle())));
            Vector2 spawnPos = camera.GlobalPosition + Vector2.FromAngle(angle) * distance;
            obj.GlobalPosition = spawnPos;
            configure(obj);
            objects.Add(obj);
            parent.AddChild(obj);
            firstFewObjects--;
        }
        firstFewObjects = FIRST_FEW_OBJECTS_RESET;
    }

    public void SpawnObjects<T>(string typeKey, int maxCount, PackedScene providedPackedScene, bool isOrdered, Node parent, Action<T> configure) where T : Node2D
    {
        if (!spawnedObjects.ContainsKey(typeKey))
        {
            spawnedObjects[typeKey] = (new List<Node>(), maxCount, providedPackedScene, parent, x => configure((T)x));
        }
        var (objects, _, _, _, _) = spawnedObjects[typeKey];
        while (objects.Count < maxCount)
        {
            var obj = providedPackedScene.Instantiate<T>();
            // The camera's canvas_transform projects camera-local coordinates to viewport coordinates.
            // Its inverse projects viewport coordinates to camera-local coordinates.
            // The scale of the canvas_transform indicates how many viewport pixels correspond to one unit in camera-local space.
            // Dividing the viewport's pixel size by this scale gives the size of the viewport in camera-local units.
            // If the camera itself isn't scaled or rotated relative to its parent, these are effectively global units.
            Transform2D camCanvasTransform = camera.GetCanvasTransform();
            Vector2 viewSizeInGlobalUnits = viewportSize / camCanvasTransform.Scale;

            // Determine the furthest distance from camera center to any edge of the screen in global units.
            float screenEdgeDist = Mathf.Max(viewSizeInGlobalUnits.X, viewSizeInGlobalUnits.Y) / 2.0f;

            float innerRadius = screenEdgeDist * InnerRadiusBufferFactor; // Spawn just outside the screen edge.
                                                                          // spawnRadius (class field, e.g., 1.5f) determines how much further out from innerRadius objects can spawn.
            float outerRadius = innerRadius * spawnRadius;

            // Ensure outerRadius is always greater than innerRadius, providing a valid spawn ring.
            // If spawnRadius is too small (e.g., <= 1.0), default to a 1.5x multiplier for the outer ring to prevent issues.
            if (outerRadius <= innerRadius)
            {
                outerRadius = innerRadius * DefaultOuterRadiusFactor;
            }

            float randomAngle = GD.Randf() * Mathf.Pi * 2f; // Random angle in radians (0 to 2PI).
            float actualSpawnDistance = (float)GD.RandRange(innerRadius, outerRadius); // Random distance within the ring.

            Vector2 spawnRingOffset = Vector2.FromAngle(randomAngle) * actualSpawnDistance;
            obj.GlobalPosition = camera.GlobalPosition + spawnRingOffset;
            configure(obj);
            objects.Add(obj);
            if (isOrdered)
                objects.Sort((a, b) => ((Node2D)a).Scale.Length().CompareTo(((Node2D)b).Scale.Length()));
            parent.AddChild(obj);
        }
    }
}
