using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class CableGeoCreator : MonoBehaviour
{
    public Material cableMaterial;
    public float cableDiameter = 0.1f; 
    public int radialSegments = 8;

    private void Awake()
    {
        UpdateCableGeometry();
    }

    private void OnDrawGizmos()
    {
        UpdateCableGeometry();
    }

    private void UpdateCableGeometry()
    {
        CableCreator cableCreator = GetComponent<CableCreator>();
        if (cableCreator == null || cableCreator.nextPoints == null || cableCreator.nextPoints.Count == 0)
            return;

        List<Vector3> bezierPointsList = new List<Vector3>();
        foreach (var cablePoint in cableCreator.nextPoints)
        {
            if (cablePoint != null && cablePoint.point != null)
            {
                int curvesCount = Mathf.Min(cablePoint.startOffsets.Count, cablePoint.endOffsets.Count);
                for (int i = 0; i < curvesCount; i++)
                {
                    Vector3[] points = GetBezierPoints(cableCreator, cablePoint, cableCreator.resolution, i);
                    bezierPointsList.AddRange(points);
                }
            }
        }

        Vector3[] bezierPoints = bezierPointsList.ToArray();
        for (int i = 0; i < bezierPoints.Length; i++)
        {
            bezierPoints[i] = transform.InverseTransformPoint(bezierPoints[i]);
        }
        Mesh mesh = new Mesh();
        CreateCylinderMesh(mesh, bezierPoints, cableDiameter, radialSegments);

        GetComponent<MeshFilter>().sharedMesh = mesh;
        if (cableMaterial != null)
            GetComponent<MeshRenderer>().sharedMaterial = cableMaterial;
    }

private Vector3[] GetBezierPoints(CableCreator cableCreator, CableCreator.CablePoint cablePoint, int resolution, int offsetIndex)
{
    Vector3[] points = new Vector3[resolution];
    for (int i = 0; i < resolution; i++)
    {
        float t = i / (float)(resolution - 1);

        // Используем позицию cableCreator в качестве фиксированной стартовой точки
        Vector3 startPoint = cableCreator.transform.position;

        // Вычисляем конечную точку для стартового сегмента
        Vector3 endPoint = cablePoint.point.transform.TransformPoint(cablePoint.endOffsets[offsetIndex]);

        // Вычисляем интерполированную точку между startPoint и endPoint
        Vector3 interpolatedPoint = Vector3.Lerp(startPoint, endPoint, t);

        // Вычисляем оффсет для данного момента времени
        Vector3 offset = Vector3.Lerp(cablePoint.startOffsets[offsetIndex], cablePoint.endOffsets[offsetIndex], t);

        // Используем линейную интерполяцию между startPoint и interpolatedPoint
        Vector3 interpolatedStartPoint = Vector3.Lerp(startPoint, interpolatedPoint, t);

        // Теперь используем interpolatedStartPoint и offset для расчета точки на кривой
        points[i] = CableCreator.CalculateBezierPoint(t, 
            interpolatedStartPoint, 
            Vector3.Lerp(interpolatedStartPoint, interpolatedStartPoint + offset, 0.5f),
            interpolatedPoint, 
            cablePoint.point.transform.position);
    }
    return points;
}


    private void CreateCylinderMesh(Mesh mesh, Vector3[] bezierPoints, float diameter, int radialCount)
    {
        int pointCount = bezierPoints.Length;
        int vertCount = pointCount * radialCount;
        Vector3[] vertices = new Vector3[vertCount];
        Vector2[] uv = new Vector2[vertCount];
        int[] triangles = new int[(pointCount - 1) * radialCount * 6];
        float radius = diameter / 2f;

        for (int i = 0; i < pointCount; i++)
        {
            float t = i / (pointCount - 1f);
            
            Quaternion rotation;
            if (i < pointCount - 1)
            {
                Vector3 directionToNext = bezierPoints[i + 1] - bezierPoints[i];
                rotation = Quaternion.LookRotation(directionToNext);
            }
            else
            {
                Vector3 directionToPrevious = bezierPoints[i] - bezierPoints[i - 1];
                rotation = Quaternion.LookRotation(directionToPrevious);
            }


            for (int j = 0; j < radialCount; j++)
            {
                float angle = j * 360f / radialCount * Mathf.Deg2Rad;
                Vector3 circlePoint = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0) * radius;
                Vector3 worldPoint = rotation * circlePoint + bezierPoints[i];
                vertices[i * radialCount + j] = worldPoint;
            }

        } 

        for (int i = 0; i < pointCount - 1; i++)
        {
            for (int j = 0; j < radialCount; j++)
            {
                int currentSegment = i * radialCount + j;
                int nextSegment = currentSegment + radialCount;
                int nextVert = (j + 1) % radialCount + i * radialCount;
                int nextVertNextSegment = nextVert + radialCount;

                int triangleIndex = (i * radialCount + j) * 6;
                triangles[triangleIndex] = currentSegment;
                triangles[triangleIndex + 1] = nextSegment;
                triangles[triangleIndex + 2] = nextVertNextSegment;

                triangles[triangleIndex + 3] = currentSegment;
                triangles[triangleIndex + 4] = nextVertNextSegment;
                triangles[triangleIndex + 5] = nextVert;
            }
        }

        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.triangles = triangles;
        mesh.RecalculateNormals(); 
        mesh.RecalculateBounds();
    }
}
