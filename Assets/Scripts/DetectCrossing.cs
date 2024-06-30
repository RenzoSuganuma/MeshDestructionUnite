using UnityEngine;

public class DetectCrossing : MonoBehaviour
{
    [SerializeField] private Transform _p;
    [SerializeField] private Transform _q;
    [SerializeField] private Transform _a;
    [SerializeField] private Transform _b;
    
    void Update()
    {
        var pq = _q.position - _p.position;
        var pa = _a.position - _p.position;
        var pb = _b.position - _p.position;
        var ab = _b.position - _a.position;
        var ap = _p.position - _a.position;
        var aq = _q.position - _a.position;
        
        Debug.Log($"{Vector3.Cross(pa , pq).y}");
    }
}