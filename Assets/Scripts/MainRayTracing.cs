using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Events;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class MainRayTracing : MonoBehaviour
{
    public int width;
    public int height;
    [Range(1,100)]
    public int samplersPerPixel = 100;
    [Range(1,50)]
    public int maxDepth = 50;
    public Texture2D rayTracingRenderTarget;

    private Action onRenderFinish;
    private UnityAction<float> onRendering;
    
    private static MainRayTracing _instance;
    public static MainRayTracing Instance => _instance;
    private Color[] color;

    /// <summary>
    /// 随机场景
    /// </summary>
    /// <returns></returns>
    HittableList RandomScene() {
        HittableList world = new HittableList();

        Lambertian groundMaterial = new Lambertian(new Color(0.5f, 0.5f, 0.5f));
        world.Add(new Sphere(new Vector3(0,-1000,0), 1000, groundMaterial));

        for (int a = -11; a < 11; a++) {
            for (int b = -11; b < 11; b++) {
                float choose_mat = UnityEngine.Random.value;
                Vector3 center = new Vector3(a + 0.9f * Random.value, 0.2f, b + 0.9f * Random.value);

                if ((center - new Vector3(4, 0.2f, 0)).magnitude > 0.9f) {
                    CustomMaterial sphere_material;

                    if (choose_mat < 0.8) {
                        // diffuse
                        Color albedo = Random.ColorHSV() * Random.ColorHSV();
                        sphere_material = new Lambertian(albedo);
                        world.Add(new Sphere(center, 0.2f, sphere_material));
                    } else if (choose_mat < 0.95) {
                        // metal
                        Color albedo = Random.ColorHSV();
                        float fuzz = Random.Range(0, 0.5f);
                        sphere_material = new Metal(albedo, fuzz);
                        world.Add(new Sphere(center, 0.2f, sphere_material));
                    } else {
                        // glass
                        sphere_material = new Dielectric(1.5f);
                        world.Add(new Sphere(center, 0.2f, sphere_material));
                    }
                }
            }
        }

        Dielectric material1 =new Dielectric(1.5f);
        world.Add(new Sphere(new Vector3(0, 1, 0), 1.0f, material1));

        Lambertian material2 = new Lambertian(new Color(0.4f, 0.2f, 0.1f));
        world.Add(new Sphere(new Vector3(-4, 1, 0), 1.0f, material2));

        Metal material3 = new Metal(new Color(0.7f, 0.6f, 0.5f), 0.0f);
        world.Add(new Sphere(new Vector3(4, 1, 0), 1.0f, material3));

        return world;
    }
    
    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
        
        width = Screen.width;
        height = Screen.height;
        //rayTracingRenderTarget = new Texture2D(width, height);
    }
    
    /// <summary>
    /// 击中球体检测，通过delta项检测是否射线在球体外部还是内部
    /// </summary>
    /// <param name="center"></param>
    /// <param name="radius"></param>
    /// <param name="r"></param>
    /// <returns></returns>
    private float HitSphere(Vector3 center, float radius, Ray r) {
        Vector3 oc = r.orig - center;
        float a = r.dir.magnitude;
        float halfB = Vector3.Dot(oc, r.dir);
        float c = oc.magnitude - radius * radius;
        float discriminant = halfB * halfB - a * c;
        if (discriminant < 0)
        {
            return -1;
        }
        else
        {
            return (-halfB - Mathf.Sqrt(discriminant)) / (a);
        }
    }

    private Vector3 RandomInHemisphere(Vector3 normal)
    {
        Vector3 inUnitSphere = Random.insideUnitSphere;
        if (Vector3.Dot(inUnitSphere, normal) > 0.0) // In the same hemisphere as the normal
            return inUnitSphere;
        else
            return -inUnitSphere;
    }
    
    private Color RayColor(Ray r, Hittable world, int depth)
    {
        HitRecord rec = new HitRecord();
        
        if(depth <= 0)
            return Color.black;
        
        if (world.Hit(r, 0.001f, Mathf.Infinity, ref rec))
        {
            Ray scattered = new Ray();
            Color attenuation = Color.black;
            if (rec.material.Scatter(ref r, ref rec, ref attenuation, ref scattered))
            {
                return attenuation * RayColor(scattered, world, depth - 1);
            }
            return Color.black;
        }
        
        Vector3 unit_direction = Vector3.Normalize(r.dir);
        var t = 0.5f * unit_direction.y + 1;
        return (1 - t) * Color.white + t * new Color(0.5f, 0.7f, 1.0f);
    }
    
    private Color WriteColor(Color pixelColor, int samples_per_pixel) {
        float r = pixelColor.r;
        float g = pixelColor.g;
        float b = pixelColor.b;

        // Divide the color by the number of samples.
        float scale = 1.0f / samplersPerPixel;
        r = Mathf.Sqrt(r * scale);
        g = Mathf.Sqrt(g * scale);
        b = Mathf.Sqrt(b * scale);
        return new Color(r, g, b, 1);
    }

    public IEnumerator StartRender(Action onRenderFinishAction, UnityAction<float> onRenderingAction)
    {

        // Image
        rayTracingRenderTarget = new Texture2D(width, height);
        
        // world
        var R = Mathf.Cos(Mathf.PI * 0.25f);
        var world = RandomScene();
        
        // camera
        float aspectRatio = Camera.main.aspect;
        Vector3 lookfrom = new Vector3(13, 3, 2);
        Vector3 lookat = new Vector3(0,0,0);
        Vector3 vup = Vector3.up;
        //float distToFocus = (lookfrom-lookat).magnitude;
        float distToFocus =  10.0f;
        float aperture = 0.1f;
        CustomCamera cam = new CustomCamera(lookfrom, lookat, vup, 20, aspectRatio, aperture, distToFocus);
        
        color = new Color[width * height];
        for (int j = height - 1; j >= 0; --j)
        {                
            yield return null;
            for (int i = 0; i < width; ++i)
            {
                Color pixelColor = Color.black;
                for (int k = 0; k < samplersPerPixel; ++k)
                {
                    var u = (float)(i+Random.value) / (width-1);
                    var v = (float)(j+Random.value) / (height-1);
                    Ray r = cam.GetRay(u, v);
                    pixelColor += RayColor(r, world, maxDepth);
                }
                
                color[j * width + i] = WriteColor(pixelColor, samplersPerPixel);
                onRenderingAction?.Invoke((float)(1 - (j * width + width - i) / (float)(width * height)) * 100);
            }
        }
        rayTracingRenderTarget.SetPixels(color);
        rayTracingRenderTarget.Apply();
        onRenderFinishAction?.Invoke();
        yield return null;
    }
}
