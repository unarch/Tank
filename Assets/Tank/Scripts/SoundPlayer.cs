using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Events;
public class SoundPlayer : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        var x = 1;
        if (Input.GetKey(KeyCode.UpArrow))
        {
            print(x);
            Debug.Log("按住上键");
        }
        if (Input.GetKeyDown(KeyCode.C))
        {
            Debug.Log("按下了C");
        }
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
