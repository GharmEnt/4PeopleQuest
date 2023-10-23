using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Grenade : MonoBehaviour
{
    [SerializeField] int damage;
    [SerializeField] float lifeTime;

    PhotonView photonView;

    private void Start()
    {
        photonView = GetComponent<PhotonView>();
        photonView.RPC("DestroyGrenade", RpcTarget.All, lifeTime);
    }

    [PunRPC]

    private void OnTriggerEnter2D(Collider2D collision)
    {
        HP hp = collision.GetComponent<HP>();
        
        if (hp != null)
        {
            int viewID = hp.gameObject.GetComponent<PhotonView>().ViewID;
            hp.ChangeHPGlobal(damage, viewID);
        }

        photonView.RPC("DestroyGrenade", RpcTarget.All, lifeTime);
    }

    [PunRPC]
    private void DestroyGrenade(float lifeTime)
    {
        Destroy(gameObject, lifeTime);
    }

}
