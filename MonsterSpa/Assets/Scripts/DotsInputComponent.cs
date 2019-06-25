using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public class DotsInputComponent : MonoBehaviour
{
    private EntityManager entityManager;
    private Entity inputSingleton;
    private float2 lastPosition;
    void Start()
    {
        entityManager = World.Active.EntityManager;
        inputSingleton = entityManager.CreateEntity(ComponentType.ReadWrite<DotsMouseEvent>());
        var buffer = entityManager.GetBuffer<DotsMouseEvent>(inputSingleton);
        var mousePos = Input.mousePosition;
        lastPosition = new float2(mousePos.x, mousePos.y);
        buffer.Add(new DotsMouseEvent {Type = DotsMouseEvent.EventType.MouseMove, Position = lastPosition, Button = 0});
    }

    void Update()
    {
        var buffer = entityManager.GetBuffer<DotsMouseEvent>(inputSingleton);
        buffer.Clear();
        var mousePos = Input.mousePosition;
        if (lastPosition.x != mousePos.x || lastPosition.y != mousePos.y)
        {
            lastPosition = new float2(mousePos.x, mousePos.y);
            buffer.Add(new DotsMouseEvent
                {Type = DotsMouseEvent.EventType.MouseMove, Position = lastPosition, Button = 0});
        }

        for (int btn = 0; btn < 3; ++btn)
        {
            if (Input.GetMouseButtonDown(btn))
            {
                buffer.Add(new DotsMouseEvent
                    {Type = DotsMouseEvent.EventType.MouseDown, Position = lastPosition, Button = btn});
            }

            if (Input.GetMouseButtonUp(btn))
            {
                buffer.Add(new DotsMouseEvent
                    {Type = DotsMouseEvent.EventType.MouseUp, Position = lastPosition, Button = btn});
            }
        }
    }
}
