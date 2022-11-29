using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TextPlayer2 : MonoBehaviour {

    public Text Player2Text;

    CharacterManager charM;


    void Start () {

        charM = CharacterManager.GetInstance();

        if (charM.numberOfUsers == 1)
        {
            Player2Text.gameObject.SetActive(false);

        }
    }
	
	// Update is called once per frame
	void Update () {

	}
}
