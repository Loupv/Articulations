using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UserManager : MonoBehaviour
{

    public List<UserData> usersPlaying;
    public UserData me;
    public Dictionary<string, Vector3> pendingPositionsActualizations;
    public Dictionary<string, Quaternion> pendingRotationsActualizations;
    public GameObject playerPrefab, viewerPrefab, trackerPrefab, TrailRendererPrefab, SparkParticlesPrefab, particles3, particles4;


    [HideInInspector]
    public GameObject _userGameObject;
    public bool keepNamesVisibleForPlayers;
    public Color userCol, whiteColor, cyanColor;


    // Arm Extension variables
    private float lerpDuration;
    private float distanceToMove;
    private bool _isLerping, _hasLerped;
    private Vector3 [] _startPositionRightHand = new Vector3[8], _startPositionLeftHand = new Vector3[8];
    private Vector3 [] _endPositionRightHand = new Vector3[8], _endPositionLeftHand = new Vector3[8];
    private Vector3[] _initialPositionRightHand, _initialPositionLeftHand;
    private float _timeStartedLerping;
    // end of arm extension variables
    public string trailsCondition;
    public float dist;
    bool trailsRelated;
    bool _areFar;
    bool _areClose;
    ParticleSystem[] trailSystemsR = new ParticleSystem[4];
    ParticleSystem[] trailSystemsL = new ParticleSystem[4];



    // Start is called before the first frame update


    void Start()
    {
        pendingPositionsActualizations = new Dictionary<string, Vector3>();
        pendingRotationsActualizations = new Dictionary<string, Quaternion>();
        userCol = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));

        whiteColor = Color.white;
        cyanColor = Color.cyan;

        trailsCondition = null;
        trailsRelated = false;
    }


    public UserData InitLocalUser(GameEngine gameEngine, int ID, string name, string address, int localPort, bool isMe, UserRole userRole) {

        if (userRole == UserRole.Player) _userGameObject = Instantiate(playerPrefab);
        else if (userRole == UserRole.Tracker) _userGameObject = Instantiate(trackerPrefab);
        else if (userRole == UserRole.Viewer || userRole == UserRole.Server) _userGameObject = Instantiate(viewerPrefab);

        if (userRole == UserRole.Viewer || userRole == UserRole.Tracker || userRole == UserRole.Server) {
            gameEngine.uiHandler.viewerController = _userGameObject.GetComponent<ViewerController>();
            gameEngine.uiHandler.viewerController.InitViewerController(isMe);
        }
        UserData user = _userGameObject.GetComponent<UserData>();
        user.Init(gameEngine, CountPlayers(), ID, name, address, localPort, _userGameObject, isMe, userRole);

        if (userRole != UserRole.Server )
        {
            usersPlaying.Add(user);
            if(userRole == UserRole.Player) StoreUserParts(user);
        }

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
        else go = new GameObject(); // useless

        UserData p = go.GetComponent<UserData>();

        p.Init(gameEngine, CountPlayers(), ID, name, address, port, go, false, role);

        if (role == UserRole.Player) {
            ChangePlayerColor(p, whiteColor);
            StoreUserParts(p);
        }
        usersPlaying.Add(p);
        return p;
    }


    public int CountPlayers(){
        int i=0;
        foreach(UserData user in usersPlaying){
            if(user._userRole == UserRole.Player) i+=1;
        }
        return i;
    }


    public void ChangeVisualisationMode(string mode, GameEngine gameEngine, bool fade) {

        if(!fade){
            // for clients
            if(me._userRole == UserRole.Server)
                gameEngine.osc.sender.SendVisualisationChange(mode, usersPlaying);


            // main parameters
            // TODO CLEAN THIS PART
            if(mode == "0") gameEngine.scenarioEvents.SetTimeOfDay(6);            
            else gameEngine.scenarioEvents.SetTimeOfDay(8);

            if(_hasLerped && (mode != "1Ca" || mode != "1Cb" || mode != "1Cc")) RevertLerping();

            if((mode != "2B" && mode !="2C" && gameEngine.scenarioEvents.mirrorAct) || 
            ((mode == "2B" || mode =="2C") && !gameEngine.scenarioEvents.mirrorAct)) 
                gameEngine.scenarioEvents.ToggleMirror();

            if (mode != "2C")
                foreach (UserData user in usersPlaying)
                {
                    if(user._userRole == UserRole.Player) ChangePlayerColor(user, whiteColor);
                }
            if (mode != "3B" && mode != "3Ca" && mode != "3Cb" && mode != "3Cc")
            {
                gameEngine.sendToAudioDevice = false;
                trailsCondition = null;
                trailsRelated = false;
            }
            else gameEngine.sendToAudioDevice = true;
            gameEngine.uiHandler.sendToAudioDeviceToggle.isOn = gameEngine.sendToAudioDevice;

            gameEngine.currentVisualisationMode = mode;


            // per user parameters
            foreach (UserData user in usersPlaying) {
                
                if(user._userRole == UserRole.Player ){

                    if (mode == "0")  // basic condition
                    {
                        if (me._userRole == UserRole.Server || me._userRole == UserRole.Viewer){
                            user.ChangeSkin(this, "all");
                        }
                        else{
                            if (user._ID == me._ID)
                                user.ChangeSkin(this, "all");
                            else user.ChangeSkin(this, "nothing");     
                        }
                    }

                    else if (mode == "1A") // every spheres visible
                    { 
                        user.ChangeSkin(this, "all");  
                    }

                    else if (mode == "1B") // other's hand visible, mine are not
                    { 
                        if (user._ID == me._ID)
                            user.ChangeSkin(this, "noHands");
                        else user.ChangeSkin(this, "all");
                        
                    }
                    else if (mode == "1C" || mode == "1Ca" || mode == "1Cb" || mode == "1Cc") // change arms length
                    {
                        user.ChangeSkin(this, "all");
                        if (mode == "1Ca"){
                            Debug.Log("move arms");
                            distanceToMove = -0.3f;
                            lerpDuration = 4f;
                            StartLerping();
                        }
                        else if (mode == "1Cb")
                        {
                            distanceToMove = 1f;
                            lerpDuration = 3f;
                            StartLerping();
                        }
                        else if (mode == "1Cc")
                        {
                            distanceToMove = 4f;
                            lerpDuration = 6f;
                            StartLerping();
                        }
                    }


                    else if (mode == "2A") { // every sphere visible
                        user.ChangeSkin(this, "all");
                    }
                    else if (mode == "2B") // mirror mode , side to side, same color
                    { 
                        user.ChangeSkin(this, "all");   
                    }
                    else if (mode == "2C") // mirror mode, different color
                    {
                        user.ChangeSkin(this, "all");
                        if (user._ID == me._ID)
                            ChangePlayerColor(user, whiteColor);
                        else ChangePlayerColor(user, cyanColor);
                        // different color
                    }


                    else if (mode == "3A") // trails individual
                    {
                        
                        if (me._userRole == UserRole.Server || me._userRole == UserRole.Viewer)
                            user.ChangeSkin(this, "trails");
                        else
                        {
                            if (user._ID == me._ID)
                                user.ChangeSkin(this, "trails");
                            else user.ChangeSkin(this, "trails");
                        }
                        trailsRelated = true;
                        GetAllParticleSystems();
                       // DeActivateParticleVelocity();
                        trailsCondition = null;
                    }

                    else if (mode == "3B") // trails individual + sound
                    { // trails mode2
                        if (me._userRole == UserRole.Server || me._userRole == UserRole.Viewer)
                            user.ChangeSkin(this, "trails");
                        else
                        {
                            if (user._ID == me._ID)
                                user.ChangeSkin(this, "trails");
                            else user.ChangeSkin(this, "trails");
                        }
                        trailsRelated = true;
                        GetAllParticleSystems();
                       // DeActivateParticleVelocity();
                        trailsCondition = "soloR";
                    }

                    else if (mode == "3Ca" || mode == "3Cb" || mode == "3Cc") // intersubject
                    { // trails mode2
                        Debug.Log("TO DO (sound)");
                        user.ChangeSkin(this, "trails");
                        trailsCondition = "relation";
                        trailsRelated = true;
                        GetAllParticleSystems();
                        _areFar = true;
                        _areClose = true;
                    }


                    // other modes
                    else if (mode == "4A") // trails mode3
                    {
                        if (user._ID == me._ID)
                            user.ChangeSkin(this, "noHands");
                        else user.ChangeSkin(this, "shortTrails");                    
                    }
                    else if (mode == "5A") // one player has left hand visible, other player has right hand visible
                    {
                        user.ChangeSkin(this, "onehand");   
                    }
                    else{
                        Debug.Log("%% Wrong VisualisationMode Request ! %%");
                    }

                }
            }
            Debug.Log("Visualisation changed : "+mode);
            

        }
        else{
            gameEngine.pendingVisualisationMode = mode;
            Camera.main.GetComponent<CameraFade>().FadeOut();
        }
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

        pendingPositionsActualizations.Add(user._ID + "Head", user.head.transform.position);
        pendingPositionsActualizations.Add(user._ID + "LeftHand", user.leftHand.transform.position);
        pendingPositionsActualizations.Add(user._ID + "RightHand", user.rightHand.transform.position);
        pendingRotationsActualizations.Add(user._ID + "Head", user.head.transform.rotation);
        pendingRotationsActualizations.Add(user._ID + "LeftHand", user.leftHand.transform.rotation);
        pendingRotationsActualizations.Add(user._ID + "RightHand", user.rightHand.transform.rotation);

    }



    // adjust players positions from stored one
    public void ActualizePlayersPositions(UserData me) {
        int i = 0;
        foreach (UserData user in usersPlaying)
        {
            if (user._ID != me._ID && user._userRole == UserRole.Player) // if it's not actual instance's player
            {

                usersPlaying[i].head.transform.position = pendingPositionsActualizations[user._ID + "Head"];
                usersPlaying[i].leftHand.transform.position = pendingPositionsActualizations[user._ID + "LeftHand"];
                usersPlaying[i].rightHand.transform.position = pendingPositionsActualizations[user._ID + "RightHand"];

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
        int i = 0;
        foreach (UserData user in usersPlaying) {
            if (user._userRole == UserRole.Player) r += 1; // if we find a player thats number r
            if (r == n) return i; // if r was needed, return it
            i++;
        }
        Debug.Log("Player not found");
        return -1;
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


    public void TranslateUser(){
        me.gameObject.transform.position += me.calibrationPositionGap;
    }


    public Vector3 GetCalibrationGap(int playerID){
        foreach(UserData user in usersPlaying){
            if(user._ID== playerID) return user.calibrationPositionGap;
        }
        return new Vector3();
    }

    // arm extension and particle trail functions

    void StartLerping()
    {
        
        int i = 0;
        if( _initialPositionRightHand == null || _initialPositionLeftHand == null){
            _initialPositionRightHand = new Vector3[8];
            _initialPositionLeftHand = new Vector3[8];
            foreach (UserData user in usersPlaying) // store initial local position 
            {
                if (user._userRole == UserRole.Player)
                {
                    _initialPositionRightHand[i] = usersPlaying[i].rightHand.transform.localPosition;
                    _initialPositionLeftHand[i] = usersPlaying[i].leftHand.transform.localPosition;
                    i++;
                }
            }
        }

        i = 0;
        _isLerping = true;
        _timeStartedLerping = Time.time;
        foreach (UserData user in usersPlaying)
        {
            if (user._userRole == UserRole.Player)
            {
                _startPositionRightHand[i] = usersPlaying[i].rightHand.transform.localPosition;
                _endPositionRightHand[i] = usersPlaying[i].rightHand.transform.localPosition + Vector3.up * distanceToMove;
                _startPositionLeftHand[i] = usersPlaying[i].leftHand.transform.localPosition;
                _endPositionLeftHand[i] = usersPlaying[i].leftHand.transform.localPosition + Vector3.up * distanceToMove;
                i++;
            }
        }
        _hasLerped = true;
    }


    void RevertLerping(){

        _isLerping = true;
        _timeStartedLerping = Time.time;
        int i = 0;
        foreach (UserData user in usersPlaying)
        {
            if (user._userRole == UserRole.Player)
            {
                _startPositionRightHand[i] = usersPlaying[i].rightHand.transform.localPosition;
                _endPositionRightHand[i] = _initialPositionRightHand[i];
                _startPositionLeftHand[i] = usersPlaying[i].leftHand.transform.localPosition;
                _endPositionLeftHand[i] = _initialPositionLeftHand[i];
                i++;
            }
        }
        _hasLerped = false;

    }


    void FixedUpdate()
    {
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
                    usersPlaying[i].rightHand.transform.localPosition = Vector3.Lerp(_startPositionRightHand[i], _endPositionRightHand[i], percentageComplete);
                    usersPlaying[i].leftHand.transform.localPosition = Vector3.Lerp(_startPositionLeftHand[i], _endPositionLeftHand[i], percentageComplete);
                    i++;
                }
            }

            if (percentageComplete >= 1.0f)
            {
                _isLerping = false;
            }
        }

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
                int i = 0;
                foreach (UserData user in usersPlaying)
                {
                    if (user._userRole == UserRole.Player)
                    {
                        trailSystemsR[i] = usersPlaying[i].rightHand.GetComponentInChildren<ParticleSystem>();
                        trailSystemsL[i] = usersPlaying[i].leftHand.GetComponentInChildren<ParticleSystem>();
                        i++;
                    }
                }
            Debug.Log("got that many particle systems: " + trailSystemsR.Length);
                 trailsRelated = false;
            }                
    }

    void ActivateParticleVelocity()
    {
        int i = 0;
        foreach (ParticleSystem p in trailSystemsR)
        {
            var veloModuleR = trailSystemsR[i].inheritVelocity;
            veloModuleR.enabled = true;
            var veloModuleL = trailSystemsL[i].inheritVelocity;
            veloModuleL.enabled = true;
            i++;
        }
    }

    void DeActivateParticleVelocity()
    {
        int i = 0;
        foreach (ParticleSystem p in trailSystemsR)
        {
            var veloModuleR = trailSystemsR[i].inheritVelocity;
            veloModuleR.enabled = false;
            var veloModuleL = trailSystemsL[i].inheritVelocity;
            veloModuleL.enabled = false;
            i++;
        }
    }

    void SetParticleLifetime(float lf)
    {
        int i = 0;
        foreach (ParticleSystem p in trailSystemsR)
        {
            trailSystemsR[i].startLifetime = lf;
            trailSystemsL[i].startLifetime = lf;
            i++;
        }
    }

    // end of arm extension and particle trail functions



}
