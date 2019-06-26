using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using System;
using System.Threading;
using System.Xml;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine.UIElements;

public enum MonsterType
{
    Chick = 0,
    Ghost = 1,
    Sandal = 2,
    Hundun = 3
}

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
    //public float spawnInterval;

    public static List<Entity> monsterEnts = new List<Entity>();

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
        
        countdown = spawnrate;
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
        monsterEnts.Add(monEnt);
        monEnt =  GameObjectConversionUtility.ConvertGameObjectHierarchy(Ghost, World.Active);
        monsterEnts.Add(monEnt);
        monEnt =  GameObjectConversionUtility.ConvertGameObjectHierarchy(Sandal, World.Active);
        monsterEnts.Add(monEnt);
        monEnt =  GameObjectConversionUtility.ConvertGameObjectHierarchy(Hundun, World.Active);
        monsterEnts.Add(monEnt);
        
        
        //monsters.Add(entityManager.Instantiate(monsterEnts[0]));
        
        
        //entityManager.SetComponentData(monsters[(int)MonsterType.Chick], new InsideRoom(){RoomEntity = rooms[(int)RoomType.Lobby]});

    }

    // Update is called once per frame
    void Update()
    {
        EntityManager entityManager = World.Active.EntityManager;
        var dt = Time.deltaTime;
        if (countdown < 0.01)
        {
        
            
        
            //spawn a new monster
            var monsterEnt = entityManager.Instantiate(monsterEnts[(int)MonsterType.Chick]);
            monsters.Add(monsterEnt);

            float3 spawnPos = FindSpawnInCircle(rooms[(int) RoomType.Lobby]);
            entityManager.SetComponentData(monsterEnt, new Translation(){Value=spawnPos});

            var roomEntity = rooms[(int) RoomType.Lobby];
            entityManager.SetComponentData(monsterEnt, new InsideRoom(){RoomEntity = roomEntity });

            var monsterBuffer = entityManager.GetBuffer<Monster>(roomEntity);
            //so we need to make a monster type
            var mon = new Monster();
            mon.Value = monsterEnt;
            monsterBuffer.Add(mon);
            countdown = spawnrate;
        }
        else
        {
            countdown -= dt;
        }
        
        
        
        //lets destroy monsters! this happens outside of jobs to make sure we dont need to schedule weird things.
        var destroyArray = monstersToDestroyQuery.ToEntityArray(Allocator.TempJob);

        foreach (var dyingMon in destroyArray)
        {
            monsters.Remove(dyingMon);
            var room = entityManager.GetComponentData<InsideRoom>(dyingMon).RoomEntity;
            var monsterBuffer = entityManager.GetBuffer<Monster>(room);
            for (int i = monsterBuffer.Length; i < 0 ; i--)
            {
                if (monsterBuffer[i].Value == dyingMon)
                {
                    monsterBuffer.RemoveAt(i);
                }
            }
        }
        entityManager.DestroyEntity(destroyArray);
        destroyArray.Dispose();
    }

    public static float3 FindSpawnInCircle(Entity room)
    {
        EntityManager entityManager = World.Active.EntityManager;
        var roomSpotComp = entityManager.GetComponentData<RoomSpots>(room);
        var roomPos = entityManager.GetComponentData<Translation>(room).Value;
        
        //lets place monsters around the circle!
        var numMons = entityManager.GetBuffer<Monster>(room).Length;
        
        
        //there will only be a few spots that is available for monsters to be in the spa. lets make it equidistant from the root.
        float intervalInRads = (350.0f / roomSpotComp.Value) * (math.PI/180) * numMons;
        float posx = (float) Math.Cos(intervalInRads);
        float posz = (float) Math.Sin(intervalInRads);

        return roomPos + new float3(posx, 0.0f, posz);

    }
}
