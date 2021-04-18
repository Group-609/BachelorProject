using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
                PlayerReadyButton.onClick.AddListener(() =>
                {
                    isPlayerReady = !isPlayerReady;
                    SetPlayerReady(isPlayerReady);

                    Debug.Log("Players ready clicked");
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
            PlayerColorImage.color = AsteroidsGame.GetColor(ownerId);
        }

        public void SetPlayerReady(bool playerReady)
        {
            PlayerReadyButton.GetComponentInChildren<Text>().text = playerReady ? "Ready!" : "Ready?";
            PlayerReadyImage.enabled = playerReady;
        }
    }
}
