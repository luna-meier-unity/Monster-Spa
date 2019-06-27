using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Collections;
using UnityEngine;
using UnityEngine.UI;

public struct Dialogue
{
    public GameObject MainElement;
    public List<GameObject>[] LineGroups;
}

public struct Transition
{
    public float originalAlpha;
    public Image image;
    public TextMeshProUGUI text;
    public bool isImage;

    public Transition(GameObject go)
    {
        isImage = true;
        image = go.GetComponent<Image>();
        text = null;
        if (image == null)
        {
            isImage = false;
            text = go.GetComponent<TextMeshProUGUI>();
            if (text == null)
            {
                originalAlpha = Single.NaN;
                return;
            }
        } 
        
        originalAlpha = isImage ? image.color.a : text.color.a;
        
    }

    public void SetAlpha(float percent)
    {
        var alpha = percent * originalAlpha;
        
        if (isImage)
        {
            var original = image.color;
            image.color = new Color(original.r, original.g, original.b, alpha);
        }
        else
        {
            var original = text.color;
            text.color = new Color(original.r, original.g, original.b, alpha);
        }
    }
}

public class DialogueManager : MonoBehaviour
{
    private Hashtable Sections;

    public float TimeToFade;

    public float PercentThroughTransition;

    public List<Transition> Transitions;

    public static DialogueManager g;
    
    // Start is called before the first frame update
    void Start()
    {
        if(g != null)
            throw new Exception("Multiple dialogue managers in scene, please fix so theres only one");

        g = this;
        
        Transitions = new List<Transition>();

        Sections = new Hashtable();
        
        var children = this.GetComponentsInChildren<Transform>();
        for(var go = 0; go < this.transform.childCount; ++go)
        {
            var dialogue = new Dialogue()
            {
                MainElement = this.transform.GetChild(go).gameObject
            };
            
            dialogue.MainElement.SetActive(false);

            var lines = new List<GameObject>();
            for (var i = 0; i < dialogue.MainElement.transform.childCount; ++i)
            {
                lines.Add(dialogue.MainElement.transform.GetChild(i).gameObject);
                lines[i].gameObject.SetActive(false);
            }


            var numberOfLines = 0;
            foreach (var line in lines)
            {
                numberOfLines = Math.Max(numberOfLines, 1 + int.Parse(line.name.Substring(0, 1)));
            }

            var LineGroups = new List<GameObject>[numberOfLines];
            for (var i = 0; i < numberOfLines; ++i)
            {
                LineGroups[i] = new List<GameObject>();
            }

            foreach (var line in lines)
            {
                LineGroups[int.Parse(line.name.Substring(0, 1))].Add(line.gameObject);
            }

            dialogue.LineGroups = LineGroups;
            
            Sections.Add(int.Parse(dialogue.MainElement.name.Substring(0,1)),dialogue);

            currentSection = -1;
        }
        
    }

    public int currentSection = -1;
    public bool playSuccessSound = false;
    public int currentLine = 0;
    private enum DialogueState
    {
        Hidden,
        Advancing
    }

    private DialogueState state;

    private void SetAlpha(float percent, float goal, GameObject go)
    {
        var alpha = percent * goal;
        
        var img = go.GetComponent<Image>();

        if (img != null)
        {
            var original = img.color;
            img.color = new Color(original.r, original.g, original.b, alpha);
        }
        else
        {
            var text = go.GetComponent<TextMeshProUGUI>();

            var original = text.color;
            text.color = new Color(original.r, original.g, original.b, alpha);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (state == DialogueState.Hidden && currentSection != -1)
        {
            VictoryConditionManager.g.PauseChecking = true;
            
            state = DialogueState.Advancing;
            Transitions.Clear();
            
            PercentThroughTransition = 0.0f;
            
            ((Dialogue)Sections[currentSection]).MainElement.SetActive(true);
            {
                var trans = new Transition(((Dialogue)Sections[currentSection]).MainElement);
                
                trans.SetAlpha(0);
                    
                Transitions.Add(trans);
            }
            
            currentLine = 0;
            foreach (var go in ((Dialogue)Sections[currentSection]).LineGroups[currentLine])
            {
                go.SetActive(true);
                
                var trans = new Transition(go);
                
                if (!Single.IsNaN(trans.originalAlpha))
                {
                    trans.SetAlpha(0);
                    Transitions.Add(trans);
                }
            }

            currentLine++;
        }

        if (state == DialogueState.Advancing && (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space)))
        {
            if (currentLine == ((Dialogue)Sections[currentSection]).LineGroups.Length)
            {
                if (PercentThroughTransition < TimeToFade)
                {
                    PercentThroughTransition = TimeToFade;
                    foreach (var trans in Transitions)
                    {
                        trans.SetAlpha(1);
                    }
                }
                else
                {
                    state = DialogueState.Hidden;
                    currentLine = 0;
                    ((Dialogue)Sections[currentSection]).MainElement.SetActive(false);
                    currentSection = -1;
                    VictoryConditionManager.g.PauseChecking = false;

                    if (playSuccessSound)
                    {
                        SoundEffectManager.g.PlaySoundEffect(SoundEffectType.Success);
                    }
                }
            }
            else
            {
                if (PercentThroughTransition < TimeToFade)
                {
                    PercentThroughTransition = TimeToFade;
                    foreach (var trans in Transitions)
                    {
                        trans.SetAlpha(1);
                    }
                }
                else
                {

                    Transitions.Clear();

                    foreach (var go in ((Dialogue)Sections[currentSection]).LineGroups[currentLine])
                    {
                        go.SetActive(true);

                        var trans = new Transition(go);

                        if (!Single.IsNaN(trans.originalAlpha))
                        {
                            trans.SetAlpha(0);
                            Transitions.Add(trans);
                        }
                    }

                    PercentThroughTransition = 0;

                    currentLine++;
                }
            }
        }

        if (PercentThroughTransition < TimeToFade)
        {
            PercentThroughTransition += Time.deltaTime;

            if (PercentThroughTransition > TimeToFade)
                PercentThroughTransition = TimeToFade;

            foreach (var trans in Transitions)
            {
                trans.SetAlpha(PercentThroughTransition / TimeToFade);
            }
        }
    }
}
