using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics.Systems;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Assertions;
using static Unity.Physics.Math;
using Unity.Physics.Extensions;

namespace Unity.Physics.Extensions
{
    // A mouse pick collector which stores every hit. Based off the ClosestHitCollector
    // Creating an ICollector<RaycastHit> specifically as the base IQueryResult interface doesn't have RigidBodyIndex etc
    // This is a workaround to filter out static bodies and trigger volumes from the mouse picker.
    // Currently filtered in the TransformHits function when the Body Index is available.
    // This interface will be changed in future so that hits can be filtered appropriately during AddHit instead.
    // With this temporary filtering workaround CastRay will return true even if we filtered hits.
    // Hence, the MaxFraction is checked instead to see if a true hit was collected.
    [BurstCompile]
    public struct MousePickCollector : ICollector<RaycastHit>
    {
        public bool IgnoreTriggers;
        public NativeSlice<RigidBody> Bodies;
        public int NumDynamicBodies;

        public bool EarlyOutOnFirstHit => false;
        public float MaxFraction { get; private set; }
        public int NumHits { get; private set; }

        private RaycastHit m_OldHit;
        private RaycastHit m_ClosestHit;
        public RaycastHit Hit => m_ClosestHit;

        public MousePickCollector(float maxFraction, NativeSlice<RigidBody> rigidBodies, int numDynamicBodies)
        {
            m_OldHit = default(RaycastHit);
            m_ClosestHit = default(RaycastHit);
            MaxFraction = maxFraction;
            NumHits = 0;
            IgnoreTriggers = true;
            Bodies = rigidBodies;
            NumDynamicBodies = numDynamicBodies;
        }

        #region ICollector

        public bool AddHit(RaycastHit hit)
        {
            Assert.IsTrue(hit.Fraction < MaxFraction);
            MaxFraction = hit.Fraction;
            m_OldHit = m_ClosestHit;
            m_ClosestHit = hit;
            NumHits = 1;            
            return true;
        }

        void CheckIsAcceptable(float oldFraction)
        {
            var isAcceptable =  (0 <= m_ClosestHit.RigidBodyIndex) && (m_ClosestHit.RigidBodyIndex < NumDynamicBodies);
            if (isAcceptable)
            {                
                var body = Bodies[m_ClosestHit.RigidBodyIndex];
            }
            if (!isAcceptable)
            {
                m_ClosestHit = m_OldHit;
                NumHits = 0;
                MaxFraction = oldFraction;
                m_OldHit = default(RaycastHit);
            }
        }

        public void TransformNewHits(int oldNumHits, float oldFraction, MTransform transform, uint numSubKeyBits, uint subKey)
        {
            m_ClosestHit.Transform(transform, numSubKeyBits, subKey);
        }

        public void TransformNewHits(int oldNumHits, float oldFraction, MTransform transform, int rigidBodyIndex)
        {
            m_ClosestHit.Transform(transform, rigidBodyIndex);
            CheckIsAcceptable(oldFraction);
        }
        #endregion
    }
    
    public struct MousePick : IComponentData
    {
        public int IgnoreTriggers;
    }

    public class MousePickBehaviour : MonoBehaviour, IConvertGameObjectToEntity
    {
        const float k_MaxDistance = 100.0f;
        public bool IgnoreTriggers = true;

        void IConvertGameObjectToEntity.Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            dstManager.AddComponentData(entity, new MousePick()
            {
                IgnoreTriggers = IgnoreTriggers ? 1 : 0,
            });
        }

