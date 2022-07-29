using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct HitRecord
{
    public Vector3 p;
    public Vector3 normal;
    public float t;
    public Boolean frontFace;
    public CustomMaterial material;

    public void SetFaceNormal(Ray r, Vector3 outwardNormal)
    {
        frontFace = Vector3.Dot(r.dir, outwardNormal) < 0;
        normal = frontFace ? outwardNormal : -outwardNormal;
    }
}

public abstract class Hittable
{
    public abstract Boolean Hit(Ray r, float tMin, float tMax, ref HitRecord rec);
}
