using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Photon.Pun;
using Photon.Realtime;

public class roommanager : MonoBehaviourPunCallbacks
{
    [Header("UI Elements")]
    public InputField roomInput;
    public Text statusText;
    public Button createButton;
    public Button joinButton;

    [Header("Mobile Keyboard Settings")]
    public TouchScreenKeyboard mobileKeyboard;
    private bool useNativeKeyboard = true;
    private bool isInLobby = false;

    void Awake()
    {
        FindUIElements();
        SetupMobileInput();
        DisableButtons(); // Disable buttons until in lobby
    }

    void FindUIElements()
    {
        if (roomInput == null)
        {
            string[] possibleNames = { "roomInput", "Room Input", "InputField (Legacy)" };

            foreach (string name in possibleNames)
            {
                GameObject obj = GameObject.Find(name);
                if (obj != null)
                {
                    roomInput = obj.GetComponent<InputField>();
                    if (roomInput == null)
                        roomInput = obj.GetComponentInChildren<InputField>();

                    if (roomInput != null)
                    {
                        Debug.Log("✓ Found InputField: " + roomInput.gameObject.name);
                        break;
                    }
                }
            }
        }

        if (statusText == null)
        {
            string[] possibleNames = { "status msg", "statusText", "Status Text" };
            foreach (string name in possibleNames)
            {
                GameObject obj = GameObject.Find(name);
                if (obj != null)
                {
                    statusText = obj.GetComponent<Text>();
                    if (statusText != null) break;
                }
            }
        }

        // Find buttons by name
        if (createButton == null)
        {
            GameObject[] allObjects = FindObjectsOfType<GameObject>();
            foreach (GameObject obj in allObjects)
            {
                if (obj.name.ToLower().Contains("create"))
                {
                    createButton = obj.GetComponent<Button>();
                    if (createButton != null)
                    {
                        Debug.Log("✓ Found Create Button: " + obj.name);
                        break;
                    }
                }
            }
        }

        if (joinButton == null)
        {
            GameObject[] allObjects = FindObjectsOfType<GameObject>();
            foreach (GameObject obj in allObjects)
            {
                if (obj.name.ToLower().Contains("join"))
                {
                    joinButton = obj.GetComponent<Button>();
                    if (joinButton != null)
                    {
                        Debug.Log("✓ Found Join Button: " + obj.name);
                        break;
                    }
                }
            }
        }
    }

    void DisableButtons()
    {
        if (createButton != null) createButton.interactable = false;
        if (joinButton != null) joinButton.interactable = false;
        Debug.Log("Buttons disabled - waiting for lobby");
    }

    void EnableButtons()
    {
        if (createButton != null) createButton.interactable = true;
        if (joinButton != null) joinButton.interactable = true;
        Debug.Log("✓ Buttons enabled - ready to create/join rooms");
    }

    void SetupMobileInput()
    {
        if (roomInput == null) return;

        Debug.Log("=== MOBILE SETUP ===");
        Debug.Log("Platform: " + Application.platform);
        Debug.Log("Is Mobile: " + Application.isMobilePlatform);

        // Force mobile keyboard settings - CRITICAL!
        roomInput.shouldHideMobileInput = false;
        roomInput.readOnly = false;

        // Double-check it's actually set
        if (roomInput.shouldHideMobileInput)
        {
            Debug.LogError("❌ WARNING: shouldHideMobileInput is still TRUE!");
        }

        Debug.Log("shouldHideMobileInput: " + roomInput.shouldHideMobileInput);
        Debug.Log("readOnly: " + roomInput.readOnly);

        // On mobile, use native keyboard
        if (Application.isMobilePlatform)
        {
            useNativeKeyboard = true;
            Debug.Log("✓ Will use native mobile keyboard");
        }
    }

    void Start()
    {
        if (statusText != null)
        {
            statusText.text = "Connecting to Photon...";
        }

        Debug.Log("=== CONNECTING TO PHOTON ===");
        PhotonNetwork.ConnectUsingSettings();
        PhotonNetwork.AutomaticallySyncScene = true;
    }

    void Update()
    {
        // Handle native keyboard on mobile
        if (useNativeKeyboard && mobileKeyboard != null && Application.isMobilePlatform)
        {
            if (mobileKeyboard.status == TouchScreenKeyboard.Status.Done ||
                mobileKeyboard.status == TouchScreenKeyboard.Status.Canceled)
            {
                roomInput.text = mobileKeyboard.text;
                mobileKeyboard = null;
            }
        }
    }

