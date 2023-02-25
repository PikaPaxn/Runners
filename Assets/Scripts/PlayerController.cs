using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Mirror;
using UnityEngine.InputSystem;

public class PlayerController : NetworkBehaviour
{
    [Header("Movement")]
    public float moveSpeed;
    public float jumpForce;

    float _horizontalInput;
    bool _jumpInput;
    bool _isJumping;
    bool _canDoubleJump;
    Rigidbody2D _rb;

    [Header("Better Jumping")]
    public float fallMultiplier = 2.5f;
    public float lowJumpMultiplier = 2f;

    [Header("Ground and Slopes")]
    public float groundCheckDistance = 0.1f;
    public LayerMask groundLayer;
    public float slopeCheckDistance = 0.25f;
    float _colliderRadius;
    bool _isGrounded;
    bool _isOnSlope;
    Vector2 _slopeNormalPerp;

    public PhysicsMaterial2D fullFriction;
    public PhysicsMaterial2D noFriction;

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

        var col = GetComponent<CapsuleCollider2D>();
        _colliderRadius = col.size.x / 3f;
    }

    #region Input Handling
    void OnHorizontalMove(InputValue value) {
        _horizontalInput = value.Get<float>();
    }

    void OnJump(InputValue value) {
        _jumpInput = value.isPressed;

        if ((_isGrounded || _canDoubleJump) && value.isPressed) {
            // Kill vertical momentum
            var vel = _rb.velocity;
            vel.y = jumpForce;
            _rb.velocity = vel;
            //_rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            _isJumping = true;

            // If it wasn't grounded, then it was its double jump
            if (!_isGrounded)
                _canDoubleJump = false;
        }
    }
    #endregion


    void FixedUpdate() {
        if (isLocalPlayer) {
            GroundCheck();
            SlopeCheck();

            // Apply horizontal movement
            Vector2 velocity = _rb.velocity;

            if (_isGrounded && !_isOnSlope && !_isJumping) {
                velocity.x = _horizontalInput * moveSpeed * Time.fixedDeltaTime;
                //velocity.y = 0f;
            } else if (_isGrounded && _isOnSlope && !_isJumping) {
                velocity.x = -_horizontalInput * moveSpeed * _slopeNormalPerp.x * Time.fixedDeltaTime;
                velocity.y = -_horizontalInput * moveSpeed * _slopeNormalPerp.y * Time.fixedDeltaTime;
            } else if (!_isGrounded) {
                velocity.x = _horizontalInput * moveSpeed * Time.fixedDeltaTime;
            }

            // Apply Better Jumping
            if (velocity.y < 0)
                velocity.y += Physics2D.gravity.y * (fallMultiplier - 1) * Time.fixedDeltaTime;
            else if (velocity.y > 0 && !_jumpInput) {
                velocity.y += Physics2D.gravity.y * (lowJumpMultiplier - 1) * Time.fixedDeltaTime;
            }

            _rb.velocity = velocity;
        }
    }

    void GroundCheck() {
        // Ground Check
        Collider2D hit = Physics2D.OverlapCircle(transform.position + Vector3.down * groundCheckDistance, _colliderRadius, groundLayer);
        _isGrounded = hit != null;

        if (_rb.velocity.y <= 0f)
            _isJumping = false;

        if (_isGrounded)
            _canDoubleJump = true;
    }

    void SlopeCheck() {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, slopeCheckDistance, groundLayer);

        if (hit) {
            _slopeNormalPerp = Vector2.Perpendicular(hit.normal).normalized;
            var slopeDownAngle = Vector2.Angle(hit.normal, Vector2.up);

            _isOnSlope = slopeDownAngle != 0;
        } else {
            _slopeNormalPerp = Vector2.zero;
            _isOnSlope = false;
        }

        if (_isOnSlope && _horizontalInput == 0f) {
            _rb.sharedMaterial = fullFriction;
        } else {
            _rb.sharedMaterial = noFriction;
        }
    }

    void OnDrawGizmosSelected() {
        float colRadius = GetComponent<CapsuleCollider2D>().size.x / 3f;

        // Ground Check
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position + Vector3.down * groundCheckDistance, colRadius);
        // Slope Check
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, Vector3.down * slopeCheckDistance);
    }
}
