using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using static Unity.Mathematics.math;

public struct DialogueText : IBufferElementData
{
    public NativeString512 text;
}

public struct DialogueLineTracker : IComponentData
{
    public int currentLine;
    public int finalLine;
}
public struct DialogueEventSkip : IComponentData {}

public struct DialogueEventFinished : IComponentData {}

[UpdateInGroup(typeof(SimulationSystemGroup))]
public class DialogueSystem : JobComponentSystem
{
    [RequireComponentTag(typeof(UIEventMouseDown))][ExcludeComponent(typeof(DialogueEventSkip))]
    private struct ProgressDialogueJob : IJobForEachWithEntity<DialogueLineTracker>
    {
        public EntityCommandBuffer ecb;
        
        public void Execute(Entity entity, int index, ref DialogueLineTracker tracker)
        {
            tracker.currentLine++;

            if (tracker.currentLine != tracker.finalLine) return;
            
            DialogueEventFinished tag;
            ecb.AddComponent(entity, tag);
        }
    }
    
    [RequireComponentTag(typeof(DialogueEventSkip))]
    private struct SkipDialogueJob : IJobForEachWithEntity<DialogueLineTracker>
    {
        public EntityCommandBuffer ecb;
        
        public void Execute(Entity entity, int index, ref DialogueLineTracker tracker)
        {
            tracker.currentLine = tracker.finalLine;
            ecb.RemoveComponent<DialogueEventSkip>(entity);

            DialogueEventFinished tag;
            ecb.AddComponent(entity, tag);
        }
    }

    private EntityCommandBufferSystem endSimEcb;

    protected override void OnCreate()
    {
        endSimEcb = World.Active.GetExistingSystem<EndSimulationEntityCommandBufferSystem>();
    }

    protected override JobHandle OnUpdate(JobHandle inputDependencies)
    {
        var progressDialogueJob = new ProgressDialogueJob()
        {
            ecb = endSimEcb.CreateCommandBuffer()
        }.ScheduleSingle(this, inputDependencies);

        var skipDialogueJob = new SkipDialogueJob()
        {
            ecb = endSimEcb.CreateCommandBuffer()
        }.ScheduleSingle(this, progressDialogueJob);
        
        endSimEcb.AddJobHandleForProducer(skipDialogueJob);
        
        return skipDialogueJob;
    }
}