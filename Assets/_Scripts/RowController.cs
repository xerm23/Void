using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RowController : MonoBehaviour
{
    [SerializeField] private Transform playerPos;
    [SerializeField] private Transform mainCam;
    [SerializeField] private float camVerticalSpeed=5;
    [SerializeField] private GameObject[] platforms;


    int topHeight = 20;



    // Start is called before the first frame update
    void Start()
    {
        for (int i = -2; i < 3; i++)
        {
            for (int j = -2; j < 3; j++)
            {
                if (i == 0 && j == 0)
                    Instantiate(platforms[2], new Vector3(i,j,0), Quaternion.identity, this.gameObject.transform);
                else
                    Instantiate(platforms[Random.Range(0,platforms.Length)], new Vector3(i*5, j*10, 0), Quaternion.identity, this.gameObject.transform);

            }

        }
        
    }



    // Update is called once per frame
    void Update()
    {
        mainCam.position += new Vector3(0, camVerticalSpeed * Time.deltaTime, 0);

      //  print(mainCam.position.y + " " + topHeight);

        if (mainCam.position.y > topHeight - 20)
        {
            topHeight += 10;
            for (int i = -2; i < 3; i++)
            {
                Instantiate(platforms[Random.Range(0, platforms.Length)], new Vector3(i * 5 + (int) ((mainCam.position.x)/5)*5 , topHeight, 0), Quaternion.identity, this.gameObject.transform);

            }

        }
    }

}
