using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SkyPortalTrigger : MonoBehaviour
{
    //public Animator anim;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            StartCoroutine("LevelExit");
        }
    }

    IEnumerator LevelExit()
    {
        SkyRealmGameManager.instance.SetLevelExiting();

        yield return new WaitForSeconds(0.1f);

        SkyRealmUIManager.instance.DisableMobileControls();
        SkyRealmUIManager.instance.fadeToBlack = true;

        yield return new WaitForSeconds(2f);

        SkyRealmGameManager.instance.LevelComplete();
    }
}
