using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SmasherDestruction.Editor
{
    /// <summary>
    /// 被破壊メッシュを生成する機能を提供する
    /// </summary>
    /// Ver 1.0.0
    public static class Gouwang
    {
        /// <summary>
        /// 保存先ディレクトリを探す。
        /// </summary>
        /// <param name="filePath"></param>
        public static void FindSaveTargetDirectory(string filePath)
        {
            if (!Directory.Exists(filePath))
            {
                Directory.CreateDirectory(filePath);
            }
        }

        /// <summary>
        /// メッシュをカットする
        /// </summary>
        public static void CutTheMesh(
            GameObject victim,
            List<GameObject> cuttedMeshes,
            Vector3 anchorPos,
            Vector3 planeNormal,
            Material capMaterial)
        {
            List<GameObject> frag = new List<GameObject>();

            if (cuttedMeshes.Count > 0) // もすでに切られている場合
            {
                foreach (var mesh in cuttedMeshes)
                {
                    var result = 
                        Tsujigiri.CutMesh(mesh, anchorPos, planeNormal, capMaterial, false);
                    AddFragmentToList(frag, result.ToList());
                }
                AddFragmentToList(cuttedMeshes, frag);
            }
            else // まだ切られてない場合
            {
                cuttedMeshes.Clear();
                var result = 
                    Tsujigiri.CutMesh(victim, anchorPos, planeNormal, capMaterial, false);
                AddFragmentToList(cuttedMeshes, result.ToList());
            }
        }

        /// <summary>
        /// 要素の重複を許さないでリストにリストを追加する
        /// </summary>
        /// <param name="fragmentList"></param>
        /// <param name="sourceList"></param>
        private static void AddFragmentToList(List<GameObject> fragmentList, List<GameObject> sourceList)
        {
            foreach (var obj in sourceList)
            {
                if (!fragmentList.Contains(obj))
                {
                    fragmentList.Add(obj);
                }
            }
        }
    }
}