using System;
using SmasherDestruction.Nurikabe.Delanuay;
using UnityEngine;
using UnityEngine.PlayerLoop;
using Random = UnityEngine.Random;

public class Nurikabe_Dev : UnityEngine.MonoBehaviour
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
        Vector2[] xz = new Vector2[_mf.sharedMesh.vertices.Length];
        for (int i = 0; i < _mf.sharedMesh.vertices.Length; i++)
        {
            xz[i] = new Vector2(_mf.sharedMesh.vertices[i].x, _mf.sharedMesh.vertices[i].z);
        }
        var m = _d.CreateMesh(xz);
        var obj = new GameObject();
        var mf = obj.AddComponent<MeshFilter>();
        var mr = obj.AddComponent<MeshRenderer>();
        mf.sharedMesh = m;
    }
}