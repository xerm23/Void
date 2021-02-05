using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviourPunCallbacks
{
    #region SerializeFields

    [SerializeField] List<Sprite> sprites;
    [SerializeField] GameObject bombObject;
    [SerializeField] GameObject trailObj;

    #endregion


    #region PrivateValues

    List<Vector3> movesList = new List<Vector3>();

    private SpriteRenderer spriteRenderer;
    PhotonView PV;

    private float moveSpeed = 10f;
    private float gridSize = 5f;
    private int fallAmount = 0;
    private int moveDecrep = 0;
    private bool landed = true;

    private Vector2 input;
    private bool isMoving = false;
    private Vector3 startPosition;
    private Vector3 endPosition;
    private float t;
    private float factor;
    private bool isColliding = false;

    #endregion

    #region Unity-functions

    private void Awake()
    {
        PV = GetComponent<PhotonView>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = sprites[(int)PV.Owner.CustomProperties["spriteIndex"]];
        GameManager.instance.bombButton.onClick.AddListener(onBombButtonClicked);

    }



    private void Update() {
        if (PV.IsMine)
        {
            checkInput();
            if (checkIfBelowCam()) triggerGameOver();
        }
    }

    private void LateUpdate()
    {
        if (PV.IsMine)
        {

            movePlayer();

        }

    }

    #endregion


    #region Movement

    public IEnumerator move(Transform transform, Vector3 moveDir)
    {
        isMoving = true;
        startPosition = transform.position;
        t = 0;

        endPosition = new Vector3(
            startPosition.x + moveDir.x * gridSize,
            startPosition.y + moveDir.y * gridSize * 2,
            startPosition.z);

        if (!(endPosition.x < -2 * gridSize || endPosition.x > 2 * gridSize || endPosition.y > Camera.main.transform.position.y + 30))
        {
            factor = 1f;

            while (t < 1f)
            {
                t += Time.deltaTime * (moveSpeed / gridSize) * factor;
                transform.position = Vector3.Lerp(startPosition, endPosition, t);
                if (moveDir.x != 0)
                    transform.eulerAngles = Vector3.Lerp(Vector3.zero, new Vector3(0, 0, 720), t);
                yield return null;
            }
        }


        isMoving = false;
        movesList.RemoveAt(0);
        yield return 0;
    }


    private void movePlayer()
    {
        if (!isMoving && movesList.Count > 0)
        {
            StartCoroutine(move(transform, movesList[0]));
        }
        //fall down
        if (!isMoving && !landed)
        {
            movesList.Add(Vector3.down);

        }
    }


    #endregion


    #region InputHandling
    private void checkInput()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow) && moveDecrep == 0)
            movesList.Add(Vector3.up);
        if (Input.GetKeyDown(KeyCode.RightArrow) && moveDecrep == 0)
            movesList.Add(Vector3.right);
        if (Input.GetKeyDown(KeyCode.LeftArrow) && moveDecrep == 0)
            movesList.Add(Vector3.left);
        if (Input.GetKeyDown(KeyCode.DownArrow) && moveDecrep == 0)
            movesList.Add(Vector3.down);

        if (Input.anyKeyDown && moveDecrep > 0) moveDecrep--;

    }
    #endregion

    #region CollisionHandling

    private void OnCollisionEnter2D(Collision2D collision)
    {
        
        landed = true;
        if (collision.gameObject.tag == "Grass")
        {
            fallAmount = 0;
        }
        if (collision.gameObject.tag == "Bomb" && PV.IsMine)
        {
            triggerGameOver();
        }
        //if (collision.gameObject.tag == "Bomb" && !collision.gameObject.GetComponent<PhotonView>().IsMine && PV.IsMine)
        //{
        //    triggerGameOver();
        //}

        if (fallAmount >= 2 && PV.IsMine)
        {
            triggerGameOver();
        }
        else
        {
            if (collision.gameObject.tag == "Breakable")
            {
                collision.gameObject.SetActive(false);
                landed = false;
            }
            if (collision.gameObject.tag == "Spike" && PV.IsMine)
            {
                triggerGameOver();
            }
            if (collision.gameObject.tag == "Slime")
            {
                moveDecrep = 2;
            }
            if (collision.gameObject.tag == "SideLeft")
            {
                movesList.Add(Vector3.left);
            }
            if (collision.gameObject.tag == "SideRight")
            {
                movesList.Add(Vector3.right);
            }
            fallAmount = 0;

        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        landed = false;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isColliding) return;
        isColliding = true;
        if (movesList.Count > 0)
            if (movesList[0] != Vector3.up)
                fallAmount++;

        StartCoroutine(Reset());
    }

    IEnumerator Reset()
    {
        yield return new WaitForEndOfFrame();
        isColliding = false;
    }


    #endregion


    #region PUN-RPCs

    [PunRPC]
    void spawnBomb(Vector3 pos)
    {
        Instantiate(bombObject, pos + new Vector3(0, 1.5f, 0), Quaternion.identity);
    }
    [PunRPC]
    void spawnCanvas(string winnerName)
    {
        GameManager.instance.showGameOver(winnerName);

    }
    #endregion


    #region CustomFunctions
    void triggerGameOver()
    {
        foreach (Player p in PhotonNetwork.PlayerListOthers)
        {
            GameManager.instance.showGameOver(p.NickName);
            PV.RPC("spawnCanvas", RpcTarget.Others, p.NickName);
        }
    }

    bool checkIfBelowCam()
    {
        if (transform.position.y < Camera.main.transform.position.y - 30 && PV.IsMine)
        {
            return true;
        }
        else return false;
    }


    public void onBombButtonClicked()
    {
        if (PV.IsMine)
        {
            PV.RPC("spawnBomb", RpcTarget.Others, transform.position);

        }
    }
    

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        GameManager.instance.showPlayerLeft();
    }



    #endregion
}
