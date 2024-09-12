using System;
using UnityEngine;

namespace NawabariDestruction
{
    public class NawabariRuntime : MonoBehaviour
    {
        [SerializeField] private MeshFilter _mf;

        Nawabari _n = new();

        private void Start()
        {
            _n.Make(5, _mf.sharedMesh);
        }
    }
}