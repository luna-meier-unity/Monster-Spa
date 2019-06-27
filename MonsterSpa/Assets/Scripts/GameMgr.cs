using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using System.Linq;

public enum RoomType
{
    Lobby = 0,
    Sauna = 1,
    HotTub = 2,
    ColdBath = 3,
    Cafe = 4
}

class MonsterConfig
{
    public SoundEffectType onPickSoundEffect;
    public Entity entity;
}

struct MonsterTypeComponent : IComponentData
{
    public int GameObjectId;
}

public class GameMgr : MonoBehaviour
{    
    public static GameMgr g;
    public static List<Entity> rooms = new List<Entity>();
    public static List<Entity> monsters = new List<Entity>();
    public List<GameObject> spawnables = new List<GameObject>();
    private System.Random randomizer = new System.Random();

    Dictionary<GameObject, MonsterConfig> gameObjectToMonsterEntityMap = new Dictionary<GameObject, MonsterConfig>();
    Dictionary<GameObject, Entity> gameObjectToRoomEntityMap = new Dictionary<GameObject, Entity>();

    //Prefabs
    public GameObject lobby;
    public GameObject sauna;
    public GameObject hotTub;
    public GameObject coldBath;
    public GameObject cafe;
    public GameObject Chick;
    public GameObject Ghost;
    public GameObject Sandal;
    public GameObject Hundun;

    public float spawnrate; //in seconds
    private float countdown;
    private EntityQuery monstersToDestroyQuery;

    // Start is called before the first frame update
    void Start()
    {
        //starttime = Time.time;
        if(g != null)
            throw new Exception("Multiple GameMgr gameobject components in scene, please only have one constant gameobject in the scene.");

        g = this;

        EntityManager entityManager = World.Active.EntityManager;

        monstersToDestroyQuery = entityManager.CreateEntityQuery(typeof(Tag_RemoveMonster));

        
        //SpawnRoom(lobby);
        var roomEnt = GameObjectConversionUtility.ConvertGameObjectHierarchy(lobby, World.Active);
        var instantiatedRoom = entityManager.Instantiate(roomEnt);
        rooms.Add(instantiatedRoom);
        gameObjectToRoomEntityMap[lobby] = instantiatedRoom;

        roomEnt =  GameObjectConversionUtility.ConvertGameObjectHierarchy(sauna, World.Active);
        instantiatedRoom = entityManager.Instantiate(roomEnt);
        rooms.Add(entityManager.Instantiate(instantiatedRoom));
        gameObjectToRoomEntityMap[sauna] = instantiatedRoom;


        //these will be unlocked later, and should be removed from here when the time comes.
        roomEnt =  GameObjectConversionUtility.ConvertGameObjectHierarchy(hotTub, World.Active);
        instantiatedRoom = entityManager.Instantiate(roomEnt);
        rooms.Add(entityManager.Instantiate(instantiatedRoom));
        gameObjectToRoomEntityMap[hotTub] = instantiatedRoom;
        
        
        
        
        roomEnt =  GameObjectConversionUtility.ConvertGameObjectHierarchy(coldBath, World.Active);
        instantiatedRoom = entityManager.Instantiate(roomEnt);
        rooms.Add(entityManager.Instantiate(instantiatedRoom));
        gameObjectToRoomEntityMap[coldBath] = instantiatedRoom;

        //this no longer exists
        //roomEnt =  GameObjectConversionUtility.ConvertGameObjectHierarchy(cafe, World.Active);
        //instantiatedRoom = entityManager.Instantiate(roomEnt);
        //rooms.Add(entityManager.Instantiate(instantiatedRoom));
        //gameObjectToRoomEntityMap[cafe] = instantiatedRoom;

        // Mapping game objects to entities
        var monEnt = GameObjectConversionUtility.ConvertGameObjectHierarchy(Chick, World.Active);
        gameObjectToMonsterEntityMap[Chick] = new MonsterConfig()
        {
            onPickSoundEffect = SoundEffectType.Chirp,
            entity = monEnt,
        };

        monEnt =  GameObjectConversionUtility.ConvertGameObjectHierarchy(Ghost, World.Active);
        gameObjectToMonsterEntityMap[Ghost] = new MonsterConfig()
        {
            onPickSoundEffect = SoundEffectType.Wind,
            entity = monEnt,
        };

        monEnt =  GameObjectConversionUtility.ConvertGameObjectHierarchy(Sandal, World.Active);
        gameObjectToMonsterEntityMap[Sandal] = new MonsterConfig()
        {
            onPickSoundEffect = SoundEffectType.Cockneye,
            entity = monEnt,
        };
        
        monEnt =  GameObjectConversionUtility.ConvertGameObjectHierarchy(Hundun, World.Active);
        gameObjectToMonsterEntityMap[Hundun] = new MonsterConfig()
        {
            onPickSoundEffect = SoundEffectType.Buluboop,
            entity = monEnt,
        };
        
        spawnables = new List<GameObject>() { Chick };
    }

