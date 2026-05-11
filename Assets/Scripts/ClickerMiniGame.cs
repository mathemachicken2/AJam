using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Collections;
using System.Threading;


public class ClickerMiniGame : MonoBehaviour
{
    public RectTransform bar;
    public RectTransform indicator;
    public RectTransform targetZone;

    public float speed = 400f;
    public int maxMisses = 4;

    private int misses = 0;
    private bool movingRight = true;
    private bool isActive = false;

    public GameObject gameOverPanel;
    public GameObject clickerUIRoot;
    

    public Camera mainCamera;

    [Range(0.2f, 1f)]
    public float travelRangePercent = 1f;

    public Image indicatorImage;
    public int hitsToWin = 8;
    public int hits;

    public Color normalColor = Color.white;
    public Color hitColor = Color.green;
    public Color missColor = Color.red;

    public GameObject cursorUI;

    public GameObject bloodParticlePrefab;

    public Image damageOverlay;

    private GameObject currentBloodEffect;

    public LevelManager levelManager;

    void Awake()
    {
        clickerUIRoot.SetActive(false);
        gameOverPanel.SetActive(false);
        cursorUI.SetActive(false);

        damageOverlay.gameObject.SetActive(false);
    }

    public void StartMiniGame()
    {
        Cursor.visible = false;
        cursorUI.SetActive(true);

        clickerUIRoot.SetActive(true);
        isActive = true;
        misses = 0;

        misses = 0;
        hits = 0;

        indicatorImage.color = normalColor;

       

        ResetIndicator();
    }

    void EndMiniGame()
    {
        cursorUI.SetActive(false);
        Cursor.visible = true;

    }

    void Update()
    {
        if (!isActive) return;

        MoveIndicator();

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            if (IsMouseOverMouth(out RaycastHit hit))
            {
                CheckHit(hit); // pass position
            }
        }
    }

    bool IsMouseOverMouth(out RaycastHit hitInfo)
    {
        Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (Physics.Raycast(ray, out hitInfo))
        {
            if (hitInfo.collider.CompareTag("Mouth"))
            {
                return true;
            }
        }

        return false;
    }
    void MoveIndicator()
    {
        float halfWidth = bar.rect.width / 2f;
        float range = halfWidth * travelRangePercent;

        float dir = movingRight ? 1 : -1;

        indicator.anchoredPosition += new Vector2(dir * speed * Time.deltaTime, 0);

        if (indicator.anchoredPosition.x > range)
        {
            indicator.anchoredPosition = new Vector2(range, indicator.anchoredPosition.y);
            movingRight = false;
            RandomizeSpeed();
        }
        else if (indicator.anchoredPosition.x < -range)
        {
            indicator.anchoredPosition = new Vector2(-range, indicator.anchoredPosition.y);
            movingRight = true;
            RandomizeSpeed();
        }
    }

    void CheckHit(RaycastHit hit)
    {
        float indicatorX = indicator.anchoredPosition.x;
        float targetX = targetZone.anchoredPosition.x;
        float tolerance = targetZone.rect.width / 2f;

        if (Mathf.Abs(indicatorX - targetX) <= tolerance)
        {
            Debug.Log("Hit!");

            hits++;

            StartCoroutine(FlashColor(hitColor));

            if (hits >= hitsToWin)
            {
                StartCoroutine(WinFadeOut());
                return;
            }

            ResetIndicator();
        }
        else
        {
            misses++;
            Debug.Log("Miss " + misses);

            StartCoroutine(ShowDamageOverlay());
            StartCoroutine(FlashColor(missColor));

            bool isFinalMiss = misses >= maxMisses;
            SpawnBloodEffect(hit, !isFinalMiss);

            if (isFinalMiss)
            {
                GameOver();
            }
        }
    }

    IEnumerator ShowDamageOverlay()
    {
        damageOverlay.gameObject.SetActive(true);

        Color c = damageOverlay.color;

        float maxAlpha = 0.6f;

        // FADE IN
        float fadeInDuration = 0.1f;
        float t = 0f;

        while (t < fadeInDuration)
        {
            if (!isActive) yield break;

            t += Time.deltaTime;

            c.a = Mathf.Lerp(0f, maxAlpha, t / fadeInDuration);
            damageOverlay.color = c;

            yield return null;
        }

        // FADE OUT
        float fadeOutDuration = 0.9f;
        t = 0f;

        while (t < fadeOutDuration)
        {
            if (!isActive) yield break;

            t += Time.deltaTime;

            c.a = Mathf.Lerp(maxAlpha, 0f, t / fadeOutDuration);
            damageOverlay.color = c;

            yield return null;
        }

        c.a = 0f;
        damageOverlay.color = c;

        damageOverlay.gameObject.SetActive(false);
    }

    void SpawnBloodEffect(RaycastHit hit, bool autoDestroy)
    {
        Quaternion rotation = Quaternion.LookRotation(hit.normal);

        currentBloodEffect = Instantiate(
            bloodParticlePrefab,
            hit.point,
            rotation
        );

        if (autoDestroy)
        {
            Destroy(currentBloodEffect, 1f);
        }
    }

    IEnumerator WinFadeOut()
    {
        yield return new WaitForSeconds(1f);
        Win();
    }
    void Win()
    {
        Debug.Log("LEVEL COMPLETE");
        bar.gameObject.SetActive(false);
        EndMiniGame();
        isActive = false;
        clickerUIRoot.SetActive(false);

        levelManager.StartNextLevel(); 
    }
    IEnumerator FlashColor(Color flashColor)
    {
        indicatorImage.color = flashColor;

        yield return new WaitForSeconds(0.2f);

        indicatorImage.color = normalColor;
    }

    void ResetIndicator()
    {
        float halfWidth = bar.rect.width / 2f;

        // always start on LEFT edge (slightly inside so it's visible)
        float startX = -halfWidth;

        indicator.anchoredPosition = new Vector2(startX, indicator.anchoredPosition.y);

        movingRight = true; // always move toward center first

        RandomizeSpeed();
    }

    void RandomizeSpeed()
    {
        speed = Random.Range(20f, 100f);
    }

    void GameOver()
    {
        isActive = false;
        gameOverPanel.SetActive(true);
        cursorUI.SetActive(false);
        Cursor.visible = true;
        damageOverlay.gameObject.SetActive(true);
        Color c = damageOverlay.color;
        c.a = 0.6f;
        damageOverlay.color = c;

        Debug.Log("GAME OVER");
    }
}