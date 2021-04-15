using ExitGames.Client.Photon;
using Photon.Realtime;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Photon.Pun.Demo.Asteroids
{
    public class SecondConditionMainLobby : MonoBehaviourPunCallbacks
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
            //PhotonNetwork.CurrentRoom.IsOpen = false;
            //PhotonNetwork.CurrentRoom.IsVisible = false;
        }

        private void Start()
        {
            SetActivePanel(SecondConditionInfoPanel.name);

            LoadPlayers();
        }

        private void LoadPlayers()
        {
            Debug.Log("SecondCondition. Loading players. Player count: " + PhotonNetwork.PlayerList.Length);
            foreach (Player p in PhotonNetwork.PlayerList)
            {
                GameObject entry = Instantiate(PlayerListEntryPrefab);
                entry.transform.SetParent(InsideRoomPanel.transform);
                entry.transform.localScale = Vector3.one;
                entry.GetComponent<PlayerListEntry>().Initialize(p.ActorNumber, p.NickName);
                entry.GetComponent<PlayerListEntry>().SetPlayerReady(false);
                
                playerListEntries.Add(p.ActorNumber, entry);
            }

            StartGameButton.gameObject.SetActive(false);

            Hashtable props = new Hashtable
            {
                {AsteroidsGame.PLAYER_LOADED_LEVEL, false}
            };
            PhotonNetwork.LocalPlayer.SetCustomProperties(props);
        }

        #endregion

        #region PUN CALLBACKS

        public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
        {
            if (playerListEntries.TryGetValue(targetPlayer.ActorNumber, out GameObject entry))
            {
                if (changedProps.TryGetValue(AsteroidsGame.PLAYER_READY, out object isPlayerReady))
                {
                    entry.GetComponent<PlayerListEntry>().SetPlayerReady((bool)isPlayerReady);
                }
            }

            StartGameButton.gameObject.SetActive(CheckPlayersReady());
        }

        #endregion

        #region UI CALLBACKS

        public void OnStartGameButtonClicked()
        {
            PhotonNetwork.CurrentRoom.IsOpen = false;
            PhotonNetwork.CurrentRoom.IsVisible = false;

            PhotonNetwork.LoadLevel(SceneName);
        }

        #endregion

        private bool CheckPlayersReady()
        {
            if (!PhotonNetwork.IsMasterClient)
                return false;

            foreach (Player p in PhotonNetwork.PlayerList)
            {
                if (p.CustomProperties.TryGetValue(AsteroidsGame.PLAYER_READY, out object isPlayerReady))
                {
                    if (!(bool)isPlayerReady)
                        return false;
                }
                else return false;
            }
            return true;
        }

        public void LocalPlayerPropertiesUpdated()
        {
            StartGameButton.gameObject.SetActive(CheckPlayersReady());
        }

        public void OnEnterRoomClicked()
        {
            Debug.Log("Trying to enter room. Room name: " + PhotonNetwork.CurrentRoom.Name);
            SetActivePanel(InsideRoomPanel.name);
        }

        public void SetActivePanel(string activePanel)
        {
            InsideRoomPanel.SetActive(activePanel.Equals(InsideRoomPanel.name));
            SecondConditionInfoPanel.SetActive(activePanel.Equals(SecondConditionInfoPanel.name));
        }
    }
}