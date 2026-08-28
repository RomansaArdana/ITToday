using UnityEngine;

/// <summary>
/// Pasang script ini di meja (objek 'E').
/// Meja TIDAK bisa disenggol/didorong oleh physics (Kinematic by default).
/// Player harus TAHAN tombol Interact → meja mengikuti player.
/// Lepas tombol → meja berhenti di tempat.
///
/// Setup:
/// 1. Rigidbody2D → Body Type: Kinematic, Freeze Rotation Z
/// 2. BoxCollider2D
/// 3. Layer: PuzzleObstacle
/// 4. Assign playerInputReader di Inspector (atau otomatis dari Player GameObject)
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class DraggableObject : InteractableBase
{
    [Header("References")]
    [SerializeField] private PlayerInputReader playerInputReader;

    [Header("Drag Settings")]
    [Tooltip("Multiplier kecepatan player saat menarik/mendorong meja (misal 0.5 = 50% kecepatan normal)")]
    [SerializeField, Range(0.1f, 1f)] private float dragSpeedMultiplier = 0.5f;
    [Tooltip("Penyesuaian offset jarak tempel (misal: -0.1 jika ingin sprite meja sedikit overlap/lebih masuk ke player)")]
    [SerializeField] private float snapOffsetAdjustment = 0f;
    [Tooltip("Jika true, meja hanya bisa digeser di sumbu X saja")]
    [SerializeField] private bool lockYAxis = true;

    private Rigidbody2D rb;
    private Collider2D[] objectColliders;
    private Collider2D[] holderColliders;
    private PlayerMovement playerMovement;
    private bool isBeingDragged = false;
    private GameObject currentHolder;
    private Vector2 dragOffset;
    private float lockedY;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        objectColliders = GetComponentsInChildren<Collider2D>();
        lockedY = transform.position.y;

        // Kinematic by default: tidak bisa disenggol / didorong physics
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;
    }

    private void OnDisable()
    {
        if (isBeingDragged)
        {
            ReleaseDrag();
        }
    }

    private void Update()
    {
        if (!isBeingDragged || currentHolder == null)
            return;

        bool held = playerInputReader != null && (playerInputReader.InteractHeld || playerInputReader.IsInteractHeld);

        // Lepas meja jika tombol E sudah tidak ditahan
        if (!held)
        {
            ReleaseDrag();
        }
    }

    private void FixedUpdate()
    {
        if (!isBeingDragged || currentHolder == null)
            return;

        // Target posisi selalu menempel rapat persis di sisi player
        Vector2 targetPos = (Vector2)currentHolder.transform.position + dragOffset;

        if (lockYAxis)
            targetPos.y = lockedY;

        // MovePosition langsung tanpa lerp agar meja tidak tertinggal / tidak berjarak saat player bergerak
        rb.MovePosition(targetPos);
    }

    public override void Interact(GameObject interactor)
    {
        // Dipanggil saat tombol Interact pertama kali ditekan (pressed)
        if (isBeingDragged)
            return;

        // Cari PlayerInputReader dari interactor jika belum di-assign
        if (playerInputReader == null)
            playerInputReader = interactor.GetComponent<PlayerInputReader>();

        if (playerInputReader == null)
        {
            Debug.LogError("[DraggableObject] PlayerInputReader tidak ditemukan! Assign manual di Inspector.");
            return;
        }

        StartDrag(interactor);
    }

    private void StartDrag(GameObject holder)
    {
        isBeingDragged = true;
        currentHolder = holder;
        lockedY = transform.position.y;
        rb.linearVelocity = Vector2.zero;

        // Ambil collider dan nonaktifkan tabrakan fisik player & meja selama di-drag
        holderColliders = holder.GetComponentsInChildren<Collider2D>();
        SetIgnoreCollisionWithHolder(true);

        // Tentukan sisi meja relatif terhadap player (+1 = kanan, -1 = kiri)
        float side = (transform.position.x >= holder.transform.position.x) ? 1f : -1f;

        // Cari collider fisik utama (non-trigger) dari player dan meja
        Collider2D playerCol = GetMainPhysicalCollider(holderColliders);
        Collider2D deskCol = GetMainPhysicalCollider(objectColliders);

        float calculatedOffsetX = 0f;

        if (playerCol != null && deskCol != null)
        {
            if (side > 0)
            {
                // Meja di sebelah kanan player: tempelkan ujung kiri collider meja ke ujung kanan collider player
                float playerRightDist = playerCol.bounds.max.x - holder.transform.position.x;
                float deskLeftDist = transform.position.x - deskCol.bounds.min.x;
                calculatedOffsetX = playerRightDist + deskLeftDist + snapOffsetAdjustment;
            }
            else
            {
                // Meja di sebelah kiri player: tempelkan ujung kanan collider meja ke ujung kiri collider player
                float playerLeftDist = holder.transform.position.x - playerCol.bounds.min.x;
                float deskRightDist = deskCol.bounds.max.x - transform.position.x;
                calculatedOffsetX = -(playerLeftDist + deskRightDist + snapOffsetAdjustment);
            }
        }
        else
        {
            // Fallback jika tidak ada collider
            calculatedOffsetX = side * 1.0f;
        }

        dragOffset = new Vector2(calculatedOffsetX, 0f);

        // Langsung posisikan meja menempel rapat seketika
        Vector2 startTargetPos = (Vector2)holder.transform.position + dragOffset;
        if (lockYAxis)
            startTargetPos.y = lockedY;

        rb.position = startTargetPos;
        transform.position = startTargetPos;

        // Kurangi kecepatan gerak player saat memegang meja (simulasi berat meja)
        playerMovement = holder.GetComponent<PlayerMovement>();
        playerMovement?.SetDragMultiplier(dragSpeedMultiplier);

        // Langsung keluar dari state Interact agar player tetap bisa bergerak bebas
        PlayerStateController stateController = holder.GetComponent<PlayerStateController>();
        stateController?.ExitInteraction();

        Debug.Log($"[DraggableObject] Mulai memegang {gameObject.name} (Offset X: {calculatedOffsetX})");
    }

    private Collider2D GetMainPhysicalCollider(Collider2D[] cols)
    {
        if (cols == null || cols.Length == 0)
            return null;

        // Prioritaskan collider yang BUKAN trigger
        foreach (var col in cols)
        {
            if (col != null && !col.isTrigger)
                return col;
        }

        return cols[0];
    }

    private void ReleaseDrag()
    {
        if (!isBeingDragged)
            return;

        isBeingDragged = false;

        // Kembalikan kecepatan gerak normal player
        playerMovement?.SetDragMultiplier(1f);
        playerMovement = null;

        // Aktifkan kembali tabrakan fisik antara meja dan player
        SetIgnoreCollisionWithHolder(false);

        PlayerStateController stateController = currentHolder?.GetComponent<PlayerStateController>();
        stateController?.ExitInteraction();

        currentHolder = null;
        holderColliders = null;

        Debug.Log($"[DraggableObject] Meja dilepas di posisi {transform.position}");
    }

    private void SetIgnoreCollisionWithHolder(bool ignore)
    {
        if (objectColliders == null || holderColliders == null)
            return;

        foreach (var objCol in objectColliders)
        {
            if (objCol == null) continue;
            foreach (var hCol in holderColliders)
            {
                if (hCol == null) continue;
                Physics2D.IgnoreCollision(objCol, hCol, ignore);
            }
        }
    }

    public bool IsBeingDragged => isBeingDragged;

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = isBeingDragged ? Color.green : Color.cyan;
        Gizmos.DrawWireCube(transform.position, Vector3.one * 0.2f);
    }
}
