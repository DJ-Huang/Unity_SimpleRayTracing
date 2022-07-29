using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sphere : Hittable
{

    public Vector3 center;
    public float radius;
    public CustomMaterial material;
    
    public Sphere()
    {
        
    }

    public Sphere(Vector3 center, float r, CustomMaterial m)
    {
        this.center = center;
        this.radius = r;
        this.material = m;
    }

    public Sphere(Vector3 center, float r)
    {
        this.center = center;
        this.radius = r;
    }
    
    public override bool Hit(Ray r, float tMin, float tMax, ref HitRecord rec)
    {
        Vector3 oc = r.orig - center;
        float a = r.dir.sqrMagnitude;
        float half_b = Vector3.Dot(oc, r.dir);
        float c = oc.sqrMagnitude - radius*radius;

        float discriminant = half_b*half_b - a*c;
        if (discriminant < 0) return false;
        float sqrtd = Mathf.Sqrt(discriminant);

        // Find the nearest root that lies in the acceptable range.
        float root = (-half_b - sqrtd) / a;
        if (root < tMin || tMax < root) {
            root = (-half_b + sqrtd) / a;
            if (root < tMin || tMax < root)
                return false;
        }

        rec.t = root;
        rec.p = r.At(rec.t);
        Vector3 outwardNormal = (rec.p - center) / radius;
        rec.SetFaceNormal(r, outwardNormal);
        rec.material = material;

        return true;
    }
}
