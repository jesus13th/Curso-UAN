using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : Character
{
    [SerializeField] private Rigidbody _rigidbody;
    [SerializeField] private Animator _animator;
    private CameraManager _camera;

    [SerializeField] private float speedTarget;
    [SerializeField] private float speedRunningTarget;
    [SerializeField] private float speedCurrent;
    private Vector2 axis;
    private float refSpeed;
    [SerializeField] private float speedSmooth;
    private float angle;
    [SerializeField] private float rotationSmooth;
    private Quaternion targetRotation;
    [SerializeField] private float jumpForce;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float maxDistance;
    [SerializeField] private bool IsOnGround;
    private int extraJump = 1;
    [SerializeField] private bool isRunning;
    [SerializeField] private int health = 100;
    [SerializeField] private Slider healthSlider;
    [SerializeField] private TMP_Text healthText;
    [SerializeField] private bool isDead = false;

    [SerializeField] private AudioClip jumpClip;

    private void Awake()
    {
        _camera = Camera.main.GetComponent<CameraManager>();
        healthSlider.maxValue = health;
        healthSlider.value = health;
        healthText.text = $"Vida: {health}";
    }

    void Update()
    {
        IsOnGround = Physics.Raycast(transform.position + Vector3.up, Vector3.down, out RaycastHit hit, maxDistance, groundLayer);
        
        if (IsOnGround)
        {
            extraJump = 1;
            axis = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
            isRunning = Input.GetKey(KeyCode.LeftShift);

            if (Input.GetKeyDown(KeyCode.LeftAlt))
            {
                _animator.SetTrigger("Dodge");
            }
        }
        speedCurrent = Mathf.SmoothDamp(speedCurrent, (isRunning ? speedRunningTarget : speedTarget) * axis.magnitude, ref refSpeed, speedSmooth);
        
        if (axis.magnitude > 0.1f)
        {
            angle = Mathf.Atan2(axis.y, -axis.x) * Mathf.Rad2Deg;
            targetRotation = Quaternion.Euler(0, angle + 180 - _camera.Angle, 0);
        }
        if (Input.GetKeyDown(KeyCode.Space) && (IsOnGround || extraJump >= 1))
        {
            extraJump--;
            _rigidbody.AddForce(Vector3.up * jumpForce * _rigidbody.mass);
            AudioManager.Instance.PlaySound(jumpClip);
        }
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation , Time.deltaTime * rotationSmooth);
    }
    private void FixedUpdate()
    {
        _rigidbody.velocity = new Vector3(transform.forward.x * speedCurrent, _rigidbody.velocity.y, transform.forward.z * speedCurrent);
    }
    private void LateUpdate()
    {
        _animator.SetFloat("Velocity", !IsOnGround ? 0 : (speedCurrent / (isRunning ? speedRunningTarget : speedTarget)));
        _animator.SetBool("IsRunning", isRunning);
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position + Vector3.up, transform.position + Vector3.up + Vector3.down * maxDistance);
    }
    public void ApplyDamage(int damage)
    {
        if (isDead)
        {
            return;
        }
        health -= damage;

        healthSlider.value = health;
        healthText.text = $"Vida: {health}";

        if (health <= 0)
        {
            isDead = true;
            _animator.SetTrigger("IsDead");
        }
    }
}