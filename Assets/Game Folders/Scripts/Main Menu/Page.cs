using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Page : MonoBehaviour
{
    public PageName nama = PageName.Menu;

    public void ChangeScene(string namaScene)
    {
        SceneManager.LoadScene(namaScene);
    }
}



public enum PageName
{
    Menu,
    Setting,
    Info,
    Level
}