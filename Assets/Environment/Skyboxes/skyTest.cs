using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class sky_Test : MonoBehaviour
{
    Material skyboxes;
    float seedPing;
    // Start is called before the first frame update
    void Start()
    {
        skyboxes = RenderSettings.skybox;
    }

    // animate the game object from -1 to +1 and back
    float minimum = -.3F;
    float maximum = .3F;

    // starting value for the Lerp
    static float t = 0.0f;

    void Update()
    {
        // animate the position of the game object...
        skyboxes.SetFloat("_Seed", Mathf.Lerp(minimum, maximum, t));
        //transform.position = new Vector3(Mathf.Lerp(minimum, maximum, t), 0, 0);

        // .. and increase the t interpolater
        t += 0.005f * Time.deltaTime;

        // now check if the interpolator has reached 1.0
        // and swap maximum and minimum so game object moves
        // in the opposite direction.
        if (t > 1.0f)
        {
            float temp = maximum;
            maximum = minimum;
            minimum = temp;
            t = 0.0f;
        }
    }
}
