using UnityEngine;
using UnityEngine.UI;

/*
 * This is the main player controller. The player controller is rigidbody based with state machines.
 * It controls the movement - both grounded and in-air, as well as the camera movement.
 * Finally the grapple is also controlled here too.
*/


public class PlayerController : MonoBehaviour
{
    //Declaring Variables
    [Header("Inputs")]
    private Vector3 moveInput;
    private Vector3 moveDirection;
    private Vector2 mouseInput;

    [Header("Move Speeds")]
    [SerializeField] float walkSpeed = 8f;
    [SerializeField] float sprintSpeed = 12f;
    [SerializeField] float airSpeed = 7f;
    [SerializeField] float maxAirSpeed = 20f;
    private float speed;

    [Header("Jump Values")]
    [SerializeField] float jumpForce = 6f;
    private bool isJumping = false;
    [SerializeField] float jumpBufferTime = 0.2f;
    private float jumpBufferCounter;

    [Header("Camera")]
    [SerializeField] float sensitivity = 1f;
    [SerializeField] float cameraClamp = 90f;
    private Camera playerCamera;
    private CameraFov cameraFOV;
    private float xRotation;
    private float yRotation;
    [SerializeField] const float normalFOV = 80f;
    [SerializeField] const float grappleFOV = 100f;

    [Header("Grapple")]
    [SerializeField] Transform grapple;
    private Vector3 grapplePos;
    private float grappleSpeed;
    [SerializeField] float grappleRange = 40f;
    [SerializeField] float grappleSpeedMultiplier = 1f;
    [SerializeField] float minGrappleSpeed = 5f;
    [SerializeField] float maxGrappleSpeed = 40f;
    private bool isGrappling = false;
    private bool grapplePressed = false;
    private bool grappleJumpPressed = false;
    private float grappleSize;
    private Vector3 grappleEndPos;
    [SerializeField] LayerMask grappleLayer;

    [Header("Ground")]
    [SerializeField] LayerMask groundLayer;
    [SerializeField] Transform groundPos;
    [SerializeField] float groundCheckRadius = 0.1f;
    private bool isGrounded;

    [Header("Enemies")]
    [SerializeField] Text enemiesKilledText;
    private int enemiesKilled = 0;

    [Header("Checkpoints")]
    private Vector3 spawnPos;

    [Header("Misc. References")]
    private Rigidbody rb;
    private SoundManager soundManager;
    private PauseMenu pauseMenu;

    //Player State Machine
    private enum State
    {
        Normal,
        ThrowGrapple,
        Grappling,
        EndOfGrapple

    }

    private State state;

    //Getting Components
    private void Awake()
    {
        playerCamera = GameObject.FindGameObjectWithTag("PlayerCamera").GetComponent<Camera>();
        cameraFOV = playerCamera.GetComponent<CameraFov>();
        rb = GetComponent<Rigidbody>();
        soundManager = GameObject.FindGameObjectWithTag("OtherSoundManager").GetComponent<SoundManager>();
        pauseMenu = GameObject.FindGameObjectWithTag("PauseMenu").GetComponent<PauseMenu>();
    }

    //Setting Default Values
    private void Start()
    {
        //Hiding Cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        //Setting Default Spawnpoint
        spawnPos = transform.position;

        //Setting Default Speed
        speed = walkSpeed;

        //Setting Default State
        state = State.Normal;

        //Hiding Grapple
        grapple.gameObject.SetActive(false);
    }

    //Standard Updates (Every Frame)
    private void Update()
    {
        if (!pauseMenu.paused)
        {
            GetInputs();
            UpdateUI();

            //Calculates jump buffer
            if (isGrounded)
                jumpBufferCounter = jumpBufferTime;
            else if (!isGrounded && jumpBufferCounter > -0.1f)
                jumpBufferCounter -= Time.deltaTime;

            //Resets state when not grappling
            if (!isGrappling)
                state = State.Normal;
        }
    }

    //Physics Updates (Movement etc)
    private void FixedUpdate()
    {
        if (!pauseMenu.paused)
        {
            GroundCheck();

            switch (state)
            {
                case State.Normal:
                    MovePlayer();
                    StartGrapple();
                    break;

                case State.ThrowGrapple:
                    ThrowGrapple();
                    MovePlayer();
                    break;

                case State.Grappling:
                    GrappleMovement();
                    CancelGrapple();
                    break;

                case State.EndOfGrapple:
                    EndOfGrapple();
                    StartGrapple();
                    CancelGrapple();
                    break;
            }
        }
    }

