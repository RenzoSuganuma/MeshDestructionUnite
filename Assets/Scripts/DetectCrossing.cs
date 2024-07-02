using UnityEngine;

public static class DetectCrossing
{
    /// <summary> ２つの線分【正規化されていないベクトル】が交差しているか判定する </summary>
    public static bool IsCrossing(Vector3 p, Vector3 q, Vector3 a, Vector3 b)
    {
        var pq = q - p;
        var pa = a - p;
        var pb = b - p;
        var ab = b - a;
        var ap = p - a;
        var aq = q - a;

        var crossing = (Vector3.Cross(pq, pb).y * Vector3.Cross(pq, pa).y < 0) &&
                       (Vector3.Cross(ab, ap).y * Vector3.Cross(ab, aq).y < 0);

        return crossing;
    }
}