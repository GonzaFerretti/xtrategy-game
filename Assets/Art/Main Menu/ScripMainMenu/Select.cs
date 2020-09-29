using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Select : MonoBehaviour
{
    public string nombreEscenaDestino;

    // Start is called before the first frame update
    void Start()
    {

    }

    public void CambiarEscena()
    {

        SceneManager.LoadScene(nombreEscenaDestino);
    }

    // Update is called once per frame
    void Update()
    {

    }
}
