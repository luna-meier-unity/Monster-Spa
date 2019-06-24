using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[Serializable]
public struct RoomTemperature : IComponentData
{
    public float Value;
}
