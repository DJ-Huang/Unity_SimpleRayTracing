using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Utils
{
    public static bool NearZero(Vector3 v)
    {
        return Mathf.Abs(v.x) < Mathf.NegativeInfinity
               && Mathf.Abs(v.y) < Mathf.NegativeInfinity
               && Mathf.Abs(v.z) < Mathf.NegativeInfinity;
    }

    public static Vector3 Refract(Vector3 uv, Vector3 n, float etaiOverEtat)
    {
        float cosTheta = Mathf.Min(Vector3.Dot(-uv, n), 1.0f);
        Vector3 r_out_perp =  etaiOverEtat * (uv + cosTheta*n);
        Vector3 r_out_parallel = -Mathf.Sqrt(Mathf.Abs(1.0f - r_out_perp.sqrMagnitude)) * n;
        return r_out_perp + r_out_parallel;
    }

    public static Vector3 RandomInUnitDisk()
    {
        while (true)
        {
            Vector3 p = new Vector3(Random.Range(-1, 1), Random.Range(-1, 1), 0);
            if(p.sqrMagnitude >= 1) continue;
            return p;
        }
    }
}
