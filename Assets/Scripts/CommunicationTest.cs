using UnityEngine;
using System.Runtime.InteropServices;

public class CommunicationTest : MonoBehaviour
{

    [DllImport("__Internal")]
    private static extern void ShowMessage(string str);

    void Start()
    {
        ShowMessage("Greetings world!");
    }
}