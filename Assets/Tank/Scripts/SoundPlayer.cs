using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class SoundPlayer : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Play()
    {
        AudioSource audio = GetComponent<AudioSource>();
        audio.Play();
    }

    public void Stop() {
        AudioSource audio = GetComponent<AudioSource>();
        audio.Stop();
    }

    public void ChangeScene() {
        SceneManager.LoadScene("b", LoadSceneMode.Additive);
    }
}
