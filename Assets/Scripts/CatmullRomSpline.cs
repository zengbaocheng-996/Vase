using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CatmullRomSpline : MonoBehaviour
{
    public Transform[] controlPointsList;
    [HideInInspector]
    public List<Vector3> controlPoints;
    [HideInInspector]
    public bool isLooping = false;
    // [HideInInspector]
    public Vector3[] SamplePoints;
    [HideInInspector]
    public List<Vector3> SamplePointsTemp;
    [HideInInspector]
    public float resolution = 0.2f;

    private void Awake()
    {
        if (controlPointsList.Length == 0)
        {
            return;
        }
        else
        {
            controlPoints = new List<Vector3>();
            for (int i = 0; i < controlPointsList.Length; i++)
            {
                controlPoints.Insert(i, controlPointsList[i].position);
            }
            SamplePointsTemp = GeneratePointByCutMullRomWithDistance(controlPoints, 0.1f, false);
            SamplePoints = SamplePointsTemp.ToArray();
        }
    }

    // void OnDrawGizmos()
    // {
    //     Gizmos.color = Color.red;
    //     //旋转轴
    //     Gizmos.DrawLine(new Vector3(0,0,0), new Vector3(0, 100, 0));
    //     //采样点
    //     Gizmos.color = Color.cyan;
    //     for (int i = 0; i < SamplePointsTemp.Count; i++)
    //     {
    //         Gizmos.DrawSphere(transform.TransformPoint(SamplePointsTemp[i]), 0.02f);
    //     }
    // }
    public static List<Vector3> GeneratePointByCutMullRomWithDistance(List<Vector3> controlPoints, float stepDistance = 1f, bool closedLoop = false)
    {
        return GeneratePointByDistance(controlPoints, stepDistance, closedLoop);
    }

    public static List<Vector3> GeneratePointByDistance(List<Vector3> controlPoints, float stepDistance, bool closedLoop = false)
    {
        var first = controlPoints[0];
        controlPoints = AddFirstLastPoint(controlPoints, closedLoop);

        List<Vector3> points = new List<Vector3>();
        points.Add(first);//插值时从1开始的，所以加入第一个点

        float leftDistance = 0;
        int count = controlPoints.Count - 3;
        for (int i = 0; i < count; i++)
        {
            var p0 = controlPoints[i];
            var p1 = controlPoints[i + 1];
            var p2 = controlPoints[i + 2];
            var p3 = controlPoints[i + 3];

            float segDistance = (p2 - p1).magnitude;
            float dis = segDistance + leftDistance;
            float fStepCount = dis / stepDistance;
            int stepCount = Mathf.FloorToInt(fStepCount);

            int maxCount = stepCount;
            float offset = 0;
            int startIndex = 1;
            if (leftDistance > 0)
            {
                offset = (stepDistance - leftDistance) / segDistance;
                startIndex = 0;
                maxCount -= 1;
            }
            else
            {
                offset = 0;
                startIndex = 1;
            }

            float rate = stepDistance / segDistance;

            //从1开始,减少重复点
            for (int index = startIndex; index <= maxCount; index++)
            {
                float t = index * rate + offset;
                var pos = CatmullRom(p0, p1, p2, p3, t);
                //var pos = Vector3.Lerp(p1, p2, t);
                points.Add(pos);
            }

            leftDistance = (fStepCount - stepCount) * stepDistance;
        }
        return points;
    }

    public static List<Vector3> AddFirstLastPoint(List<Vector3> rawControlPoints, bool closedLoop)
    {
        List<Vector3> controlPoints = new List<Vector3>(rawControlPoints);

        if (closedLoop)
        {
            var last = controlPoints[controlPoints.Count - 1]; //先缓存最后一个

            controlPoints.Add(controlPoints[0]);
            controlPoints.Add(controlPoints[1]);

            controlPoints.Insert(0, last);
        }
        else
        {
            int numPoints = controlPoints.Count;

            var first0 = controlPoints[0];
            var first1 = controlPoints[1];

            var last1 = controlPoints[numPoints - 1];
            var last2 = controlPoints[numPoints - 2];

            //根据第一个点与第二个点的方向，反方向延长同样的距离新生成一个点
            Vector3 first = first0 + (first0 - first1);
            Vector3 last = last1 + (last1 - last2);

            controlPoints.Insert(0, first);
            controlPoints.Add(last);
        }

        return controlPoints;
    }

    public static Vector3 CatmullRom(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t /* between 0 and 1 */, float alpha = 0.5f /* between 0 and 1 */ )
    {
        float t0 = 0.0f;
        float t1 = GetT(t0, alpha, p0, p1);
        float t2 = GetT(t1, alpha, p1, p2);
        float t3 = GetT(t2, alpha, p2, p3);
        t = Mathf.Lerp(t1, t2, t);
        Vector3 A1 = (t1 - t) / (t1 - t0) * p0 + (t - t0) / (t1 - t0) * p1;
        Vector3 A2 = (t2 - t) / (t2 - t1) * p1 + (t - t1) / (t2 - t1) * p2;
        Vector3 A3 = (t3 - t) / (t3 - t2) * p2 + (t - t2) / (t3 - t2) * p3;
        Vector3 B1 = (t2 - t) / (t2 - t0) * A1 + (t - t0) / (t2 - t0) * A2;
        Vector3 B2 = (t3 - t) / (t3 - t1) * A2 + (t - t1) / (t3 - t1) * A3;
        Vector3 C = (t2 - t) / (t2 - t1) * B1 + (t - t1) / (t2 - t1) * B2;
        return C;
    }

    public static float GetT(float t, float alpha, Vector3 p0, Vector3 p1)
    {
        var d = p1 - p0;
        float a = Vector3.Dot(d, d); // Dot product
        float b = Mathf.Pow(a, alpha * 0.5f);
        return (b + t);
    }
}
