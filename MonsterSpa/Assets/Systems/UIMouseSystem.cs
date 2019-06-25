using System;
using System.Reflection;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Rendering;
using UnityEngine;
using UnityEngine.Rendering;

//These just happen to be tags, they could have data in the future.

/// <summary>
/// Component added to an entity if the mouse was pressed down on it this frame.
/// </summary>
public struct UIEventMouseDown : ISystemStateComponentData{}
/// <summary>
/// Component added to an entity if the mouse was moved on it this frame.
/// </summary>
public struct UIEventMouseMove : ISystemStateComponentData{}
/// <summary>
/// Component added to an entity if the mouse was released on it this frame.
/// </summary>
public struct UIEventMouseUp : ISystemStateComponentData{}
/// <summary>
/// Component added to an entity if the mouse was hovering over it on the current frame.
/// </summary>
public struct UIEventMouseHover : ISystemStateComponentData{}
/// <summary>
/// Component added to an entity if the mouse is holding the entity on the current frame.
/// </summary>
public struct UIEventMouseHeld : ISystemStateComponentData {}
/// <summary>
/// Component added to an entity if the mouse was previously holding the entity and just released.
/// </summary>
public struct UIEventMouseRelease : ISystemStateComponentData{}

/// <summary>
/// Is the mouse hovering over the entity this frame?
/// </summary>
public struct UICollidedWithMouseThisFrame : IComponentData
{
    public bool b;
}

[UpdateInGroup(typeof(InitializationSystemGroup))]
public class UIMouseSystem : JobComponentSystem
{
    private EntityQuery _allTagsNeedCollidersQuery;
    
    private EntityQuery _mouseEventBufferQuery;
    private EntityCommandBufferSystem _initSystem;
    private EntityCommandBufferSystem _endSystem;

