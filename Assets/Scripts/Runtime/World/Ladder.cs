using System;
using UnityEngine;

public class Ladder : MonoBehaviour
{
    public float Length;
    public float Offset;

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawRay(Vector3.up * Offset, Vector3.up * Length);
    }

    private void OnValidate()
    {
        gameObject.tag = "Ladder";
    }

    public Vector3 GetPosition(float ladderPos)
    {
        return transform.position + transform.up * (ladderPos - Offset);
    }

    public float GetPosition(Vector3 ladderPos)
    {
        return Vector3.Dot(ladderPos - transform.position, transform.up) - Offset;
    }
}
