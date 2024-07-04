using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace VoronoiDiagram
{
    // 1．母点 の 配置
    // 2．二等分線の算出
    // 3．二等分線同士の交点の算出 【これがボロノイ頂点】
    // 4．二等分線どうしの交点が算出できたのでそれより先に線を伸ばす事はしない 【これがボロノイ辺】
    // 5．ボロノイ辺、頂点、母点をひとくくりにしたのがボロノイセル(VoronoiCell)

    /* 【 今回はXZ平面上でやる 】 */

    public class MonoVoronoi : MonoBehaviour
    {
        [SerializeField] private Int32 _sitesAmount;
        [SerializeField] private Vector3 _bounds;

        public List<VoronoiCell> VoronoiCells = new();

        private List<Vector3> _midPoints = new();
        private List<Vector3> _sites = new();

        private void Start()
        {
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(transform.position, _bounds);
            Gizmos.color = Color.cyan;

            foreach (var site in _sites)
            {
                Gizmos.DrawCube(site, Vector3.one / 2f);
            }

            Gizmos.color = Color.green;

            foreach (var tuple in _midPoints)
            {
                Gizmos.DrawCube(tuple, Vector3.one / 2f);
            }
        }

        private void Update()
        {
        }

        private void OnGUI()
        {
            if (GUILayout.Button(" CreateCells "))
            {
                CreateSites();
            }

            if (GUILayout.Button(" Bisectors "))
            {
                CalculateMidPointAndBisectorDir();
            }
        }

        private void CreateSites()
        {
            var x = Random.Range(-(_bounds.x / 2f), (_bounds.x / 2f));
            var y = 0f;
            var z = Random.Range(-(_bounds.z / 2f), (_bounds.z / 2f));

            List<Vector3> sites = new List<Vector3>();

            for (int i = 0; i < _sitesAmount; i++)
            {
                sites.Add(new Vector3(x, y, z));

                x = Random.Range(-(_bounds.x / 2f), (_bounds.x / 2f));
                z = Random.Range(-(_bounds.z / 2f), (_bounds.z / 2f));
            } // 母点 の 数だけループする

            _sites = sites;
        }

        private void CalculateMidPointAndBisectorDir()
        {
        }
    }

    public struct VoronoiCell
    {
        public HashSet<Tuple<Vector3, Vector3>> VoronoiEdges;
        public HashSet<Vector3> VoronoiVertices;
        public Vector3 Site;
    }
}