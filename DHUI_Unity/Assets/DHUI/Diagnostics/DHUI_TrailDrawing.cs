using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DHUI
{
    [RequireComponent(typeof(LineRenderer))]
    public class DHUI_TrailDrawing : MonoBehaviour
    {
        private List<Vector3> positions = new List<Vector3>();
        private LineRenderer lineRenderer = null;

        private void Start()
        {
            lineRenderer = gameObject.GetComponent<LineRenderer>();
            if (lineRenderer == null)
            {
                lineRenderer = gameObject.AddComponent<LineRenderer>();
            }
        }

        private void FixedUpdate()
        {
            if (positions.Count > 0 && positions[positions.Count - 1] == transform.position) return;
            
            positions.Add(transform.position);
            RenderTrail();
        }

        protected void RenderTrail()
        {
            if (positions.Count < 2)
            {
                lineRenderer.enabled = false;
            }
            else
            {
                lineRenderer.enabled = true;
            }
            lineRenderer.positionCount = positions.Count;
            lineRenderer.SetPositions(positions.ToArray());
        }
    }

}