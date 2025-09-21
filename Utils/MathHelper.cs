namespace Utils;

public static class MathHelper
{
    public static float DegreesToRadians(float degrees)
    {
        return degrees * (float)Math.PI / 180f;
    }

    public static float RadiansToDegrees(float radians)
    {
        return radians * 180f / (float)Math.PI;
    }
}