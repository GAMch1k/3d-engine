using System.Numerics;
using Utils;

namespace Prefabs;

public class Camera : GameObject
{
    public Vector3 Front { get; set; } = new Vector3(0, 0, -1);
    public Vector3 Up { get; set; } = new Vector3(0, 1, 0);
    public Vector3 Right { get; set; }
    public float Yaw { get; set; } = -90f;   // Left/right rotation
    public float Pitch { get; set; } = 0f;    // Up/down rotation
    public float MovementSpeed { get; set; } = 2f;
    public float MouseSensitivity { get; set; } = 0.1f;
    public Camera(
        Vector3 position,
        Vector3 rotation,
        Vector3 scale)
        : base(position, rotation, scale)
    {
        Right = Vector3.Normalize(Vector3.Cross(Front, Up));
    }

    public Matrix4x4 GetViewMatrix()
    {
        return Matrix4x4.CreateLookAt(Position, Position + Front, Up);
    }

    public void ProcessMouseMovement(float xOffset, float yOffset, bool constrainPitch = true)
    {
        xOffset *= MouseSensitivity;
        yOffset *= MouseSensitivity;

        Yaw += xOffset;
        Pitch += yOffset;

        if (constrainPitch)
        {
            Pitch = Math.Clamp(Pitch, -89f, 89f);
        }

        UpdateCameraVectors();
    }

    public void ProcessKeyboard(CameraMovement direction, float deltaTime)
    {
        float velocity = MovementSpeed * deltaTime;
        
        switch (direction)
        {
            case CameraMovement.Forward:
                Position += Front * velocity;
                break;
            case CameraMovement.Backward:
                Position -= Front * velocity;
                break;
            case CameraMovement.Left:
                Position -= Right * velocity;
                break;
            case CameraMovement.Right:
                Position += Right * velocity;
                break;
            case CameraMovement.Up:
                Position += Up * velocity;
                break;
            case CameraMovement.Down:
                Position -= Up * velocity;
                break;
        }
    }

    private void UpdateCameraVectors()
    {
        // Calculate new front vector
        Vector3 newFront;
        newFront.X = (float)(Math.Cos(MathHelper.DegreesToRadians(Yaw)) * Math.Cos(MathHelper.DegreesToRadians(Pitch)));
        newFront.Y = (float)Math.Sin(MathHelper.DegreesToRadians(Pitch));
        newFront.Z = (float)(Math.Sin(MathHelper.DegreesToRadians(Yaw)) * Math.Cos(MathHelper.DegreesToRadians(Pitch)));

        Front = Vector3.Normalize(newFront);

        // Recalculate right and up vectors
        Right = Vector3.Normalize(Vector3.Cross(Front, Vector3.UnitY));
        Up = Vector3.Normalize(Vector3.Cross(Right, Front));
    }
    
    public enum CameraMovement
    {
        Forward,
        Backward,
        Left,
        Right,
        Up,
        Down
    }
    
}