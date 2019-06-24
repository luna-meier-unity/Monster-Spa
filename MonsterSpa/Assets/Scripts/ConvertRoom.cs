using Unity.Entities;
using UnityEngine;

[RequiresEntityConversion]
public class ConvertRoom : MonoBehaviour, IConvertGameObjectToEntity
{
    public void Convert(Entity entity, EntityManager entityManager, GameObjectConversionSystem conversionSystem)
    {
        var roomTag = new Tag_Room();

        var temp = new Temperature();
        temp.Value = 100; //do some changes based on what room we are in!
        
        var name = new RoomName();

        name.name = new NativeString64(gameObject.name);

        entityManager.AddComponentData(entity, roomTag);
        entityManager.AddComponentData(entity, temp);
        entityManager.AddComponentData(entity, name);

    }
}