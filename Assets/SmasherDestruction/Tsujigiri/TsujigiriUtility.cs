using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SmasherDestruction.Editor
{
    /// <summary> ランタイム/エディタで辻斬りクラスを簡単に利用するためのヘルパークラス </summary>
    public static class TsujigiriUtility
    {
        /// <summary> メッシュをカットする </summary>
        public static void CutTheMesh(
            GameObject victim,
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
                    var frag = Tsujigiri.CutMesh(mesh, anchorPos, planeNormal, insideMaterial, makeGap);
                    AddFragmentToList(results, frag.ToList());
                }

                AddFragmentToList(cuttedMeshes, results);
            }
            else // まだ切られてない場合
            {
                cuttedMeshes.Clear();
                var frag = Tsujigiri.CutMesh(victim, anchorPos, planeNormal, insideMaterial, makeGap);
                AddFragmentToList(cuttedMeshes, frag.ToList());
            }
        }

        /// <summary> 要素の重複を許さないでリストにリストを追加する </summary>
        /// <param name="fragmentList">破片が格納されているリスト</param>
        /// <param name="newFragments">新たに生成された破片のリスト</param>
        private static void AddFragmentToList(
            List<GameObject> fragmentList,
            List<GameObject> newFragments)
        {
            foreach (var obj in newFragments)
            {
                if (!fragmentList.Contains(obj))
                {
                    fragmentList.Add(obj);
                }
            }
        }
    }
}