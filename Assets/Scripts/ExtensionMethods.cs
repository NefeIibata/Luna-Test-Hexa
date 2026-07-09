using UnityEngine;

public static class ExtensionMethods
{
    public static Vector3 With(this Vector3 vector, float? x = null, float? y = null, float? z = null)
    {
        return new Vector3(x ?? vector.x, y ?? vector.y, z ?? vector.z);
    }

    public static void Clear(this Transform transform)
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            if (Application.isPlaying)
                GameObject.Destroy(transform.GetChild(i).gameObject);
            else
                GameObject.Destroy(transform.GetChild(i).gameObject);
        }
    }
}
