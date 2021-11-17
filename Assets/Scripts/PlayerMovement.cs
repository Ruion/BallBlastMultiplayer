using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Photon.Pun;

//Althought this script is name PlayerMovement, Shooting is also described here.
//Sorry for the slight confusion.
public class PlayerMovement : MonoBehaviour, IPunObservable
{
    private Rigidbody rigid; 
	private Vector3 moveDirection = Vector3.zero;
    private bool canShoot = true;
    public float speed;
    public float angleOffset;
    private float shootDelay;
    private IEnumerator coroutine;
    private bool isFiring = false;

    private PhotonView photonView;

    void Start()
    {
        rigid = GetComponent<Rigidbody>();
        shootDelay = Stats.sharedInstance.shootDelay;

        photonView = GetComponent<PhotonView>();
    }

    void FixedUpdate()
    {
        if (!photonView.IsMine && PhotonNetwork.IsConnected)
            return;

        //Get inputs for movement from the Horizontal axis. Move with velocity for smoother movement.
		Vector3 velocity = (transform.right * Input.GetAxis("Horizontal")) * speed * Time.fixedDeltaTime;
		rigid.velocity = velocity;

        //Shoot if there is no shoot delay coroutine in progress.
        if (Input.GetMouseButton(0)) {
            isFiring = true;
            if (canShoot) {
                Shoot();
                coroutine = WaitToShoot(shootDelay);
                StartCoroutine(coroutine);
            }

            canShoot = false;
        }
        if (Input.GetMouseButtonUp(0))
                isFiring =false;
    }

    private void CheckIsFiring()
    {
        if (isFiring)
        {
            if (canShoot)
            {
                Shoot();
                coroutine = WaitToShoot(shootDelay);
                StartCoroutine(coroutine);
            }

            canShoot = false;
        }
    }

    void OnCollisionEnter(Collision collision) 
    {
        if(collision.gameObject.tag == "Enemy")
            LevelProgress.sharedInstance.DisplayLoseScreen();
    }

    void Shoot() {
        //Get the bullets  from the object pool and activate it
        //above the player.
        List<GameObject> bullets = new List<GameObject>();
        int bulletCount = Stats.sharedInstance.bulletCount;
        Vector3 abovePlayer = transform.position + (Vector3.up / 2f);

        for(int i = 0; i < bulletCount; i++) 
        {
            GameObject bullet = ObjectPooler.sharedInstance.GetPooledObject("Ammo");
            // bullet.SetActive(true);
            bullets.Add(bullet);
            bullets[i].SetActive(true);
        }

        //If there is only one bullet count (1st level) just place it above the player.
        if(bulletCount == 1) 
        {
            bullets[0].transform.position = abovePlayer;
            bullets[0].SetActive(true);
            return;
        }

        //The following logic is for spacing the bullets above the player
        //in an ordinarily way. 
        int midpoint = bulletCount / 2;
        float gutter = Stats.sharedInstance.bulletWidth / (float) bulletCount;
        float start = midpoint * gutter;
        int index = midpoint;

        for(int i = 0; i < midpoint; i++) 
        {
            bullets[i].transform.position = abovePlayer;
            bullets[i].transform.localEulerAngles = new Vector3(0, 0, 360f - angleOffset) * index;
            index--;
        }

        //If bullet count is odd, place one bullet right above the player.
        //Else continue spacing them.
        index = (bulletCount % 2 == 0 ? 1 : 0);

        for(int i = midpoint; i < bulletCount; i++) 
        {
            bullets[i].transform.position = abovePlayer;
            bullets[i].transform.localEulerAngles = new Vector3(0, 0, angleOffset) * index;
            index++;
        }

        foreach (GameObject bullet in bullets)
            bullet.GetComponent<BulletMovement>().SetSpeed();
    }

    private IEnumerator WaitToShoot(float waitTime)
    {
        //The delay before next shot is allowed.
        yield return new WaitForSeconds(waitTime);
        canShoot = true;
    }

    #region IPunObservable implementation


    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            // We own this player: send the others our data
            stream.SendNext(isFiring);
        }
        else
        {
            // Network player, receive data
            isFiring = (bool)stream.ReceiveNext();
            CheckIsFiring();
        }
    }


    #endregion
}
