using UnityEngine;

namespace Runtime.Utility
{
    public static class Extension
    {
        public static Vector3 ClosestPointOnRay(this Ray ray, Vector3 point)
        {
            var dot = Vector3.Dot(point - ray.origin, ray.direction);
            return ray.GetPoint(dot);
        }
    }
}