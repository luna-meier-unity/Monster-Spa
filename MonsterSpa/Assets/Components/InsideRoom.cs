using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[Serializable]
public struct InsideRoom : IComponentData
{
    public Entity RoomEntity;
}