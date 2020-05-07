using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UserManager : MonoBehaviour
{

    public List<UserData> usersPlaying;
    public UserData me;
    public Dictionary<string, Vector3> pendingPositionsActualizations;
    public Dictionary<string, Quaternion> pendingRotationsActualizations;
    public GameObject playerPrefab, viewerPrefab, trackerPrefab, playbackPrefab, TrailRendererPrefab, SparkParticlesPrefab, particles3, particles4;
    public ViewerController viewerController;

    public string viveSystemName = "[CameraRig]", 
    viveHeadName  = "Camera", 
    viveLeftHandName = "Controller (left)", 
    viveRightHandName = "Controller (right)";

    [HideInInspector]
    public GameObject _userGameObject;
    public GameObject barycenterPrefab;
    Barycenter barycenter;
    public bool keepNamesVisibleForPlayers;
    public Color userCol, whiteColor, cyanColor, playbackColor1, playbackColor2;


    // Arm Extension variables
    private float lerpDuration;
    private float distanceToMove;
    private bool _isLerping, _hasLerped;
    private Vector3[] _startPositionRightHand = new Vector3[8], _startPositionLeftHand = new Vector3[8];
    private Vector3[] _endPositionRightHand = new Vector3[8], _endPositionLeftHand = new Vector3[8];
    //private Vector3[] _initialPositionRightHand, _initialPositionLeftHand;
   // private List<Vector3> _startPositionRightHand = new List<Vector3>(), _startPositionLeftHand = new List<Vector3>();
  //  private List<Vector3> _endPositionRightHand = new List<Vector3>(), _endPositionLeftHand = new List<Vector3>();
    private List<Vector3> _initialPositionRightHand = new List<Vector3>(), _initialPositionLeftHand = new List<Vector3>();
    private float _timeStartedLerping;
    // end of arm extension variables
    public string trailsCondition;
    public float dist;
    bool trailsRelated;
    bool _areFar;
    bool _areClose;
    public bool playerNameVisible;
    List<ParticleSystem> trailSystems = new List<ParticleSystem>();
    //ParticleSystem[] trailSystemsR = new ParticleSystem[4];
    //ParticleSystem[] trailSystemsL = new ParticleSystem[4];



    // Start is called before the first frame update


    void Start()
    {
        pendingPositionsActualizations = new Dictionary<string, Vector3>();
        pendingRotationsActualizations = new Dictionary<string, Quaternion>();
        userCol = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));

        whiteColor = Color.white;
        cyanColor = Color.cyan;

        playbackColor1 = whiteColor;//new Color(0.8f, 0.8f, 0.8f);
        playbackColor2 = cyanColor;//new Color(0.6f, 0.6f, 0.6f);


        trailsCondition = null;
        trailsRelated = false;
    }


    public UserData InitLocalUser(GameEngine gameEngine, int ID, string name, string address, int localPort, bool isMe, UserRole userRole) {

        if (userRole == UserRole.Player) _userGameObject = Instantiate(playerPrefab);
        else if (userRole == UserRole.Tracker) _userGameObject = Instantiate(trackerPrefab);
        else if (userRole == UserRole.Viewer || userRole == UserRole.Server
        || (userRole == UserRole.Playback && gameEngine.playbackManager.mode == PlaybackMode.Offline)) // offline playback is considered as a normal viewer
            _userGameObject = Instantiate(viewerPrefab);
        else if (userRole == UserRole.Playback && gameEngine.playbackManager.mode == PlaybackMode.Online) {
            _userGameObject = Instantiate(playerPrefab);
            _userGameObject.AddComponent<ViewerController>();
        }

        if (userRole == UserRole.Viewer || userRole == UserRole.Tracker || userRole == UserRole.Server || userRole == UserRole.Playback) {
            viewerController = _userGameObject.GetComponent<ViewerController>();
            viewerController.InitViewerController(isMe);
        }

        UserData user = _userGameObject.GetComponent<UserData>();
    
        //int rank;

        //if(userRole == UserRole.Playback && gameEngine.playbackManager.mode == PlaybackMode.Offline) rank = -1;
        //else rank = CountPlayers();

        user.InitUserData(ID, name, address, localPort, _userGameObject, isMe, playerNameVisible, userRole, gameEngine.ViveSystemPrefab, gameEngine.useVRHeadset, 
            viveHeadName, viveLeftHandName, viveRightHandName);

        if (userRole != UserRole.Server)
        {
            usersPlaying.Add(user);
            if (userRole == UserRole.Player) StoreUserParts(user);
            foreach (GameObject model in GameObject.FindGameObjectsWithTag("SteamModel"))
            {
                model.SetActive(false);
            }
        }

        // print names above head
        if(gameEngine._userRole == UserRole.Player || gameEngine.gameData.showNamesAboveHead == 0) // mask UI for players when not wanted
            gameEngine.userManager.playerNameVisible = false;
        else gameEngine.userManager.playerNameVisible = true;

        me = user;

        return user;
    }


    public UserData AddNewUser(GameEngine gameEngine, int ID, string name, string address, int port, UserRole role, int rank)
    {
        GameObject go;

        if (role == UserRole.Player) go = Instantiate(playerPrefab);
        else if (role == UserRole.Tracker)
        {
            go = Instantiate(trackerPrefab);
            go.GetComponent<ViewerController>().InitViewerController(false);
        }
        else if (role == UserRole.Viewer)
        {
            go = Instantiate(viewerPrefab);
            go.GetComponent<ViewerController>().InitViewerController(false);
        }
        else if (role == UserRole.Playback)
            go = Instantiate(playbackPrefab);
        else go = new GameObject(); // useless

        UserData p = go.GetComponent<UserData>();

        p.InitUserData(ID, name, address, port, go, false, playerNameVisible, role, gameEngine.ViveSystemPrefab, gameEngine.useVRHeadset, 
            viveHeadName, viveLeftHandName, viveRightHandName);

        p._registeredRank = rank;
        if(p._registeredRank == 0) p.ChangeLayers(p.transform, "Player1");
        else if(p._registeredRank == 1) p.ChangeLayers(p.transform, "Player2");

        if (role == UserRole.Player) {
            ChangePlayerColor(p, whiteColor);
            StoreUserParts(p);
        }
        usersPlaying.Add(p);
        return p;
    }


    public int CountPlayers() {
        int i = 0, j = 0;
        foreach (UserData user in usersPlaying) {
            if (user._userRole == UserRole.Player || user._userRole == UserRole.Playback) i += 1;
            if (user._userRole == UserRole.Playback) j = 1;
        }
        return i - j;
    }


    public void ChangeVisualisationMode(string newVisualisationMode, GameEngine gameEngine, bool fade) {

            // for clients
            if (me._userRole == UserRole.Server)
                gameEngine.osc.sender.SendVisualisationChange(newVisualisationMode, usersPlaying);

            if(gameEngine.currentVisualisationMode == "2D") Camera.main.cullingMask = gameEngine.scenarioEvents.oldMirrorMask; // revert camera layers if last mode was 2D


        // main parameters
        // TODO CLEAN THIS PART
        Debug.Log("here " + newVisualisationMode);
            if (newVisualisationMode == "0") gameEngine.scenarioEvents.SetTimeOfDay(6);

            else if (newVisualisationMode == "9x") gameEngine.scenarioEvents.SetTimeOfDay(0);

            else gameEngine.scenarioEvents.SetTimeOfDay(8);


            if (_hasLerped && (newVisualisationMode != "1Ca" || newVisualisationMode != "1Cb" || newVisualisationMode != "1Cc")) ReverseArmsLerping(); // TODO corriger ici

            if((newVisualisationMode == "2B" || newVisualisationMode == "2C" || newVisualisationMode == "2D" || newVisualisationMode == "2E")){
                if(newVisualisationMode == "2D" && gameEngine._userRole == UserRole.Player) gameEngine.scenarioEvents.ToggleMirror(true, 1, ReturnMyRank()+1); // only the others reflection
                else if(newVisualisationMode == "2E" && gameEngine._userRole == UserRole.Player) gameEngine.scenarioEvents.ToggleMirror(true, 2, ReturnMyRank()+1); // only the others reflection
                else gameEngine.scenarioEvents.ToggleMirror(true, 0, 0); // normal mode 
            }
            else if (gameEngine.scenarioEvents.mirrorAct) gameEngine.scenarioEvents.ToggleMirror(false, 0, 0);
            

            if (newVisualisationMode != "2C")// && !(gameEngine._userRole == UserRole.Playback && gameEngine.playbackManager.mode == PlaybackMode.Offline)) // if we're in playback offline mode, we keep different colors
            {
                if (usersPlaying.Count > 1) ChangePlayerColor(usersPlaying[1], whiteColor);
                if (usersPlaying.Count > 2) ChangePlayerColor(usersPlaying[2], whiteColor);
            }
            
            else if (gameEngine._userRole == UserRole.Playback && gameEngine.playbackManager.mode == PlaybackMode.Offline) {
                if (usersPlaying.Count > 1) ChangePlayerColor(usersPlaying[1], playbackColor1);
                if (usersPlaying.Count > 2) ChangePlayerColor(usersPlaying[2], playbackColor2);
            }

            if (newVisualisationMode != "3B" && newVisualisationMode != "3Ca" && newVisualisationMode != "3Cb" && newVisualisationMode != "3Cc")
            {
                gameEngine.networkManager.sendToAudioDevice = false;
                trailsCondition = null;
                trailsRelated = false;
            }
            else gameEngine.networkManager.sendToAudioDevice = true;
            gameEngine.uiHandler.sendToAudioDeviceToggle.isOn = gameEngine.networkManager.sendToAudioDevice;

            if (newVisualisationMode == "3A" || newVisualisationMode == "3B" || newVisualisationMode == "3Ca" || newVisualisationMode == "3Cb" || newVisualisationMode == "3Cc")
                gameEngine.uiHandler.trailsDecaySlider.gameObject.SetActive(true);
            else
                gameEngine.uiHandler.trailsDecaySlider.gameObject.SetActive(false);

            if(newVisualisationMode == "9A"){
                if(barycenter == null) barycenter = GameObject.Instantiate(barycenterPrefab).GetComponent<Barycenter>();
                barycenter.Activate(0);
            }
            else if(newVisualisationMode == "9B"){
                if(barycenter == null) barycenter = GameObject.Instantiate(barycenterPrefab).GetComponent<Barycenter>();
                barycenter.Activate(1);
            }



            // per user parameters
            foreach (UserData user in usersPlaying) {

                if (user._userRole == UserRole.Player) {

                    if(gameEngine.currentVisualisationMode == "2C" && newVisualisationMode != "2C") ChangePlayerColor(user, whiteColor); // revert


                    if (newVisualisationMode == "0")  // basic condition
                    {
                        if (me._userRole == UserRole.Server || me._userRole == UserRole.Viewer) {
                            user.ChangeSkin(this, "all");
                        }
                        else {
                            GetInitialArmsPositions();  // this is needed once to reverse all arm extension that may occur

                            if (user._ID == me._ID)
                                user.ChangeSkin(this, "all");
                            else user.ChangeSkin(this, "nothing");
                        }
                    }

                    else if (newVisualisationMode == "1A") // every spheres visible
                    {
                        user.ChangeSkin(this, "all");
                    }

                    else if (newVisualisationMode == "1B") // other's hand visible, mine are not
                    {
                        if (user._ID == me._ID)
                            user.ChangeSkin(this, "noHands");
                        else user.ChangeSkin(this, "all");

                    }

                    else if (newVisualisationMode == "1C" || newVisualisationMode == "1Ca" || newVisualisationMode == "1Cb" || newVisualisationMode == "1Cc") // change arms length
                    {
                        Debug.Log("extension arms");
                        user.ChangeSkin(this, "all");
                        if (newVisualisationMode == "1Ca") {
                            Debug.Log("move arms");
                            distanceToMove = -0.2f;
                            lerpDuration = 3f;
                            StartArmsLerping();
                        }
                        else if (newVisualisationMode == "1Cb")
                        {
                            distanceToMove = 1f;
                            lerpDuration = 5f;
                            StartArmsLerping();
                        }
                        else if (newVisualisationMode == "1Cc")
                        {
                            distanceToMove = 4f;
                            lerpDuration = 10f;
                            StartArmsLerping();
                        }
                    }


                    else if (newVisualisationMode == "2A") { // every sphere visible
                        user.ChangeSkin(this, "all");
                    }
                    else if (newVisualisationMode == "2B" || newVisualisationMode == "2D") // mirror mode , side to side, same color
                    {
                        user.ChangeSkin(this, "all");
                    }
                    else if (newVisualisationMode == "2C") // mirror mode, different color
                    {
                        user.ChangeSkin(this, "all");
                        if (user._ID == me._ID)
                            ChangePlayerColor(user, whiteColor);
                        else ChangePlayerColor(user, cyanColor);
                        // different color
                    }


                    else if (newVisualisationMode == "3A") // trails individual
                    {

                        if (me._userRole == UserRole.Server || me._userRole == UserRole.Viewer)
                            user.ChangeSkin(this, "trails");
                        else
                        {
                            if (user._ID == me._ID)
                            {
                                user.ChangeSkin(this, "trails");

                            }
                            else user.ChangeSkin(this, "trails");
                        }
                       // trailsRelated = true;
                       // GetAllParticleSystems();
                        // DeActivateParticleVelocity();
                        trailsCondition = null;
                    }

                    else if (newVisualisationMode == "3B") // trails individual + sound
                    { // trails mode2
                        if (me._userRole == UserRole.Server || me._userRole == UserRole.Viewer)
                            user.ChangeSkin(this, "trails");
                        else
                        {
                            if (user._ID == me._ID)
                                user.ChangeSkin(this, "trails");
                            else user.ChangeSkin(this, "trails");
                        }
                      //  trailsRelated = true;
                       // GetAllParticleSystems();
                        // DeActivateParticleVelocity();
                        trailsCondition = "soloR";
                    }

                    else if (newVisualisationMode == "3Ca" || newVisualisationMode == "3Cb" || newVisualisationMode == "3Cc") // intersubject
                    { // trails mode2
                        Debug.Log("TO DO (sound)");
                        user.ChangeSkin(this, "trails");
                        trailsCondition = "relation";
                        trailsRelated = true;
                        GetAllParticleSystems();
                       // ActivateParticleVelocity();
                        _areFar = true;
                        _areClose = true;
                    }


                    // other modes
                    else if (newVisualisationMode == "4A") // trails mode3
                    {
                        if (user._ID == me._ID)
                            user.ChangeSkin(this, "noHands");
                        else user.ChangeSkin(this, "shortTrails");
                    }
                    else if (newVisualisationMode == "5A") // one player has left hand visible, other player has right hand visible
                    {
                        user.ChangeSkin(this, "onehand");
                    }
                    else {
                        Debug.Log("%% Wrong VisualisationMode Request ! %%");
                    }

                }
            }
            Debug.Log("Visualisation changed : " + newVisualisationMode);

            gameEngine.currentVisualisationMode = newVisualisationMode;

    }



    public void ChangeVisualisationParameter(int valueId, float value) {

        if (valueId == 0) {
            foreach (GameObject hand in GameObject.FindGameObjectsWithTag("HandParticleSystem")) {
                hand.GetComponent<TrailRenderer>().time = value;
            }
        }
    }


    public void ChangePlayerColor(UserData user, Color col)
    {
        user.head.GetComponent<MeshRenderer>().materials[0].color = col;
        user.leftHand.GetComponent<MeshRenderer>().materials[0].color = col;
        user.rightHand.GetComponent<MeshRenderer>().materials[0].color = col;
    }



    public void StoreUserParts(UserData user) {

        if (!pendingPositionsActualizations.ContainsKey(user._ID + "Head"))
        {
            pendingPositionsActualizations.Add(user._ID + "Head", user.head.transform.position);
            pendingPositionsActualizations.Add(user._ID + "LeftHand", user.leftHand.transform.position);
            pendingPositionsActualizations.Add(user._ID + "RightHand", user.rightHand.transform.position);
            pendingRotationsActualizations.Add(user._ID + "Head", user.head.transform.rotation);
            pendingRotationsActualizations.Add(user._ID + "LeftHand", user.leftHand.transform.rotation);
            pendingRotationsActualizations.Add(user._ID + "RightHand", user.rightHand.transform.rotation);
        }
    }



    // adjust players positions from stored one
    public void ActualizePlayersPositions(UserData me) {
        int i = 0;
        foreach (UserData user in usersPlaying)
        {
            if (user._ID != me._ID && user._userRole == UserRole.Player) // if it's not actual instance's player
            {

                usersPlaying[i].SetPlayerPosition(pendingPositionsActualizations[user._ID + "Head"],
                    pendingPositionsActualizations[user._ID + "LeftHand"],
                    pendingPositionsActualizations[user._ID + "RightHand"]);

                // in order to make calibration work for non vrUser, we need to differenciate between user with VR and user without
                /*
                if (me._userRole == UserRole.Server && vrUser){ 
                    usersPlaying[i].head.transform.position = pendingPositionsActualizations[user._ID + "Head"]  + usersPlaying[i].calibrationPositionGap;
                    usersPlaying[i].leftHand.transform.position = pendingPositionsActualizations[user._ID + "LeftHand"]  + usersPlaying[i].calibrationPositionGap ;
                    usersPlaying[i].rightHand.transform.position = pendingPositionsActualizations[user._ID + "RightHand"]  + usersPlaying[i].calibrationPositionGap ;
                }
                else {
                    usersPlaying[i].head.transform.position = pendingPositionsActualizations[user._ID + "Head"];
                    usersPlaying[i].leftHand.transform.position = pendingPositionsActualizations[user._ID + "LeftHand"];
                    usersPlaying[i].rightHand.transform.position = pendingPositionsActualizations[user._ID + "RightHand"];
                }*/

                usersPlaying[i].head.transform.rotation = pendingRotationsActualizations[user._ID + "Head"];
                usersPlaying[i].leftHand.transform.rotation = pendingRotationsActualizations[user._ID + "LeftHand"];
                usersPlaying[i].rightHand.transform.rotation = pendingRotationsActualizations[user._ID + "RightHand"];
            }
            i++;
        }
    }



    public int ReturnPlayerRank(int n) { // n may be equal to 1 or 2 (player1 or 2) 
        int r = 0;
        int i = 0, j = 0;
        foreach (UserData user in usersPlaying) {
            if (user._userRole == UserRole.Player || (user._userRole == UserRole.Playback)) r += 1; // if we find a player thats number r
            if (user._userRole == UserRole.Playback) j = 1;
            if (r == n) return i + j; // if r was needed, return it
            i++;
        }
        Debug.Log("Player not found");
        return -1;
    }

    public int ReturnMyRank(){
        foreach (UserData user in usersPlaying) {
            if(user._isMe) return user._registeredRank;
        }
        return 0;
    }

    // server's reaction to clienthasleft message
    public void ErasePlayer(int playerID)
    {
        pendingPositionsActualizations.Remove(playerID + "Head");
        pendingPositionsActualizations.Remove(playerID + "LeftHand");
        pendingPositionsActualizations.Remove(playerID + "RightHand");
        pendingRotationsActualizations.Remove(playerID + "Head");
        pendingRotationsActualizations.Remove(playerID + "LeftHand");
        pendingRotationsActualizations.Remove(playerID + "RightHand");

        foreach (UserData p in usersPlaying)
        {
            if (p._ID == playerID)
            {
                Destroy(p.gameObject);
                usersPlaying.Remove(p);
                Destroy(p);
                break;
            }
        }
    }

    public void EraseAllPlayers() {
        /*foreach (UserData p in usersPlaying)
        {
            pendingPositionsActualizations.Remove(p._ID + "Head");
            pendingPositionsActualizations.Remove(p._ID + "LeftHand");
            pendingPositionsActualizations.Remove(p._ID + "RightHand");
            pendingRotationsActualizations.Remove(p._ID + "Head");
            pendingRotationsActualizations.Remove(p._ID + "LeftHand");
            pendingRotationsActualizations.Remove(p._ID + "RightHand");
            Destroy(p.gameObject);
            Destroy(p);
        }*/
        Debug.Log("Erasing All players");
        pendingPositionsActualizations.Clear();
        pendingPositionsActualizations = new Dictionary<string, Vector3>();
        Debug.Log(pendingPositionsActualizations.Count);
        pendingRotationsActualizations.Clear();
        pendingRotationsActualizations = new Dictionary<string, Quaternion>();
        usersPlaying.Clear();
        foreach (GameObject go in GameObject.FindGameObjectsWithTag("Viewer"))
        {
            Destroy(go);
        }
        foreach (GameObject go in GameObject.FindGameObjectsWithTag("Player"))
        {
            Destroy(go);
        }
    }


    public void TranslateUser() {
        me.gameObject.transform.position += me.calibrationPositionGap;
    }


    public Vector3 GetCalibrationGap(int playerID) {
        foreach (UserData user in usersPlaying) {
            if (user._ID == playerID) return user.calibrationPositionGap;
        }
        return new Vector3();
    }

    // arm extension and particle trail functions

    void GetInitialArmsPositions(){

        // _initialPositionRightHand.Clear();
        // _initialPositionLeftHand.Clear();

        // int i = 0;
        //  if (_initialPositionRightHand == null || _initialPositionLeftHand == null)
        if (_initialPositionRightHand.Count == 0 || _initialPositionLeftHand.Count == 0)
        {
            // _initialPositionRightHand = new Vector3[8];
            //  _initialPositionLeftHand = new Vector3[8];
            foreach (UserData user in usersPlaying) // store initial local position 
            {
                if (user._userRole == UserRole.Player)
                {
                    if (user._ID == me._ID)
                    {
                       // _initialPositionRightHand[i] = user.rightHand.transform.localPosition;
                      //  _initialPositionLeftHand[i] = user.leftHand.transform.localPosition;
                _initialPositionRightHand.Add(user.rightHand.transform.localPosition);
                _initialPositionLeftHand.Add(user.leftHand.transform.localPosition);
                Debug.Log("initial list filled with " + _initialPositionLeftHand.Count);
               // i++;
                    }
                }
            }
        }
    }

    void StartArmsLerping()
    {
        int i = 0;
        _isLerping = true;
        _timeStartedLerping = Time.time;
        foreach (UserData user in usersPlaying)
        {
            if (user._userRole == UserRole.Player)
            {
                if (user._ID == me._ID)
                {
                    _startPositionRightHand[i] = user.rightHand.transform.localPosition;
                    _endPositionRightHand[i] = user.rightHand.transform.localPosition + (Vector3.up * distanceToMove) - user.rightHand.transform.localPosition;
                    _startPositionLeftHand[i] = user.leftHand.transform.localPosition;
                    _endPositionLeftHand[i] = user.leftHand.transform.localPosition + (Vector3.up * distanceToMove) - user.rightHand.transform.localPosition;
                   i++;
                }
            }
        }
        _hasLerped = true;
    }


    void ReverseArmsLerping(){

        _isLerping = true;
        _timeStartedLerping = Time.time;
        int i = 0;
        foreach (UserData user in usersPlaying)
        {
            if (user._userRole == UserRole.Player)
            {
                if (user._ID == me._ID)
                {
                    _startPositionRightHand[i] = user.rightHand.transform.localPosition;
                    _endPositionRightHand[i] = _initialPositionRightHand[i];
                    _startPositionLeftHand[i] = user.leftHand.transform.localPosition;
                    _endPositionLeftHand[i] = _initialPositionLeftHand[i];
                    i++;
                }
            }
        }
        _hasLerped = false;

    }


    void FixedUpdate()
    {
        // arms extension
        if (_isLerping)
        {

            float timeSinceStarted = Time.time - _timeStartedLerping;
            float t = timeSinceStarted / lerpDuration;
            float percentageComplete = t * t * t * (t * (6f * t - 15f) + 10f);  // smoother curve, ease in and ease out 

            int i = 0;
            foreach (UserData user in usersPlaying)
            {
                if (user._userRole == UserRole.Player)
                {
                    if (user._ID == me._ID)
                    {
                        user.rightHand.transform.localPosition = Vector3.Lerp(_startPositionRightHand[i], _endPositionRightHand[i], percentageComplete);
                        user.leftHand.transform.localPosition = Vector3.Lerp(_startPositionLeftHand[i], _endPositionLeftHand[i], percentageComplete);
                        i++;
                    }
                }
            }

            if (percentageComplete >= 1.0f)
            {
                _isLerping = false;
            }
        }
        // end of arms extension
        if (Input.GetKeyUp(KeyCode.P))
            ActivateParticleVelocity();
        // trails proximity
        if (trailsCondition == "relation")
        {

            int i = 0;
            foreach (UserData user in usersPlaying)
            {
                if (user._userRole == UserRole.Player)
                {
                    if  (usersPlaying.Count > 1)
                    {                        
                        dist = Vector3.Distance(usersPlaying[i].head.transform.position, usersPlaying[i + 1].head.transform.position);
                        /*
                        if (dist > 0.5f)
                        {
                            if (_areClose == true)
                            {
                                _areFar = false;
                                _areClose = false;
                            }
                            if (_areFar == false)
                            {
                                //ActivateParticleVelocity();
                                _areFar = true;
                            }
                        }
                        else if (dist < 0.5f)
                        {

                            if (_areClose == false)
                            {
                               // DeActivateParticleVelocity();
                                _areClose = true;
                            }
                            float lfTime = dist.Remap(0.5f, 0f, 3f, 12f);
                            SetParticleLifetime(lfTime);
                        }
                        */
                    }
                }
            }
        }
    }


    void GetAllParticleSystems()
    {
        if (trailsRelated == true)
        {
            if (trailSystems.Count == 0)
            {
                //trailSystems.Clear();
                // int i = 0;
                foreach (UserData user in usersPlaying)
                {
                    if (user._userRole == UserRole.Player)
                    {
                        if (user._ID == me._ID)
                        {
                            //trailSystemsR[i] = user.rightHand.GetComponentInChildren<ParticleSystem>();
                            // trailSystemsL[i] = user.leftHand.GetComponentInChildren<ParticleSystem>();
                            trailSystems.Add(user.rightHand.GetComponentInChildren<ParticleSystem>());
                            //trailSystems[i].startLifetime = 100f;
                            //var veloModule = trailSystems[i].inheritVelocity;
                            //veloModule.enabled = true;
                           //  i++;
                            trailSystems.Add(user.leftHand.GetComponentInChildren<ParticleSystem>());
                            //var veloModule = trailSystems[i].inheritVelocity;
                            //veloModule.enabled = true;
                           // trailSystems[i].startLifetime = 50f;
                          //  i++;
                        }
                    }
                }
                Debug.Log("got that many particle systems: " + trailSystems.Count);
                Debug.Log(trailSystems.GetType());
                // Debug.Log("got that many particle systems: " + trailSystemsR.Length);
                //      trailsRelated = false;
            }
        }
    }

    void ActivateParticleVelocity()
    {
                    foreach (ParticleSystem p in trailSystems)
                    {
                        var velomoduler = p.inheritVelocity;
                        velomoduler.enabled = true;
            p.startLifetime = 100f;
                    }
    }

    //void DeActivateParticleVelocity()
    //{
    //    int i = 0;
    //    foreach (ParticleSystem p in trailSystemsR)
    //    {
    //        var veloModuleR = trailSystemsR[i].inheritVelocity;
    //        veloModuleR.enabled = false;
    //        var veloModuleL = trailSystemsL[i].inheritVelocity;
    //        veloModuleL.enabled = false;
    //        i++;
    //    }
    //}

    //void SetParticleLifetime(float lf)
    //{
    //    int i = 0;
    //    foreach (ParticleSystem p in trailSystemsR)
    //    {
    //        trailSystemsR[i].startLifetime = lf;
    //        trailSystemsL[i].startLifetime = lf;
    //        i++;
    //    }
    //}

    // end of arm extension and particle trail functions



}
