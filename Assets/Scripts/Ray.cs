using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ray
{
    public Vector3 orig;
    public Vector3 dir;

    public Ray()
    {
        
    }
    
    public Ray(Vector3 orig, Vector3 dir)
    {
        this.orig = orig;
        this.dir = dir;
    }
    
    public Vector3 At(float t)
    {
        return orig + dir * t;
    }
}
