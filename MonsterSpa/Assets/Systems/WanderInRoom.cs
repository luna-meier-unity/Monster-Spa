using System.ComponentModel;
using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using static Unity.Mathematics.math;
using Random = UnityEngine.Random;

//public class WanderInRoom : JobComponentSystem
//{
//    // This declares a new kind of job, which is a unit of work to do.
//    // The job is declared as an IJobForEach<Translation, Rotation>,
//    // meaning it will process all entities in the world that have both
//    // Translation and Rotation components. Change it to process the component
//    // types you want.
//    //
//    // The job is also tagged with the BurstCompile attribute, which means
//    // that the Burst compiler will optimize it for the best performance.
//    [BurstCompile]
//    struct WanderInRoomJob : IJobForEach<Translation, Rotation, InsideRoom >
//    {
//        // Add fields here that your job needs to do its work.
//        // For example,
//        //    public float deltaTime;
//        
//        
//        
//        public void Execute(ref Translation translation, [Unity.Collections.ReadOnly] ref Rotation rotation, [Unity.Collections.ReadOnly] ref InsideRoom inRoom)
//        {
//
//            EntityManager entityMgr = World.Active.EntityManager;
//            var roomTranslation = entityMgr.GetComponentData<Translation>(inRoom.RoomEntity);
//            if (length(translation.Value - roomTranslation.Value) > 5)
//            {
//                //teleport back to a random point within 5 units of the location.
//                //float3 thing = Random.insideUnitCircle// Random(float3);
//                //translation.Value = 
//            }
//
//
//        }
//    }
//    
//    protected override JobHandle OnUpdate(JobHandle inputDependencies)
//    {
//        var job = new WanderInRoomJob();
//        
//        // Assign values to the fields on your job here, so that it has
//        // everything it needs to do its work when it runs later.
//        // For example,
//        //     job.deltaTime = UnityEngine.Time.deltaTime;
//        
//        
//        
//        // Now that the job is set up, schedule it to be run. 
//        return job.Schedule(this, inputDependencies);
//    }
//}