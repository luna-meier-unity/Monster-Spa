using Unity.Entities;
using Unity.Mathematics;

public struct DotsMouseEvent : IBufferElementData
{
    public enum EventType
    {
        MouseDown,
        MouseUp,
        MouseMove
    }

    public EventType Type;
    public float2 Position;
    public int Button;
}
