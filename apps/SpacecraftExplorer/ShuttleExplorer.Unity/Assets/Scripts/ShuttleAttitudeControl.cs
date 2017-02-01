using UnityEngine;
using System.Collections;

public class ShuttleAttitudeControl : MonoBehaviour {
    public static float RotX, RotY, RotZ;

    // Use this for initialization
    void Start()
    {
        RotX = 45.0f;
        RotY = 45.0f;
        RotZ = 0.0f;
    }

    // Update is called once per frame
    void Update()
    {
        this.transform.localRotation = Quaternion.Lerp(transform.localRotation, Quaternion.Euler(RotX, RotY, RotZ), 0.1f);
    }


    // Rotational controls
    public static void RotateX(float ds)
    {
        RotX += ds;
    }
    public static void RotateY(float ds)
    {
        RotY += ds;
    }
    public static void RotateZ(float ds)
    {
        RotZ += ds;
    }

    public static void SetRotateX(float ds)
    {
        RotX = ds;
    }
    public static void SetRotateY(float ds)
    {
        RotY = ds;
    }
    public static void SetRotateZ(float ds)
    {
        RotZ = ds;
    }
}
