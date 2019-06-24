using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using static Unity.Mathematics.math;
using UnityEngine;

public class DecrementTime : JobComponentSystem
{
    
    [BurstCompile]
    struct FatherTimeJob : IJobForEach<TimeToLeave>
    {
        public float dt;
         
        public void Execute(ref TimeToLeave timeToLeave)
        {
            timeToLeave.TimeRemaining -= dt;
            if (timeToLeave.TimeRemaining < 0)
                timeToLeave.TimeRemaining = 0;


        }
    }
    
    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        var job = new FatherTimeJob()
        {
            dt = Time.deltaTime
        }.Schedule(this, inputDependencies);

        return job;

    }
}