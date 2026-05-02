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
    public TextMeshProUGUI levelText;
    public float fadeDuration = 1f;

    public GameObject startingPatient;

    public Transform cameraStartPoint;

    private bool isTransitioning = false;


    void Start()
    {
        currentPatient = startingPatient;
        levelPanel.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            StartCoroutine(NextLevelSequence());
        }
    }

    void SpawnPatient()
    {
        Destroy(currentPatient);
        currentPatient = Instantiate(patientPrefabs[currentLevel], spawnPoint.position, spawnPoint.rotation);
        
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
        levelText.text = $"The appointment went very well. Time for your patient.";

        SetAlpha(1f);
        SpawnPatient();

        yield return new WaitForSeconds(2f);

        float t = 0f;
        while (t < fadeDuration)
        {
            float alpha = Mathf.Lerp(1f, 0f, t / fadeDuration);
            SetAlpha(alpha);

            t += Time.deltaTime;
            yield return null;
        }

        SetAlpha(0f);
        levelPanel.SetActive(false);

        

        isTransitioning = false;
    }

    void SetAlpha(float alpha)
    {
        Color imgColor = panelImage.color;
        imgColor.a = alpha;
        panelImage.color = imgColor;

        Color textColor = levelText.color;
        textColor.a = alpha;
        levelText.color = textColor;
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
