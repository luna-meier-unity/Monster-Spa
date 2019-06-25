using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using System;
using Unity.Transforms;
using UnityEngine.UIElements;

public class GameMgr : MonoBehaviour
{
    public static GameMgr g;
    public List<Entity> rooms = new List<Entity>();
    public static List<Entity> monsters = new List<Entity>();
    public GameObject lobby;
    public GameObject sauna;
    public GameObject hotTub;
    public GameObject coldBath;
    public GameObject cafe;
    public GameObject Chick;
    public GameObject Ghost;
    public GameObject Sandal;
    public GameObject Hundun;

    public static List<Entity> monsterEnts = new List<Entity>();

    public int spawnrate; //in seconds

    private float starttime;
    // Start is called before the first frame update
    void Start()
    {
        starttime = Time.time;
        if(g != null)
            throw new Exception("Multiple GameMgr gameobject components in scene, please only have one constant gameobject in the scene.");

        g = this;
        
        EntityManager entityManager = World.Active.EntityManager;
        
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
        var monEnt =  GameObjectConversionUtility.ConvertGameObjectHierarchy(Chick, World.Active);
        monsterEnts.Add(monEnt);
        monEnt =  GameObjectConversionUtility.ConvertGameObjectHierarchy(Ghost, World.Active);
        monsterEnts.Add(monEnt);
        monEnt =  GameObjectConversionUtility.ConvertGameObjectHierarchy(Sandal, World.Active);
        monsterEnts.Add(monEnt);
        monEnt =  GameObjectConversionUtility.ConvertGameObjectHierarchy(Hundun, World.Active);
        monsterEnts.Add(monEnt);
        
        
        monsters.Add(entityManager.Instantiate(monsterEnts[0]));
        
        
        entityManager.SetComponentData(monsters[0], new InsideRoom(){RoomEntity = rooms[0]}); //room 0 is lobby!
        //entityManager.SetComponentData();

    }

    // Update is called once per frame
    void Update()
    {
        if (Time.time % spawnrate < 0.1)
        {
            EntityManager entityManager = World.Active.EntityManager;
        
            //spawn a new monster!
            var monsterEnt = GameObjectConversionUtility.ConvertGameObjectHierarchy(Chick, World.Active);
            var thing = entityManager.Instantiate((monsterEnt));
            entityManager.SetComponentData(thing, new Translation(){Value=sauna.transform.position});
            monsters.Add(entityManager.Instantiate(monsterEnt));
            
            entityManager.SetComponentData(monsterEnt, new InsideRoom(){RoomEntity = rooms[0]}); //room 0 is lobby!
        }
    }
}
