﻿using System;
using System.Linq;
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


            // 頂点ごとの色をキューブへ割り当て
            for (int i = 0; i < _v3d.Sites.Length; i++)
            {
                var list = _v3d.Sites[i];
                foreach (var vertexIndex in list)
                {
                    foreach (var v3dBorderIndices in _v3d.BorderVertices)
                    {
                        if (!v3dBorderIndices.Contains(vertexIndex))
                        {
                            var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                            cube.transform.localPosition = vertices[vertexIndex];
                            cube.transform.localScale = Vector3.one * .05f;
                            cube.GetComponent<MeshRenderer>().material.color = _v3d.Colors[i];
                        }
                    }
                }
            }

            for (int i = 0; i < _v3d.BorderVertices.Length; i++)
            {
                var list = _v3d.BorderVertices[i];
                foreach (var item in list)
                {
                    Debug.Log($"border = {item}");
                    var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    cube.transform.localPosition = vertices[item];
                    cube.transform.localScale = Vector3.one * .05f;
                    cube.GetComponent<MeshRenderer>().material.color = Color.black;
                }
            }
        }
    }
}