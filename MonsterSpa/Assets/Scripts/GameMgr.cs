using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public enum RoomType
{
    Lobby = 0,
    Sauna = 1,
    HotTub = 2,
    ColdBath = 3,
    Cafe = 4
}

public class GameMgr : MonoBehaviour
{
    public static GameMgr g;
    public static List<Entity> rooms = new List<Entity>();
    public static List<Entity> monsters = new List<Entity>();
    public List<GameObject> spawnables = new List<GameObject>();
    private System.Random randomizer = new System.Random();

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

        var roomEnt = GameObjectConversionUtility.ConvertGameObjectHierarchy(lobby, World.Active);
        rooms.Add(entityManager.Instantiate(roomEnt));
        roomEnt =  GameObjectConversionUtility.ConvertGameObjectHierarchy(sauna, World.Active);
        rooms.Add(entityManager.Instantiate(roomEnt));


        //these will be unlocked later, and should be removed from here when the time comes.
        roomEnt =  GameObjectConversionUtility.ConvertGameObjectHierarchy(hotTub, World.Active);
        rooms.Add(entityManager.Instantiate(roomEnt));
        roomEnt =  GameObjectConversionUtility.ConvertGameObjectHierarchy(coldBath, World.Active);
        rooms.Add(entityManager.Instantiate(roomEnt));
        roomEnt =  GameObjectConversionUtility.ConvertGameObjectHierarchy(cafe, World.Active);
        rooms.Add(entityManager.Instantiate(roomEnt));


        //monsters should just come inside over time
        var monEnt = GameObjectConversionUtility.ConvertGameObjectHierarchy(Chick, World.Active);
        gameObjectToMonsterEntityMap[Chick] = monEnt;
        monEnt =  GameObjectConversionUtility.ConvertGameObjectHierarchy(Ghost, World.Active);
        gameObjectToMonsterEntityMap[Ghost] = monEnt;
        monEnt =  GameObjectConversionUtility.ConvertGameObjectHierarchy(Sandal, World.Active);
        gameObjectToMonsterEntityMap[Sandal] = monEnt;
        monEnt =  GameObjectConversionUtility.ConvertGameObjectHierarchy(Hundun, World.Active);
        gameObjectToMonsterEntityMap[Hundun] = monEnt;

        spawnables = new List<GameObject>() { Chick };
    }

    Dictionary<GameObject, Entity> gameObjectToMonsterEntityMap = new Dictionary<GameObject, Entity>();

    private Entity? InstantiateRandomMonster()
    {
        if (spawnables.Count == 0)
        {
            return null;
        }

        var index = randomizer.Next(spawnables.Count);
        var gameObjectToSpawn = spawnables[index];
        return World.Active.EntityManager.Instantiate(gameObjectToMonsterEntityMap[gameObjectToSpawn]);
    }

    public static void MoveMonsterToRoom(Entity monsterEntity, Entity roomEntity, float3 spawnPos, float timeToLeave)
    {
        EntityManager entityManager = World.Active.EntityManager;
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
        // TODO: Check if room is full
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
            var room = entityManager.GetComponentData<InsideRoom>(dyingMon).RoomEntity;
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
        finalPos = roomPos + new float3(posx, 0.1f, posz);

        return finalPos;

    }
}
