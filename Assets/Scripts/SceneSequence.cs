using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;
using UnityEngine.UI;


public class SceneSequence : MonoBehaviour
{
    public Transform lightTransform;

    public Transform startPoint;
    public Transform endPoint;

    //public float moveDuration = 4f;
    public float moveSpeed = 4f;

    public DialogueManager dialogueManager;

    public Transform cameraTransform;
    public Transform zoomPoint;
    public float cameraMoveDuration = 2f;
    public Animator playerAnimator;
   

    private SkinnedMeshRenderer activeRenderer;
    private Texture activeTexture;
    public int currentLevel;

    public ClickerMiniGame clickerMiniGame;

    public Image instructionPanel;

    public List<string> dialogueLines = new List<string>()

    {
        "You: Hello, how are you feeling?",
        "Patient: I’ve been having a tooth ache all week. Please help. ",
        "You: No worries pal. Open up as wide as you can."
    };
    void Start()
    {

        AudioManager.Instance.PlayAmbient();

        lightTransform.position=startPoint.position;

        dialogueManager.OnDialogueEnd += HandleDialogueEnd;

        instructionPanel.gameObject.SetActive(false);

    }

    public void StartSequence(List<string> newDialogue)
    {
        StopAllCoroutines(); // important to prevent overlap

        dialogueLines = newDialogue;

       

        lightTransform.position = startPoint.position;

        StartCoroutine(PlaySequence());
    }
    IEnumerator PlaySequence()
    {
        yield return new WaitForSeconds(2f);

        while (Vector3.Distance(lightTransform.position, endPoint.position) > 0.01f)
        {
            lightTransform.position = Vector3.MoveTowards(
                lightTransform.position,
                endPoint.position,
                moveSpeed * Time.deltaTime
            );

            yield return null;
        }
        lightTransform.position = endPoint.position;
        yield return new WaitForSeconds(1f);
        dialogueManager.StartDialogue(dialogueLines);
        Debug.Log(Vector3.Distance(startPoint.position, endPoint.position));
    }
   
    void HandleDialogueEnd()
    {
        StartCoroutine(CameraMoveSequence());

        if (playerAnimator != null)
        {
            playerAnimator.enabled= false;
        }
        
    }

    IEnumerator CameraMoveSequence()
    {
        

        AudioManager.Instance.PlayZoomInSound();
        Vector3 initialPosition = cameraTransform.position;
        Quaternion initialRotation = cameraTransform.rotation;
        float time = 0f;
        while (time < cameraMoveDuration)
        {
            cameraTransform.position = Vector3.Lerp(initialPosition, zoomPoint.position, time / cameraMoveDuration);
            cameraTransform.rotation = Quaternion.Slerp(initialRotation, zoomPoint.rotation, time / cameraMoveDuration);
            time += Time.deltaTime;
            yield return null;
        }
        cameraTransform.position = zoomPoint.position;
        cameraTransform.rotation = zoomPoint.rotation;

        activeRenderer.material.mainTexture = activeTexture;
        yield return new WaitForSeconds(0.5f);
        instructionPanel.gameObject.SetActive(true);

        yield return new WaitForSeconds(4f);
        instructionPanel.gameObject.SetActive(false);
        yield return new WaitForSeconds(0.5f);
        clickerMiniGame.StartMiniGame();


    }

    public void SetCurrentRenderer(SkinnedMeshRenderer r, Texture t)
    {
        activeRenderer = r;
        activeTexture = t;

       
    }


}
