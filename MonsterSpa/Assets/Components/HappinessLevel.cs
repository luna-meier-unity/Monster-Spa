using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[Serializable]
public struct HappinessLevel : IComponentData
{
    public float Value;
}
