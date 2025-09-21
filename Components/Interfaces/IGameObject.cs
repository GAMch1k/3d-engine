namespace Interfaces;

using System.Numerics;

public interface IGameObject
{
    string Name { get; set; }
    Vector3 Position { get; set; }
    Vector3 Rotation { get; set; }
    Vector3 Scale { get; set; }

    void Update(float deltatime);
}