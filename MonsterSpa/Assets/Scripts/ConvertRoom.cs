using Unity.Entities;
using UnityEngine;

[RequiresEntityConversion]
public class ConvertRoom : MonoBehaviour, IConvertGameObjectToEntity
{
    public float temprature;
    public int Spots;
    public void Convert(Entity entity, EntityManager entityManager, GameObjectConversionSystem conversionSystem)
    {
        var roomTag = new Tag_Room();

        var temp = new RoomTemperature();
        temp.Value = temprature;
        
        var name = new RoomName();
        name.name = new NativeString64(gameObject.name);
        
        var spots = new RoomSpots();
        spots.Value = Spots;


        entityManager.AddBuffer<Monster>(entity);
        entityManager.AddComponentData(entity, roomTag);
        entityManager.AddComponentData(entity, temp);
        entityManager.AddComponentData(entity, name);
        entityManager.AddComponentData(entity,spots);

    }
}