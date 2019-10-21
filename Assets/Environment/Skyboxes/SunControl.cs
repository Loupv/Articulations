using UnityEngine;
using System.Collections;

public class SunControl : MonoBehaviour {

	//Range for min/max values of variable
	[Range(-10f, 10f)]
	public float sunRotationSpeed_x, sunRotationSpeed_y;

    float minimum;
    float maximum;
    float duration = 10.0f;
    float t;
    float startTime;

    private void Start()
    {
        StartCoroutine(sunsetSky());
    }


    // Sun Movement
    IEnumerator sunsetSky() {
        while (true)
        {
            // sun goes up....
            minimum = -4.5f;
            maximum = 6.0f;

            startTime = Time.time;
            t = 0.0f;

            while (t <= 1.0f)
            {
                t = (Time.time - startTime) / duration;
                transform.eulerAngles = new Vector3(Mathf.SmoothStep(minimum, maximum, t), transform.eulerAngles.y, 0);
                yield return new WaitForEndOfFrame();
            }

            // sun goes down...
            yield return new WaitForSeconds(5f);
            startTime = Time.time;
            t = 0.0f;
            while (t <= 1.0f)
            {
                t = (Time.time - startTime) / duration;
                transform.eulerAngles = new Vector3(Mathf.SmoothStep(maximum, minimum, t), transform.eulerAngles.y, 0);
                yield return new WaitForEndOfFrame();
            }

            // sun rotates randomly...
            yield return new WaitForSeconds(2f);
            startTime = Time.time;
            t = 0.0f;
            minimum = transform.eulerAngles.y;
            maximum = 360f * Random.Range(-1.5f, 1.5f);
            while (t <= 1.0f)
            {
                t = (Time.time - startTime) / duration;
                transform.eulerAngles = new Vector3(transform.eulerAngles.x, Mathf.SmoothStep(minimum, maximum, t), 0);
                yield return new WaitForEndOfFrame();
            }
            yield return new WaitForSeconds(2f);

        }
    }

    //void Update()
    //{
    //    gameObject.transform.Rotate(sunRotationSpeed_x * Time.deltaTime, sunRotationSpeed_y * Time.deltaTime, 0);
    //    //gameObject.transform.Rotate(Mathf.PingPong(sunRotationSpeed_x * Time.deltaTime, 10f), sunRotationSpeed_y * Time.deltaTime, 0);

    //    // Calculate the fraction of the total duration that has passed.
    //    t = (Time.time - startTime) / duration;
    //    transform.eulerAngles = new Vector3(Mathf.SmoothStep(minimum, maximum, t), 0, 0);
    //    //transform.eulerAngles = new Vector3(Mathf.SmoothStep(maximum, minimum, t), 0, 0);


    //    // t = (Time.time - startTime) / duration;
    //    // transform.eulerAngles = new Vector3(0, Mathf.SmoothStep(minimum, maximum, t), 0);
    //}
}
