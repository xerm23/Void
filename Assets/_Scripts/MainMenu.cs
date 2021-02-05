using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ExitGames.Client.Photon;

public class MainMenu : MonoBehaviourPunCallbacks
{
    public InputField PlayerNameInput;
    public Button playButton;
    [Header("Join Random Room Panel")]
    public GameObject JoinRandomRoomPanel;

    [Header("Inside Room Panel")]
    public GameObject InsideRoomPanel;

    public Button StartGameButton;
    public GameObject PlayerListEntryPrefab;

    private Dictionary<int, GameObject> playerListEntries;

    public const string PLAYER_READY = "IsPlayerReady";
    public const string INIT_R_SEED = "InitRandSeed";


    private const string PlayerPrefsNameKey = "PlayerName";
    private const string charModelIDPrefsKey = "CharModelID";

    [SerializeField]
    private List<Sprite> sprites;
    [SerializeField]
    private Image imagePreview;
    private int currentSpriteID=0;



    #region UNITY

    public void Awake()
    {
        PhotonNetwork.AutomaticallySyncScene = true;

        PlayerNameInput.text = PlayerPrefs.GetString(PlayerPrefsNameKey);
        changeButtonState();

        currentSpriteID = PlayerPrefs.GetInt(charModelIDPrefsKey);
        imagePreview.sprite = sprites[currentSpriteID];
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {

        }
    }
    #endregion

    #region PUN CALLBACKS

    public override void OnConnectedToMaster()
    {
        //this.SetActivePanel(SelectionPanel.name);
        PhotonNetwork.JoinRandomRoom();

        Hashtable spriteIndexID = new Hashtable() { { "spriteIndex", PlayerPrefs.GetInt(charModelIDPrefsKey) } };
        // if(photonView.IsMine)
        PhotonNetwork.LocalPlayer.SetCustomProperties(spriteIndexID);
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        string roomName = "Room " + Random.Range(1000, 10000);

        RoomOptions options = new RoomOptions { MaxPlayers = 2 };
        options.CustomRoomProperties = new ExitGames.Client.Photon.Hashtable();
        options.CustomRoomProperties.Add(INIT_R_SEED, (int)System.DateTime.Now.Ticks);

        PhotonNetwork.CreateRoom(roomName, options, null);
    }

    public override void OnJoinedRoom()
    {
        SetActivePanel(InsideRoomPanel.name);

        if (playerListEntries == null)
        {
            playerListEntries = new Dictionary<int, GameObject>();
        }
        int index = 0;
        foreach (Player p in PhotonNetwork.PlayerList)
        {

            GameObject entry = Instantiate(PlayerListEntryPrefab);
            entry.transform.SetParent(InsideRoomPanel.transform);
            entry.transform.localScale = Vector3.one;
            entry.GetComponent<PlayerListEntry>().Initialize(p.ActorNumber, p.NickName, sprites[(int)p.CustomProperties["spriteIndex"]]);


            Hashtable props = new Hashtable() { { "spawnIndex", index++ }};
            p.SetCustomProperties(props);


            object isPlayerReady;
            if (p.CustomProperties.TryGetValue(PLAYER_READY, out isPlayerReady))
            {
                entry.GetComponent<PlayerListEntry>().SetPlayerReady((bool)isPlayerReady);
            }

            playerListEntries.Add(p.ActorNumber, entry);
        }

        StartGameButton.gameObject.SetActive(CheckPlayersReady());

    }

