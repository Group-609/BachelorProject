using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using Hashtable = ExitGames.Client.Photon.Hashtable;

namespace Photon.Pun.Demo.Asteroids
{
    public class CustomPlayerListEntry : MonoBehaviour
    {
        public interface ILocalPlayerPropertiesListener
        {
            void OnLocalPlayerPropertiesUpdated();
        }

        public void AddLocalPlayerPropertiesListener(ILocalPlayerPropertiesListener listener)
        {
            localPlayerPropertiesListeners.Add(listener);
        }

        public void RemoveLocalPlayerPropertiesListener(ILocalPlayerPropertiesListener listener)
        {
            localPlayerPropertiesListeners.Remove(listener);
        }

        private List<ILocalPlayerPropertiesListener> localPlayerPropertiesListeners = new List<ILocalPlayerPropertiesListener>();

        [Header("UI References")]
        public Text PlayerNameText;

        public Image PlayerColorImage;
        public Button PlayerReadyButton;
        public Image PlayerReadyImage;

        private int ownerId;
        public bool isPlayerReady;

        #region UNITY

        public void Start()
        {
            if (PhotonNetwork.LocalPlayer.ActorNumber != ownerId)
            {
                PlayerReadyButton.gameObject.SetActive(false);
            }
            else
            {
                Hashtable initialProps = new Hashtable() { { AsteroidsGame.PLAYER_READY, isPlayerReady } };
                PhotonNetwork.LocalPlayer.SetCustomProperties(initialProps);

                Debug.Log("Player " + ownerId + " IS READY: " + isPlayerReady);

                PlayerReadyButton.onClick.AddListener(() =>
                {
                    isPlayerReady = !isPlayerReady;
                    SetPlayerReady(isPlayerReady);

                    Hashtable props = new Hashtable() { { AsteroidsGame.PLAYER_READY, isPlayerReady } };
                    PhotonNetwork.LocalPlayer.SetCustomProperties(props);

                    Debug.Log("Player " + ownerId + " IS READY: " + isPlayerReady);
                    Debug.Log("Players ready clicked for player: " + name);
                    if (PhotonNetwork.IsMasterClient)
                        localPlayerPropertiesListeners.ForEach(listener => listener.OnLocalPlayerPropertiesUpdated());
                });
            }
        }

        #endregion

        public void Initialize(int playerId, string playerName)
        {
            ownerId = playerId;
            PlayerNameText.text = playerName;
            Debug.Log("Custom player entry initialized. Owner id: " + ownerId);
            PlayerColorImage.color = AsteroidsGame.GetColor(ownerId - 1);
        }

        public void SetPlayerReady(bool playerReady)
        {
            PlayerReadyButton.GetComponentInChildren<Text>().text = playerReady ? "Ready!" : "Ready?";
            PlayerReadyImage.enabled = playerReady;
        }
    }
}
