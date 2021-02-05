using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BombScript : MonoBehaviour
{
    private SpriteRenderer spriteR;

    void Start()
    {
        spriteR = GetComponent<SpriteRenderer>();
        StartCoroutine(flash());
    }
    public IEnumerator flash()
    {
        for (int i = 0; i < 6; i++)
        {
            spriteR.color = new Color(1, 1, 1, 0);
            yield return new WaitForSeconds(.1f);
            spriteR.color = new Color(1, 1, 1, 1);
            yield return new WaitForSeconds(.1f);
        }
        activateColliders();
    }
    void activateColliders()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            transform.GetChild(i).gameObject.SetActive(true);

        }
    }
}