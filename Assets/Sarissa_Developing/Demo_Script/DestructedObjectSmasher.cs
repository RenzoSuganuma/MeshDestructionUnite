using System;
using System.Linq;
using Sarissa;
using Unity.VisualScripting;
using UnityEngine;

public class DestructedObjectSmasher : MonoBehaviour
{
    [SerializeField] private GameObject _target;
    [SerializeField] private Vector3 _forceDir;

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(_target.transform.position, _target.transform.position + _forceDir);
    }

    private void OnGUI()
    {
        if (GUI.Button(new Rect(700, 1000, 300, 50), "PUNCH"))
        {
            var c = _target.GetChildObjects();
            c.ForEach(_ => _.parent = null);
            c.Add(_target.transform);

            c.ForEach(_ => _.AddComponent<Rigidbody>());
            c.ForEach(_ => _.GetComponent<Rigidbody>().AddExplosionForce(10, _forceDir, 10, 10, ForceMode.Impulse));
        }
    }
}
