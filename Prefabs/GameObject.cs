namespace Prefabs;

using System.Numerics;


public class GameObject
{
    public Vector3 Position { get; set; } = Vector3.Zero;
    public Vector3 Rotation { get; set; } = Vector3.Zero;
    public Vector3 Scale { get; set; } = Vector3.One;

    public GameObject(
        Vector3 position,
        Vector3 rotation,
        Vector3 scale
    )
    {
        this.Position = position;
        this.Rotation = rotation;
        this.Scale = scale;
    }
}