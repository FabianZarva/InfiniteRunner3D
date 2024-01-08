using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;
using LootLocker.Requests;

namespace InfiniteRunner3D.Player
{
    [RequireComponent(typeof(CharacterController), typeof(PlayerInput))]
    public class PlayerController : MonoBehaviour
    {
        
        [SerializeField] private float initialPlayerSpeed = 4f;
        [SerializeField] private float maximumPlayerSpeed = 30f;
        [SerializeField] private float playerSpeedIncreaseRate = 0.1f;
        [SerializeField] private float jumpHeight = 1.0f;
        [SerializeField] private float initialGravityValue = -9.81f;
        [SerializeField] private LayerMask groundLayer;
        [SerializeField] private LayerMask turnLayer;
        [SerializeField] private LayerMask obstacleLayer;
        [SerializeField] private Animator animator;
        [SerializeField] private AnimationClip slideAnimationClip;
        [SerializeField] private float playerSpeed;
        [SerializeField] private float scoreMultiplier = 10f;
        [SerializeField] private GameObject mesh;

        //  gravity, movement direction, and player velocity
        private float gravity;
        private Vector3 movementDirection = Vector3.forward;
        private Vector3 playerVelocity;

        // Input system-related variables for jumping, turning and sliding
        private PlayerInput playerInput;
        private InputAction turnAction;
        private InputAction jumpAction;
        private InputAction slideAction;

        // Character controller involved for player movement
        private CharacterController controller;

        // Sliding will have an animation of tilting the character, so it will have and ID and a boolean, if it is executed
        private int slidingAnimationId;
        private bool sliding = false;

        // tracking the game session score, starting from 0 , upwards.
        private float score = 0;

        // touch input is involved
        private Vector2 touchStartPos;

        // Unity events, for turning , game over, and updating score, that are connected to the other game elements and mechanics
        [SerializeField] private UnityEvent<Vector3> turnEvent;
        [SerializeField] private UnityEvent<int> gameOverEvent;
        [SerializeField] private UnityEvent<int> scoreUpdateEvent;

        // This is called when the script is loaded
        private void Awake()
        {
            // Gets the important components and set-ups input actions.
            playerInput = GetComponent<PlayerInput>();
            controller = GetComponent<CharacterController>();
            slidingAnimationId = Animator.StringToHash("Sliding");
            turnAction = playerInput.actions["Turn"];
            jumpAction = playerInput.actions["Jump"];
            slideAction = playerInput.actions["Slide"];
        }

        // This is called when the script is enabled
        private void OnEnable()
        {
            // Turns on input actions 
            turnAction.performed += PlayerTurn;
            slideAction.performed += PlayerSlide;
            jumpAction.performed += PlayerJump;
        }

        // This is called when the script is disabled
        private void OnDisable()
        {
            // Turns off input actions
            turnAction.performed -= PlayerTurn;
            slideAction.performed -= PlayerSlide;
            jumpAction.performed -= PlayerJump;
        }

        // Start is called before the first frame update
        private void Start()
        {
            // Initializes player's speed and gravity
            playerSpeed = initialPlayerSpeed;
            gravity = initialGravityValue;
        }

        // The callback turns the player based on input
        private void PlayerTurn(InputAction.CallbackContext context)
        {
            // Checks if the player can turn, otherwise it ends the game(game over)
            Vector3? turnPosition = CheckTurn(context.ReadValue<float>());
            if (!turnPosition.HasValue)
            {
                GameOver();
                return;
            }
            // Calculates the target direction after the turn(90 degrees left or right) and invokes the turn event(changes direction based on input)
            Vector3 targetDirection = Quaternion.AngleAxis(90 * context.ReadValue<float>(), Vector3.up) * movementDirection;
            turnEvent.Invoke(targetDirection);
            Turn(context.ReadValue<float>(), turnPosition.Value);
        }

        // Can the player turn at the current position(is he at the turn's pivot)? if yes, returns the turn position
        private Vector3? CheckTurn(float turnValue)
        {
            Collider[] hitColliders = Physics.OverlapSphere(transform.position, 0.1f, turnLayer);
            if (hitColliders.Length != 0)
            {
                Tile tile = hitColliders[0].transform.parent.GetComponent<Tile>();
                TileType type = tile.type;
                if ((type == TileType.LEFT && turnValue == -1) ||
                    (type == TileType.RIGHT && turnValue == 1) ||
                    (type == TileType.SIDEWAYS))
                {
                    return tile.pivot.position;
                }
            }
            return null;
        }

        // Turning the player(left or right 90 degrees)
        private void Turn(float turnValue, Vector3 turnPosition)
        {
            Vector3 tempPlayerPosition = new Vector3(turnPosition.x, transform.position.y, turnPosition.z);
            controller.enabled = false;
            transform.position = tempPlayerPosition;
            controller.enabled = true;

            Quaternion targetRotation = transform.rotation * Quaternion.Euler(0, 90 * turnValue, 0);
            transform.rotation = targetRotation;
            movementDirection = transform.forward.normalized;
        }

        // Initiating sliding
        private void PlayerSlide(InputAction.CallbackContext context)
        {
            // Check if the player is not currently sliding and is grounded
            if (!sliding && IsGrounded())
            {
                StartCoroutine(Slide());
            }
        }

