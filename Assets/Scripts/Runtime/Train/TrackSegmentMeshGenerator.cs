using System;
using UnityEngine;

namespace Runtime.Train
{
    [RequireComponent(typeof(TrackSegment))]
    public class TrackSegmentMeshGenerator : MonoBehaviour
    {
        private TrackSegment segment;

        private void Awake()
        {
            segment = GetComponent<TrackSegment>();
        }
    }
}