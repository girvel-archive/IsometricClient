using UnityEngine;
using VectorNet;

namespace Assets.Code.Tools.Extensions
{
    public static class VectorHelper
    {
        public static IntVector ToIntVector(this Vector2 vector2)
        {
            return new IntVector((int) vector2.x, (int) vector2.y);
        }

        public static IntVector ToIntVector(this Vector3 vector3)
        {
            return new IntVector((int) vector3.x, (int) vector3.y, (int) vector3.z);
        }

        public static Vector2 ToVector2(this IntVector intVector)
        {
            return new Vector2(intVector.X, intVector.Y);
        }

        public static Vector3 ToVector3(this IntVector intVector)
        {
            return new Vector3(intVector.X, intVector.Y, intVector.Z);
        }
    }
}