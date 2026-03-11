using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 3.5f;
    public float gravity = -9.8f;
    
    [Header("Audio - Footsteps")]
    public AudioSource footstepAudioSource;
    public AudioClip footstepClip;
    [Tooltip("Ngưỡng tốc độ ngang để coi là đang di chuyển.")]
    public float footstepMoveThreshold = 0.08f;
    public float footstepMinPitch = 0.95f;
    public float footstepMaxPitch = 1.05f;
    public float footstepGroundCheckDistance = 0.25f;
    [Tooltip("Âm lượng tối đa của footstep loop.")]
    public float footstepMaxVolume = 1f;
    [Tooltip("Tốc độ tăng âm khi bắt đầu đi (đơn vị volume/giây).")]
    public float footstepFadeInSpeed = 8f;
    [Tooltip("Tốc độ giảm âm khi dừng (đơn vị volume/giây).")]
    public float footstepFadeOutSpeed = 10f;
    public bool debugFootstep = true;
    public KeyCode debugPlayFootstepKey = KeyCode.Y;

    private CharacterController controller;
    private Vector3 velocity;
    private float debugLogCooldown;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        if (footstepAudioSource == null)
        {
            footstepAudioSource = GetComponent<AudioSource>();
            if (footstepAudioSource == null)
            {
                footstepAudioSource = gameObject.AddComponent<AudioSource>();
            }
        }
        
        footstepAudioSource.playOnAwake = false;
        footstepAudioSource.spatialBlend = 0f;
        footstepAudioSource.volume = 0f;
        footstepAudioSource.mute = false;
        footstepAudioSource.loop = true;
        footstepAudioSource.clip = footstepClip;
        EnsureSfxChannelVolume(footstepAudioSource);
        // Cursor được quản lý bởi InputModeManager
    }

    private void OnDisable()
    {
        StopFootstepImmediately();
    }

    void Update()
    {
        if (debugFootstep && Input.GetKeyDown(debugPlayFootstepKey))
        {
            PreviewFootstepOnce();
        }
        
        // Không di chuyển nếu đang ở Intro (đang fade) hoặc SittingAtDesk hoặc các state sau đó
        if (GameFlow.Instance != null && 
            (GameFlow.Instance.IsState(GameState.Intro) ||
             GameFlow.Instance.IsState(GameState.SittingAtDesk) ||
             GameFlow.Instance.IsState(GameState.ComputerActive) ||
             GameFlow.Instance.IsState(GameState.ComputerFinished) ||
             GameFlow.Instance.IsState(GameState.AlbumFocus) ||
             GameFlow.Instance.IsState(GameState.AlbumReading) ||
             GameFlow.Instance.IsState(GameState.AlbumInteractable)))
        {
            FadeFootstepTo(false);
            return;
        }

        float x = Input.GetAxis("Horizontal"); // A/D
        float z = Input.GetAxis("Vertical");   // W/S

        Vector3 move = transform.right * x + transform.forward * z;
        controller.Move(move * moveSpeed * Time.deltaTime);
        HandleFootsteps(move);

        // Gravity
        if (controller.isGrounded)
        {
            if (velocity.y < 0)
                velocity.y = 0f;
        }
        else
        {
            velocity.y += gravity * Time.deltaTime;
        }

        controller.Move(velocity * Time.deltaTime);
    }
    
    private void HandleFootsteps(Vector3 moveInput)
    {
        if (footstepAudioSource == null || footstepClip == null)
        {
            if (debugFootstep)
            {
                Debug.LogWarning($"[Footstep] Missing setup. Source: {footstepAudioSource != null}, Clip: {footstepClip != null}");
            }
            return;
        }
        
        Vector3 horizontalVelocity = controller != null ? controller.velocity : Vector3.zero;
        horizontalVelocity.y = 0f;
        bool isMoving = horizontalVelocity.magnitude > footstepMoveThreshold || moveInput.sqrMagnitude > 0.01f;
        bool groundedForFootstep = IsGroundedForFootstep();
        bool canStep = groundedForFootstep && isMoving;

        if (footstepAudioSource.clip != footstepClip)
        {
            bool wasPlaying = footstepAudioSource.isPlaying;
            footstepAudioSource.clip = footstepClip;
            footstepAudioSource.loop = true;
            if (wasPlaying)
            {
                footstepAudioSource.Play();
            }
        }
        
        if (!canStep)
        {
            FadeFootstepTo(false);
            if (debugFootstep)
            {
                debugLogCooldown -= Time.deltaTime;
                if (debugLogCooldown <= 0f)
                {
                    debugLogCooldown = 1.5f;
                    Debug.Log($"[Footstep] canStep=false | grounded={groundedForFootstep} | isMoving={isMoving} | vel={horizontalVelocity.magnitude:F2}");
                }
            }
            return;
        }

        float speed01 = Mathf.Clamp01(horizontalVelocity.magnitude / Mathf.Max(0.01f, moveSpeed));
        float targetPitch = Mathf.Lerp(footstepMinPitch, footstepMaxPitch, speed01);
        footstepAudioSource.pitch = targetPitch;
        FadeFootstepTo(true);

        if (debugFootstep)
        {
            Debug.Log($"[Footstep] Resume/Play | grounded={groundedForFootstep} | speed={horizontalVelocity.magnitude:F2} | time={footstepAudioSource.time:F2}");
        }
    }

    private void FadeFootstepTo(bool shouldPlay)
    {
        if (footstepAudioSource == null)
        {
            return;
        }

        if (shouldPlay)
        {
            if (!footstepAudioSource.isPlaying)
            {
                // Nếu đang pause thì tiếp tục từ vị trí cũ; nếu chưa từng play thì bắt đầu từ đầu clip.
                if (footstepAudioSource.time > 0.001f)
                {
                    footstepAudioSource.UnPause();
                }
                else
                {
                    footstepAudioSource.Play();
                }
            }

            footstepAudioSource.volume = Mathf.MoveTowards(
                footstepAudioSource.volume,
                Mathf.Clamp01(footstepMaxVolume),
                Mathf.Max(0.01f, footstepFadeInSpeed) * Time.deltaTime
            );
            return;
        }

        footstepAudioSource.volume = Mathf.MoveTowards(
            footstepAudioSource.volume,
            0f,
            Mathf.Max(0.01f, footstepFadeOutSpeed) * Time.deltaTime
        );

        if (footstepAudioSource.isPlaying && footstepAudioSource.volume <= 0.001f)
        {
            footstepAudioSource.Pause();
            footstepAudioSource.volume = 0f;
            if (debugFootstep)
            {
                Debug.Log($"[Footstep] Pause (faded) | time={footstepAudioSource.time:F2}");
            }
        }
    }
    
    private void PreviewFootstepOnce()
    {
        if (footstepAudioSource == null || footstepClip == null)
        {
            return;
        }
        
        footstepAudioSource.PlayOneShot(footstepClip);
        Debug.Log($"[Footstep] Preview once | clip={footstepClip.name}");
    }

    private bool IsGroundedForFootstep()
    {
        if (controller == null)
        {
            return false;
        }

        if (controller.isGrounded)
        {
            return true;
        }

        Vector3 origin = controller.bounds.center;
        float rayLength = controller.bounds.extents.y + Mathf.Max(0.05f, footstepGroundCheckDistance);
        return Physics.Raycast(origin, Vector3.down, rayLength, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);
    }

    private void EnsureSfxChannelVolume(AudioSource source)
    {
        if (source == null) return;

        AudioChannelVolume channelVolume = source.GetComponent<AudioChannelVolume>();
        if (channelVolume == null)
        {
            channelVolume = source.gameObject.AddComponent<AudioChannelVolume>();
        }

        channelVolume.channel = AudioChannelType.Sfx;
        channelVolume.useAudioSourceVolumeAsBaseOnAwake = false;
        channelVolume.baseVolume = Mathf.Max(channelVolume.baseVolume, footstepMaxVolume);
        channelVolume.ApplyCurrentVolume();
    }

    private void StopFootstepImmediately()
    {
        if (footstepAudioSource == null) return;
        if (!footstepAudioSource.isPlaying) return;

        footstepAudioSource.Stop();
        footstepAudioSource.volume = 0f;
    }
}
