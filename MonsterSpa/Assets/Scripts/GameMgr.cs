using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using System;
using UnityEngine.UIElements;

public class GameMgr : MonoBehaviour
{
    public static GameMgr g;
    public List<Entity> rooms = new List<Entity>();
    public GameObject lobby;
    public GameObject sauna;
    public GameObject hotTub;
    public GameObject coldBath;
    public GameObject cafe;
    
    
    // Start is called before the first frame update
    void Start()
    {
        if(g != null)
            throw new Exception("Multiple GameMgr gameobject components in scene, please only have one constant gameobject in the scene.");

        g = this;
        
        EntityManager entityManager = World.Active.EntityManager;
        
        var roomEnt = GameObjectConversionUtility.ConvertGameObjectHierarchy(lobby, World.Active);
        rooms.Add(entityManager.Instantiate(roomEnt));
        roomEnt =  GameObjectConversionUtility.ConvertGameObjectHierarchy(sauna, World.Active);
        rooms.Add(entityManager.Instantiate(roomEnt));
        roomEnt =  GameObjectConversionUtility.ConvertGameObjectHierarchy(hotTub, World.Active);
        rooms.Add(entityManager.Instantiate(roomEnt));
        roomEnt =  GameObjectConversionUtility.ConvertGameObjectHierarchy(coldBath, World.Active);
        rooms.Add(entityManager.Instantiate(roomEnt));
        roomEnt =  GameObjectConversionUtility.ConvertGameObjectHierarchy(cafe, World.Active);
        rooms.Add(entityManager.Instantiate(roomEnt));

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
