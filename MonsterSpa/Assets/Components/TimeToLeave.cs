using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[Serializable]
public struct TimeToLeave : IComponentData
{
    public float TimeRemaining; //in seconds
    
    
}
