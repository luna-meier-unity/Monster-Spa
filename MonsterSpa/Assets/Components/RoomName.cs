using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[Serializable]
public struct RoomName : IComponentData
{
    public NativeString64 name;

}
