
using System;
using SmasherDestruction.Nurikabe.Delanuay;
using UnityEngine;
using UnityEngine.PlayerLoop;
using Random = UnityEngine.Random;

public class Nurikabe_Dev : UnityEngine.MonoBehaviour
{
    private DelanuayTriangulator2D _d = new();
    
    Vector2[] MakePoints(int count)
    {
        var arr = new Vector2[count];
        for (int i = 0; i < count; i++)
        {
            var v = new Vector2(Random.Range(0, 32), Random.Range(0, 32));
            arr[i] = v;
        }

        return arr;
    }

    private void Start()
    {
        var m = _d.CreateMesh(MakePoints(100));
        var obj = new GameObject();
        var mf = obj.AddComponent<MeshFilter>();
        var mr = obj.AddComponent<MeshRenderer>();
        mf.sharedMesh = m;
    }
}