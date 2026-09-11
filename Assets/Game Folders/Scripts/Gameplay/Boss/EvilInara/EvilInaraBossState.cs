/// <summary>
/// Seluruh state yang dapat dimasuki oleh Evil Inara selama boss fight.
/// </summary>
public enum EvilInaraBossState
{
    /// <summary>Sequence masuk — boss muncul, animasi intro berjalan.</summary>
    Intro,

    /// <summary>Boss diam menunggu sebelum memilih attack berikutnya.</summary>
    Idle,

    /// <summary>Boss menampilkan indikator telegraph sebelum menyerang.</summary>
    Telegraph,

    /// <summary>Boss sedang melakukan serangan aktif.</summary>
    Attacking,

    /// <summary>Boss recovery setelah attack selesai.</summary>
    Recovery,

    /// <summary>Boss kelelahan — tidak menyerang, membuka vulnerability window.</summary>
    Exhausted,

    /// <summary>Boss rentan terhadap serangan Lantern player.</summary>
    Vulnerable,

    /// <summary>HP boss berkurang — transisi ke phase berikutnya, minion spawn.</summary>
    PhaseTransition,

    /// <summary>Boss telah dikalahkan — memulai death/purification sequence.</summary>
    Dead
}
