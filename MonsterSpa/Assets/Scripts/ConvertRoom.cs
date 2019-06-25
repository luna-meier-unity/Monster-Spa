using Unity.Entities;
using UnityEngine;

[RequiresEntityConversion]
public class ConvertRoom : MonoBehaviour, IConvertGameObjectToEntity
{
    public float temprature;
    public void Convert(Entity entity, EntityManager entityManager, GameObjectConversionSystem conversionSystem)
    {
        var roomTag = new Tag_Room();

        var temp = new Temperature();
        temp.Value = temprature;
        
        var name = new RoomName();
        name.name = new NativeString64(gameObject.name);

        entityManager.AddComponentData(entity, roomTag);
        entityManager.AddComponentData(entity, temp);
        entityManager.AddComponentData(entity, name);

    }
}