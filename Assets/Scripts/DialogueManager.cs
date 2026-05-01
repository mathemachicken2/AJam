using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using UnityEngine.InputSystem;
using System;
public class DialogueManager : MonoBehaviour
{
    public GameObject dialoguePanel;
    public TextMeshProUGUI dialogueText;

    private Queue<string> lines = new Queue<string>();
    private bool isActive = false;
    public event Action OnDialogueEnd;

    List<string> Patient1 = new List<string>()
    {
        "You: Hello, how are you feeling?",
        "Patient: I’ve been having a tooth ache all week. Please help. ",
        "You: No worries pal. Open up as wide as you can."
    };

    void Start()
    {
        dialoguePanel.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (isActive && Mouse.current.leftButton.wasPressedThisFrame)
        {
            DisplayNextLine();
        }
    }

    public void StartDialogue(List<string> dialogueLines) 
    {
        dialoguePanel.SetActive(true);
        lines.Clear();

        foreach (string line in dialogueLines)
        {
            lines.Enqueue(line);
        }
        isActive = true;
        DisplayNextLine();  
    } 

    void DisplayNextLine()
    {
        if ( lines.Count == 0)
        {
            EndDialogue();
            return;
        }

        dialogueText.text = lines.Dequeue();
    }

    void EndDialogue()
    {
        dialoguePanel.SetActive(false);
        isActive= false;
        OnDialogueEnd?.Invoke();
    }
}
