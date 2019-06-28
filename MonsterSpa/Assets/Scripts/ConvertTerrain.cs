using Unity.Entities;
using UnityEngine;



[RequiresEntityConversion]
public class ConvertTerrain : MonoBehaviour, IConvertGameObjectToEntity
{
    public void Convert(Entity entity, EntityManager entityManager, GameObjectConversionSystem conversionSystem)
    {
        entityManager.AddComponent(entity, typeof(Tag_Terrain));
    }
}