using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class TimedFloatingBridge : MonoBehaviour
{
    [System.Serializable]
    public class PlatformPiece
    {
        public Transform pieceTransform;
        public Vector3 floatingPos;
        public Vector3 assembledPos;

        [HideInInspector] public SpriteRenderer spriteRenderer;
        [HideInInspector] public Collider2D collider2d;
    }

    [SerializeField] private PlatformPiece[] pieces;
    [SerializeField] private float activeDuration = 5f;
    [SerializeField] private float assemblySpeed = 6f;
    [SerializeField] private float warningTime = 1.5f;
    [SerializeField] private bool enableBobbing = true;
    [SerializeField] private float bobbingSpeed = 2f;
    [SerializeField] private float bobbingAmount = 0.15f;
    [SerializeField] private Collider2D activationZone;

    private bool isPlayerInZone = false;
    private bool isBridgeActive = false;
    private Coroutine bridgeRoutine;
    private float[] randomBobbingOffsets;

    private void Awake()
    {
        if (pieces != null)
        {
            randomBobbingOffsets = new float[pieces.Length];

            for (int i = 0; i < pieces.Length; i++)
            {
                randomBobbingOffsets[i] = Random.Range(0f, 2f * Mathf.PI);

                if (pieces[i].pieceTransform != null)
                {
                    if (pieces[i].floatingPos == Vector3.zero)
                    {
                        pieces[i].floatingPos = pieces[i].pieceTransform.position;
                    }

                    pieces[i].spriteRenderer = pieces[i].pieceTransform.GetComponentInChildren<SpriteRenderer>();
                    pieces[i].collider2d = pieces[i].pieceTransform.GetComponentInChildren<Collider2D>();
                }
            }
        }
    }

    private void Update()
    {
        if (!isBridgeActive && enableBobbing && pieces != null)
        {
            for (int i = 0; i < pieces.Length; i++)
            {
                if (pieces[i].pieceTransform != null)
                {
                    float yOffset = Mathf.Sin(Time.time * bobbingSpeed + randomBobbingOffsets[i]) * bobbingAmount;
                    pieces[i].pieceTransform.position = pieces[i].floatingPos + new Vector3(0, yOffset, 0);
                }
            }
        }

        bool fPressed = false;

        if (Keyboard.current != null && Keyboard.current.fKey.wasPressedThisFrame)
        {
            fPressed = true;
        }

        if (fPressed)
        {
            if (activationZone != null && !isPlayerInZone)
                return;

            ActivateBridge();
        }
    }

    public void ActivateBridge()
    {
        if (bridgeRoutine != null)
        {
            StopCoroutine(bridgeRoutine);
        }

        bridgeRoutine = StartCoroutine(BridgeLifeCycleRoutine());
    }

    private IEnumerator BridgeLifeCycleRoutine()
    {
        isBridgeActive = true;

        SetCollidersActive(true);

        float elapsed = 0f;
        Vector3[] startPositions = new Vector3[pieces.Length];
        for (int i = 0; i < pieces.Length; i++)
        {
            startPositions[i] = pieces[i].pieceTransform != null ? pieces[i].pieceTransform.position : Vector3.zero;
        }

        while (elapsed < 1f)
        {
            elapsed += Time.deltaTime * assemblySpeed;
            for (int i = 0; i < pieces.Length; i++)
            {
                if (pieces[i].pieceTransform != null)
                {
                    pieces[i].pieceTransform.position = Vector3.Lerp(startPositions[i], pieces[i].assembledPos, elapsed);
                }
            }
            yield return null;
        }

        for (int i = 0; i < pieces.Length; i++)
        {
            if (pieces[i].pieceTransform != null)
            {
                pieces[i].pieceTransform.position = pieces[i].assembledPos;
            }
        }

        float mainWaitTime = Mathf.Max(activeDuration - warningTime, 0f);
        yield return new WaitForSeconds(mainWaitTime);

        float warningTimer = 0f;
        bool visible = true;
        while (warningTimer < warningTime)
        {
            warningTimer += 0.15f;
            visible = !visible;
            SetSpritesAlpha(visible ? 1f : 0.35f);
            yield return new WaitForSeconds(0.15f);
        }

        SetSpritesAlpha(1f);

        elapsed = 0f;
        for (int i = 0; i < pieces.Length; i++)
        {
            startPositions[i] = pieces[i].pieceTransform != null ? pieces[i].pieceTransform.position : Vector3.zero;
        }

        while (elapsed < 1f)
        {
            elapsed += Time.deltaTime * (assemblySpeed * 0.75f);
            for (int i = 0; i < pieces.Length; i++)
            {
                if (pieces[i].pieceTransform != null)
                {
                    pieces[i].pieceTransform.position = Vector3.Lerp(startPositions[i], pieces[i].floatingPos, elapsed);
                }
            }
            yield return null;
        }

        isBridgeActive = false;
        bridgeRoutine = null;
    }

    private void SetCollidersActive(bool active)
    {
        if (pieces == null) return;
        foreach (var piece in pieces)
        {
            if (piece.collider2d != null)
            {
                piece.collider2d.enabled = active;
            }
        }
    }

    private void SetSpritesAlpha(float alpha)
    {
        if (pieces == null) return;
        foreach (var piece in pieces)
        {
            if (piece.spriteRenderer != null)
            {
                Color c = piece.spriteRenderer.color;
                c.a = alpha;
                piece.spriteRenderer.color = c;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") || other.GetComponent<PlayerMovement>() != null)
        {
            isPlayerInZone = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player") || other.GetComponent<PlayerMovement>() != null)
        {
            isPlayerInZone = false;
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (pieces == null) return;

        Gizmos.color = Color.cyan;
        foreach (var piece in pieces)
        {
            if (piece.pieceTransform != null)
            {
                Vector3 fPos = (piece.floatingPos != Vector3.zero) ? piece.floatingPos : piece.pieceTransform.position;
                Gizmos.DrawWireCube(fPos, piece.pieceTransform.localScale);
                Gizmos.DrawLine(fPos, piece.assembledPos);
                Gizmos.color = Color.green;
                Gizmos.DrawWireCube(piece.assembledPos, piece.pieceTransform.localScale);
                Gizmos.color = Color.cyan;
            }
        }
    }
}
