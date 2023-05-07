using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenerateMesh : MonoBehaviour
{
    public AnimationCurve animationCurve;
    [HideInInspector]
    public Vector3[] SamplePoints;
    [HideInInspector]
    public List<Vector3> MeshVertex;
    [HideInInspector]
    public List<int> MeshTriangleOut;
    [HideInInspector]
    public List<int> MeshTriangleIn;
    [HideInInspector]
    public Vector2[] UV;
    [HideInInspector]
    public float waitSeconds;
    //��忪�Ų���
    public int density = 60;
    public float seconds = 10.0f;
    
    #region ��������
    private void OnEnable()
    {
        DataStructureInit();
        GetSamplePoints();
        GenerateVase();
    }
    //private void Awake()
    //{

    //}

    //void Start()
    //{

    //}
    //private void OnDisable()
    //{

    //}

    //private void OnDestroy()
    //{

    //}
    #endregion

    #region ���ݽṹ��ʼ��
    private void DataStructureInit()
    {
        waitSeconds = (float)seconds / density / 2.0f / 2.0f;
        SamplePoints = new Vector3[0];
        UV = new Vector2[0];
        MeshVertex.Clear();
        MeshTriangleOut.Clear();
        MeshTriangleIn.Clear();
    }
    #endregion

    #region ��ò�����
    private void GetSamplePoints()
    {
        float timeValue=0f;
        List<Vector3> samplePoints=new List<Vector3>();
        while (timeValue <= 1f)
        {
            // Debug.Log(animationCurve.Evaluate(timeValue));
            timeValue = timeValue + 0.01f;
            samplePoints.Add(new Vector3(0f, 10*timeValue, 10*animationCurve.Evaluate(timeValue)));
        }

        SamplePoints = samplePoints.ToArray();
    
        // AnimationCurve1 other = (AnimationCurve1)GetComponent(typeof(AnimationCurve1));
        // CatmullRomSpline other = (CatmullRomSpline)GetComponent(typeof(CatmullRomSpline));
        // SamplePoints = other.SamplePoints;
    }
    #endregion

    #region ���ɻ�ƿ
    #region ����Э��
    private void GenerateVase()
    {
        GameObject vaseIn = GameObject.Find("vaseIn");
        Mesh meshIn;
        meshIn = new Mesh();
        vaseIn.GetComponent<MeshFilter>().mesh = meshIn = new Mesh();
        meshIn.name = "vaseIn";

        GameObject vaseOut = GameObject.Find("vaseOut");
        Mesh meshOut;
        vaseOut.GetComponent<MeshFilter>().mesh = meshOut = new Mesh();
        meshOut.name = "vaseOut";

        StartCoroutine(GenerateVaseMesh(meshIn, meshOut));
    }
    #endregion
    #region ���ɻ�ƿmesh
    private IEnumerator GenerateVaseMesh(Mesh meshIn, Mesh meshOut)
    {
        yield return StartCoroutine(GetMeshVertex());
        
        meshIn.vertices = MeshVertex.ToArray();
        meshOut.vertices = MeshVertex.ToArray();

        yield return StartCoroutine(GetMeshTriangle(meshIn, meshOut));
    
        // �ײ�
        meshOut.triangles = MeshTriangleOut.ToArray();
        meshIn.triangles = MeshTriangleIn.ToArray();
    }
    #endregion
    #region ����mesh����
    private IEnumerator GetMeshVertex()
    {
        Vector3 axis = new Vector3(0, 1, 0);
        for (int i = 0; i <= density; i++)
        {
            float angle = (i * 360.0f / density);

            Quaternion rotation = Quaternion.AngleAxis(angle, axis);
            for (int j = 0; j < SamplePoints.Length; j++)
            {
                var temp = SamplePoints[j];
                temp = rotation * temp;
                MeshVertex.Add(temp);
            }
            yield return new WaitForSeconds(waitSeconds);
        }
        MeshVertex.Add(new Vector3(0, 0, 0));
    }
    #endregion
    #region ���mesh�����κ�uv
    private IEnumerator GetMeshTriangle(Mesh meshIn, Mesh meshOut)
    {
        int offset = SamplePoints.Length;
        float segmentU = (float)1.0f / (density);
        float segmentV = (float)1.0f / (SamplePoints.Length);

        int index = 0;
        UV = new Vector2[MeshVertex.Count];
        // ��ƿ����������
        for (int i = 0; i < density; i++)
        {
            for (int j = 0; j < SamplePoints.Length - 1; j++)
            {
                SetQuad(index, j + offset * i, j + 1 + offset * i, j + offset * (i + 1), j + 1 + offset * (i + 1));
                index = index + 6;
                UV[i * offset + j] = new Vector2((float)i * segmentU, (float)j * segmentV);
            }
            // ��ƿƿ�ڵ�uv
            UV[i * offset + SamplePoints.Length - 1] = new Vector2((float)i * segmentU, (float)(SamplePoints.Length - 1) * segmentV);
            // ��һ������ meshOut uv
            meshOut.uv = UV;
            yield return new WaitForSeconds(waitSeconds);
            // ��һ���ظ��� uv
            if (i == density - 1)
            {
                i = density;
                for (int j = 0; j < SamplePoints.Length; j++)
                {
                    UV[i * offset + j] = new Vector2((float)i * segmentU, (float)j * segmentV);
                }
            }
            // ������������ʾ
            meshOut.triangles = MeshTriangleOut.ToArray();
            meshIn.triangles = MeshTriangleIn.ToArray();
        }
        // �ڶ������� meshOut uv
        meshOut.uv = UV;
        // ��ƿ���������Σ���meshIn���ƣ�
        for (int i = 0; i < density; i++)
        {
            if (i == density - 1)
            {
                MeshTriangleIn.Add(i * offset);
                MeshTriangleIn.Add(MeshVertex.Count - 1);
                MeshTriangleIn.Add(0);

                MeshTriangleIn.Add(i * offset);
                MeshTriangleIn.Add(0);
                MeshTriangleIn.Add(MeshVertex.Count - 1);

                index = index + 6;
                continue;
            }
            MeshTriangleIn.Add(i * offset);
            MeshTriangleIn.Add(MeshVertex.Count - 1);
            MeshTriangleIn.Add((i + 1) * offset);

            MeshTriangleIn.Add(i * offset);
            MeshTriangleIn.Add((i + 1) * offset);
            MeshTriangleIn.Add(MeshVertex.Count - 1);
            index = index + 6;
        }
    }
    #endregion
    #region �ı������ǻ�
    private void SetQuad(int index, int v00, int v01, int v10, int v11)
    {
        MeshTriangleOut.Add(v00);
        MeshTriangleOut.Add(v11);
        MeshTriangleOut.Add(v01);
        MeshTriangleOut.Add(v11);
        MeshTriangleOut.Add(v00);
        MeshTriangleOut.Add(v10);

        MeshTriangleIn.Add(v11);
        MeshTriangleIn.Add(v00);
        MeshTriangleIn.Add(v01);
        MeshTriangleIn.Add(v00);
        MeshTriangleIn.Add(v11);
        MeshTriangleIn.Add(v10);
    }
    #endregion
    #endregion

    #region Gizmos��mesh����
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        if (MeshVertex.Count != 0)
        {
            for (int i = 0; i < MeshVertex.Count; i++)
            {
                Gizmos.DrawSphere(MeshVertex[i], 0.02f);
            }
        }
    }
    #endregion
    
}
