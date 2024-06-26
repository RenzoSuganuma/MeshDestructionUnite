using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
using Random = System.Random;

/// <summary> これがアタッチされているオブジェクトをヒエラルキーに投げる。 </summary>
public class ArmStrongUser : MonoBehaviour
{
    public enum FragmentationMode
    {
        WithoutPhysics,
        WithPhysics,
    }

    [SerializeField] private FragmentationMode _fragmentationMode;
    [SerializeField] private float _rotateSpeed;
    [SerializeField] private Material _capMaterial;
    [SerializeField] private GameObject _victimObject;
    [SerializeField] private GameObject _planeObject;
    [SerializeField] private string _objectName = "Cutted Mesh";
    [SerializeField] private bool _makeGap = false;

    [SerializeField, Range(0, 5)] private int _fragType;

    public FragmentationMode FragMode => _fragmentationMode;

    private Vector3 _planeNormal;
    private float _deltaAngle;
    private Random _random = new Random();
    private List<GameObject> _cuttedMeshes = new List<GameObject>();

    public void CheckDirectory() // 保存パスが存在するか確認する
    {
        ArmStrong.FindSaveTargetDirectory(ArmStrong.CuttedMeshesFolderAbsolutePath + $"{_objectName}/");
        ArmStrong.FindSaveTargetDirectory(ArmStrong.CuttedMeshesPrefabFolderAbsolutePath);
    }

    public void CutMesh() // メッシュのカットを実施する
    {
        if (_victimObject is null) return;

        ArmStrongHelper.CutTheMesh(_victimObject, _cuttedMeshes, _planeObject.transform.position,
            _planeObject.transform.up,
            _capMaterial, _makeGap);
    }

    public void CutRandomly()
    {
        if (_fragType > 1) _fragType = 1;
        Invoke($"Frag_{_fragType}", 0);
    }

    #region 破壊パターン

    private void Frag_0()
    {
        var r = _random.Next(0, 256);
        var r1 = _random.Next(0, 256);
        var r2 = _random.Next(0, 256);

        _planeObject.transform.up = Vector3.up;
        CutMesh();

        _planeObject.transform.up = Vector3.right;
        CutMesh();

        _planeObject.transform.position += Vector3.down * .5f;

        var v = (Vector3.right - Vector3.up).normalized;
        _planeObject.transform.up = v;
        CutMesh();

        v = (Vector3.right + Vector3.up).normalized;
        _planeObject.transform.up = v;
        CutMesh();

        _planeObject.transform.position -= Vector3.right * .5f;

        _planeObject.transform.up = new Vector3(-1, 0, 1);
        CutMesh();

        _planeObject.transform.right = new Vector3(r, r1, r2);
        CutMesh();

        _planeObject.transform.position = Vector3.zero;

        r = _random.Next(Int32.MinValue >> 30, Int32.MaxValue);
        r1 = _random.Next(Int32.MinValue >> 17, Int32.MaxValue);
        r2 = _random.Next(Int32.MinValue >> 12, Int32.MaxValue);

        _planeObject.transform.up = new Vector3(r, r1, r2);
        CutMesh();

        r = _random.Next(Int32.MinValue >> 10, Int32.MaxValue);
        r1 = _random.Next(Int32.MinValue >> 15, Int32.MaxValue);
        r2 = _random.Next(Int32.MinValue >> 25, Int32.MaxValue);

        _planeObject.transform.up = new Vector3(r + r2, r1, 1);
        CutMesh();
        
        _planeObject.transform.up = new Vector3(0, -1, 1);
        CutMesh();
        _planeObject.transform.up = new Vector3(_random.Next(-1, 1), -1, 1);
        CutMesh();
        _planeObject.transform.up = new Vector3(-.5f, 0, 1);
        CutMesh();
    }

