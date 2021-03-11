using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;

namespace Photon.Pun.Demo.PunBasics
{
	/// <summary>
	/// Camera work. Follow a target
	/// </summary>
	public class CameraWork : MonoBehaviour
	{
        #region Private Fields

        // cached transform of the main camera
        Transform cameraTransform;

		//Transform of the player character (FirstPersonCharacter)
		Transform characterCamera;

		// maintain a flag internally to reconnect if target is lost or camera is switched
		bool isFollowing;
		
        #endregion

        #region MonoBehaviour Callbacks

		void LateUpdate()
		{
			// The transform target may not destroy on level load, 
			// so we need to cover corner cases where the Main Camera is different everytime we load a new scene, and reconnect when that happens
			if (cameraTransform == null && isFollowing)
			{
				OnStartFollowing();
			}

			// only follow is explicitly declared
			if (isFollowing) {
				Follow ();
			}
		}

		#endregion

		#region Public Methods

		/// <summary>
		/// Raises the start following event. 
		/// Use this when you don't know at the time of editing what to follow, typically instances managed by the photon network.
		/// </summary>
		public void OnStartFollowing()
		{
            cameraTransform = gameObject.transform.Find("FirstPersonCharacter");
			GetComponent<FirstPersonController>().enabled = true;
			isFollowing = true;
			// we don't smooth anything, we go straight to the right camera shot
			Follow();
		}
		
		#endregion

		#region Private Methods

		void Follow()
		{
			//cameraTransform.gameObject.SetActive(true);
			cameraTransform.gameObject.GetComponent<Camera>().enabled = true;
			cameraTransform.gameObject.GetComponent<AudioListener>().enabled = true;
			cameraTransform.gameObject.GetComponent<FlareLayer>().enabled = true;
		}
		#endregion
	}
}