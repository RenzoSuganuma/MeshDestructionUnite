using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SmasherDestruction.Editor
{
    /// <summary> ランタイムで辻斬りクラスを簡単に利用するためのヘルパークラス </summary>
    public static class TsujigiriUtility
    {
        /// <summary> 要素の重複を許さないでリストにリストを追加する </summary>
        /// <param name="fragmentList"></param>
        /// <param name="sourceList"></param>
        private static void AddCuttedFragmentListToList(
            List<GameObject> fragmentList,
            List<GameObject> sourceList)
        {
            foreach (var obj in sourceList)
            {
                if (!fragmentList.Contains(obj))
                {
                    fragmentList.Add(obj);
                }
            }
        }

        /// <summary> メッシュをカットする </summary>
        /// <param name="victim"></param>
        /// <param name="cuttedMeshes"></param>
        /// <param name="anchorPos"></param>
        /// <param name="planeNormal"></param>
        /// <param name="insideMaterial"></param>
        /// <param name="makeGap"></param>
        public static void CutTheMesh(GameObject victim,
            List<GameObject> cuttedMeshes,
            Vector3 anchorPos,
            Vector3 planeNormal,
            Material insideMaterial,
            bool makeGap = true)
        {
            List<GameObject> results = new List<GameObject>();

            if (cuttedMeshes.Count > 0) // もすでに切られている場合
            {
                foreach (var mesh in cuttedMeshes)
                {
                    var result = Tsujigiri.CutMesh(mesh, anchorPos, planeNormal, insideMaterial, makeGap);
                    AddCuttedFragmentListToList(results, result.ToList());
                }

                AddCuttedFragmentListToList(cuttedMeshes, results);
            }
            else // まだ切られてない場合
            {
                cuttedMeshes.Clear();
                var result = Tsujigiri.CutMesh(victim, anchorPos, planeNormal, insideMaterial, makeGap);
                AddCuttedFragmentListToList(cuttedMeshes, result.ToList());
            }
        }
    }
}