using System;
using SmasherDestruction.Nurikabe.Delanuay;
using UnityEngine;
using Random = UnityEngine.Random;

public class Nurikabe_Dev : MonoBehaviour
{
    [SerializeField] private MeshFilter _mf;
    private DelanuayTriangulator2D _d = new();

    Vector2[] MakePoints(int count, float boundsHalf)
    {
        var arr = new Vector2[count];
        for (int i = 0; i < count; i++)
        {
            var v = new Vector2(Random.Range(-boundsHalf, boundsHalf), Random.Range(-boundsHalf, boundsHalf));
            arr[i] = v;
        }

        return arr;
    }

    private void Start()
    {
        Vector2[] xy = new Vector2[_mf.sharedMesh.vertices.Length];
        for (int i = 0; i < _mf.sharedMesh.vertices.Length; i++)
        {
            xy[i] = new Vector2(_mf.sharedMesh.vertices[i].x, _mf.sharedMesh.vertices[i].y);
        }
        var m = _d.CreateMeshFromXY(xy);
        var obj = new GameObject();
        var mf = obj.AddComponent<MeshFilter>();
        var mr = obj.AddComponent<MeshRenderer>();
        mf.sharedMesh = m;
    }
}