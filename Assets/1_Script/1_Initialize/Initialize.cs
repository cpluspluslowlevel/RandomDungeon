using RandomDungeon.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Initialize : MonoBehaviour
{

    private void Awake()
    {
        Framework.Instance.Initialize();
        SceneManager.LoadScene("Play");
    }

}
