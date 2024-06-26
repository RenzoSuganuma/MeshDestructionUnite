using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RydenUser : MonoBehaviour
{
    [SerializeField] private float _rotateSpeed;
    [SerializeField] private Material _capMaterial;
    [SerializeField] private GameObject _victimObject;
    [SerializeField] private GameObject _planeObject;

    private Vector3 _planeNormal;
    private List<GameObject> _cuttedMeshes = new List<GameObject>();

    private void Start()
    {
        _planeNormal = _planeObject.transform.up;
    }

    private void Update()
    {
        _planeNormal = _planeObject.transform.up;

        var input = Input.GetAxis("Horizontal");

        if (input < 0) // 左
        {
            _planeObject.transform.Rotate(Vector3.forward, Time.deltaTime * _rotateSpeed);
        }
        else if (input > 0) // 右
        {
            _planeObject.transform.Rotate(Vector3.forward, -Time.deltaTime * _rotateSpeed);
        }
    }

    private void OnGUI()
    {
        if (GUI.Button(new Rect(10, 700, 100, 50), "CUT"))
        {
            RydenHelper.CutTheMesh(_victimObject, _cuttedMeshes, Vector3.zero, _planeNormal, _capMaterial);
        }
    }
}
