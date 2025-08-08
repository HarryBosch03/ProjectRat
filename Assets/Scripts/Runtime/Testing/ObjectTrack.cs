using System;
using UnityEngine;

public class ObjectTrack : MonoBehaviour
{
    public float distance;
    public float speed;
    public Rigidbody target;

    private float position;
    private bool reverse;
    
    private void FixedUpdate()
    {
        position = Mathf.MoveTowards(position, reverse ? 0 : 1, Time.deltaTime * speed / distance);
        if (position >= 1f) reverse = true;
        if (position <= 0f) reverse = false;
        
        target.MovePosition(transform.forward * (position - 0.5f) * distance);
    }
}
