using System;
using SmasherDestruction.Kamaitachi.Voronoi;
using UnityEngine;

namespace SmasherDestruction.Kamaitachi.Dev
{
    public class Kamaitachi_Dev : MonoBehaviour
    {
        [SerializeField] private MeshFilter _mf;

        private Voronoi3D _v3d = new();

        private void Start()
        {
            _v3d.CreateVoronoi(5, _mf.sharedMesh);
            var vertices = _mf.sharedMesh.vertices;
            for (int i = 0; i < _v3d.Sites.Length; i++)
            {
                var list = _v3d.Sites[3];
                foreach (var item in list)
                {
                    var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    cube.transform.localPosition = vertices[item];
                    cube.transform.localScale = Vector3.one * .05f;
                    cube.GetComponent<MeshRenderer>().material.color = _v3d.Colors[i];
                }
            }
        }
    }
}