        public static RaycastInput CreateRayCastFromMouse()
        {
            var unityRay = Camera.main.ScreenPointToRay(Input.mousePosition);
            return new RaycastInput
            {
                Start = unityRay.origin,
                End = unityRay.origin + unityRay.direction * k_MaxDistance,
                Filter = CollisionFilter.Default,
            };
        }
    }

    // Attaches a virtual spring to the picked entity
    [UpdateAfter(typeof(BuildPhysicsWorld)), UpdateBefore(typeof(EndFramePhysicsSystem))]
    public class MousePickSystem : JobComponentSystem
    {
        EntityQuery m_MouseGroup;
        public BuildPhysicsWorld m_BuildPhysicsWorldSystem;

        public NativeArray<SpringData> SpringDatas;
        public JobHandle? PickJobHandle;

        public struct SpringData
        {
            public Entity Entity;
            public int Dragging; // bool isn't blittable
            public float3 PointOnBody;
        }
        
        [BurstCompile]
        struct Pick : IJob
        {
            [ReadOnly] public CollisionWorld CollisionWorld;
            [ReadOnly] public int NumDynamicBodies;
            public NativeArray<SpringData> SpringData;
            public RaycastInput RayInput;
            public float Near;
            public float3 Forward;
            [ReadOnly] public bool IgnoreTriggers;

            public void Execute()
            {
                var mousePickCollector = new MousePickCollector(1.0f, CollisionWorld.Bodies, NumDynamicBodies);
                mousePickCollector.IgnoreTriggers = IgnoreTriggers;
                
                CollisionWorld.CastRay(RayInput, ref mousePickCollector);
                if (mousePickCollector.MaxFraction < 1.0f)
                {
                    float fraction = mousePickCollector.Hit.Fraction;
                    RigidBody hitBody = CollisionWorld.Bodies[mousePickCollector.Hit.RigidBodyIndex];

                    MTransform bodyFromWorld = Inverse(new MTransform(hitBody.WorldFromBody));
                    float3 pointOnBody = Mul(bodyFromWorld, mousePickCollector.Hit.Position);

                    SpringData[0] = new SpringData
                    {
                        Entity = hitBody.Entity,
                        Dragging = 1,
                        PointOnBody = pointOnBody,
                    };
                }
                else
                {
                    SpringData[0] = new SpringData
                    {
                        Dragging = 0
                    };
                }
            }
        }

        public MousePickSystem()
        {
            SpringDatas = new NativeArray<SpringData>(1, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
            SpringDatas[0] = new SpringData();
        }

        protected override void OnCreate()
        {
            m_BuildPhysicsWorldSystem = World.GetOrCreateSystem<BuildPhysicsWorld>();
            m_MouseGroup = GetEntityQuery(new EntityQueryDesc
            {
                All = new ComponentType[] { typeof(MousePick) }
            });
        }

        protected override void OnDestroyManager()
        {
            SpringDatas.Dispose();
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            if (m_MouseGroup.CalculateLength() == 0)
            {
                return inputDeps;
            }
            
            var handle = JobHandle.CombineDependencies(inputDeps, m_BuildPhysicsWorldSystem.FinalJobHandle);

            if (Input.GetMouseButtonDown(0) && (Camera.main != null))
            {                
                var mice = m_MouseGroup.ToComponentDataArray<MousePick>(Allocator.TempJob);
                var IgnoreTriggers = mice[0].IgnoreTriggers != 0;
                mice.Dispose();

                // Schedule picking job, after the collision world has been built
                handle = new Pick
                {
                    CollisionWorld = m_BuildPhysicsWorldSystem.PhysicsWorld.CollisionWorld,
                    NumDynamicBodies = m_BuildPhysicsWorldSystem.PhysicsWorld.NumDynamicBodies,
                    SpringData = SpringDatas,
                    RayInput = MousePickBehaviour.CreateRayCastFromMouse(),
                    Near = Camera.main.nearClipPlane,
                    Forward = Camera.main.transform.forward,
                    IgnoreTriggers = IgnoreTriggers,
                }.Schedule(JobHandle.CombineDependencies(handle, m_BuildPhysicsWorldSystem.FinalJobHandle));

                PickJobHandle = handle;

                handle.Complete(); // TODO.ma figure out how to do this properly...we need a way to make physics sync wait for
                // any user jobs that touch the component data, maybe a JobHandle LastUserJob or something that the user has to set
            }

            if (Input.GetMouseButtonUp(0))
            {
                SpringDatas[0] = new SpringData();
            }

            return handle;
        }
    }

    //[UpdateAfter(typeof(MousePickSystem))]
    public class MouseSpringSystem : JobComponentSystem
    {
        const float k_MaxDistance = 100.0f;
        EntityQuery m_MouseGroup;
        MousePickSystem m_PickSystem;
        bool m_WasDragging = false;
        RigidBody? m_TerrainBody;
        RigidBody[] m_RoomBodies;
        Entity m_SelectedEntity;
        EntityQuery m_TerrainEntityQuery;
        EntityQuery m_RoomEntityQuery;
        
        protected override void OnCreate()
        {
            m_PickSystem = World.GetOrCreateSystem<MousePickSystem>();
            m_MouseGroup = GetEntityQuery(new EntityQueryDesc
            {
                All = new ComponentType[] { typeof(MousePick) }
            });
            m_TerrainEntityQuery = EntityManager.CreateEntityQuery(typeof(Tag_Terrain));
            m_RoomEntityQuery = EntityManager.CreateEntityQuery(typeof(Tag_Room));
        }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            if (m_MouseGroup.CalculateLength() == 0)
            {
                return inputDeps;
            }

            var Positions = GetComponentDataFromEntity<Translation>();

            // If there's a pick job, wait for it to finish
            if (m_PickSystem.PickJobHandle != null)
            {
                JobHandle.CombineDependencies(inputDeps, m_PickSystem.PickJobHandle.Value).Complete();
            }
            
            // If there's a picked entity, drag it
            MousePickSystem.SpringData springData = m_PickSystem.SpringDatas[0];
            var physicsWorld = m_PickSystem.m_BuildPhysicsWorldSystem.PhysicsWorld;

            if (springData.Dragging != 0)
            {
                Entity entity = m_PickSystem.SpringDatas[0].Entity;

                if (!m_WasDragging)
                {
                    GameMgr.g.PlayPickMonsterSoundEffect(entity);
                }

                m_WasDragging = true;                
                Translation posComponent = Positions[entity];
                m_SelectedEntity = entity;
       
                var hits = new NativeList<RaycastHit>(Allocator.TempJob);
                physicsWorld.CollisionWorld.CastRay(MousePickBehaviour.CreateRayCastFromMouse(), ref hits);
                var terrainEntities = m_TerrainEntityQuery.ToEntityArray(Allocator.TempJob);
                foreach (var terrainEntity in terrainEntities) 
                {
                    var index = physicsWorld.GetRigidBodyIndex(terrainEntity);
                    foreach (var hit in hits.ToArray())
                    {
                        if (hit.RigidBodyIndex == index)
                        {
                            posComponent.Value.x = hit.Position.x;
                            posComponent.Value.z = hit.Position.z;
                            Positions[entity] = posComponent;
                            break;
                        }
                    }
                }
                
                hits.Dispose();
                terrainEntities.Dispose();
            }
            else if (m_WasDragging)
            {
                m_WasDragging = false;
                var hits = new NativeList<RaycastHit>(Allocator.TempJob);
                physicsWorld.CollisionWorld.CastRay(MousePickBehaviour.CreateRayCastFromMouse(), ref hits);

                var roomEntities = m_RoomEntityQuery.ToEntityArray(Allocator.TempJob);                
                foreach (var room in roomEntities)
                {
                    var index = physicsWorld.GetRigidBodyIndex(room);
                    foreach (var hit in hits.ToArray())
                    {
                        if (hit.RigidBodyIndex == index)
                        {
                            GameMgr.MoveMonsterToRoom(m_SelectedEntity, room, 0);
                        }
                    }
                }

                hits.Dispose();
                roomEntities.Dispose();
            }

            return inputDeps;
        }
    }
}
