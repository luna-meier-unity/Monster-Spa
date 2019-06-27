using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[Serializable]
public struct RoomSpots : IComponentData
{
    public float Value;

}

[Serializable]
public struct RoomTemperature : IComponentData
{
    public float Value;
}


[Serializable]
public struct RoomName : IComponentData
{
    public NativeString64 name;

}

[Serializable]
public struct SpawnRadius : IComponentData
{
    public float Value;

}

//we could use a list, but this is hackweek bby!
public struct Monster : IBufferElementData
{
    public Entity Value;
}