    //Camera Updates (Fixed Jittering)
    private void LateUpdate()
    {
        if (!pauseMenu.paused)
        {
            MoveCamera();
        }
    }

    private void GetInputs()
    {
        //WASD and Mouse Inputs
        moveInput = new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical")).normalized;
        mouseInput = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"));

        //Checks if player is trying to sprint and updates speeds
        if (Input.GetKey(KeyCode.LeftShift) && isGrounded)
            speed = maxAirSpeed = sprintSpeed;

        //Reset to walking speed when sprint key is let go.
        else if (!Input.GetKey(KeyCode.LeftShift) && isGrounded)
            speed = maxAirSpeed = walkSpeed;

        //Check if the player is trying to jump
        if (Input.GetKeyDown(KeyCode.Space) && jumpBufferCounter > 0f)
            isJumping = true;

        if (Input.GetMouseButtonDown(1))
            grapplePressed = true;

        if (Input.GetKeyDown(KeyCode.Space) && isGrappling)
            grappleJumpPressed = true;
    }

    private void MovePlayer()
    {
        //Grounded Movement
        if (isGrounded)
        {
            //Sets max air speed to whatever speed the player was last moving at.
            maxAirSpeed = speed;

            //Calculate move direction relative to camera
            moveDirection = transform.TransformDirection(moveInput).normalized * speed;

            //Moves the player in the movedirection. ignores its y velocity to let gravity work properly.
            rb.velocity = new Vector3(moveDirection.x, rb.velocity.y, moveDirection.z);
        }
        //In Air Movement
        else
        {
            //Limits max air velocity
            if (rb.velocity.magnitude > maxAirSpeed)
                rb.velocity = Vector3.ClampMagnitude(rb.velocity, maxAirSpeed);

            //Only updates if player is trying to move in air.
            if (moveInput != Vector3.zero)
            {
                //Calculates move direction
                moveDirection = transform.TransformDirection(moveInput).normalized * airSpeed;

                //Stops the player moving quicker than the max speed.
                //(Not perfect as it doesnt allow any input once the player moves quicker than the max speed. Cant even be used to slow down).
                if (rb.velocity.magnitude < maxAirSpeed)
                    rb.AddForce(moveDirection, ForceMode.Acceleration);
            }
        }

        //Checks if the player is trying to jump, if so will move the player upwards.
        if (isJumping)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            //Resets the jump buffer counter to stop double jumps.
            jumpBufferCounter = 0f;
            isJumping = false;
        }
    }

    private void MoveCamera()
    {
        //Calculates vertical rotation and clamps it so the player cant look too far up or down.
        xRotation -= mouseInput.y * sensitivity;
        xRotation = Mathf.Clamp(xRotation, -cameraClamp, +cameraClamp);

        //Calulcates horizontal Rotation
        yRotation = mouseInput.x * sensitivity;

        //Rotates the player object
        transform.Rotate(0f, yRotation, 0f);

        //Rotates the camera to match the player
        playerCamera.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
    }

    //Starts the grapple by checking if the player is in range and looking at a grapplable object.
    private void StartGrapple()
    {
        //Checks if the player is trying to grapple
        if (grapplePressed)
        {
            //Resets the grapple pressed bool
            grapplePressed = false;

            //Fires a raycast that looks for objects ion the grapple layer mask.
            if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out RaycastHit hit, grappleRange, grappleLayer))
            {
                //Plays Grapple SFX
                soundManager.PlaySound("Grapple");

                //Sets grapple position to the raycast hit's position
                grapplePos = hit.point;

                //Updates player state
                state = State.ThrowGrapple;

                //Activates the grapple object and resets the scale
                grapple.gameObject.SetActive(true);
                grapple.localScale = Vector3.zero;
                grappleSize = 0f;

                //Updates local bool
                isGrappling = true;
            }
        }
    }

    //Throws the grapple towards the raycast.
    private void ThrowGrapple()
    {
        //Rotates the grapple to face the grapple position.
        grapple.LookAt(grapplePos);

        //Speed at which the grapple moves towards the grapple position
        float grappleThrowSpeed = 50f;
        //Increases the grapple size over time
        grappleSize += grappleThrowSpeed * Time.deltaTime;
        grapple.localScale = new Vector3(1, 1, grappleSize);

        //Once the grapple has reached the grapple position the player will start grappling in.
        if (grappleSize >= Vector3.Distance(transform.position, grapplePos))
        {
            //State is updated
            state = State.Grappling;
            isGrappling = true;

            //The cameras FOV is changed to make it more dynamic.
            cameraFOV.SetCameraFov(grappleFOV);
        }
    }

    //Moves the player towards the end point of the grapple.
    private void GrappleMovement()
    {
        //Ensures the grapple remains facing the grapple position
        grapple.LookAt(grapplePos);

        //Calculate grapple direction
        Vector3 grappleDirection = (grapplePos - transform.position).normalized;

        //Stops the grapple speed being too fast.
        grappleSpeed = Mathf.Clamp(Vector3.Distance(transform.position, grapplePos), minGrappleSpeed, maxGrappleSpeed);

        //Calulcates final grapple direction.
        Vector3 grappleDirectionFinal = grappleDirection * grappleSpeed * grappleSpeedMultiplier;

        //Move player towards grapple point
        rb.velocity = new Vector3(grappleDirectionFinal.x, grappleDirectionFinal.y, grappleDirectionFinal.z);

        //Once the player has got close enough to the end point, the state is updated and the player will hang in place
        float grappleDistance = 1f;
        if (Vector3.Distance(transform.position, grapplePos) < grappleDistance)
        {
            grappleEndPos = transform.position;
            state = State.EndOfGrapple;
        }
    }

    //Keeps the player in place once they have reached the end of the grapple.
    private void EndOfGrapple()
    {
        //Keeps the player in place
        transform.position = grappleEndPos;
        //Hides the grapple object
        grapple.gameObject.SetActive(false);

    }

    //Checks if the player is trying to cancel the current grapple.
    private void CancelGrapple()
    {
        //Checks if the player is trying to stop the grapple normally. They will drop out without any velocity change.
        if (grapplePressed)
        {
            grapplePressed = false;
            StopGrapple();
        }
            
        //Checks if the player is trying to jump off the grapple. If so they will jump up and gain some vertical height.
        if (grappleJumpPressed)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            grappleJumpPressed = false;
            StopGrapple();
        }
    }

    //Stops the grapple
    private void StopGrapple()
    {
        maxAirSpeed = 20f;
        isGrappling = false;
        grapple.gameObject.SetActive(false);
        cameraFOV.SetCameraFov(normalFOV);
    }


    //Updates the UI
    private void UpdateUI()
    {
        enemiesKilledText.text = "Enemies: " + enemiesKilled + "/10";
    }

    //Checks if the player is grounded
    private void GroundCheck()
    {
        isGrounded = Physics.CheckSphere(groundPos.position, groundCheckRadius, groundLayer);
    }

    //Checks for collisions
    private void OnCollisionEnter(Collision collision)
    {
        //If the player goes out of bounds they will be reset to the last checkpoint position.
        if (collision.gameObject.layer == LayerMask.NameToLayer("OutOfBounds"))
        {
            //Teleports player to checkpoint position
            transform.localPosition = spawnPos;

            //Reset state if necessary.
            state = State.Normal;

            //Resets velocity to stop any momentum
            rb.velocity = Vector3.zero;

            //Stops grapple incase they died while grappling.
            StopGrapple();
        }
    }

    //Checks for trigger collisions
    private void OnTriggerEnter(Collider other)
    {
        //If the player passes through a checkpoint collider their spawn position will be updated.
        if (other.gameObject.CompareTag("Checkpoint"))
        {
            spawnPos = other.gameObject.GetComponent<Checkpoint>().GetCheckpointPos();
        }
    }

    public bool GetIsGrounded()
    {
        return isGrounded;
    }

    public void SetMoveDirection(Vector3 newVector)
    {
        moveDirection = newVector;
    }

    public void SetMaxAirSpeed(float _speed)
    {
        maxAirSpeed = _speed;
    }

    public void SetEnemiesKilled(int amount)
    {
        enemiesKilled += amount;
    }
}
