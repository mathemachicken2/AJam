using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;

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
    public Texture2D newCursor;

    public Texture newTexture;
    public SkinnedMeshRenderer targetRenderer;

    public List<string> dialogueLines = new List<string>()
    {
        "You: Hello, how are you feeling?",
        "Patient: I’ve been having a tooth ache all week. Please help. ",
        "You: No worries pal. Open up as wide as you can."
    };
    void Start()
    {
        lightTransform.position=startPoint.position;

        dialogueManager.OnDialogueEnd += HandleDialogueEnd;

        StartCoroutine(PlaySequence());


    }

    IEnumerator PlaySequence()
    {
        yield return new WaitForSeconds(4f);

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

        if (newCursor != null)
        {
            Cursor.SetCursor(newCursor, Vector2.zero, CursorMode.Auto);
        }
    }

    IEnumerator CameraMoveSequence()
    {
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

        targetRenderer.material.mainTexture = newTexture;
    }

}
