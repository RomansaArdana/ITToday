using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class MemoryPlatform : MonoBehaviour
{
    [SerializeField] private float lightRange = 6f;
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private float solidDuration = 4.0f;
    [SerializeField] private float warningDuration = 1.5f;
    [SerializeField, Range(0f, 1f)] private float transparentAlpha = 0.3f;
    [SerializeField, Range(0f, 1f)] private float solidAlpha = 1.0f;
    [SerializeField] private Color memoryColor = new Color(0.9f, 0.95f, 1.0f, 1f);
    [SerializeField] private bool enableFloatingBobbing = true;
    [SerializeField] private float bobbingSpeed = 2f;
    [SerializeField] private float bobbingAmount = 0.1f;
    [SerializeField] private List<GameObject> platforms = new List<GameObject>();

    private List<SpriteRenderer> spriteRenderers = new List<SpriteRenderer>();
    private List<Collider2D> platformColliders = new List<Collider2D>();
    private List<Vector3> initialPositions = new List<Vector3>();

    private bool isSolid = false;
    private Coroutine solidifyRoutine;
    private Transform playerTransform;
    private PlayerInputReader playerInputReader;
    private float bobbingOffset;

    private void Awake()
    {
        bobbingOffset = Random.Range(0f, 2f * Mathf.PI);

        if (platforms.Count == 0)
        {
            if (transform.childCount > 0)
            {
                foreach (Transform child in transform)
                {
                    platforms.Add(child.gameObject);
                }
            }
            else
            {
                platforms.Add(gameObject);
            }
        }

        foreach (var obj in platforms)
        {
            if (obj == null) continue;

            SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();
            if (sr == null) sr = obj.GetComponentInChildren<SpriteRenderer>();
            if (sr != null) spriteRenderers.Add(sr);

            Collider2D col = obj.GetComponent<Collider2D>();
            if (col == null) col = obj.GetComponentInChildren<Collider2D>();
            if (col != null) platformColliders.Add(col);

            initialPositions.Add(obj.transform.position);
        }

        SetPlatformSolidState(false);
    }

    private void Start()
    {
        FindPlayer();
    }

    private void Update()
    {
        if (!isSolid && enableFloatingBobbing)
        {
            float yOffset = Mathf.Sin(Time.time * bobbingSpeed + bobbingOffset) * bobbingAmount;
            for (int i = 0; i < platforms.Count; i++)
            {
                if (platforms[i] != null && i < initialPositions.Count)
                {
                    platforms[i].transform.position = initialPositions[i] + new Vector3(0, yOffset, 0);
                }
            }
        }

        if (IsFPressed())
        {
            if (IsPlayerInRange())
            {
                Solidify();
            }
        }
    }

    private bool IsFPressed()
    {
        if (playerInputReader != null && playerInputReader.AttackPressed)
        {
            return true;
        }

        if (Keyboard.current != null && Keyboard.current.fKey.wasPressedThisFrame)
        {
            return true;
        }

        return false;
    }

    private bool IsPlayerInRange()
    {
        if (lightRange <= 0f)
            return true;

        if (playerTransform == null)
        {
            FindPlayer();
            if (playerTransform == null) return true;
        }

        float distance = Vector2.Distance(transform.position, playerTransform.position);
        return distance <= lightRange;
    }

    private void FindPlayer()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj == null)
        {
            PlayerMovement pm = Object.FindFirstObjectByType<PlayerMovement>();
            if (pm != null) playerObj = pm.gameObject;
        }

        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
            playerInputReader = playerObj.GetComponent<PlayerInputReader>();
        }
    }

    public void Solidify()
    {
        if (solidifyRoutine != null)
        {
            StopCoroutine(solidifyRoutine);
        }

        solidifyRoutine = StartCoroutine(SolidifyLifecycleRoutine());
    }

    private IEnumerator SolidifyLifecycleRoutine()
    {
        isSolid = true;

        for (int i = 0; i < platforms.Count; i++)
        {
            if (platforms[i] != null && i < initialPositions.Count)
            {
                platforms[i].transform.position = initialPositions[i];
            }
        }

        SetPlatformSolidState(true);

        float mainWaitTime = Mathf.Max(solidDuration - warningDuration, 0f);
        yield return new WaitForSeconds(mainWaitTime);

        float warningTimer = 0f;
        bool toggle = false;
        while (warningTimer < warningDuration)
        {
            warningTimer += 0.15f;
            toggle = !toggle;
            SetPlatformAlpha(toggle ? solidAlpha : transparentAlpha);
            yield return new WaitForSeconds(0.15f);
        }

        SetPlatformSolidState(false);
        isSolid = false;
        solidifyRoutine = null;
    }

    private void SetPlatformSolidState(bool solid)
    {
        foreach (var col in platformColliders)
        {
            if (col != null)
            {
                col.enabled = solid;
            }
        }

        SetPlatformAlpha(solid ? solidAlpha : transparentAlpha);
    }

    private void SetPlatformAlpha(float alpha)
    {
        foreach (var sr in spriteRenderers)
        {
            if (sr != null)
            {
                Color c = memoryColor;
                c.a = alpha;
                sr.color = c;
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (lightRange > 0f)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, lightRange);
        }
    }
}
