using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundInstructionPlayer : MonoBehaviour
{

    public AudioSource[] instructions;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void PlayInstructions(int i)
    {
        if(instructions != null && instructions[i] != null)
            instructions[i].Play();


    }

}
