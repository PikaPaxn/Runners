using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Mirror;
using UnityEngine.InputSystem;

public class PlayerController : NetworkBehaviour
{
    // Movement
    public float moveSpeed;
    public float jumpForce;

    float _horizontalInput;
    bool _didPressJump;
    bool _canJump = true;
    Rigidbody2D _rb;

    [Header("Appareance")]
    public RuntimeAnimatorController[] skins;
    [SyncVar] int _skinID;

    public override void OnStartServer() {
        _skinID = Random.Range(0, skins.Length);
    }

    public override void OnStartClient() {
        // If is not local, delete the input
        if (!isLocalPlayer) {
            var playerInput = GetComponent<PlayerInput>();
            if (playerInput != null)
                Destroy(playerInput);
        }

        GetComponent<Animator>().runtimeAnimatorController = skins[_skinID];
    }

    public override void OnStartLocalPlayer() {
        _rb = GetComponent<Rigidbody2D>();
        Camera.main.GetComponent<FollowObject>().SetTarget(transform);
    }

    #region Input Handling
    void OnHorizontalMove(InputValue value) {
        _horizontalInput = value.Get<float>();
    }

    void OnJump(InputValue value) {
        bool jumpPressed = value.isPressed;

        // Just in the frame
        if (_canJump && jumpPressed && !_didPressJump)
            _rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        _didPressJump = jumpPressed;
    }
    #endregion

    void Update() {
        if (isLocalPlayer)
            _didPressJump = false;
    }

    void FixedUpdate() {
        if (isLocalPlayer) {
            Vector2 velocity = _rb.velocity;
            velocity.x = _horizontalInput * moveSpeed * Time.fixedDeltaTime;
            _rb.velocity = velocity;
        }
    }
}
