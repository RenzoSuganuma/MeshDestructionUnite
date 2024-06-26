using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary> Rydenクラスを簡単に利用するためのヘルパークラス </summary>
public static class RydenHelper
{
    /// <summary> 要素の重複を許さないでリストにリストを追加する </summary>
    /// <param name="targetList"></param>
    /// <param name="addingList"></param>
    private static void AddCuttedListToList(List<GameObject> targetList, List<GameObject> addingList)
    {
        foreach (var obj in addingList)
        {
            if (!targetList.Contains(obj))
            {
                targetList.Add(obj);
            }
        }
    }

    /// <summary> メッシュをカットする </summary>
    /// <param name="victim"></param>
    /// <param name="cuttedMeshes"></param>
    /// <param name="anchorPos"></param>
    /// <param name="planeNormal"></param>
    /// <param name="capMaterial"></param>
    /// <param name="makeGap"></param>
    public static void CutTheMesh(GameObject victim,
        List<GameObject> cuttedMeshes, Vector3 anchorPos, Vector3 planeNormal, Material capMaterial,
        bool makeGap = true)
    {
        List<GameObject> results = new List<GameObject>();

        if (cuttedMeshes.Count > 0) // もすでに切られている場合
        {
            foreach (var mesh in cuttedMeshes)
            {
                var result = Ryden.CutMesh(mesh, anchorPos, planeNormal, capMaterial, makeGap);
                AddCuttedListToList(results, result.ToList());
            }

            AddCuttedListToList(cuttedMeshes, results);
        }
        else // まだ切られてない場合
        {
            cuttedMeshes.Clear();
            var result = Ryden.CutMesh(victim, anchorPos, planeNormal, capMaterial, makeGap);
            AddCuttedListToList(cuttedMeshes, result.ToList());
        }
    }
}
