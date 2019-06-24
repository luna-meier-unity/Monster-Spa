using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using static Unity.Mathematics.math;

/// <summary>
/// Creatures with the HungerLevel tag will slowly 
/// </summary>
public class HungerOverTime : JobComponentSystem
{
    [BurstCompile]
    struct HungerOverTimeJob : IJobForEach<HungerLevel>
    {
        public float dt;
        public float HungerLossPerSecond;
        
        public void Execute(ref HungerLevel hungerLevel)
        {
            hungerLevel.Value -= HungerLossPerSecond * dt;
            
            //Hunger cannot be below zero.
            if (hungerLevel.Value < 0)
                hungerLevel.Value = 0;
        }
    }
    
    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        var hungerJob = new HungerOverTimeJob()
        {
            dt = Time.deltaTime,
            HungerLossPerSecond = Constants.g.HungerLossPerSecond
        }.Schedule(this, inputDependencies);

        return hungerJob;
    }
}