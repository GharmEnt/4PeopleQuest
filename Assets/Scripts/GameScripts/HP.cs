using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using TMPro;

public class HP : MonoBehaviour
{
    [SerializeField] float maxHP;
    float currHP;
    [SerializeField] Image hpImage;
    private Animator animator;


    PhotonView photonView;
    PlayerController controller;
    Rigidbody2D RB;
    BoxCollider2D collider;
    SpriteRenderer sprite;
    [SerializeField]TextMeshPro NNtext;
    [SerializeField]GameObject HPui;



    private void Start()
    {
        photonView = GetComponent<PhotonView>();
        currHP = maxHP;

        animator = GetComponent<Animator>();
        RB = GetComponent<Rigidbody2D>();
        controller = GetComponent<PlayerController>();
        collider = GetComponent<BoxCollider2D>();
        sprite = GetComponent<SpriteRenderer>();
    }

    [PunRPC]
    private void ChangeHPLocal(float amount)
    {
        currHP = Mathf.Clamp(currHP + amount, 0, maxHP);
        hpImage.fillAmount = currHP / maxHP;

        if (currHP == 0)
        {
            animator.SetTrigger("Dead");
            controller.enabled = false;
            RB.constraints = RigidbodyConstraints2D.FreezeAll;
            collider.enabled = false;           
        }

    }

    public void ChangeHPGlobal(float amount, int viewID)
    {
        if (photonView.ViewID == viewID)
        {
            ChangeHPLocal(amount);
        }

        if (PhotonNetwork.IsMasterClient)
        {
            photonView.RPC(nameof(ChangeHPLocal), RpcTarget.All, amount);
        }
    }

    public void SpriteDisable()
    {
        sprite.enabled = false;
        HPui.SetActive(false);
        NNtext.enabled = false;
    }
}
