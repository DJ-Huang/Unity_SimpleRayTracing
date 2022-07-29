using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomCamera
{

    private Vector3 origin;
    private Vector3 lower_left_corner;
    private Vector3 horizontal;
    private Vector3 vertical;
    private float lensRadius;
    private Vector3 w, u, v;

    public CustomCamera(Vector3 lookFrom, Vector3 lookAt, Vector3 vup, float vfov /*vertical field-of-view in degrees*/, 
        float aspectRatio, float aperture, float focusDist)
    {
        float theta = Mathf.Deg2Rad * vfov;
        float h = Mathf.Tan(theta/2);
        float viewportHeight = 2.0f * h;
        float viewportWidth = aspectRatio * viewportHeight;

        w = Vector3.Normalize(lookFrom - lookAt);
        u = Vector3.Normalize(Vector3.Cross(vup, w));
        v = Vector3.Cross(w, u);

        origin = lookFrom;
        horizontal = focusDist * viewportWidth * u;
        vertical = focusDist * viewportHeight * v;
        lower_left_corner = origin - horizontal * 0.5f - vertical * 0.5f - focusDist * w;
        lensRadius = aperture * 0.5f;
    }

    public Ray GetRay(float s, float t) {
        Vector3 rd = lensRadius * Utils.RandomInUnitDisk();
        Vector3 offset = new Vector3(s * rd.x + t * rd.y, s * rd.x + t * rd.y,s * rd.x + t * rd.y);
        return new Ray(origin + offset, lower_left_corner + s*horizontal + t*vertical - origin - offset);
    }
}
