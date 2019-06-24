using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

public enum Food
{
    PickledHerring,
    Meatballs,
    SmokedSalmon,
    LingonberryJam
}

public struct NeedFood : ISharedComponentData
{
    public Food Value;
}