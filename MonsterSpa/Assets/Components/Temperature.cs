using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[Serializable]
public struct Temperature : IComponentData
{
    private float Value;
}
