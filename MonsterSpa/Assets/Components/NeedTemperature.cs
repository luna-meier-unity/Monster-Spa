using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

//Entities desire their Temperature component to be close to their NeedTemperature components.
public struct NeedTemperature : IComponentData
{
    private float Value;
}
