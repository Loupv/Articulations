using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ViewerController : MonoBehaviour
{

    Camera camera;

	public float turnSpeed = 4.0f;		// Speed of camera turning when mouse moves in along an axis
	public float panSpeed = 4.0f;		// Speed of the camera when being panned
	public float zoomSpeed = 4.0f;		// Speed of the camera going back and forth
	public float translateSpeed = 0.3f;
	private Vector3 mouseOrigin;	// Position of cursor when mouse dragging starts
	private bool isPanning;		// Is the camera being panned?
	private bool isRotating;	// Is the camera being rotated?
	private bool isZooming;		// Is the camera zooming?
	Transform initialTransform;
	public bool isFollowing1, isFollowing2;
	public int followingPlayerID;
	private GameEngine gameEngine;
	private UIHandler UIHandler;

	
    // Start is called before the first frame update
    void Start()
    {
		gameEngine = GameObject.FindGameObjectWithTag("GameEngine").GetComponent<GameEngine>();
        camera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
		UIHandler = GameObject.FindGameObjectWithTag("UIHandler").GetComponent<UIHandler>();
		this.transform.position = camera.transform.position;
		this.transform.rotation = camera.transform.rotation;
        camera.transform.parent = this.transform; 
		//camera.transform.position = Vector3.zero;
		initialTransform = this.transform;
    }
	
	void Update () 
	{
		if((isFollowing1 || isFollowing2) && followingPlayerID != -1){
            this.transform.position = gameEngine.usersPlaying[followingPlayerID].head.transform.TransformPoint(gameEngine.usersPlaying[followingPlayerID].head.transform.position);
            this.transform.rotation = gameEngine.usersPlaying[followingPlayerID].head.transform.rotation;
		}	

		else{ // freecam
			if(Input.GetKey("up")){
				transform.Translate(new Vector3(0,0,1)*translateSpeed);
			}

			if(Input.GetKey("down")){
				transform.Translate(new Vector3(0,0,-1)*translateSpeed);
			}

			if(Input.GetKey("left")){
				transform.Translate(new Vector3(-1,0,0)*translateSpeed);
			}

			if(Input.GetKey("right")){
				transform.Translate(new Vector3(1,0,0)*translateSpeed);
			}


			// Get the left mouse button
			if(Input.GetMouseButtonDown(0))
			{
				// Get mouse origin
				mouseOrigin = Input.mousePosition;
				isRotating = true;
			}
			
			// Get the right mouse button
			if(Input.GetMouseButtonDown(1))
			{
				// Get mouse origin
				mouseOrigin = Input.mousePosition;
				isPanning = true;
			}
			
			// Get the middle mouse button
			if(Input.GetMouseButtonDown(2))
			{
				// Get mouse origin
				mouseOrigin = Input.mousePosition;
				isZooming = true;
			}
			
			// Disable movements on button release
			if (!Input.GetMouseButton(0)) isRotating=false;
			if (!Input.GetMouseButton(1)) isPanning=false;
			if (!Input.GetMouseButton(2)) isZooming=false;
			
			// Rotate camera along X and Y axis
			if (isRotating)
			{
					Vector3 pos = Camera.main.ScreenToViewportPoint(Input.mousePosition - mouseOrigin);

				transform.RotateAround(transform.position, transform.right, -pos.y * turnSpeed);
				transform.RotateAround(transform.position, Vector3.up, pos.x * turnSpeed);
			}
			
			// Move the camera on it's XY plane
			if (isPanning)
			{
					Vector3 pos = Camera.main.ScreenToViewportPoint(Input.mousePosition - mouseOrigin);

					//Vector3 move = new Vector3(pos.x * panSpeed, pos.y * panSpeed, 0);
					Vector3 move = new Vector3(0, pos.y * panSpeed, 0);
					transform.Translate(move, Space.Self);
			}
			
			// Move the camera linearly along Z axis
			if (isZooming)
			{
					Vector3 pos = Camera.main.ScreenToViewportPoint(Input.mousePosition - mouseOrigin);

					Vector3 move = pos.y * zoomSpeed * transform.forward; 
					transform.Translate(move, Space.World);
			}
		}
	}

	public void UpdatePOV(int pov, int id){

		switch(pov){

		case 0: // free mode
		isFollowing1 = false;
		isFollowing2 = false;
		
		break;

		case 1:
		
			isFollowing1 = true;
			isFollowing2 = false;
			followingPlayerID = id;
			
		
		break;

		case 2:
		
			isFollowing1 = false;
			isFollowing2 = true;
			followingPlayerID = id;
			
		
		break;

		case 3:
		isFollowing1 = false;
		isFollowing2 = false;
		this.transform.position = gameEngine.POVs[0].transform.position;
		this.transform.rotation = gameEngine.POVs[0].transform.rotation;
		
		break;


		}
	}
}



