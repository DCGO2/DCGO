using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AutoLayout3D
{
    [ExecuteInEditMode]
    public class LayoutElement3D : MonoBehaviour
    {
        public Vector3 center;
        public Vector3 size = Vector3.one;
        
        private void Reset()
        {
            MeshRenderer r = GetComponent<MeshRenderer>();
            if (r != null)
            {
                center = r.bounds.center - transform.position;
                center.x /= transform.lossyScale.x;
                center.y /= transform.lossyScale.y;
                center.z /= transform.lossyScale.z;
                size.x = r.bounds.size.x / transform.lossyScale.x;
                size.y = r.bounds.size.y / transform.lossyScale.y;
                size.z = r.bounds.size.z / transform.lossyScale.z;
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = new Color(0.5f, 0.5f, 1.0f, 1.0f);
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(center, size);
        }
    }
}
