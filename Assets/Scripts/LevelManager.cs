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
    public float fadeDuration = 3f;


    public Transform cameraStartPoint;
    public Transform cameraTransform;
    public float cameraMoveDuration = 2f;

    public SceneSequence sceneSequence;

    public List<List<string>> allDialogues = new List<List<string>>();

    private bool isTransitioning = false;

    public List<Texture> levelTextures;

    public Image endFadeImage;
    public float endFadeDuration = 5f;
    public string mainMenuSceneName = "MainMenu";





    void Start()
    {
        AudioManager.Instance.PlayAmbient();
        levelPanel.SetActive(false);
        allDialogues = new List<List<string>>()
    {
        new List<string>()
        {
            "You: Good morning, how are you feeling today?",
            "Patient: I've had the worst tooth ache all week.. Help me !!! Please please please",
            "You: No worries, I'll take care of it. Open up wide."
        },

        new List<string>()
        {
            "You: Hello, how are you feeling today?",
            "Patient: I'm scared!!!! Oh my god I haven't been to the dentist in 10 years. What if I need to get a root canal?!! I can't afford that HELP ME",
            "You: Don't worry, this is going to go super well - I've been practicing for years. You won't feel a thing."
        },

        new List<string>()
        {
            "You: Hello, how are you?",
            "Patient: I'm great! I love coming to the dentist. Can we start?",
            "You: Yes ...... Open up."
        }
    };

        SpawnPatient(); // spawn first patient
        
        sceneSequence.currentLevel = currentLevel;
        sceneSequence.StartSequence(allDialogues[currentLevel]);
    }

    // Update is called once per frame

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

    public void StartNextLevel()
    {
        if (!isTransitioning)
        {
            isTransitioning = true;
            StartCoroutine(NextLevelSequence());
        }
    }

    IEnumerator EndGameSequence()
    {
        // fade to black
        yield return new WaitForSeconds(2f); 
        endFadeImage.gameObject.SetActive(true);

        Color c = endFadeImage.color;
        c.a = 0f;
        endFadeImage.color = c;

        float t = 0f;

        while (t < endFadeDuration)
        {
            t += Time.deltaTime;

            c.a = Mathf.Lerp(0f, 1f, t / endFadeDuration);
            endFadeImage.color = c;

            yield return null;
        }

        c.a = 1f;
        endFadeImage.color = c;

        // hold on black screen
        yield return new WaitForSeconds(5f);

        // load main menu
        UnityEngine.SceneManagement.SceneManager.LoadScene(mainMenuSceneName);
    }
    public IEnumerator NextLevelSequence()
    {
        if (currentLevel >= patientPrefabs.Count - 1)
        {
            yield return StartCoroutine(EndGameSequence());
            yield break;
        }
        currentLevel++;
        

        levelPanel.SetActive(true);
        levelText1.text = "The appointment went very well.";
        levelText2.text = "Time for your next patient.";

        SetAlpha(0f, 0f, 0f);

        yield return new WaitForSeconds(0.5f);

        AudioManager.Instance.PlayDoorSound();

        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;

            float a = Mathf.Lerp(0f, 1f, t / fadeDuration);

            SetAlpha(a, a, 0f);

            yield return null;
        }

        SetAlpha(1f, 1f, 0f);

        yield return new WaitForSeconds(2f);

        // -------------------------
        // FADE TEXT2 IN
        // -------------------------
        t = 0f;

        while (t < fadeDuration)
        {
            t += Time.deltaTime;

            float a = Mathf.Lerp(0f, 1f, t / fadeDuration);

            SetAlpha(1f, 1f, a);

            yield return null;
        }

        SetAlpha(1f, 1f, 1f);

        yield return new WaitForSeconds(4f);
        SpawnPatient();

        // -------------------------
        // FADE EVERYTHING OUT
        // -------------------------
        t = 0f;

        while (t < fadeDuration)
        {
            t += Time.deltaTime;

            float a = Mathf.Lerp(1f, 0f, t / fadeDuration);

            SetAlpha(a, a, a);

            yield return null;
        }
        

        levelPanel.SetActive(false);
        SetAlpha(1f, 1f, 1f);


       
        yield return StartCoroutine(ResetCamera());

        

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