    public void SpawnRoom(GameObject room)
    {
        //FOR THE LOVE OF GOD DO NOT SPAWN THE SAME ROOM TWICE
        //room is found in Gamemgr.<roomname>, such as sauna. this is a reference from the editor field.
        var entityManager = World.Active.EntityManager;
        var roomEnt =  GameObjectConversionUtility.ConvertGameObjectHierarchy(room, World.Active);
        var instantiatedRoom = entityManager.Instantiate(roomEnt);
        rooms.Add(entityManager.Instantiate(instantiatedRoom));
        gameObjectToRoomEntityMap[coldBath] = instantiatedRoom;
    }
    
    private Entity? InstantiateRandomMonster()
    {
        if (spawnables.Count == 0)
        {
            return null;
        }

        var index = randomizer.Next(spawnables.Count);
        var gameObjectToSpawn = spawnables[index];
        
        var entityManager = World.Active.EntityManager;
        var entity = entityManager.Instantiate(gameObjectToMonsterEntityMap[gameObjectToSpawn].entity);
        return entity;
    }
    
    Entity GetRoomEntity(GameObject gameObject)
    {
        return gameObjectToRoomEntityMap[gameObject];
    }

    public void MoveMonsterToLobbyWithEcb(EntityCommandBuffer.Concurrent ecb, int index, Entity monsterEntity, float timeToLeave)
    {
        ecb.SetComponent(index, monsterEntity, new InsideRoom() { RoomEntity = GetRoomEntity(lobby) });

        // EntityManager entityManager = World.Active.EntityManager;
        // entityManager.SetComponentData(monsterEntity, new Translation() { Value = spawnPos });
        // entityManager.SetComponentData(monsterEntity, new InsideRoom() { RoomEntity = roomEntity });
        // entityManager.SetComponentData(monsterEntity, new TimeToLeave() { TimeRemaining = timeToLeave });

        // var monsterBuffer = entityManager.GetBuffer<Monster>(roomEntity);
        //so we need to make a monster type
        /*var mon = new Monster();
        mon.Value = monsterEntity;
        monsterBuffer.Add(mon);*/
    }

    public void PlayPickMonsterSoundEffect(Entity monsterEntity)
    {
        var monsterTypeComponent = World.Active.EntityManager.GetComponentData<MonsterTypeComponent>(monsterEntity);

        foreach (var kvp in gameObjectToMonsterEntityMap)
        {            
            if (kvp.Key.GetInstanceID() == monsterTypeComponent.GameObjectId)
            {
                SoundEffectManager.g.PlaySoundEffect(kvp.Value.onPickSoundEffect);
                return;
            }
        }        
    }

    public static void MoveMonsterToRoom(Entity monsterEntity, Entity roomEntity, float3 spawnPos, float timeToLeave)
    {


        EntityManager entityManager = World.Active.EntityManager;
        var thing = entityManager.HasComponent<InsideRoom>(monsterEntity);
        //we only want to remove from previous room if it had a room to begin with.
        if (entityManager.GetComponentData<InsideRoom>(monsterEntity).RoomEntity != Entity.Null)
        {
            //we need to remove the monster from the room it was in before adding it to the target room
            var previousBuffer = entityManager
                .GetBuffer<Monster>(entityManager.GetComponentData<InsideRoom>(monsterEntity).RoomEntity);
            for (int i = previousBuffer.Length-1; i >= 0; i--)
            {
                if (previousBuffer[i].Value.Equals(monsterEntity))
                {
                    previousBuffer.RemoveAt(i);
                }
            }
        }



        entityManager.SetComponentData(monsterEntity, new Translation() { Value = spawnPos });
        entityManager.SetComponentData(monsterEntity, new InsideRoom() { RoomEntity = roomEntity });
        entityManager.SetComponentData(monsterEntity, new TimeToLeave() { TimeRemaining = timeToLeave });

        var monsterBuffer = entityManager.GetBuffer<Monster>(roomEntity);
        //so we need to make a monster type
        var mon = new Monster();
        mon.Value = monsterEntity;
        monsterBuffer.Add(mon);
    }

    public static bool MoveMonsterToRoom(Entity monsterEntity, Entity roomEntity, float timeToLeave)
    {
        var entityManager = World.Active.EntityManager;
        var monsterBuffer = entityManager.GetBuffer<Monster>(roomEntity);
        if (entityManager.GetComponentData<RoomSpots>(roomEntity).Value <= monsterBuffer.Length)
        {
            return false; //room is full, return false
        }
        var spawnPos = FindSpawnInCircle(roomEntity);
        if (!spawnPos.HasValue)
        {
            return false;
        }

        MoveMonsterToRoom(monsterEntity, roomEntity, spawnPos.Value, timeToLeave);
        return true;
    }

