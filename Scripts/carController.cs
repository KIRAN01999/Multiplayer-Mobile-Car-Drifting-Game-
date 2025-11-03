using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;

public class carController : MonoBehaviour
{
    public float MoveSpeed = 50f;
    public float MaxSpeed = 15f;
    public float Drag = 0.98f;
    public float SteerAngle = 20f;
    public float Traction = 1f;

    // Variables
    private Vector3 MoveForce;
    private float steerInput = 0f;
    public ParticleSystem leftSmoke;
    public ParticleSystem rightSmoke;
    private PhotonView photonView;

    // Scene control - car only moves in Scene2
    private bool canMove = false;

    void Start()
    {
        photonView = GetComponent<PhotonView>();

        // Check if we're in Scene2 (gameplay scene)
        string currentScene = SceneManager.GetActiveScene().name;
        canMove = (currentScene == "Scene2");

        // Disable smoke in Scene1
        if (!canMove)
        {
            if (leftSmoke != null) leftSmoke.Stop();
            if (rightSmoke != null) rightSmoke.Stop();
        }

        Debug.Log($"Car in scene: {currentScene}, Movement enabled: {canMove}");
    }

    void Update()
    {
        // Don't move if we're not in Scene2
        if (!canMove) return;

        // Don't control other players' cars
        if (photonView != null && !photonView.IsMine) return;

        HandleTouchInput();

        // Move forward automatically
        MoveForce += transform.forward * MoveSpeed * Time.deltaTime;
        transform.position += MoveForce * Time.deltaTime;

        // Steering
        transform.Rotate(Vector3.up * steerInput * MoveForce.magnitude * SteerAngle * Time.deltaTime);

        // Drag and speed limit
        MoveForce *= Drag;
        MoveForce = Vector3.ClampMagnitude(MoveForce, MaxSpeed);

        // Traction (drift control)
        MoveForce = Vector3.Lerp(MoveForce.normalized, transform.forward, Traction * Time.deltaTime) * MoveForce.magnitude;

        // Debug (optional)
        Debug.DrawRay(transform.position, MoveForce.normalized * 3, Color.red);
        Debug.DrawRay(transform.position, transform.forward * 3, Color.blue);

        HandleDriftSmoke();
        CheckFall();
    }

    void HandleTouchInput()
    {
        steerInput = 0f;

        if (Input.touchCount > 0)
        {
            Vector2 touchPos = Input.GetTouch(0).position;
            if (touchPos.x < Screen.width / 2f)
            {
                steerInput = -1f; // Turn left
            }
            else
            {
                steerInput = 1f; // Turn right
            }
        }

        // Desktop testing with keyboard
#if UNITY_EDITOR
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
        {
            steerInput = -1f;
        }
        else if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        {
            steerInput = 1f;
        }
#endif
    }

    void HandleDriftSmoke()
    {
        bool isTouching = Input.touchCount > 0;

        if (isTouching && steerInput != 0f)
        {
            if (leftSmoke != null && !leftSmoke.isPlaying) leftSmoke.Play();
            if (rightSmoke != null && !rightSmoke.isPlaying) rightSmoke.Play();
        }
        else
        {
            if (leftSmoke != null && leftSmoke.isPlaying) leftSmoke.Stop();
            if (rightSmoke != null && rightSmoke.isPlaying) rightSmoke.Stop();
        }
    }

    void CheckFall()
    {
        if (transform.position.y < -5f)
        {
            if (photonView != null && photonView.IsMine)
            {
                photonView.RPC("PlayerFell", RpcTarget.All);
            }
        }
    }

    [PunRPC]
    void PlayerFell()
    {
        // Find the endgame manager and show game over for everyone
        endgame endGameManager = FindObjectOfType<endgame>();
        if (endGameManager != null)
        {
            endGameManager.ShowGameOver();
        }
        else
        {
            Debug.LogError("EndGame manager not found!");
        }
    }
}