    public void OpenKeyboardManually()
    {
        if (roomInput == null) return;

        Debug.Log("=== OPENING KEYBOARD ===");

        if (Application.isMobilePlatform)
        {
            string currentText = roomInput.text;
            mobileKeyboard = TouchScreenKeyboard.Open(
                currentText,
                TouchScreenKeyboardType.Default,
                false,
                false,
                false,
                false,
                "Enter Room Name",
                20
            );
            Debug.Log("✓ Native keyboard opened");
        }
        else
        {
            roomInput.Select();
            roomInput.ActivateInputField();
        }
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("✓✓✓ CONNECTED TO MASTER SERVER");
        if (statusText != null)
        {
            statusText.text = "Joining lobby...";
        }

        // Join the lobby - CRITICAL STEP!
        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby()
    {
        Debug.Log("✓✓✓ JOINED LOBBY - Ready to create/join rooms!");
        isInLobby = true;

        if (statusText != null)
        {
            statusText.text = "Ready! Enter room name";
        }

        // Enable buttons now that we're in lobby
        EnableButtons();
    }

    public void CreateRoom()
    {
        Debug.Log("=== CREATE ROOM CLICKED ===");

        // Check if in lobby
        if (!isInLobby)
        {
            Debug.LogError("❌ NOT IN LOBBY YET! Wait for connection...");
            if (statusText != null)
                statusText.text = "Wait... connecting to lobby";
            return;
        }

        // Get text from native keyboard if active
        if (mobileKeyboard != null && Application.isMobilePlatform)
        {
            roomInput.text = mobileKeyboard.text;
        }

        if (roomInput == null)
        {
            Debug.LogError("❌ Room Input is NULL!");
            if (statusText != null)
                statusText.text = "ERROR: No input field!";
            return;
        }

        string roomName = roomInput.text.Trim();
        Debug.Log("Room name: '" + roomName + "' (length: " + roomName.Length + ")");

        if (roomName.Length > 0)
        {
            if (statusText != null)
                statusText.text = "Creating room: " + roomName;

            RoomOptions roomOptions = new RoomOptions();
            roomOptions.MaxPlayers = 2;
            roomOptions.IsVisible = true;
            roomOptions.IsOpen = true;

            PhotonNetwork.CreateRoom(roomName, roomOptions);
            Debug.Log("✓ Attempting to create room: " + roomName);
        }
        else
        {
            if (statusText != null)
                statusText.text = "Please enter a room name!";

            OpenKeyboardManually();
            Debug.LogWarning("⚠ Room name empty - opening keyboard");
        }
    }

    public void JoinRoom()
    {
        Debug.Log("=== JOIN ROOM CLICKED ===");

        // Check if in lobby
        if (!isInLobby)
        {
            Debug.LogError("❌ NOT IN LOBBY YET! Wait for connection...");
            if (statusText != null)
                statusText.text = "Wait... connecting to lobby";
            return;
        }

        // Get text from native keyboard if active
        if (mobileKeyboard != null && Application.isMobilePlatform)
        {
            roomInput.text = mobileKeyboard.text;
        }

        if (roomInput == null)
        {
            Debug.LogError("❌ Room Input is NULL!");
            if (statusText != null)
                statusText.text = "ERROR: No input field!";
            return;
        }

        string roomName = roomInput.text.Trim();
        Debug.Log("Room name: '" + roomName + "' (length: " + roomName.Length + ")");

        if (roomName.Length > 0)
        {
            if (statusText != null)
                statusText.text = "Joining room: " + roomName;

            PhotonNetwork.JoinRoom(roomName);
            Debug.Log("✓ Attempting to join room: " + roomName);
        }
        else
        {
            if (statusText != null)
                statusText.text = "Please enter a room name!";

            OpenKeyboardManually();
            Debug.LogWarning("⚠ Room name empty - opening keyboard");
        }
    }

    public override void OnJoinedRoom()
    {
        if (statusText != null)
            statusText.text = "Joined! Loading race...";

        Debug.Log("✓✓✓ SUCCESSFULLY JOINED ROOM: " + PhotonNetwork.CurrentRoom.Name);
        Debug.Log("Players in room: " + PhotonNetwork.CurrentRoom.PlayerCount);

        // Load the race scene
        PhotonNetwork.LoadLevel("Scene2");
    }

    public override void OnCreatedRoom()
    {
        Debug.Log("✓✓✓ SUCCESSFULLY CREATED ROOM: " + PhotonNetwork.CurrentRoom.Name);
        if (statusText != null)
            statusText.text = "Room created! Waiting for opponent...";
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        if (statusText != null)
            statusText.text = "Room not found!";
        Debug.LogError("❌ Join Room Failed: " + message);

        // Re-enable buttons
        EnableButtons();
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        if (statusText != null)
            statusText.text = "Room already exists!";
        Debug.LogError("❌ Create Room Failed: " + message);

        // Re-enable buttons
        EnableButtons();
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        if (statusText != null)
            statusText.text = "Disconnected! Reconnecting...";
        Debug.LogError("❌ Disconnected: " + cause);

        isInLobby = false;
        DisableButtons();

        // Try to reconnect
        PhotonNetwork.ConnectUsingSettings();
    }
}