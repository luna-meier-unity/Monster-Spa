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
    
    public bool GreaterThan(ref RoomState other)
    {
        return chicks >= other.chicks && hunduns >= other.hunduns && glooms >= other.glooms && sandles >= other.sandles;
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
            return otherColdBathState.GreaterThan(ref ColdBathState) && otherHotSpringsState.GreaterThan(ref HotSpringsState) &&
                   otherSaunaState.GreaterThan(ref SaunaState);
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
            new RoomState(0,0,0,0),
            new RoomState(2,0,0,0)));
        
        UnmetConditions.Add(new VictoryCondition(1,
            new RoomState(0,0,0,0),
            new RoomState(0,0,0,0),
            new RoomState(4,0,0,0)));
        
        
        //10////////////////////////////////////////////////////////////////////
        UnmetConditions.Add(new VictoryCondition(10,
            new RoomState(2,0,0,0),
            new RoomState(0,0,0,0),
            new RoomState(0,0,0,0)));
        
        UnmetConditions.Add(new VictoryCondition(11,
            new RoomState(0,0,0,0),
            new RoomState(4,0,0,0),
            new RoomState(0,0,0,0)));
        
        UnmetConditions.Add(new VictoryCondition(12,
            new RoomState(0,0,0,0),
            new RoomState(2,0,0,0),
            new RoomState(0,0,0,0)));
            
            UnmetConditions.Add(new VictoryCondition(13,
            new RoomState(0,0,0,0),
            new RoomState(0,0,0,0),
            new RoomState(3,0,1,0)));
        
        UnmetConditions.Add(new VictoryCondition(14,
            new RoomState(0,0,0,0),
            new RoomState(0,0,0,0),
            new RoomState(1,0,0,1)));
        
        //15////////////////////////////////////////////////////////////////////
        UnmetConditions.Add(new VictoryCondition(15,
            new RoomState(0,0,0,0),
            new RoomState(0,0,0,0),
            new RoomState(1,0,0,2)));
        
        UnmetConditions.Add(new VictoryCondition(16,
            new RoomState(0,0,1,1),
            new RoomState(0,0,0,0),
            new RoomState(0,0,0,0)));
        
        UnmetConditions.Add(new VictoryCondition(17,
            new RoomState(0,0,0,0),
            new RoomState(0,0,0,0),
            new RoomState(0,1,1,0)));
        
        UnmetConditions.Add(new VictoryCondition(18,
            new RoomState(0,0,0,0),
            new RoomState(0,1,2,0),
            new RoomState(0,0,0,0)));
        
        UnmetConditions.Add(new VictoryCondition(19,
            new RoomState(0,0,0,0),
            new RoomState(0,0,1,1),
            new RoomState(0,0,0,0)));

        CurrentCheckIndex = 0;

        PauseChecking = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (saunaState.chicks + coldBathState.chicks + hotSpringsState.chicks > 0)
        {
            SoundEffectManager.g.StartAmbientNoise(AmbientNoiseType.Chirping);
        }
        else
        {
            SoundEffectManager.g.StopAmbientNoise(AmbientNoiseType.Chirping);
        }
        
        if (saunaState.glooms + coldBathState.glooms + hotSpringsState.glooms > 0)
        {
            SoundEffectManager.g.StartAmbientNoise(AmbientNoiseType.Rain);
        }
        else
        {
            SoundEffectManager.g.StopAmbientNoise(AmbientNoiseType.Rain);
        }
        
        if (saunaState.chicks + saunaState.hunduns + saunaState.glooms + saunaState.sandles > 0)
        {
            SoundEffectManager.g.StartAmbientNoise(AmbientNoiseType.LowSimmer);
        }
        else
        {
            SoundEffectManager.g.StopAmbientNoise(AmbientNoiseType.LowSimmer);
        }
        
        
        if (UnmetConditions.Count <= 0 || PauseChecking)
            return;
        
        if (UnmetConditions[CurrentCheckIndex].IsVictoryMet(ref coldBathState, ref hotSpringsState, ref saunaState))
        {
            //TODO WIN

            bool playSuccessSound = false;

            switch (UnmetConditions[CurrentCheckIndex].DialogueIndex)
            {
                case 1://Unlock Cold Bath
                    UnmetConditions.Add(new VictoryCondition(2,
                        new RoomState(3,0,0,0),
                        new RoomState(0,0,0,0),
                        new RoomState(0,0,0,0)));
        
                    UnmetConditions.Add(new VictoryCondition(3,
                        new RoomState(5,0,0,0),
                        new RoomState(0,0,0,0),
                        new RoomState(0,0,0,0)));

                    playSuccessSound = true;
                    GameMgr.g.SpawnRoom(GameMgr.g.coldBath);
                    break;
                case 3: //Unlock Hundun
                    
                    GameMgr.g.spawnables = new List<GameObject>() { GameMgr.g.Chick, GameMgr.g.Hundun };
                    
                    UnmetConditions.Add(new VictoryCondition(4,
                        new RoomState(0,2,0,0),
                        new RoomState(0,0,0,0),
                        new RoomState(0,0,0,0)));
        
        
                    UnmetConditions.Add(new VictoryCondition(5,
                        new RoomState(1,3,0,0),
                        new RoomState(0,0,0,0),
                        new RoomState(0,0,0,0)));
                    
                    playSuccessSound = true;
                    break;
                
                case 5: //Unlock Hot spring

                    UnmetConditions.Add(new VictoryCondition(6,
                        new RoomState(0,0,0,0),
                        new RoomState(1,3,0,0),
                        new RoomState(0,0,0,0)));
                    
                    playSuccessSound = true;
                    
                    GameMgr.g.SpawnRoom(GameMgr.g.hotTub);
                    
                    break;
                
                case 6: //Unlock Gloom
                    
                    GameMgr.g.spawnables = new List<GameObject>() { GameMgr.g.Chick, GameMgr.g.Hundun, GameMgr.g.Ghost };
                    
                    UnmetConditions.Add(new VictoryCondition(7,
                        new RoomState(0,0,0,0),
                        new RoomState(2,0,1,0),
                        new RoomState(0,0,0,0)));
        
                    UnmetConditions.Add(new VictoryCondition(8,
                        new RoomState(0,0,0,0),
                        new RoomState(2,1,2,0),
                        new RoomState(0,0,0,0)));
                    
                    playSuccessSound = true;
                    
                    break;
                
                case 8: //Unlock Sandal
                    
                    GameMgr.g.spawnables = new List<GameObject>() { GameMgr.g.Chick, GameMgr.g.Hundun, GameMgr.g.Ghost, GameMgr.g.Sandal };
                    
                    UnmetConditions.Add(new VictoryCondition(9,
                        new RoomState(0,0,0,0),
                        new RoomState(0,0,0,0),
                        new RoomState(1,1,1,1)));

                    
                    playSuccessSound = true;
                    break;
                
                case 9: //Victory!
                    
                    
                    playSuccessSound = true;
                    break;
                    
                default:
                    break;
            }

            DialogueManager.g.currentSection = UnmetConditions[CurrentCheckIndex].DialogueIndex;
            DialogueManager.g.playSuccessSound = playSuccessSound;
            
            UnmetConditions.RemoveAt(CurrentCheckIndex);
            return;
        }

        CurrentCheckIndex++;
        CurrentCheckIndex %= UnmetConditions.Count;

    }
}
