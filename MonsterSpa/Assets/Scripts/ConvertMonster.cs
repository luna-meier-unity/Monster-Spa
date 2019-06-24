using Unity.Entities;
using UnityEngine;

[RequiresEntityConversion]
public class ConvertMonster : MonoBehaviour, IConvertGameObjectToEntity
{
    public void Convert(Entity entity, EntityManager entityManager, GameObjectConversionSystem conversionSystem)
    {
        
    }
}