    private void CleanupMonsters()
    {
        //lets destroy monsters! this happens outside of jobs to make sure we dont need to schedule weird things.
        EntityManager entityManager = World.Active.EntityManager;
        var destroyArray = monstersToDestroyQuery.ToEntityArray(Allocator.TempJob);

        foreach (var dyingMon in destroyArray)
        {
            monsters.Remove(dyingMon);
            var room = entityManager.GetComponentData<InsideRoom>(dyingMon).RoomEntity; //this is wrong...
            var monsterBuffer = entityManager.GetBuffer<Monster>(room);
            for (int i = monsterBuffer.Length-1; i >= 0; i--)
            {
                if (monsterBuffer[i].Value.Equals(dyingMon))
                {
                    monsterBuffer.RemoveAt(i);
                }
            }
        }
        entityManager.DestroyEntity(destroyArray);
        destroyArray.Dispose();
    }

    void UpdateRoomState(Entity roomEntity)
    {
        //VictoryConditionManager.g.coldBathState
    }

    void UpdateRoomStates()
    {
        //VictoryConditionManager.g.coldBathState
    }

    // Update is called once per frame
    void Update()
    {
        EntityManager entityManager = World.Active.EntityManager;
        var dt = Time.deltaTime;
        if (countdown < 0.01)
        {
            var roomEntity = rooms[(int)RoomType.Lobby];
            var spawnPoint = FindSpawnInCircle(roomEntity);
            if (!spawnPoint.HasValue)
            {
                return;
            }

            var monster = InstantiateRandomMonster();
            if (!monster.HasValue)
            {
                return;
            }

            SoundEffectManager.g.PlaySoundEffect(SoundEffectType.Bell);
            MoveMonsterToRoom(monster.Value, roomEntity, spawnPoint.Value, 5);
            countdown = spawnrate;
        }
        else
        {
            countdown -= dt;
        }

        CleanupMonsters();
    }

    public static float3? FindSpawnInCircle(Entity room)
    {
        EntityManager entityManager = World.Active.EntityManager;
        var roomSpotComp = entityManager.GetComponentData<RoomSpots>(room);
        var roomPos = entityManager.GetComponentData<Translation>(room).Value;

        //we should check if there is a monster in the spot we are trying to spawn at!
        foreach (var existingMonster in monsters)
        {
            var existingPos = entityManager.GetComponentData<Translation>(existingMonster);
            //if existingPos.Value - //consider how to do this with FindSpawnInCircle
        }
        //lets place monsters around the circle!

        var monsterBuff = entityManager.GetBuffer<Monster>(room);
        var numMons = monsterBuff.Length;
        var iterateAngle = (360.0f / roomSpotComp.Value) * (math.PI/180);
        var intervalAngle = iterateAngle;
        var finalPos = new float3(0,0,0);
        float posx = 0;
        float posz = 0;


        for (int j = 0; j < roomSpotComp.Value; j++) //for each of our spots, check if there is an entity in that spot
        {
            posx = (float) Math.Cos(iterateAngle);
            posz = (float) Math.Sin(iterateAngle);
            finalPos = roomPos + new float3(posx, 0.1f, posz); //the position we are going to be looking at
            bool clearedForLanding = true;
            for (int i = 0; i < numMons; i++) //iterate over the monsters, monsters may not be in the order
            {

                var dv = entityManager.GetComponentData<Translation>(monsterBuff[i].Value).Value - finalPos;

                //if monsterBuff[i]

                Vector3 vectordv = (Vector3) dv;
                var b = vectordv.magnitude < 0.15f;
                if (vectordv.magnitude < 0.15f)
                {
                    clearedForLanding = false;

                    if (iterateAngle - (360.0f / roomSpotComp.Value) * (math.PI / 180) < 0.01 && i > 0)
                    {
                        //we have circled! return null
                        return null;
                    }

                }
            }

            if (clearedForLanding)
            {
                break; //we are go for spawning!
            }
            iterateAngle += intervalAngle; //move to the next spot in the circle
            clearedForLanding = true; //reset, so we can check the next spot!
        }

        //finalPos = roomPos + new float3(posx, 0.0f, posz); //dispite its name, we want to take its initial value.

        posx = (float) Math.Cos(iterateAngle);
        posz = (float) Math.Sin(iterateAngle);
        //the vector (posx, 0, posz) is a unit vector describing direction of magnitude 1 with the room's position being 0,0,0.
        var spawnCircle = new float3(posx, 0.05f, posz);
        float radius = entityManager.GetComponentData<SpawnRadius>(room).Value;
        //multiply the spawn circle unit vector by the radius to get a position around a circle of the spawn point
        finalPos = roomPos + (spawnCircle * radius);

        return finalPos;

    }
}
