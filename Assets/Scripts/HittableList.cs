using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HittableList : Hittable
{
    
    public List<Hittable> objects = new List<Hittable>();
    
    public void Add(Hittable hittable)
    {
        if (!objects.Contains(hittable))
        {
            objects.Add(hittable);
        }
    }


    public override bool Hit(Ray r, float tMin, float tMax, ref HitRecord rec)
    {
        HitRecord temp_rec = rec;
        bool hit_anything = false;
        float closest_so_far = tMax;
        
        foreach (var obj in objects)
        {
            if (obj.Hit(r, tMin, closest_so_far, ref temp_rec)) {
                hit_anything = true;
                closest_so_far = temp_rec.t;
                rec = temp_rec;
            }
        }
        
        return hit_anything;
    }
}
