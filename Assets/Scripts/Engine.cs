using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Engine : MonoBehaviour {

    [HideInInspector]
    public static Engine singleton;

    public int playerScore;
    public int opponentScore;
    [HideInInspector]public bool wonAlready;

	// Use this for initialization
	void Awake () {
        singleton = this;

        playerScore = 0;
        opponentScore = 0;
        wonAlready = false;
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public IEnumerator Reset(string winner){
        if (wonAlready == true)
            yield break;
        wonAlready = true;
        Debug.Log("win!"); //ui shit
        yield return new WaitForSeconds(1.5f);
        wonAlready = false;
        SceneManager.LoadScene("BoutEasy");
    }


}
