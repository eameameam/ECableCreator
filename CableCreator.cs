using UnityEngine;
using System.Collections.Generic;

[ExecuteInEditMode]
public class CableCreator : MonoBehaviour
{
    [System.Serializable]
    public class CablePoint
    {
        public CableCreator point;
        public List<Vector3> startOffsets;
        public List<Vector3> endOffsets;

        public CablePoint()
        {
            point = null;
            startOffsets = new List<Vector3>();
            endOffsets = new List<Vector3>();
        }

        public CablePoint(CableCreator creator)
        {
            point = creator;
            startOffsets = new List<Vector3>() { Vector3.zero };
            endOffsets = new List<Vector3>() { Vector3.zero };
        }
    }

    public void AddNewCablePoint()
    {
        CablePoint newCablePoint = new CablePoint(); 
        nextPoints.Add(newCablePoint);
    }


    [SerializeField] public List<CablePoint> nextPoints;
    [SerializeField] public int resolution = 20;

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(transform.position, 0.06f);

        if (nextPoints == null) return; 

        foreach (var cablePoint in nextPoints)
        {
            if (cablePoint != null && cablePoint.point != null && cablePoint.startOffsets != null && cablePoint.endOffsets != null)
            {
                DrawBezierToNextPoint(cablePoint);
            }
        }
    }


    private void DrawBezierToNextPoint(CablePoint cablePoint)
    {
        if (cablePoint.startOffsets == null || cablePoint.endOffsets == null) return;

        int curvesToDraw = Mathf.Min(cablePoint.startOffsets.Count, cablePoint.endOffsets.Count);

        for (int curveIndex = 0; curveIndex < curvesToDraw; curveIndex++)
        {
            if (curveIndex >= cablePoint.startOffsets.Count || curveIndex >= cablePoint.endOffsets.Count)
                continue;

            Vector3 startOffset = cablePoint.startOffsets[curveIndex];
            Vector3 endOffset = cablePoint.endOffsets[curveIndex];

            Vector3 previousPoint = transform.position;
            for (int i = 1; i <= resolution; i++)
            {
                float t = i / (float)resolution;
                Vector3 point = GetPointAt(t, cablePoint.point, startOffset, endOffset);
                Gizmos.color = Color.green;
                Gizmos.DrawLine(previousPoint, point);
                previousPoint = point;
            }
        }
    }

    private Vector3 GetPointAt(float t, CableCreator target, Vector3 startOffset, Vector3 endOffset)
    {
        Vector3 p0 = transform.position;
        Vector3 p1 = transform.TransformPoint(startOffset);
        Vector3 p2 = target.transform.TransformPoint(endOffset);
        Vector3 p3 = target.transform.position;

        return CalculateBezierPoint(t, p0, p1, p2, p3);
    }

    public static Vector3 CalculateBezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
    {
        float u = 1 - t;
        float tt = t * t;
        float uu = u * u;
        float uuu = uu * u;
        float ttt = tt * t;

        Vector3 p = uuu * p0;
        p += 3 * uu * t * p1; 
        p += 3 * u * tt * p2;
        p += ttt * p3; 

        return p;
    }
}