using NUnit.Framework;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class LevelManager : MonoBehaviour
{
    public List<GameObject> patientPrefabs;
    public Transform spawnPoint;

    private int currentLevel = 0;
    private GameObject currentPatient;

    public GameObject levelPanel;
    public Image panelImage;
    public TextMeshProUGUI levelText1;
    public TextMeshProUGUI levelText2;
    public float fadeDuration = 1f;


    public Transform cameraStartPoint;
    public Transform cameraTransform;
    public float cameraMoveDuration = 2f;

    public SceneSequence sceneSequence;

    public List<List<string>> allDialogues = new List<List<string>>();

    private bool isTransitioning = false;

    public List<Texture> levelTextures;

    



    void Start()
    {
       
        levelPanel.SetActive(false);
        allDialogues = new List<List<string>>()
    {
        new List<string>()
        {
            "You: Hello, how are you feeling?",
            "Patient: My tooth hurts a lot.",
            "You: Let me take a look."
        },

        new List<string>()
        {
            "You: Next patient please.",
            "Patient: It's getting worse...",
            "You: We’ll sort it out."
        },

        new List<string>()
        {
            "You: Final patient.",
            "Patient: Much better now!",
            "You: Great to hear."
        }
    };

        SpawnPatient(); // spawn first patient
        
        sceneSequence.currentLevel = currentLevel;
        sceneSequence.StartSequence(allDialogues[currentLevel]);
    }

    // Update is called once per frame
    void Update()
    {
        if (!isTransitioning && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            StartCoroutine(NextLevelSequence());
        }
    }

    void SpawnPatient()
    {
        Destroy(currentPatient);
        currentPatient = Instantiate(patientPrefabs[currentLevel], spawnPoint.position, spawnPoint.rotation);


        SkinnedMeshRenderer smr = null;

        foreach (var r in currentPatient.GetComponentsInChildren<SkinnedMeshRenderer>())
        {
            if (r.CompareTag("Mouth"))
            {
                smr = r;
                break;
            }
        }
        if (smr == null)
        {
            Debug.LogError("No SkinnedMeshRenderer with tag 'Mouth' found!");
            return;
        }
        sceneSequence.SetCurrentRenderer(smr, levelTextures[currentLevel]);

    }

    IEnumerator NextLevelSequence()
    {
        if (currentLevel >= patientPrefabs.Count - 1)
        {
            Debug.Log("All levels completed!");
            yield break;
        }
        currentLevel++;

        levelPanel.SetActive(true);
        levelText1.text = "The appointment went very well.";
        levelText2.text = "Time for your next patient.";

        SetAlpha(0f, 0f, 0f);
      
       

        yield return new WaitForSeconds(2f);

        float t = 0f;
        while (t < fadeDuration)
        {
            float a = Mathf.Lerp(0f, 1f, t / fadeDuration);
            SetAlpha(1f, a, 0f);

            t += Time.deltaTime;
            yield return null;
        }

        SetAlpha(1f, 1f, 0f);
        yield return new WaitForSeconds(2f);
        t = 0f;
        while (t < fadeDuration)
        {
            float a = Mathf.Lerp(0f, 1f, t / fadeDuration);
            SetAlpha(1f, 1f, a);

            t += Time.deltaTime;
            yield return null;
        }

        SetAlpha(1f, 1f, 1f);



        SpawnPatient();
        yield return StartCoroutine(ResetCamera());

        levelPanel.SetActive(false);

        sceneSequence.currentLevel = currentLevel;
        sceneSequence.StartSequence(allDialogues[currentLevel]);

        isTransitioning = false;
    }

    void SetAlpha(float alpha, float alphaText1, float alphaText2)
    {
        Color imgColor = panelImage.color;
        imgColor.a = alpha;
        panelImage.color = imgColor;

        Color t1 = levelText1.color;
        t1.a = alphaText1;
        levelText1.color = t1;

        Color t2 = levelText2.color;
        t2.a = alphaText2;
        levelText2.color = t2;
    }

    IEnumerator ResetCamera()
    {
        float time = 0f;

        Vector3 startPos = cameraTransform.position;
        Quaternion startRot = cameraTransform.rotation;

        while (time < cameraMoveDuration)
        {
            cameraTransform.position = Vector3.Lerp(startPos, cameraStartPoint.position, time / cameraMoveDuration);
            cameraTransform.rotation = Quaternion.Slerp(startRot, cameraStartPoint.rotation, time / cameraMoveDuration);

            time += Time.deltaTime;
            yield return null;
        }

        cameraTransform.position = cameraStartPoint.position;
        cameraTransform.rotation = cameraStartPoint.rotation;
    }
}
