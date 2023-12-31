using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;

public class consciousness : MonoBehaviour
{
    public float minForce = 10f;
    public float maxForce = 100f;
    private Color minColor = Color.green;
    private Color maxColor = Color.red;
    public GameManager gmanager;


    private float currentForce = 0f;
    private bool isApplyingForce = false;
    private bool canJump = false;

    private Rigidbody2D rb;

    public GameObject arrow;
    private SpriteRenderer arrowSpriteRenderer;
    private bool forceChangeDirection = true;
    private CameraFollow cameraFollow;
    public GameObject m_camera;
    public Vector3 lastCheckPoint;

    private AudioSource audioSource;
    public AudioClip tpSFX;
    public AudioClip landingSFX;
    public AudioClip nextLevelSFX;
    

    private void Start()
    {
        audioSource=GetComponent<AudioSource>();
        cameraFollow= m_camera.GetComponent<CameraFollow>();
        rb = GetComponent<Rigidbody2D>();
        arrowSpriteRenderer=arrow.GetComponent<SpriteRenderer>();
        arrowSpriteRenderer.enabled = false;
    }

    private void Update()
    {
        ColorChange();
        jump();
    }
    public void ColorChange()
    {
        float t = Mathf.InverseLerp(minForce, maxForce, currentForce);
        arrowSpriteRenderer.color = Color.Lerp(minColor, maxColor, t);
    }
    public void jump()
    {

        if (Input.GetKeyDown(KeyCode.Space) && canJump == true)
        {
            currentForce = minForce;
            isApplyingForce = true;
            arrowSpriteRenderer.enabled = true;
        }

        if (Input.GetKeyUp(KeyCode.Space) && canJump == true)
        {
            canJump = false;
            // Debug.Log ile son kuvveti g�r�nt�le
            Debug.Log("Son Kuvvet: " + currentForce);

            // Parent objesinin pozisyonundan kendi pozisyonuna do�ru bir vekt�rle kuvvet uygula
            Vector3 parentTransformPosition = transform.parent.position;
            Vector2 rbPosition = rb.position;
            // Child objeyi parent'tan ay�r
            transform.parent = null;

            Vector2 forceDirection = (rbPosition - (Vector2)parentTransformPosition).normalized;
            rb.AddForce(forceDirection * currentForce * 10);

            isApplyingForce = false;
            arrowSpriteRenderer.enabled = false;
        }

        if (isApplyingForce)
        {
            // Kuvvet de�i�kenini art�r
            Debug.Log("Su anki Kuvvet: " + currentForce);

            arrow.transform.localScale = Vector3.one * currentForce / 50;

            if (forceChangeDirection == true)
            {
                currentForce = Mathf.Clamp(currentForce + Time.deltaTime * 40f, minForce, maxForce);
                if (currentForce< maxForce)
                {
                    arrow.transform.localPosition = new Vector3(arrow.transform.localPosition.x, arrow.transform.localPosition.y + currentForce / 7000, arrow.transform.localPosition.z);

                }

            }
            else if (forceChangeDirection == false)
            {
                currentForce = Mathf.Max(currentForce - Time.deltaTime * 40f, minForce);
                if (currentForce < minForce)
                {
                    arrow.transform.localPosition = new Vector3(arrow.transform.localPosition.x, arrow.transform.localPosition.y - currentForce / 7000, arrow.transform.localPosition.z);

                }

            }
            /*if (currentForce >= maxForce)
            {
                forceChangeDirection = false;
            }
            if (currentForce <= minForce)
            {
                forceChangeDirection = true;
            }*/


        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Obstacles"))
        {
            transform.position = lastCheckPoint;
            arrow.transform.localPosition = new Vector3(0, 0.8f, 0);
        }
        else if (collision.gameObject.CompareTag("WormHole"))
        {
            arrow.transform.localPosition = new Vector3(0, 0.8f, 0);
            transform.position = collision.gameObject.transform.parent.position;
        }
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {

        if (collision.gameObject.CompareTag("Planet"))
        {
            audioSource.PlayOneShot(landingSFX);
            arrow.transform.localPosition = new Vector3(0, 0.8f, 0);
            arrow.transform.localPosition = new Vector3(0, 0.8f, 0);
            transform.parent= collision.transform;

            canJump = true;
            rb.velocity = Vector3.zero;
            Vector3 parentTransformPosition = transform.parent.position;
            Vector2 rbPosition = rb.position;

            Vector2 arrowDirection = (rbPosition - (Vector2)parentTransformPosition).normalized;
            Debug.Log(arrowDirection.x + ", " + arrowDirection.y);
            //arrow.transform.Rotate(0,0,arrowDirection);

            float angle = Mathf.Atan2(arrowDirection.y, arrowDirection.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, 0f, angle-90f);
        }
        if (collision.gameObject.CompareTag("Checkpoint"))
        {
            
            arrow.transform.localPosition = new Vector3(0, 0.8f, 0);
            transform.parent = collision.transform;

            canJump = true;
            rb.velocity = Vector3.zero;
            Vector3 parentTransformPosition = transform.parent.position;
            Vector2 rbPosition = rb.position;

            Vector2 arrowDirection = (rbPosition - (Vector2)parentTransformPosition).normalized;
            Debug.Log(arrowDirection.x + ", " + arrowDirection.y);
            //arrow.transform.Rotate(0,0,arrowDirection);

            float angle = Mathf.Atan2(arrowDirection.y, arrowDirection.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, 0f, angle - 90f);
            lastCheckPoint = gameObject.transform.position;
            //PlanetGravity planetGravity = collision.gameObject.GetComponent<PlanetGravity>();
            if (collision.gameObject.GetComponent<PlanetGravity>().cameraDirection==1)
            {
                collision.gameObject.GetComponent<PlanetGravity>().cameraDirection = 0;
                cameraFollow.HareketEt(new Vector3(m_camera.transform.position.x+16, m_camera.transform.position.y, m_camera.transform.position.z));

                audioSource.PlayOneShot(nextLevelSFX);
                ProgressBar.stage++;
            }
            else if(collision.gameObject.GetComponent<PlanetGravity>().cameraDirection == 2)
            {
                collision.gameObject.GetComponent<PlanetGravity>().cameraDirection = 0;
                cameraFollow.HareketEt(new Vector3(m_camera.transform.position.x, m_camera.transform.position.y-9, m_camera.transform.position.z));
                audioSource.PlayOneShot(nextLevelSFX);
                ProgressBar.stage++;
            }
            else if (collision.gameObject.GetComponent<PlanetGravity>().cameraDirection == 3)
            {
                collision.gameObject.GetComponent<PlanetGravity>().cameraDirection = 0;
                cameraFollow.HareketEt(new Vector3(m_camera.transform.position.x-16, m_camera.transform.position.y, m_camera.transform.position.z));
                audioSource.PlayOneShot(nextLevelSFX);
                ProgressBar.stage++;
            }
            Debug.Log("progress stage: " + ProgressBar.stage);

        }
        else if (collision.gameObject.CompareTag("WormHole"))
        {
            arrow.transform.localPosition = new Vector3(0, 0.8f, 0);
            transform.position = collision.gameObject.transform.parent.position;
            audioSource.PlayOneShot(tpSFX);
        }
        else if (collision.gameObject.CompareTag("Obstacles") || collision.gameObject.CompareTag("BlackHole"))
        {
            transform.position = lastCheckPoint;
            arrow.transform.localPosition = new Vector3(0, 0.8f, 0);
        }
        else if (collision.gameObject.CompareTag("Yatak"))
        {
            gmanager.Win();
        }
    }
}
