using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
using Random = System.Random;
using SmasherDestruction.Editor;

namespace GouwangDestruction.Core
{
    public static class GouwangDestructionCore
    {
        #region 剛腕【旧アームストロング】

        private static Material _capMaterial;
        private static GameObject _victimObject;
        private static GameObject _planeObject;
        private static bool _makeGap = false;
        private static Random _random = new Random();
        private static List<GameObject> _cuttedMeshes = new List<GameObject>();

        public static void CutRandomly_ArmStrong(GameObject victimObject, List<GameObject> cuttedMeshes,
            Transform planeObject,
            Material capMaterial, bool makeGap, int fragType)
        {
            _victimObject = victimObject;
            _cuttedMeshes = cuttedMeshes;
            _planeObject = planeObject.gameObject;
            _capMaterial = capMaterial;
            _makeGap = makeGap;

            switch (fragType)
            {
                case 0:
                {
                    Frag_0(_victimObject, _cuttedMeshes, _planeObject.transform, _capMaterial, _makeGap);
                    break;
                }
                case 1:
                {
                    Frag_1(_victimObject, _cuttedMeshes, _planeObject.transform, _capMaterial, _makeGap);
                    break;
                }
                case 2:
                    Frag_2(_victimObject, _cuttedMeshes, _planeObject.transform, _capMaterial, _makeGap);
                    break;
            }
        }

        private static void CutMesh_ArmStrong()
        {
            if (_victimObject is null) return;

            TsujigiriUtility.CutTheMesh(_victimObject, _cuttedMeshes, _planeObject.transform.position,
                _planeObject.transform.up,
                _capMaterial, _makeGap);
        }

        #region 破壊パターン：アームストロング

        private static void Frag_0(GameObject victimObject, List<GameObject> cuttedMeshes,
            Transform planeObject,
            Material capMaterial, bool makeGap)
        {
            var r = _random.Next(0, 256);
            var r1 = _random.Next(0, 256);
            var r2 = _random.Next(0, 256);

            planeObject.up = Vector3.up;

            planeObject.up = Vector3.right;
            CutMesh_ArmStrong();

            planeObject.position += Vector3.down * .5f;

            var v = (Vector3.right - Vector3.up).normalized;
            planeObject.up = v;
            CutMesh_ArmStrong();

            v = (Vector3.right + Vector3.up).normalized;
            planeObject.up = v;
            CutMesh_ArmStrong();

            planeObject.position -= Vector3.right * .5f;

            planeObject.up = new Vector3(-1, 0, 1);
            CutMesh_ArmStrong();

            planeObject.right = new Vector3(r, r1, r2);
            CutMesh_ArmStrong();

            planeObject.position = Vector3.zero;

            r = _random.Next(Int32.MinValue >> 30, Int32.MaxValue);
            r1 = _random.Next(Int32.MinValue >> 17, Int32.MaxValue);
            r2 = _random.Next(Int32.MinValue >> 12, Int32.MaxValue);

            planeObject.transform.up = new Vector3(r, r1, r2);
            CutMesh_ArmStrong();

            r = _random.Next(Int32.MinValue >> 10, Int32.MaxValue);
            r1 = _random.Next(Int32.MinValue >> 15, Int32.MaxValue);
            r2 = _random.Next(Int32.MinValue >> 25, Int32.MaxValue);

            planeObject.up = new Vector3(r + r2, r1, 1);
            CutMesh_ArmStrong();

            planeObject.up = new Vector3(0, -1, 1);
            CutMesh_ArmStrong();
            planeObject.up = new Vector3(_random.Next(-1, 1), -1, 1);
            CutMesh_ArmStrong();
            planeObject.up = new Vector3(-.5f, 0, 1);
            CutMesh_ArmStrong();
        }

        private static void Frag_1(GameObject victimObject, List<GameObject> cuttedMeshes,
            Transform planeObject,
            Material capMaterial, bool makeGap)
        {
            var r = _random.Next(0, 256);
            var r1 = _random.Next(0, 256);
            var r2 = _random.Next(0, 256);

            planeObject.up = Vector3.up;
            CutMesh_ArmStrong();

            planeObject.up = Vector3.right;
            CutMesh_ArmStrong();

            planeObject.position += Vector3.down * .5f;

            var v = (Vector3.right - Vector3.up).normalized;
            planeObject.up = v;
            CutMesh_ArmStrong();

            v = (Vector3.right + Vector3.up).normalized;
            planeObject.up = v;
            CutMesh_ArmStrong();

            planeObject.position -= Vector3.right * .5f;

            planeObject.up = new Vector3(-1, r, 1);
            CutMesh_ArmStrong();

            planeObject.right = new Vector3(r, r1, r2);
            CutMesh_ArmStrong();

            planeObject.position = Vector3.zero;

            r = _random.Next(Int32.MinValue >> 30, Int32.MaxValue);
            r1 = _random.Next(Int32.MinValue >> 17, Int32.MaxValue);
            r2 = _random.Next(Int32.MinValue >> 12, Int32.MaxValue);

            planeObject.up = new Vector3(r, r1, r2);
            CutMesh_ArmStrong();

            r = _random.Next(Int32.MinValue >> 10, Int32.MaxValue);
            r1 = _random.Next(Int32.MinValue >> 15, Int32.MaxValue);
            r2 = _random.Next(Int32.MinValue >> 25, Int32.MaxValue);

            planeObject.up = new Vector3(r, r1, 1);
            CutMesh_ArmStrong();

            r = _random.Next(Int32.MinValue >> 10, Int32.MaxValue);
            r1 = _random.Next(Int32.MinValue >> 15, Int32.MaxValue);
            r2 = _random.Next(Int32.MinValue >> 25, Int32.MaxValue);

            planeObject.up = new Vector3(r, r1, 1);
            planeObject.right = new Vector3(r, r1, r2);
            CutMesh_ArmStrong();

            planeObject.up = new Vector3(0, 0, 1);
            CutMesh_ArmStrong();
            planeObject.up = new Vector3(0, -1, 1);
            CutMesh_ArmStrong();
        }
        
        private static void Frag_2(GameObject victimObject, List<GameObject> cuttedMeshes,
            Transform planeObject,
            Material capMaterial, bool makeGap)
        {
            planeObject.up = Vector3.up;
            CutMesh_ArmStrong();

            planeObject.up = Vector3.right;
            CutMesh_ArmStrong();

            planeObject.position += Vector3.down * .5f;

            var v = (Vector3.right - Vector3.up).normalized;
            planeObject.up = v;
            CutMesh_ArmStrong();

            v = (Vector3.right + Vector3.up).normalized;
            planeObject.up = v;
            CutMesh_ArmStrong();
        }

        #endregion
        
        #endregion
    }
}
