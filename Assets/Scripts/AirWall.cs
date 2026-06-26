using UnityEngine;
using TMPro;
using System.Collections;

public class AirWall : MonoBehaviour
{
    [Header("Boundary settings")]
    public Transform player;
    public Vector3 playerStartPos;
    public TextMeshProUGUI tipText;
    public float tipShowDuration = 2f;
    public float moveLockTime = 0.5f;
    public float triggerDistance = 1f;
    public string[] tipTexts = new string[] {
        "You can explore this area later!",
        "Nothing here yet!",
        "Don't go too far!"
    };

    private bool isPlayerMoveLocked;
    private float moveLockTimer;
    private bool isEggTriggered;

    void Start()
    {
        if (player != null && playerStartPos == Vector3.zero)
        {
            playerStartPos = player.position;
        }

        if (tipText != null)
        {
            tipText.gameObject.SetActive(false);
        }
        isEggTriggered = false;
    }

    void Update()
    {
        if (isPlayerMoveLocked)
        {
            moveLockTimer += Time.deltaTime;
            if (moveLockTimer >= moveLockTime)
            {
                isPlayerMoveLocked = false;
                moveLockTimer = 0;
                isEggTriggered = false;
            }
        }
    }

    void OnTriggerStay(Collider other)
    {
        if (GameStateManager.Instance != null && !GameStateManager.Instance.IsPlaying())
            return;

        if (other.CompareTag("Player") && !isPlayerMoveLocked && !isEggTriggered)
        {
            TriggerBoundaryEasterEgg();
        }
    }

    void TriggerBoundaryEasterEgg()
    {
        if (player == null) return;

        isPlayerMoveLocked = true;
        isEggTriggered = true;

        player.position = new Vector3(
            playerStartPos.x,
            player.position.y,
            playerStartPos.z
        );

        if (tipText != null)
        {
            StartCoroutine(ShowRandomTipText());
        }
    }

    IEnumerator ShowRandomTipText()
    {
        tipText.gameObject.SetActive(true);
        tipText.text = tipTexts[Random.Range(0, tipTexts.Length)];

        float t = 0;
        while (t < 0.5f)
        {
            t += Time.deltaTime;
            tipText.alpha = Mathf.Lerp(0, 1, t / 0.5f);
            yield return null;
        }

        yield return new WaitForSeconds(tipShowDuration);

        t = 0;
        while (t < 0.5f)
        {
            t += Time.deltaTime;
            tipText.alpha = Mathf.Lerp(1, 0, t / 0.5f);
            yield return null;
        }

        tipText.gameObject.SetActive(false);
    }

    public bool IsPlayerMoveLocked()
    {
        return isPlayerMoveLocked;
    }
}
