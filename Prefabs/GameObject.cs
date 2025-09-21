namespace Prefabs;

using System.Numerics;
using Interfaces;

public class GameObject : IGameObject
{
    public string Name { get; set; } = "undefined";
    public Vector3 Position { get; set; } = Vector3.Zero;
    public Vector3 Rotation { get; set; } = Vector3.Zero;
    public Vector3 Scale { get; set; } = Vector3.One;

    public event Action<float> OnUpdate;

    public void Update(float deltatime)
    {
        OnUpdate?.Invoke(deltatime);
    }

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