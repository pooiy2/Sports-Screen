using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DartButton : MonoBehaviour
{
    public void ChangeScene(){
        SceneManager.LoadScene("Darts");
    }
}
