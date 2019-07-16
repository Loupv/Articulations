using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class armExtension : MonoBehaviour
{

    public float lerpDuration = 1f;
    public float distanceToMove = 10;


    public enum lerpDirection { Forward, Backward, Up, Down, Left, Right };
    public lerpDirection _lerpDirection;
    private Vector3 localDir;
    private Vector3 origPosition;
    private Quaternion origRotation;

    private bool _isLerping;

    private Vector3 _startPosition;
    private Vector3 _endPosition;

    private float _timeStartedLerping;



    private Vector3 directionVector()
    {
        if (_lerpDirection == lerpDirection.Forward)
            localDir = Vector3.forward;
        else if (_lerpDirection == lerpDirection.Backward)
            localDir = -Vector3.back;
        else if (_lerpDirection == lerpDirection.Up)
            localDir = Vector3.up;
        else if (_lerpDirection == lerpDirection.Down)
            localDir = -Vector3.down;
        else if (_lerpDirection == lerpDirection.Left)
            localDir = -Vector3.left;
        else if (_lerpDirection == lerpDirection.Right)
            localDir = Vector3.right;
        Debug.Log(localDir);
        return localDir;
    }


    void StartLerping()
    {
        _isLerping = true;
        _timeStartedLerping = Time.time;

        _startPosition = transform.localPosition;   
        _endPosition = transform.localPosition + localDir * distanceToMove;
    }

    void Update()
    {
        if (Input.GetKeyUp(KeyCode.Space))
        {
            directionVector();
            StartLerping();
        }
    }


    void FixedUpdate()
    {
        if (_isLerping)
        {

            float timeSinceStarted = Time.time - _timeStartedLerping;
            float t = timeSinceStarted / lerpDuration;
            float percentageComplete = t * t * t * (t * (6f * t - 15f) + 10f);  // smoother curve, ease in and ease out 


            transform.localPosition = Vector3.Lerp(_startPosition, _endPosition, percentageComplete);

            if (percentageComplete >= 1.0f)
            {
                _isLerping = false;
            }
        }
    }
}
