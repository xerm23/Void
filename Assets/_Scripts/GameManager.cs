using Photon.Pun;	
using Photon.Realtime;	
using System.Collections;	
using System.Collections.Generic;	
using UnityEngine;	
using UnityEngine.SceneManagement;	
using UnityEngine.UI;	

public class GameManager : MonoBehaviourPunCallbacks
{

    public static GameManager instance;

    #region SerializeFields	
    int spawnIndex = 0;
    [SerializeField] private Transform[] spawnPoints = null;
    [SerializeField] private GameObject playerPrefab = null;
    [SerializeField] private GameObject endGamePanel = null;
    [SerializeField] public Button bombButton = null;
    [SerializeField] private Text winnerText = null;


    private Vector3 mainCamPos;
    [SerializeField] private float camVerticalSpeed = 3;
    [SerializeField] private GameObject[] platforms;
    [SerializeField] private GameObject fallTrigger;
    [SerializeField] private GameObject bombPickup;
    string platformsStr = "Platforms/";
    #endregion


    #region PrivateValues	

    int topHeight = 20;
    #endregion



    #region Unity	

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }

        mainCamPos = Camera.main.transform.position;
    }

    void Start()
    {
        PhotonNetwork.Instantiate(playerPrefab.name, spawnPoints[(int)PhotonNetwork.LocalPlayer.CustomProperties["spawnIndex"]].position, Quaternion.identity);
        initializeRows();
    }

    private void Update()
    {
        if (!endGamePanel.activeSelf)
            updateRows();
    }
    #endregion



    #region PlatformCreation	

    void initializeRows()
    {
        if (PhotonNetwork.LocalPlayer.IsMasterClient)
        {
            for (int i = -2; i < 3; i++)
            {
                for (int j = -2; j < 3; j++)
                {
                    if (i == -2 || i == 2 && j == 0)
                        PhotonNetwork.InstantiateRoomObject(platformsStr + platforms[2].name, new Vector3(i * 5, 0, 0), Quaternion.identity);
                    else
                    {
                        int spawnObj = Random.Range(0, platforms.Length + platforms.Length / 5);
                        if (spawnObj < platforms.Length)
                            PhotonNetwork.InstantiateRoomObject(platformsStr + platforms[spawnObj].name, new Vector3(i * 5, j * 10, 0), Quaternion.identity);
                    }

                }

            }

        }
    }

    void updateRows()
    {
        mainCamPos += new Vector3(0, camVerticalSpeed * Time.deltaTime, 0);
        Camera.main.transform.position = mainCamPos;

        if (mainCamPos.y > topHeight - 20 && PhotonNetwork.LocalPlayer.IsMasterClient)
        {
            topHeight += 10;
            for (int i = -2; i < 3; i++)
            {
                int spawnObj = Random.Range(0, platforms.Length + 1);
                if (spawnObj < platforms.Length)
                    PhotonNetwork.InstantiateRoomObject(platformsStr + platforms[spawnObj].name, new Vector3(i * 5 + (int)((mainCamPos.x) / 5) * 5, topHeight, 0), Quaternion.identity);
                PhotonNetwork.InstantiateRoomObject(platformsStr + fallTrigger.name, new Vector3(i * 5 + (int)((mainCamPos.x) / 5) * 5, topHeight + 5, 0), Quaternion.identity);
                //  int bombSp = Random.Range(0, 10);	
                //  if (bombSp == 0)	
                //  PhotonNetwork.InstantiateRoomObject(platformsStr + bombPickup.name, new Vector3(i * 5 + (int)((mainCamPos.x) / 5) * 5, topHeight + 2, 0), Quaternion.identity);	

            }

        }
    }


    #endregion



    #region public-functions-singleton	

    public void showGameOver(string winnerName)
    {
        endGamePanel.SetActive(true);
        winnerText.text = winnerName + " has won the game!";
    }
    public void showPlayerLeft()
    {
        endGamePanel.SetActive(true);
        winnerText.text = "Opponent left the game.";
    }


    #endregion

    #region UI-Callbacks	

    public void onBtnLeaveRoomClicked()
    {
        PhotonNetwork.Disconnect();

    }
    #endregion


    #region PUNCallbacks	

    public override void OnDisconnected(DisconnectCause cause)
    {
        SceneManager.LoadScene(0);
    }

    #endregion

}