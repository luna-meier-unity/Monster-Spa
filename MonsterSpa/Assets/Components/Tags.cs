using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

public struct Tag_Room : IComponentData {}

public struct Tag_RemoveMonster : IComponentData
{
}

public struct Tag_Lobby : IComponentData {}

public struct Tag_Terrain : IComponentData {}