    private void Frag_1()
    {
        var r = _random.Next(0, 256);
        var r1 = _random.Next(0, 256);
        var r2 = _random.Next(0, 256);

        _planeObject.transform.up = Vector3.up;
        CutMesh();

        _planeObject.transform.up = Vector3.right;
        CutMesh();

        _planeObject.transform.position += Vector3.down * .5f;

        var v = (Vector3.right - Vector3.up).normalized;
        _planeObject.transform.up = v;
        CutMesh();

        v = (Vector3.right + Vector3.up).normalized;
        _planeObject.transform.up = v;
        CutMesh();

        _planeObject.transform.position -= Vector3.right * .5f;

        _planeObject.transform.up = new Vector3(-1, r, 1);
        CutMesh();

        _planeObject.transform.right = new Vector3(r, r1, r2);
        CutMesh();

        _planeObject.transform.position = Vector3.zero;

        r = _random.Next(Int32.MinValue >> 30, Int32.MaxValue);
        r1 = _random.Next(Int32.MinValue >> 17, Int32.MaxValue);
        r2 = _random.Next(Int32.MinValue >> 12, Int32.MaxValue);

        _planeObject.transform.up = new Vector3(r, r1, r2);
        CutMesh();

        r = _random.Next(Int32.MinValue >> 10, Int32.MaxValue);
        r1 = _random.Next(Int32.MinValue >> 15, Int32.MaxValue);
        r2 = _random.Next(Int32.MinValue >> 25, Int32.MaxValue);

        _planeObject.transform.up = new Vector3(r, r1, 1);
        CutMesh();

        r = _random.Next(Int32.MinValue >> 10, Int32.MaxValue);
        r1 = _random.Next(Int32.MinValue >> 15, Int32.MaxValue);
        r2 = _random.Next(Int32.MinValue >> 25, Int32.MaxValue);

        _planeObject.transform.up = new Vector3(r, r1, 1);
        _planeObject.transform.right = new Vector3(r, r1, r2);
        CutMesh();

        _planeObject.transform.up = new Vector3(0, 0, 1);
        CutMesh();
        _planeObject.transform.up = new Vector3(0, -1, 1);
        CutMesh();
    }

    #endregion

    public void SaveCuttedMeshes() // 保存先のパスにメッシュのアセットとプレハブを保存する
    {
        if (_cuttedMeshes.Count < 1) return;

        ArmStrong.FindSaveTargetDirectory(ArmStrong.CuttedMeshesFolderAbsolutePath + $"{_objectName}/");
        ArmStrong.FindSaveTargetDirectory(ArmStrong.CuttedMeshesPrefabFolderAbsolutePath);

        _cuttedMeshes[0].name = _objectName;

        // コンポーネントのアタッチ
        if (_fragmentationMode == FragmentationMode.WithPhysics)
        {
            foreach (var cuttedMesh in _cuttedMeshes)
            {
                cuttedMesh.AddComponent<MeshCollider>();
                cuttedMesh.GetComponent<MeshCollider>().sharedMesh = cuttedMesh.GetComponent<MeshFilter>().mesh;
                cuttedMesh.GetComponent<MeshCollider>().convex = true;
                cuttedMesh.AddComponent<Rigidbody>();
            }
        }

        // カットしたメッシュは一つのオブジェクトにする
        for (int i = 1; i < _cuttedMeshes.Count; ++i)
        {
            _cuttedMeshes[i].transform.parent = _cuttedMeshes[0].transform;
        }

        // 保存処理
        for (int i = 0; i < _cuttedMeshes.Count; ++i)
        {
            var mesh = _cuttedMeshes[i].GetComponent<MeshFilter>().mesh;

            AssetDatabase.CreateAsset(mesh,
                ArmStrong.CuttedMeshesFolderAbsolutePath + $"{_objectName}/{mesh.name}_{i}.asset");
        }

        PrefabUtility.SaveAsPrefabAsset(_cuttedMeshes[0],
            ArmStrong.CuttedMeshesPrefabFolderAbsolutePath + $"{_objectName}.prefab");
    }

    private void Start()
    {
        _planeNormal = _planeObject.transform.up;
    }

    private void Update()
    {
        _planeNormal = _planeObject.transform.up;

        var inputHor = Input.GetAxis("Horizontal");
        var inputVer = Input.GetAxis("Vertical");

        if (inputHor < 0) // 左
        {
            _planeObject.transform.Rotate(Vector3.forward, Time.deltaTime * _rotateSpeed);
        }
        else if (inputHor > 0) // 右
        {
            _planeObject.transform.Rotate(Vector3.forward, -Time.deltaTime * _rotateSpeed);
        }

        if (inputVer > 0) // 下
        {
            _planeObject.transform.Rotate(Vector3.right, Time.deltaTime * _rotateSpeed);
        }
        else if (inputVer < 0) // 上
        {
            _planeObject.transform.Rotate(Vector3.right, -Time.deltaTime * _rotateSpeed);
        }

        if (Input.GetKey(KeyCode.Q))
        {
            _planeObject.transform.position -= Vector3.up * Time.deltaTime;
        }

        if (Input.GetKey(KeyCode.E))
        {
            _planeObject.transform.position += Vector3.up * Time.deltaTime;
        }
    }
}