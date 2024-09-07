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
            Debug.Log(_v3d.GetInformation());
            Debug.Log(_v3d.Sites[0].Count);
        }
    }
}