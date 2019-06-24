using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using static Unity.Mathematics.math;

public class TemperatureNormalizeOverTime : JobComponentSystem
{

    [BurstCompile]
    private struct TemperatureNormalizeOverTimeJob : IJobForEach<Temperature, InsideRoom>
    {
        public float TemperatureNormalizeRate;
        public float dt;

        [ReadOnly] public ComponentDataFromEntity<RoomTemperature> RoomTemperature;

        public void Execute(ref Temperature temperature, [ReadOnly]ref InsideRoom room)
        {
            var roomTemperature = RoomTemperature[room.RoomEntity].Value;
            
            if (temperature.Value > roomTemperature)
            {
                temperature.Value -= TemperatureNormalizeRate * dt;

                if (temperature.Value < roomTemperature)
                {
                    temperature.Value = roomTemperature;
                }
            }
            else
            {
                temperature.Value += TemperatureNormalizeRate * dt;

                if (temperature.Value > roomTemperature)
                {
                    temperature.Value = roomTemperature;
                }

            }
        }
    }
    
    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        var job = new TemperatureNormalizeOverTimeJob()
        {
            TemperatureNormalizeRate = Constants.g.TemperatureNormalizeRate,
            dt = Time.deltaTime,
            RoomTemperature = GetComponentDataFromEntity<RoomTemperature>(true)
        };
        
        return job.Schedule(this, inputDependencies);
    }
}