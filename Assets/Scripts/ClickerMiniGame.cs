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
    public Image hitOverlay;

    private GameObject currentBloodEffect;

    public LevelManager levelManager;

    private Renderer currentMouthRenderer;

    public Color hoverColor = new Color(1f, 0.3f, 0.3f);

    private Color originalColor;

    void Awake()
    {
        clickerUIRoot.SetActive(false);
        gameOverPanel.SetActive(false);
        cursorUI.SetActive(false);

        damageOverlay.gameObject.SetActive(false);
        hitOverlay.gameObject.SetActive(false);
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

        HandleMouthHover();

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            if (IsMouseOverMouth(out RaycastHit hit))
            {
                CheckHit(hit); // pass position
            }
        }
    }

    void HandleMouthHover()
    {
        Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            if (hit.collider.CompareTag("Mouth"))
            {
                Renderer rend = hit.collider.GetComponent<Renderer>();

                if (rend != null)
                {
                    // switched to a new mouth object
                    if (currentMouthRenderer != rend)
                    {
                        ResetMouthColor();

                        currentMouthRenderer = rend;
                        originalColor = rend.material.color;

                        rend.material.color = hoverColor;
                    }
                }

                return;
            }
        }

        // mouse left the mouth
        ResetMouthColor();
    }

    void ResetMouthColor()
    {
        if (currentMouthRenderer != null)
        {
            currentMouthRenderer.material.color = originalColor;
            currentMouthRenderer = null;
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
            AudioManager.Instance.PlayHitSound();

            Debug.Log("Hit!");

            hits++;

            StartCoroutine(FlashColor(hitColor));
            StartCoroutine(ShowHitOverlay());

            if (hits >= hitsToWin)
            {
                StartCoroutine(WinFadeOut());
                return;
            }

            ResetIndicator();
        }
        else
        {
            AudioManager.Instance.PlayMissSound();
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

    IEnumerator ShowHitOverlay()
    {
        hitOverlay.gameObject.SetActive(true);

        Color c = hitOverlay.color;

        float maxAlpha = 0.35f;

        // FADE IN
        float fadeInDuration = 0.05f;
        float t = 0f;

        while (t < fadeInDuration)
        {
            if (!isActive) yield break;

            t += Time.deltaTime;

            c.a = Mathf.Lerp(0f, maxAlpha, t / fadeInDuration);
            hitOverlay.color = c;

            yield return null;
        }

        // FADE OUT
        float fadeOutDuration = 0.25f;
        t = 0f;

        while (t < fadeOutDuration)
        {
            if (!isActive) yield break;

            t += Time.deltaTime;

            c.a = Mathf.Lerp(maxAlpha, 0f, t / fadeOutDuration);
            hitOverlay.color = c;

            yield return null;
        }

        c.a = 0f;
        hitOverlay.color = c;

        hitOverlay.gameObject.SetActive(false);
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
        yield return new WaitForSeconds(2f);
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
        speed = Random.Range(40f, 190f);
    }

    void GameOver()
    {
        AudioManager.Instance.PlayGameOverSound();
    
        EndMiniGame();
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