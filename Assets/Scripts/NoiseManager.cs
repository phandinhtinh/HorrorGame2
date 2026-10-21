using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class NoiseManager : MonoBehaviour
{
    // Sự kiện phát ra tiếng động (Vị trí, Bán kính lan truyền)
    public static event Action<Vector3, float> OnNoiseEmitted;

    public static void EmitNoise(Vector3 position, float radius)
    {
        OnNoiseEmitted?.Invoke(position, radius);
    }
}