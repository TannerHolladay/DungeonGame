using UnityEngine;
using System.Collections;

public static class ExtensionMethods
{
    public static Vector3 ToVector3(this Vector2 input) => new Vector3(input.x, 0, input.y);
    public static Vector3 ToVector3(this Vector3 input) => new Vector3(input.x, 0, input.y);
    public static Vector3 Flatten(this Vector3 input) => new Vector3(input.x, 0, input.z);
    public static Vector2 ToVector2(this Vector3 input) => new Vector2(input.x, input.z);
    public static float GetAngleTo(this Transform origin, Transform target) => Vector2.Angle((target.position - origin.position).ToVector2(), origin.forward)*2;
    public static float GetAngleTo(this Transform origin, Vector3 target) => Vector2.Angle((target - origin.position).ToVector2(), origin.forward)*2;
}

public interface IDamageable
{
    void TakeDamage(float ammount, Vector3 hitPoint = default);
}