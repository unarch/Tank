using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 100f;
    public GameObject explode;
    public float maxLiftTime = 2f;
    public float instantiateTime = 0f;

    public GameObject attackTank;

    public AudioClip explodeClip;


    private void Start()
    {
        instantiateTime = Time.time;
    }

    private void Update()
    {
        //   前进
        transform.position += -1 * transform.up * speed * Time.deltaTime;
        // 摧毁
        if (Time.time - instantiateTime > maxLiftTime)
            Destroy(gameObject);

    }

    private float GetAttack()
    {
        float att = 100 - (Time.time - instantiateTime) * 40;
        if (att < 1)
            att = 1;
        return att;
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject == attackTank) return;
        var exploderI = Instantiate(explode, transform.position, transform.rotation);
        AudioSource audioSource = exploderI.AddComponent<AudioSource>();
        audioSource.spatialBlend = 1;
        audioSource.PlayOneShot(explodeClip);
        Destroy(exploderI, 3);
        Destroy(gameObject);
        
        //击中坦克
        Tank tank = other.gameObject.GetComponent<Tank>();
        if (tank != null) 
        {
            float att = GetAttack();
            tank.BeAttacked(att, attackTank);
        }
    }
}
