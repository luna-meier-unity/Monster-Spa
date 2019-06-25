﻿using System;
using System.Numerics;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using static Unity.Mathematics.math;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

public class MoveToExit : JobComponentSystem
{
    // This declares a new kind of job, which is a unit of work to do.
    // The job is declared as an IJobForEach<Translation, Rotation>,
    // meaning it will process all entities in the world that have both
    // Translation and Rotation components. Change it to process the component
    // types you want.
    //
    // The job is also tagged with the BurstCompile attribute, which means
    // that the Burst compiler will optimize it for the best performance.
    [BurstCompile]
    struct MoveToExitJob : IJobForEachWithEntity<Translation, Rotation, TimeToLeave>
    {
        // Add fields here that your job needs to do its work.
        // For example,
        //    public float deltaTime;

        public float monsterspeed;
        public EntityCommandBuffer.Concurrent ecb;
        //index is the index of this system that is executing, entity is the entity that is being executed on.
        public void Execute(Entity entity, int index, ref Translation translation, ref Rotation rotation, [ReadOnly] ref TimeToLeave timeToLeave)
        {
            if (timeToLeave.TimeRemaining < 0.01)
            {
                var directionToExit = normalize(StaticExit.exitPos - translation.Value);
                rotation.Value = Quaternion.LookRotation(directionToExit);//math.quaternion(directionToExit)  directionToExit
                translation.Value = directionToExit * monsterspeed + translation.Value;
                
                var deltaV = StaticExit.exitPos - translation.Value;
                if (math.length(deltaV) < 0.1f)
                {
                    GameMgr.monsters.Remove(entity);
                    ecb.DestroyEntity(index,entity);
                }

            }
            
        }
    }

    private EntityCommandBufferSystem endSimulationECB;

    protected override void OnCreate()
    {
        //ecb is how we do actions on object currently running this job
        endSimulationECB = World.Active.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>(); //here we define which ECB to run on
        base.OnCreate();
    }

    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        var job = new MoveToExitJob()
        {
            monsterspeed = 0.01f,
            ecb = endSimulationECB.CreateCommandBuffer().ToConcurrent()
        }.Schedule(this, inputDependencies);
        
        endSimulationECB.AddJobHandleForProducer(job); //this tells the ECB to run after this job is done


        return job;

    }
}