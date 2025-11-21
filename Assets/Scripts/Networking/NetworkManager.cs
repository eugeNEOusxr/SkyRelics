using Unity.Netcode;
using UnityEngine;

namespace SkyRelics.Networking
{
    public class NetworkManager : NetworkBehaviour
    {
        public static NetworkManager Instance { get; private set; }

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void StartHost()
        {
            Unity.Netcode.NetworkManager.Singleton.StartHost();
            Debug.Log("Started as Host");
        }

        public void StartServer()
        {
            Unity.Netcode.NetworkManager.Singleton.StartServer();
            Debug.Log("Started as Server");
        }

        public void StartClient()
        {
            Unity.Netcode.NetworkManager.Singleton.StartClient();
            Debug.Log("Started as Client");
        }
    }
}