        // Coroutine for the player's sliding animation
        private IEnumerator Slide()
        {
            sliding = true;

            // Makes the collider smaller on animation so the player can slide under obstacles without getting stuck under
            Vector3 originalControllerCenter = controller.center;
            Vector3 newControllerCenter = originalControllerCenter;
            controller.height /= 2;
            newControllerCenter.y -= controller.height / 2;
            controller.center = newControllerCenter;

            // Plays the sliding animation           
            animator.Play(slidingAnimationId);
            yield return new WaitForSeconds(slideAnimationClip.length / animator.speed);

            // Character controller's height goes back to normal after the player performs the sliding
            controller.height *= 2;
            controller.center = originalControllerCenter;
            sliding = false;
        }

        // Initiating jumping
        private void PlayerJump(InputAction.CallbackContext context)
        {
            // Is the player on the ground? If yes, then he can jump
            if (IsGrounded())
            {
                playerVelocity.y += Mathf.Sqrt(jumpHeight * gravity * -3f);
                controller.Move(playerVelocity * Time.deltaTime);
            }
        }

        // Player mobile phone swiping input for turning(left,right), jumping(up), and sliding(down)
        private void HandleSwipeInput()
        {
            foreach (Touch touch in Input.touches)
            {
                if (touch.phase == UnityEngine.TouchPhase.Began)
                {
                    // Saves the initial finger touch position on the screen
                    touchStartPos = touch.position;
                }
                else if (touch.phase == UnityEngine.TouchPhase.Ended)
                {
                    // Figures out the swiping direction based on the first swipe position and where the swiping ends
                    Vector2 swipeDelta = touch.position - touchStartPos;

                    // Is it a horizontal or vertical swipe?
                    if (Mathf.Abs(swipeDelta.x) > Mathf.Abs(swipeDelta.y))
                    {
                        // Horizontal swipe
                        float swipeDirection = Mathf.Sign(swipeDelta.x);
                        if (swipeDirection > 0)
                        {
                            // Swipe right
                            TurnPlayer(1f);
                        }
                        else
                        {
                            // Swipe left
                            TurnPlayer(-1f);
                        }
                    }
                    else
                    {
                        // Vertical swipe
                        float swipeDirection = Mathf.Sign(swipeDelta.y);
                        if (swipeDirection > 0)
                        {
                            // Swipe up (jump)
                            PlayerJump();
                        }
                        else
                        {
                            // Swipe down (slide)
                            PlayerSlide();
                        }
                    }
                }
            }
        }

        // Turning the player by swiping
        private void TurnPlayer(float turnValue)
        {
            Vector3? turnPosition = CheckTurn(turnValue);
            if (!turnPosition.HasValue)
            {
                GameOver();
                return;
            }
            Vector3 targetDirection = Quaternion.AngleAxis(90 * turnValue, Vector3.up) * movementDirection;
            turnEvent.Invoke(targetDirection);
            Turn(turnValue, turnPosition.Value);
        }

        // Jump by swiping
        private void PlayerJump()
        {
            if (IsGrounded())
            {
                playerVelocity.y += Mathf.Sqrt(jumpHeight * gravity * -3f);
                controller.Move(playerVelocity * Time.deltaTime);
            }
        }

        // Slide by swiping
        private void PlayerSlide()
        {
            if (!sliding && IsGrounded())
            {
                StartCoroutine(Slide());
            }
        }

        // Update is called once per frame
        private void Update()
        {
            // If the player is not grounded, the game is over
            if (!IsGrounded(20f))
            {
                GameOver();
                return;
            }

            // Game's scoring functionality
            score += scoreMultiplier * Time.deltaTime;
            scoreUpdateEvent.Invoke((int)score);

            // Moving the player forward automatically based on the current speed
            controller.Move(transform.forward * playerSpeed * Time.deltaTime);

            // If the player is grounded, adjust his velocity
            if (IsGrounded() && playerVelocity.y < 0)
            {
                playerVelocity.y = 0f;
            }

            // Appling gravity to player
            playerVelocity.y += gravity * Time.deltaTime;
            controller.Move(playerVelocity * Time.deltaTime);

            // Increases player speed and adjusts animation speed, both over time.
            if (playerSpeed < maximumPlayerSpeed)
            {
                playerSpeed += Time.deltaTime * playerSpeedIncreaseRate;
                gravity = initialGravityValue - playerSpeed;

                if (animator.speed < 1.25f)
                {
                    animator.speed += (1 / playerSpeed) * Time.deltaTime;
                }
            }

            // Handles swiping input
            HandleSwipeInput();
        }

        // Checks if the player is grounded within 0.2 units
        private bool IsGrounded(float length = 0.2f)
        {
            Vector3 raycastOriginFirst = transform.position;
            raycastOriginFirst.y -= controller.height / 2f;
            raycastOriginFirst.y += 0.1f;

            Vector3 raycastOriginSecond = raycastOriginFirst;
            raycastOriginFirst -= transform.forward * 0.2f;
            raycastOriginSecond += transform.forward * 0.2f;

            if (Physics.Raycast(raycastOriginFirst, Vector3.down, out RaycastHit hit, length, groundLayer) || Physics.Raycast(raycastOriginFirst, Vector3.down, out RaycastHit hit2, length, groundLayer))
            {
                return true;
            }
            return false;
        }

        // End the game and invokes the game over event system (game over canvas, stops player movement)
        private void GameOver()
        {
            Debug.Log("Game Over");
            gameOverEvent.Invoke((int)score);
            gameObject.SetActive(false);
        }

        // Player hits a collider
        private void OnControllerColliderHit(ControllerColliderHit hit)
        {
            // If the collider's layer is "obstacle", ends the game (game over)
            if (((1 << hit.collider.gameObject.layer) & obstacleLayer) != 0)
            {
                GameOver();
            }
        }
    }
}
