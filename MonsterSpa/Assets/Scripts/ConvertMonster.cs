using System.Security.Cryptography.X509Certificates;
using Unity.Entities;
using UnityEngine;

[RequiresEntityConversion]
public class ConvertMonster : MonoBehaviour, IConvertGameObjectToEntity
{
    public void Convert(Entity entity, EntityManager entityManager, GameObjectConversionSystem conversionSystem)
    {
        entityManager.AddComponent(entity,typeof(InsideRoom));
        
        
        var hunger = new HungerLevel();
        hunger.Value = 50;
        
        var happy = new HappinessLevel();
        happy.Value = 50;
        
        var temperature = new Temperature();
        temperature.Value = 68;
        
        var timeRemaining = new TimeToLeave();
        timeRemaining.TimeRemaining = 60;
        
        
        entityManager.AddComponentData(entity, hunger);
        entityManager.AddComponentData(entity, happy);
        entityManager.AddComponentData(entity, temperature);
        entityManager.AddComponentData(entity, timeRemaining);
        
    }
}