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

            // ↓ これをVoronoi3Dクラス内で実行しておく
            // １頂点１領域になるように排他処理
            for (int i = _v3d.Sites.Length - 1; i >= 0; i--)
            {
                // 上塗りを繰り返すような処理をしているので最後に上塗りをしたもの
                // を先に塗られたものから排除して。。。を繰り返す 
                var excludeIndices = _v3d.Sites[i];
                for (int j = i - 1; j >= 0; j--)
                {
                    foreach (var index in excludeIndices)
                    {
                        _v3d.Sites[j].Remove(index);
                    }
                }
            }

            // 頂点ごとの色をキューブへ割り当て
            for (int i = 0; i < _v3d.Sites.Length; i++)
            {
                var list = _v3d.Sites[i];
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