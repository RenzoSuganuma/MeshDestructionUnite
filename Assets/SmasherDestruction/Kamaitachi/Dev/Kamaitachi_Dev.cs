using System;
using SmasherDestruction.Kamaitachi.Voronoi;
using UnityEngine;

namespace SmasherDestruction.Kamaitachi.Dev
{
    public class Kamaitachi_Dev : MonoBehaviour
    {
        [SerializeField] private MeshFilter _mf;

        private Voronoi3DForMesh _v = new();

        private void Start()
        {
            _v.CreateVoronoi(32 , _mf.sharedMesh);
        }
    }
}