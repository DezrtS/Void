using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneButton : MonoBehaviour
{
    public void NextScene()
    {
        SceneManager.Instance.LoadNextScene();
    }
}
