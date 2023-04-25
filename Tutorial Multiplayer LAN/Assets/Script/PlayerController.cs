using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

[System.Obsolete]
public class PlayerController : NetworkBehaviour
{
    GameObject jumpButton;
    GameObject leftButton;
    GameObject rightButton;

    // Kecepatan karakter pemain
    public float speed = 5f;

    // Kecepatan lompat karakter pemain
    public float jumpForce = 10f;

    // Rigidbody2D untuk karakter pemain
    Rigidbody2D rb;

    [SyncVar]
    Vector2 syncPosition;

    [SerializeField]
    Transform playerTransform;

    [SerializeField]
    SpriteRenderer playerSpriteRenderer;

    // Warna untuk karakter pemain
    public Color localPlayerColor = Color.blue;
    public Color remotePlayerColor = Color.red;

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();

        jumpButton = GameObject.FindGameObjectWithTag("JumpButton");
        leftButton = GameObject.FindGameObjectWithTag("LeftButton");
        rightButton = GameObject.FindGameObjectWithTag("RightButton");

        rb = GetComponent<Rigidbody2D>();
        playerTransform = GetComponent<Transform>();
        playerSpriteRenderer = GetComponent<SpriteRenderer>();
        
        CmdEnableAuthority();
        SetPlayerColor(localPlayerColor);

        jumpButton.GetComponent<Button>().onClick.AddListener(Jump);

    }

    [Command]
    void CmdEnableAuthority()
    {
        // Enable client authority for the player
        gameObject.GetComponent<NetworkIdentity>().AssignClientAuthority(connectionToClient);
    }

    void FixedUpdate()
    {
        if (!isLocalPlayer)
        {
            if (playerTransform != null)
            {
                playerTransform.position = syncPosition;
            }
            return;
        }

        // Mengontrol gerakan karakter pemain
        float moveInput = Input.GetAxisRaw("Horizontal");
        rb.velocity = new Vector2(moveInput * speed, rb.velocity.y);

        if (leftButton.GetComponent<JoyButton>().pressed)
        {
            rb.velocity = new Vector2(speed * -1f, rb.velocity.y);
        }

        if (rightButton.GetComponent<JoyButton>().pressed)
        {
            rb.velocity = new Vector2(speed * 1f, rb.velocity.y);
        }


        // Mengontrol lompatan karakter pemain
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Jump();
        }

        CmdSyncPosition(playerTransform.position);
    }

    void Jump()
    {
        rb.velocity = new Vector2(rb.velocity.x, jumpForce);
    }

    [Command]
    void CmdSyncPosition(Vector2 position)
    {
        syncPosition = position;
        RpcUpdatePosition(syncPosition);
    }

    [ClientRpc]
    void RpcUpdatePosition(Vector2 position)
    {
        syncPosition = position;

        if (!isLocalPlayer)
        {
            if (playerTransform != null)
            {
                playerTransform.position = syncPosition;
            }
        }
    }

    void SetPlayerColor(Color color)
    {
        if (playerSpriteRenderer != null)
        {
            playerSpriteRenderer.color = color;
        }
    }

    public override void OnStartClient()
    {
        base.OnStartClient();

        if (!isLocalPlayer)
        {
            SetPlayerColor(remotePlayerColor);
        }
    }
}
