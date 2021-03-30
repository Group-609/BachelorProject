using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Runtime.InteropServices;


public class PlayerDataRecorder : MonoBehaviour
{
    [DllImport("__Internal")]
    private static extern void Save(string str);

    private List<FrameData> frames = new List<FrameData>();
    private StreamWriter writer;
    private int counter = 0;
    [SerializeField] private int framesBetweenRecordTakes = 4; // How many frames between recording of gameobject coordinates
    [SerializeField] private int framesBetweenSavingOfData = 600;   //Save data every 10 seconds
    [System.NonSerialized] public bool testEnded = false;       //Set to true when the player finishes the game
    
    //Does not depend on framerate
    void FixedUpdate()
    {
        counter++;
        if (counter % framesBetweenRecordTakes == 0)
        {
            frames.Add(new FrameData(transform.position.x, transform.position.y, transform.position.z, transform.GetChild(0).rotation.w, transform.GetChild(0).rotation.x, transform.GetChild(0).rotation.y, transform.GetChild(0).rotation.z, Time.fixedTime));
        }
        if (counter % framesBetweenSavingOfData == 0)
        {
            Save(JsonHelper.ToJson(frames.ToArray()));
        }
        if (testEnded)
        {
            Save(JsonHelper.ToJson(frames.ToArray()));
            this.enabled = false;
        }
    }

}