    protected override void OnCreate()
    {
        _initSystem = World.Active.GetExistingSystem<BeginSimulationEntityCommandBufferSystem>();
        _endSystem = World.Active.GetExistingSystem<EndSimulationEntityCommandBufferSystem>();
        
        _mouseEventBufferQuery = GetEntityQuery(new EntityQueryDesc
        {
            All = new ComponentType[] { ComponentType.ReadOnly<DotsMouseEvent>() }
        });
        
        _allTagsNeedCollidersQuery = GetEntityQuery(new EntityQueryDesc
        {
            Any = new ComponentType[]
            {
                ComponentType.ReadOnly<Tag_RespondsToMouseUp>(),
                ComponentType.ReadOnly<Tag_RespondsToMouseDown>(),
                ComponentType.ReadOnly<Tag_RespondsToMouseHeld>(),
                ComponentType.ReadOnly<Tag_RespondsToMouseMove>(),
                ComponentType.ReadOnly<Tag_RespondsToMouseHover>()
            },
            None = new ComponentType[]
            {
                typeof(UICollidedWithMouseThisFrame)
            }
        });
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {  
        EntityManager.AddComponent(_allTagsNeedCollidersQuery, typeof(UICollidedWithMouseThisFrame));  
        
        var mouseEventEntities = _mouseEventBufferQuery.ToEntityArray(Allocator.TempJob);

        if (mouseEventEntities.Length == 0)
        {
            mouseEventEntities.Dispose();
            return inputDeps;
        }

        var mouseIsPressed = false;
        var mouseIsMoved = false;
        var mouseIsReleased = false;

        var mouseEvents = EntityManager.GetBuffer<DotsMouseEvent>(mouseEventEntities[0]);

        if (mouseEvents.Length == 0)
        {
            mouseEventEntities.Dispose();
            return inputDeps;
        }
        
        for (var i = 0; i < mouseEvents.Length; ++i)
        {
            switch(mouseEvents[i].Type)
            {
                case DotsMouseEvent.EventType.MouseDown:
                    mouseIsPressed = true;
                    break;
                case DotsMouseEvent.EventType.MouseUp:
                    mouseIsReleased = true;
                    break;
                case DotsMouseEvent.EventType.MouseMove:
                    mouseIsMoved = true;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        var handleCollisionsJobHandle = new UIMouseCheckCollisionJob().Schedule(this, inputDeps);
        
        mouseEventEntities.Dispose();

        var finalHandles = new JobHandle();

        if (mouseIsPressed)
        {
            var mouseDownJobHandle = new UIMouseDownJob()
            {
                CommandBuffer = _initSystem.CreateCommandBuffer().ToConcurrent(),
                EventCleanupCommandBuffer = _endSystem.CreateCommandBuffer().ToConcurrent()
            }.Schedule(this, handleCollisionsJobHandle);

            _initSystem.AddJobHandleForProducer(mouseDownJobHandle);
            _endSystem.AddJobHandleForProducer(mouseDownJobHandle);
            finalHandles = JobHandle.CombineDependencies(finalHandles, mouseDownJobHandle);
            
            var mouseHeldBeginJobHandle = new UIMouseHeldBeginJob()
            {
                CommandBuffer = _initSystem.CreateCommandBuffer().ToConcurrent()
            }.Schedule(this, handleCollisionsJobHandle);

            _initSystem.AddJobHandleForProducer(mouseHeldBeginJobHandle);
            finalHandles = JobHandle.CombineDependencies(finalHandles, mouseHeldBeginJobHandle);
        }
        
        if (mouseIsMoved)
        {
            var mouseMoveJobHandle = new UIMouseMoveJob()
            {
                CommandBuffer = _initSystem.CreateCommandBuffer().ToConcurrent(),
                EventCleanupCommandBuffer = _endSystem.CreateCommandBuffer().ToConcurrent()
            }.Schedule(this, handleCollisionsJobHandle);

            _initSystem.AddJobHandleForProducer(mouseMoveJobHandle);
            _endSystem.AddJobHandleForProducer(mouseMoveJobHandle);
            finalHandles = JobHandle.CombineDependencies(finalHandles, mouseMoveJobHandle);
            
            var mouseHoverBeginJobHandle = new UIMouseHoverBeginJob()
            {
                CommandBuffer = _initSystem.CreateCommandBuffer().ToConcurrent()
            }.Schedule(this, handleCollisionsJobHandle);

            _initSystem.AddJobHandleForProducer(mouseHoverBeginJobHandle);
            finalHandles = JobHandle.CombineDependencies(finalHandles, mouseHoverBeginJobHandle);
            
            var mouseHoverEndHandle = new UIMouseHoverEndJob()
            {
                CommandBuffer = _initSystem.CreateCommandBuffer().ToConcurrent()
            }.Schedule(this, handleCollisionsJobHandle);

            _initSystem.AddJobHandleForProducer(mouseHoverEndHandle);
            finalHandles = JobHandle.CombineDependencies(finalHandles, mouseHoverEndHandle);
        }
        
        if (mouseIsReleased)
        {
            var mouseUpJobHandle = new UIMouseUpJob()
            {
                CommandBuffer = _initSystem.CreateCommandBuffer().ToConcurrent(),
                EventCleanupCommandBuffer = _endSystem.CreateCommandBuffer().ToConcurrent()
            }.Schedule(this, handleCollisionsJobHandle);

            _initSystem.AddJobHandleForProducer(mouseUpJobHandle);
            _endSystem.AddJobHandleForProducer(mouseUpJobHandle);
            finalHandles = JobHandle.CombineDependencies(finalHandles, mouseUpJobHandle);
            
            var mouseHeldEndJobHandle = new UIMouseHeldEndJob()
            {
                CommandBuffer = _initSystem.CreateCommandBuffer().ToConcurrent(),
                EventCleanupCommandBuffer = _endSystem.CreateCommandBuffer().ToConcurrent()
            }.Schedule(this, handleCollisionsJobHandle);

            _initSystem.AddJobHandleForProducer(mouseHeldEndJobHandle);
            _endSystem.AddJobHandleForProducer(mouseHeldEndJobHandle);
            finalHandles = JobHandle.CombineDependencies(finalHandles, mouseHeldEndJobHandle);
        }

        finalHandles = JobHandle.CombineDependencies(finalHandles, handleCollisionsJobHandle);
        
        return finalHandles;
    }

    private static bool InsideBox(float2 point, ref AABB box)
    {
        return (point.x > box.Size.x  / -2f
                && point.x < box.Size.x  / 2f
                && point.y < box.Size.y  / 2f
                && point.y > box.Size.y  / -2f);
    }
    
    [BurstCompile]
    private struct UIMouseCheckCollisionJob : IJobForEach<UICollidedWithMouseThisFrame>
    {
        public void Execute(
            ref UICollidedWithMouseThisFrame collided)
        {
            //TODO replace this with PhysicsCollision
            collided.b = true;
        }
    }

    [RequireComponentTag(typeof(Tag_RespondsToMouseUp))]
    private struct UIMouseUpJob : IJobForEachWithEntity<UICollidedWithMouseThisFrame>
    {
        public EntityCommandBuffer.Concurrent CommandBuffer;
        public EntityCommandBuffer.Concurrent EventCleanupCommandBuffer;

        public void Execute(Entity entity, int index, [ReadOnly] ref UICollidedWithMouseThisFrame collided)
        {
            if (!collided.b)
                return;

            var uiEvent = new UIEventMouseUp();

            CommandBuffer.AddComponent(index, entity, uiEvent);
            EventCleanupCommandBuffer.RemoveComponent<UIEventMouseUp>(index, entity);
        }
    }
    
    [RequireComponentTag(typeof(Tag_RespondsToMouseDown))]
    private struct UIMouseDownJob : IJobForEachWithEntity<UICollidedWithMouseThisFrame>
    {
        public EntityCommandBuffer.Concurrent CommandBuffer;
        public EntityCommandBuffer.Concurrent EventCleanupCommandBuffer;

        public void Execute(Entity entity, int index, [ReadOnly] ref UICollidedWithMouseThisFrame collided)
        {
            if (!collided.b)
                return;

            var uiEvent = new UIEventMouseDown();

            CommandBuffer.AddComponent(index, entity, uiEvent);
            EventCleanupCommandBuffer.RemoveComponent<UIEventMouseDown>(index, entity);
        }
    }
    
    [RequireComponentTag(typeof(Tag_RespondsToMouseMove))]
    private struct UIMouseMoveJob : IJobForEachWithEntity<UICollidedWithMouseThisFrame>
    {
        public EntityCommandBuffer.Concurrent CommandBuffer;
        public EntityCommandBuffer.Concurrent EventCleanupCommandBuffer;

        public void Execute(Entity entity, int index, [ReadOnly] ref UICollidedWithMouseThisFrame collided)
        {
            if (!collided.b)
                return;

            var uiEvent = new UIEventMouseMove();

            CommandBuffer.AddComponent(index, entity, uiEvent);
            EventCleanupCommandBuffer.RemoveComponent<UIEventMouseMove>(index, entity);
        }
    }
    
    [RequireComponentTag(typeof(Tag_RespondsToMouseHover))][ExcludeComponent(typeof(UIEventMouseHover))]
    private struct UIMouseHoverBeginJob : IJobForEachWithEntity<UICollidedWithMouseThisFrame>
    {
        public EntityCommandBuffer.Concurrent CommandBuffer;

        public void Execute(Entity entity, int index, [ReadOnly] ref UICollidedWithMouseThisFrame collided)
        {
            if (!collided.b)
                return;

            var uiEvent = new UIEventMouseHover();

            CommandBuffer.AddComponent(index, entity, uiEvent);
        }
    }
    
    [RequireComponentTag(typeof(Tag_RespondsToMouseHover), typeof(UIEventMouseHover))]
    private struct UIMouseHoverEndJob : IJobForEachWithEntity<UICollidedWithMouseThisFrame>
    {
        public EntityCommandBuffer.Concurrent CommandBuffer;

        public void Execute(Entity entity, int index, [ReadOnly] ref UICollidedWithMouseThisFrame collided)
        {
            if (collided.b)
                return;

            CommandBuffer.RemoveComponent<UIEventMouseHover>(index, entity);
        }
    }
    
    [RequireComponentTag(typeof(Tag_RespondsToMouseHeld))][ExcludeComponent(typeof(UIEventMouseHeld))]
    private struct UIMouseHeldBeginJob : IJobForEachWithEntity<UICollidedWithMouseThisFrame>
    {
        public EntityCommandBuffer.Concurrent CommandBuffer;

        public void Execute(Entity entity, int index, [ReadOnly] ref UICollidedWithMouseThisFrame collided)
        {
            if (!collided.b)
                return;

            var uiEvent = new UIEventMouseHeld();

            CommandBuffer.AddComponent(index, entity, uiEvent);
        }
    }
    
    [RequireComponentTag(typeof(Tag_RespondsToMouseHeld), typeof(UIEventMouseHeld))]
    private struct UIMouseHeldEndJob : IJobForEachWithEntity<UICollidedWithMouseThisFrame>
    {
        public EntityCommandBuffer.Concurrent CommandBuffer;
        public EntityCommandBuffer.Concurrent EventCleanupCommandBuffer;

        public void Execute(Entity entity, int index, [ReadOnly] ref UICollidedWithMouseThisFrame collided)
        {
            CommandBuffer.RemoveComponent<UIEventMouseHeld>(index, entity);
            
            var uiEvent = new UIEventMouseRelease(); 
            CommandBuffer.AddComponent(index, entity, uiEvent);
            EventCleanupCommandBuffer.RemoveComponent<UIEventMouseRelease>(index, entity);
        }
    }
}

/// <summary>
/// Will add a UIEventMouseDown component to the entity when mouse is pressed down over its AABB.
/// </summary>
struct Tag_RespondsToMouseDown : IComponentData { }

/// <summary>
/// Will add a UIEventMouseMove component to the entity when the mouse is moved over its AABB.
/// </summary>
struct Tag_RespondsToMouseMove : IComponentData { }

/// <summary>
/// Will add a UIEventMouseUp component to the entity when mouse is released over its AABB.
/// </summary>
struct Tag_RespondsToMouseUp : IComponentData { }

/// <summary>
/// Maintains a UIEventMouseHeld component on the entity after a mouse is pressed over the entity's AABB, and until
/// it is released anywhere.  Will add a UIEventMouseReleased component on release.
/// </summary>
struct Tag_RespondsToMouseHeld : IComponentData { }

/// <summary>
/// Maintains a UIEventMouse component on the entity after a mouse is pressed over the entity's AABB, and until
/// it is released anywhere.
/// </summary>
struct Tag_RespondsToMouseHover : IComponentData { }