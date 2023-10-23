using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Cinemachine;
using TMPro;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]

    [SerializeField] float speed;
    [SerializeField] float speedAirCoef;
    [SerializeField] float jumpForce;
    [SerializeField] float groundRayDist;
    [SerializeField] int maxJumpCount;
    [SerializeField] float jumpDalayTime;
    [SerializeField] Transform platformCheck;
    [SerializeField] float platformRayDist;

    [Header("Attack")]

    [SerializeField] GameObject grenadePrefab;
    [SerializeField] Transform grenadeSpawnPoint;
    [SerializeField] float upForce, forwardForce;
    [SerializeField] float cooldown;
    bool canThrow;



    [Header("Layers")]

    [SerializeField] LayerMask groundLayerMask;
    [SerializeField] LayerMask platformLayerMask;

    [Header("Debug")]

    bool isFacingRight;
    bool isGrounded;
    bool isTakingHit;
    int remaningJumps;
    bool wantJump;
    bool firstJumpDelay;
    bool isPlatformAbove;

    Vector2 inputVector;
    bool jumpButton;
    bool downJumpButton;
    bool downJumpButtonHold;
    bool throwGrenadeButton;


    Animator animator;
    Rigidbody2D RB;
    Transform groundCheck;

    // multiplayer
    [SerializeField] GameObject hpCanvas;
    TMP_Text nicknameText;
    PhotonView photonView;

    // layers numeration
    private int playerLayer, platformLayer, ladderLayer;

    private void Start()
    {
        photonView = GetComponent<PhotonView>();
        if (!photonView.IsMine)
        {
            Destroy(GetComponentInChildren<Camera>().gameObject);
            Destroy(GetComponentInChildren<CinemachineVirtualCamera>().gameObject);
        }

        nicknameText = GetComponentInChildren<TMP_Text>();
        if (nicknameText) nicknameText.text = photonView.Owner.NickName;
        
        RB = GetComponent<Rigidbody2D>();
        // animator
        animator = GetComponent<Animator>();
        // playerHp
        wantJump = false;
        playerLayer = LayerMask.NameToLayer("10 Player");
        platformLayer = LayerMask.NameToLayer("21 PlatformTilemap");
        ladderLayer = LayerMask.NameToLayer("22 LadderTilemap");
        canThrow = true;

        groundCheck = transform.Find("GroundCheck").transform;
        if (!groundCheck) Debug.LogError("GroundCheck не найден");
    }

    private void Update()
    {
        if (!photonView.IsMine) return;

        PlayerInput();
        GroundCheck();
        JumpControl();
        ShowRays();
        if(canThrow == true && throwGrenadeButton)
        {
            ThrowGrenade();
        }
    }

    private void FixedUpdate()
    {
        MoveControl();      
    }

    private void ThrowGrenade()
    {
        canThrow = false;

        GameObject newGrenade = Instantiate(grenadePrefab, grenadeSpawnPoint.position, Quaternion.identity);

        Vector3 direction = grenadeSpawnPoint.right * forwardForce + grenadeSpawnPoint.up * upForce;


        newGrenade.GetComponent<Rigidbody2D>().AddForce(direction, ForceMode2D.Impulse);

        Invoke("FinishCooldown", cooldown);
    }

    private void FinishCooldown()
    {
        canThrow = true;
    }

    private void FlipX()
    {
        isFacingRight = !isFacingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1f;
        transform.localScale = scale;
        hpCanvas.transform.rotation = isFacingRight ? Quaternion.Euler(0, 180f, 0) : Quaternion.Euler(0, 0, 0);
        nicknameText.transform.rotation = isFacingRight ? Quaternion.Euler(0, 180f, 0) : Quaternion.Euler(0, 0, 0);
        grenadeSpawnPoint.transform.rotation = !isFacingRight ? Quaternion.Euler(0, 180f, 0) : Quaternion.Euler(0, 0, 0);
    }

    private void PlayerInput()
    {
        if (isTakingHit) return;

        inputVector.x = Input.GetAxisRaw("Horizontal");
        jumpButton = Input.GetButtonDown("Jump");
        downJumpButton = Input.GetButtonDown("DownJump");
        downJumpButtonHold = Input.GetButton("DownJump");
        throwGrenadeButton = Input.GetButton("Fire1");
    }
    
    private void GroundCheck()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundRayDist, groundLayerMask);
    }

    private void JumpControl()
    {
        // Jump
        if((jumpButton ^ wantJump) && remaningJumps > 0 && !isTakingHit)
        {
            StartCoroutine(Jump());
        }
        // Jump memory

        // Jump counter
        if (isGrounded && remaningJumps != maxJumpCount && !firstJumpDelay) remaningJumps = maxJumpCount;
        
        if (downJumpButtonHold)
        {
            isPlatformAbove = Physics2D.OverlapCircle(platformCheck.position, platformRayDist + 0.1f, platformLayerMask);
        }
        else
        {
            isPlatformAbove = Physics2D.OverlapCircle(platformCheck.position, platformRayDist, platformLayerMask);
        }
        if (isPlatformAbove || downJumpButton) Physics2D.IgnoreLayerCollision(platformLayer, playerLayer, true);
        else Physics2D.IgnoreLayerCollision(platformLayer, playerLayer, false);

    }

    private void ShowRays()
    {
        Debug.DrawRay(platformCheck.position, Vector3.up * platformRayDist, Color.red);
        Debug.DrawRay(platformCheck.position, Vector3.down * platformRayDist, Color.red);
        Debug.DrawRay(platformCheck.position, Vector3.left * platformRayDist, Color.red);
        Debug.DrawRay(platformCheck.position, Vector3.right * platformRayDist, Color.red);
    }

    private void MoveControl()
    {
        //Flip Control
        if (inputVector.x > 0 && !isFacingRight) FlipX();
        else if (inputVector.x < 0 && isFacingRight) FlipX();

        //Common walking
        if (isGrounded && inputVector.x != 0 && !isTakingHit)
        {
            RB.velocity = new Vector2(inputVector.x * speed, RB.velocity.y);
            animator.SetBool("isRun", true);  
        }

        // air move
        else if (!isGrounded && inputVector.x != Mathf.Epsilon && !isTakingHit)
        {
            RB.velocity = new Vector2(inputVector.x * speed * speedAirCoef, RB.velocity.y);
            // animator command
        }
            
            //Idle
        else if (isGrounded && inputVector.x < Mathf.Abs(Mathf.Epsilon) &&!isTakingHit)
        {
            RB.velocity = new Vector2(0f, RB.velocity.y);
            animator.SetBool("isRun", false);

        }


    }

    private IEnumerator Jump()
    {
        remaningJumps--;
        // animator
        firstJumpDelay = true;
        RB.AddRelativeForce (Vector2.up * jumpForce, ForceMode2D.Impulse);
        yield return new WaitForSeconds(jumpDalayTime);
        firstJumpDelay = false;
    }

}




















