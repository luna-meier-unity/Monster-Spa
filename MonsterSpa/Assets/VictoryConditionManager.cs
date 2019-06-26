using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Sauna cold bath hot springs

public struct RoomState
{
    public int chicks;
    public int hunduns;
    public int glooms;
    public int sandles;

    public bool Equals(ref RoomState other)
    {
        return chicks == other.chicks && hunduns == other.hunduns && glooms == other.glooms && sandles == other.sandles;
    }

    public RoomState(int Chicks, int Hunduns, int Glooms, int Sandles)
    {
        chicks = Chicks;
        hunduns = Hunduns;
        glooms = Glooms;
        sandles = Sandles;
    }
}

public class VictoryConditionManager : MonoBehaviour
{
    public struct VictoryCondition
    {
        public int DialogueIndex;
        public RoomState ColdBathState;
        public RoomState HotSpringsState;
        public RoomState SaunaState;

        public bool IsVictoryMet(ref RoomState otherColdBathState, ref RoomState otherHotSpringsState,
            ref RoomState otherSaunaState)
        {
            return otherColdBathState.Equals(ref ColdBathState) && otherHotSpringsState.Equals(ref HotSpringsState) &&
                   otherSaunaState.Equals(ref SaunaState);
        }

        public VictoryCondition(int dialogueIndex, RoomState coldBathState, RoomState hotSpringsState,
            RoomState saunaState)
        {
            DialogueIndex = dialogueIndex;
            ColdBathState = coldBathState;
            HotSpringsState = hotSpringsState;
            SaunaState = saunaState;
        }
    }
    
    public static VictoryConditionManager g;

    public List<VictoryCondition> UnmetConditions;

    public RoomState coldBathState;
    public RoomState hotSpringsState;
    public RoomState saunaState;

    public bool PauseChecking;

    private int CurrentCheckIndex;
    
    // Start is called before the first frame update
    void Start()
    {
        if(g != null)
            throw new Exception("More than one victory condition manager in the scene.  No duplicates.");
        
        g = this;
        
        UnmetConditions = new List<VictoryCondition>();
        
        UnmetConditions.Add(new VictoryCondition(0,
            new RoomState(0,0,0,0),
            new RoomState(1,0,0,0),
            new RoomState(0,0,0,0)));
        

        CurrentCheckIndex = 0;

        PauseChecking = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (UnmetConditions.Count <= 0 || PauseChecking)
            return;
        
        if (UnmetConditions[CurrentCheckIndex].IsVictoryMet(ref coldBathState, ref hotSpringsState, ref saunaState))
        {
            //TODO WIN

            switch (UnmetConditions[CurrentCheckIndex].DialogueIndex)
            {
                case 0:
                    break;
                
                default:
                    break;
            }

            DialogueManager.g.currentSection = UnmetConditions[CurrentCheckIndex].DialogueIndex;
            
            UnmetConditions.RemoveAt(CurrentCheckIndex);
            return;
        }

        CurrentCheckIndex++;
        CurrentCheckIndex %= UnmetConditions.Count;

    }
}
