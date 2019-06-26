using Unity.Entities;
using UnityEngine;

[RequiresEntityConversion]
public class ConvertTerrain : MonoBehaviour, IConvertGameObjectToEntity
{
    public static Entity TerrainEntity;

    public void Convert(Entity entity, EntityManager entityManager, GameObjectConversionSystem conversionSystem)
    {
        TerrainEntity = entity;
    }
}