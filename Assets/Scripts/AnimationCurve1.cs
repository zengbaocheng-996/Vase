using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationCurve1 : MonoBehaviour
{
    public float timeValue=0f;
    public AnimationCurve animationCurve;
    public Vector3[] SamplePoints;
    // Start is called before the first frame update
    void Awake()
    {
        List<Vector3> samplePoints=new List<Vector3>();
        while (timeValue <= 1f)
        {
            Debug.Log(animationCurve.Evaluate(timeValue));
            timeValue = timeValue + 0.01f;
            samplePoints.Add(new Vector3(0f, timeValue, animationCurve.Evaluate(timeValue)));
        }

        SamplePoints = samplePoints.ToArray();
    }

    // Update is called once per frame
    void Update()
    {

    }
}
