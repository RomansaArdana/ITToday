using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CanvasManager : MonoBehaviour
{
    public Page[] allPage;

    protected void SetPage(PageName namaPage)
    {
        foreach(var p in allPage)
        {
            p.gameObject.SetActive(false);
        }

        //cari page yang dibutuhkan
        Page findPage = Array.Find(allPage , p => p.nama == namaPage);
        if (findPage != null)
        {
            findPage.gameObject.SetActive(true);
        }
    }
}
