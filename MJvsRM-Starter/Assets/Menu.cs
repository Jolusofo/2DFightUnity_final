using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour {
    public GameObject startText;
    float timer;

	void Start()
	{
        startText.SetActive(false);	
	}
	void Update()
    {
        timer += Time.deltaTime;
        if (timer > 0.6f)
        {
            timer = 0;
            startText.SetActive(!startText.activeInHierarchy);
        }

    }

    public void Voltar()
    {
        SceneManager.LoadScene("intro");
    }

}
