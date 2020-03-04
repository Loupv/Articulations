using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Clothing : MonoBehaviour
{
    public GameObject line;
    public float extrapolate = 1.0f;

    List<GameObject> parts = new List<GameObject>();
    List<GameObject> lines = new List<GameObject>();
    bool inited = false;
    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < 50; i++)
        {
            GameObject gc = Instantiate(line);
            gc.transform.SetParent(transform);
            lines.Add(gc);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(inited == false)
        {
            if (GameObject.Find("UserManager").GetComponent<UserManager>().usersPlaying.Count < 2) return;
            
            UserData Player1 = new UserData(), Player2 = new UserData();
            UserManager userManager = GameObject.Find("UserManager").GetComponent<UserManager>();

            int i = 0;
            
            bool player1found = false, player2found = false;

            while(!(player1found && player2found)){
                if(!player1found){
                    if(userManager.usersPlaying[i]._userRole == UserRole.Player) {
                        Player1 = userManager.usersPlaying[i];
                        Debug.Log("Player 1 id :"+Player1._ID);
                        player1found = true;
                    }
                    i+=1;
                }
                else{
                    if(userManager.usersPlaying[i]._userRole == UserRole.Player) {
                        Player2 = userManager.usersPlaying[i];
                        Debug.Log("Player 2 id :"+Player2._ID);
                        player2found = true;
                    }
                    else i+=1;
                }
            }

            parts.Add(Player1.leftHand);
            parts.Add(Player1.rightHand);

            parts.Add(Player2.leftHand);
            parts.Add(Player2.rightHand);
            inited = true;
        }

        foreach (var part in parts)
        {
            if (part == null)
            {
                parts.Clear();
                inited = false;
                return;
            }
        }

        Vector3 centroid1 = Vector3.Lerp(parts[0].transform.position, parts[1].transform.position, 0.5f);
        Vector3 centroid2 = Vector3.Lerp(parts[2].transform.position, parts[3].transform.position, 0.5f);
        float length1 = Vector3.Distance(parts[0].transform.position, parts[1].transform.position);
        float length2 = Vector3.Distance(parts[2].transform.position, parts[3].transform.position);
        for (int i = 0; i < 50; i++)
        {
            GameObject gc = lines[i];
            float lerp = ((float)i / 50) * (1 + extrapolate) - extrapolate * 0.5f;
            gc.transform.position = centroid1 * (1 - lerp) + centroid2 * lerp;
            Vector3 leftLerp = parts[0].transform.position * (1-lerp) + parts[3].transform.position * lerp;
            Vector3 rightLerp = parts[1].transform.position * (1 - lerp) + parts[2].transform.position * lerp;
            gc.transform.rotation = Quaternion.FromToRotation(Vector3.up, leftLerp - rightLerp);
            float s = (length1 * (1 - lerp) + length2 * lerp) * 0.5f;
            gc.transform.localScale = new Vector3(0.01f, s, 0.01f);
        }
    }
}