    public override void OnLeftRoom()
    {
        InsideRoomPanel.SetActive(false);
        JoinRandomRoomPanel.SetActive(true);

        foreach (GameObject entry in playerListEntries.Values)
        {
            Destroy(entry.gameObject);
        }

        playerListEntries.Clear();
        playerListEntries = null;
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        GameObject entry = Instantiate(PlayerListEntryPrefab);
        entry.transform.SetParent(InsideRoomPanel.transform);
        entry.transform.localScale = Vector3.one;
        entry.GetComponent<PlayerListEntry>().Initialize(newPlayer.ActorNumber, newPlayer.NickName, sprites[(int)newPlayer.CustomProperties["spriteIndex"]]);

        playerListEntries.Add(newPlayer.ActorNumber, entry);

        StartGameButton.gameObject.SetActive(CheckPlayersReady());
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        Destroy(playerListEntries[otherPlayer.ActorNumber].gameObject);
        playerListEntries.Remove(otherPlayer.ActorNumber);

        StartGameButton.gameObject.SetActive(CheckPlayersReady());
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        if (PhotonNetwork.LocalPlayer.ActorNumber == newMasterClient.ActorNumber)
        {
            StartGameButton.gameObject.SetActive(CheckPlayersReady());
        }
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        if (playerListEntries == null)
        {
            playerListEntries = new Dictionary<int, GameObject>();
        }

        GameObject entry;
        if (playerListEntries.TryGetValue(targetPlayer.ActorNumber, out entry))
        {
            object isPlayerReady;
            if (changedProps.TryGetValue(PLAYER_READY, out isPlayerReady))
            {
                entry.GetComponent<PlayerListEntry>().SetPlayerReady((bool)isPlayerReady);
            }
        }

        StartGameButton.gameObject.SetActive(CheckPlayersReady());
    }

#endregion

#region UI CALLBACKS

    public void OnBackButtonClicked()
    {
        if (PhotonNetwork.InLobby)
        {
            PhotonNetwork.LeaveLobby();
        }
    }
    public void OnJoinRandomRoomButtonClicked()
    {
        SetActivePanel(JoinRandomRoomPanel.name);

        PhotonNetwork.JoinRandomRoom();
    }

    public void OnLeaveGameButtonClicked()
    {
        PhotonNetwork.LeaveRoom();
    }

    public void OnLoginButtonClicked()
    {
        string playerName = PlayerNameInput.text;
        SetActivePanel(JoinRandomRoomPanel.name);

        if (!playerName.Equals(""))
        {
            PhotonNetwork.LocalPlayer.NickName = playerName;
            PhotonNetwork.ConnectUsingSettings();
        }
        else
        {
            Debug.LogError("Player Name is invalid.");
        }
    }


    public void OnStartGameButtonClicked() { 
        PhotonNetwork.CurrentRoom.IsOpen = false;
        PhotonNetwork.CurrentRoom.IsVisible = false;

        PhotonNetwork.LoadLevel("Game");
    }


    public void OnBtnNextClicked()
    {
        currentSpriteID++;
        if (currentSpriteID >= sprites.Count)
        {
            currentSpriteID = sprites.Count - 1;
        }
        PlayerPrefs.SetInt(charModelIDPrefsKey, currentSpriteID);
        imagePreview.sprite = sprites[currentSpriteID];

    }

    public void OnBtnPrevClicked()
    {
        currentSpriteID--;
        if (currentSpriteID < 0)
        {
            currentSpriteID = 0;
        }
        PlayerPrefs.SetInt(charModelIDPrefsKey, currentSpriteID);
        imagePreview.sprite = sprites[currentSpriteID];

    }

    #endregion

    private bool CheckPlayersReady()
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            return false;
        }
        if (PhotonNetwork.PlayerList.Length < PhotonNetwork.CurrentRoom.MaxPlayers)
        {
            return false;
        }

        foreach (Player p in PhotonNetwork.PlayerList)
        {
            object isPlayerReady;
            if (p.CustomProperties.TryGetValue(PLAYER_READY, out isPlayerReady))
            {
                if (!(bool)isPlayerReady)
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        return true;
    }

    public void LocalPlayerPropertiesUpdated()
    {
        StartGameButton.gameObject.SetActive(CheckPlayersReady());
    }

    private void SetActivePanel(string activePanel)
    {

        JoinRandomRoomPanel.SetActive(activePanel.Equals(JoinRandomRoomPanel.name));
        InsideRoomPanel.SetActive(activePanel.Equals(InsideRoomPanel.name));
    }

    public void changeButtonState()
    {
        playButton.interactable = !string.IsNullOrEmpty(PlayerNameInput.text);
    }


    public void saveName()
    {
        string playerName = PlayerNameInput.text;
        PlayerPrefs.SetString(PlayerPrefsNameKey, playerName);
    }
}
