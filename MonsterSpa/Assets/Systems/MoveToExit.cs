using System;
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

    struct MoveToExitJob : IJobForEachWithEntity<Translation, Rotation, TimeToLeave>
    {

        public float monsterspeed;
        public EntityCommandBuffer.Concurrent ecb;
        public Entity lobby;
        [ReadOnly] public ComponentDataFromEntity<InsideRoom> roomWeAreIn;

        public BufferFromEntity<Monster> buffFromEnt;
         

        //public ComponentDataFromEntity<InsideRoom> insideRoom;
        //index is the index of this system that is executing, entity is the entity that is being executed on.
        public void Execute(Entity entity, int index, ref Translation translation, ref Rotation rotation, [ReadOnly] ref TimeToLeave timeToLeave)
        {
            if (timeToLeave.TimeRemaining < 0.01)
            {
                //var entityManager = World.Active.EntityManager;
                var directionToExit = normalize(StaticExit.exitPos - translation.Value);
                rotation.Value = Quaternion.LookRotation(directionToExit);//math.quaternion(directionToExit)  directionToExit
                translation.Value = directionToExit * monsterspeed + translation.Value;
                //GameMgr.g.MoveMonsterToLobbyWithEcb(ecb, index, entity, 5);
                
                
                var roomEnt = roomWeAreIn[entity];
                var buff = buffFromEnt[roomEnt.RoomEntity];
                
                //var buff = entityManager.GetBuffer<Monster>(roomEnt.RoomEntity);
                for (int i = buff.Length-1; i >= 0; i--)
                {
                    if (buff[i].Value.Equals(entity))
                    {
                        buff.RemoveAt(i);
                    }
                }
                //need reference to lobby...
                //var query = entityManager.CreateEntityQuery(typeof(Tag_Lobby)).ToEntityArray(Allocator.TempJob);
                ecb.SetComponent(index, entity, new InsideRoom() { RoomEntity = lobby });
                
                
                //query.Dispose();
                
                
                

                // ecb.SetComponent(index, entity, new InsideRoom() { RoomEntity = GameMgr.g.GetRoomEntity(GameMgr.g.lobby) });

                var deltaV = StaticExit.exitPos - translation.Value;
                if (math.length(deltaV) < 0.5f)
                {
                    ecb.AddComponent(index,entity,new Tag_RemoveMonster());
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
            buffFromEnt = GetBufferFromEntity<Monster>(false),
            roomWeAreIn = GetComponentDataFromEntity<InsideRoom>(true),
            lobby = GameMgr.rooms[0],
            monsterspeed = 0.01f,
            ecb = endSimulationECB.CreateCommandBuffer().ToConcurrent()
        }.ScheduleSingle(this, inputDependencies);

        endSimulationECB.AddJobHandleForProducer(job); //this tells the ECB to run after this job is done


        return job;

    }
}