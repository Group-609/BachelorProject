using ExitGames.Client.Photon;
using Photon.Realtime;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Photon.Pun.Demo.Asteroids
{
    public class SecondConditionMainLobby : MonoBehaviourPunCallbacks, CustomPlayerListEntry.ILocalPlayerPropertiesListener
    {
        public string SceneName = "Master";

        [Header("Second Condition Selection Panel")]
        public GameObject SecondConditionInfoPanel;

        [Header("Inside Room Panel")]
        public GameObject InsideRoomPanel;
        public Button StartGameButton;

        public GameObject PlayerListEntryPrefab;

        private Dictionary<int, GameObject> playerListEntries = new Dictionary<int, GameObject>();
        
        #region UNITY

        public void Awake()
        {
            PhotonNetwork.AutomaticallySyncScene = true;
        }

        private void Start()
        {
            SetActivePanel(SecondConditionInfoPanel.name);
            LoadPlayers();
        }

        private void LoadPlayers()
        {
            //Debug.Log("SecondCondition. Loading players. Player count: " + PhotonNetwork.PlayerList.Length);
            foreach (Player p in PhotonNetwork.PlayerList)
            {
                GameObject entry = Instantiate(PlayerListEntryPrefab);
                entry.transform.SetParent(InsideRoomPanel.transform);
                entry.transform.localScale = Vector3.one;
                entry.GetComponent<CustomPlayerListEntry>().Initialize(p.ActorNumber, p.NickName);
                entry.GetComponent<CustomPlayerListEntry>().AddLocalPlayerPropertiesListener(this);
                entry.GetComponent<CustomPlayerListEntry>().SetPlayerReady(false);
                
                playerListEntries.Add(p.ActorNumber, entry);
            }
            StartGameButton.gameObject.SetActive(false);
        }

        #endregion

        #region UI CALLBACKS

        public void OnStartGameButtonClicked()
        {
            foreach (KeyValuePair<int, GameObject> entry in playerListEntries)
            {
                entry.Value.GetComponent<CustomPlayerListEntry>().RemoveLocalPlayerPropertiesListener(this);
            }
            if (PhotonNetwork.IsMasterClient)
                PhotonNetwork.LoadLevel(SceneName);
        }

        #endregion

        public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
        {
            if (playerListEntries == null)
            {
                playerListEntries = new Dictionary<int, GameObject>();
            }

            if (playerListEntries.TryGetValue(targetPlayer.ActorNumber, out GameObject entry))
            {
                if (changedProps.TryGetValue(AsteroidsGame.PLAYER_READY, out object isPlayerReady))
                {
                    entry.GetComponent<CustomPlayerListEntry>().SetPlayerReady((bool)isPlayerReady);
                }
            }

            StartGameButton.gameObject.SetActive(CheckPlayersReady());
        }

        private bool CheckPlayersReady()
        {
            if (!PhotonNetwork.IsMasterClient)
                return false;

            foreach (KeyValuePair<int, GameObject> entry in playerListEntries)
            {
                if (!entry.Value.GetComponent<CustomPlayerListEntry>().isPlayerReady)
                    return false;
            }

            /*foreach (Player p in PhotonNetwork.PlayerList)
            {
                if (p.CustomProperties.TryGetValue(AsteroidsGame.PLAYER_READY, out object isPlayerReady))
                {
                    if (!(bool)isPlayerReady)
                        return false;
                }
                else return false;
                
            }*/
            Debug.Log("All players are ready");
            return true;
        }

        public void OnEnterRoomClicked()
        {
            SetActivePanel(InsideRoomPanel.name);
        }

        public void OnLocalPlayerPropertiesUpdated()
        {
            Debug.Log("Local player properties listener responded to being ready");
            StartGameButton.gameObject.SetActive(CheckPlayersReady());
        }

        public void SetActivePanel(string activePanel)
        {
            SecondConditionInfoPanel.SetActive(activePanel.Equals(SecondConditionInfoPanel.name));
            InsideRoomPanel.SetActive(activePanel.Equals(InsideRoomPanel.name));
        }
    }
}