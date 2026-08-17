using UnityEngine;

public interface IHideable
{
    bool CanHide(GameObject player);
    Transform GetHidePoint();
    void EnterHide(GameObject player);
    void ExitHide(GameObject player);
}