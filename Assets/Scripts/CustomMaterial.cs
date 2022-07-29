using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Schema;
using TMPro.SpriteAssetUtilities;
using UnityEngine;
using Random = UnityEngine.Random;

public abstract class CustomMaterial
{
    public abstract bool Scatter(ref Ray rIn, ref HitRecord rec, ref Color attenuation, ref Ray scattered);
}

public class Lambertian : CustomMaterial
{
    public Color albedo;

    public Lambertian(Color albedo)
    {
        this.albedo = albedo;
    }
    
    public override bool Scatter(ref Ray rIn, ref HitRecord rec, ref Color attenuation, ref Ray scattered)
    {
        var scatterDirection = rec.normal + Random.insideUnitSphere;

        // 防止生成的random unit vector 和法线正好相反，两则相加为0的情况
        if (Utils.NearZero(scatterDirection))
        {
            scatterDirection = rec.normal;
        }
        
        scattered = new Ray(rec.p, scatterDirection);
        attenuation = albedo;
        return true;
    }
}

public class Metal : CustomMaterial
{
    public Color albedo;
    public float fuzz;

    public Metal(Color albedo)
    {
        this.albedo = albedo;
    }

    public Metal(Color albedo, float fuzz)
    {
        this.albedo = albedo;
        this.fuzz = fuzz;
    }
    
    public override bool Scatter(ref Ray rIn, ref HitRecord rec, ref Color attenuation, ref Ray scattered)
    {
        Vector3 reflected = Vector3.Reflect(Vector3.Normalize(rIn.dir), rec.normal);
        //scattered = new Ray(rec.p, reflected);
        scattered = new Ray(rec.p, reflected + fuzz*Random.insideUnitSphere);
        attenuation = albedo;
        return (Vector3.Dot(scattered.dir, rec.normal) > 0);
    }
}

// 介电材质
public class Dielectric : CustomMaterial {

    public float ir; // Index of Refraction
    
    public Dielectric(float index_of_refraction)
    {
        this.ir = index_of_refraction;
    }

    public override bool Scatter(ref Ray rIn, ref HitRecord rec, ref Color attenuation, ref Ray scattered) {
        attenuation = Color.white;
        float refractionRatio = rec.frontFace ? (1.0f/ir) : ir;

        Vector3 unitDirection = Vector3.Normalize(rIn.dir);
        float cosTheta = Mathf.Min(Vector3.Dot(-unitDirection, rec.normal), 1);
        float sinTheta = Mathf.Sqrt(1 - cosTheta * cosTheta);

        bool canRefract = refractionRatio * sinTheta > 1;
        Vector3 direction;
        if (canRefract || Reflectance(cosTheta, refractionRatio) > UnityEngine.Random.value)
        {
            direction = Vector3.Reflect(unitDirection, rec.normal);
        }
        else
        {
            direction = Utils.Refract(unitDirection, rec.normal, refractionRatio);
        }
        scattered = new Ray(rec.p, direction);
        return true;
    }
    
    // 反射率
    private float Reflectance(float cosine, float refIdx) {
        // Use Schlick's approximation for reflectance.            
        float r0 = (1-refIdx) / (1+refIdx);
        r0 = r0*r0;
        return r0 + (1-r0)*Mathf.Pow((1 - cosine),5);
    }
};
