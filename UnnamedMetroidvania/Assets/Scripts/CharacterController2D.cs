using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using Cinemachine;

public class CharacterController2D : MonoBehaviour
{
	[SerializeField] private float m_JumpForce = 400f;							// Amount of force added when the player jumps.
	[Range(0, 1)] [SerializeField] private float m_CrouchSpeed = .36f;			// Amount of maxSpeed applied to crouching movement. 1 = 100%
	[Range(0, .3f)] [SerializeField] private float m_MovementSmoothing = .05f;	// How much to smooth out the movement
	[SerializeField] private bool m_AirControl = false;							// Whether or not a player can steer while jumping;
	[SerializeField] private LayerMask m_WhatIsGround;							// A mask determining what is ground to the character
	[SerializeField] private Transform m_GroundCheck;                           // A position marking where to check if the player is grounded.
    [SerializeField] private Transform m_WallCheck;								// en position med en empty gameobject brugt til at tjekke for vï¿½g
    [SerializeField] private Transform m_CeilingCheck;							// A position marking where to check for ceilings
	[SerializeField] private Collider2D m_CrouchDisableCollider;                // A collider that will be disabled when crouching
    [SerializeField] private float m_dashForce = 400f;
	[Range(0, 1)][SerializeField] private float Lerp;

    const float k_GroundedRadius = .2f; // Radius of the overlap circle to determine if grounded
	private bool m_Grounded;            // Whether or not the player is grounded.
	private bool m_Walled;              // om playeren er mod en vï¿½g eller ik
	const float k_WalledRadius = .2f;	// radius til en overlapping circle til at tjekke om man er walled
	const float k_CeilingRadius = .2f; // Radius of the overlap circle to determine if the player can stand up
	private Rigidbody2D m_Rigidbody2D;
	public bool m_FacingRight = true;  // For determining which way the player is currently facing.
	private Vector3 m_Velocity = Vector3.zero;
	private int DashCount;
	private int JumpCount;

    [Header("Events")]
	[Space]

	public UnityEvent OnLandEvent;
	public UnityEvent OnWallEvent;

    [System.Serializable]
	public class BoolEvent : UnityEvent<bool> { }

	public BoolEvent OnCrouchEvent;
	private bool m_wasCrouching = false;

    #region Camera
    [Header("Camera Stuff")]
	[SerializeField] private GameObject _cameraFollowGO;
	private CameraFollowingObject _cameraFollowObject;
	private float _fallSpeedYDampingChangeThreshold;

	private void Start()
    {
		_cameraFollowObject = _cameraFollowGO.GetComponent<CameraFollowingObject>();
		_fallSpeedYDampingChangeThreshold = CameraManager.instance._fallSpeedYDampingChangeThreshold;
	}

    private void Update()
    {
		//Hvis spiller falder hurtigere end _fallSpeedYDampingChangeThreshold, bliver LerpYDamping sat til sand
		if (m_Rigidbody2D.velocity.y < _fallSpeedYDampingChangeThreshold && !CameraManager.instance.IsLerpingYDamping && !CameraManager.instance.LerpedFromPlayerFalling)
        {
			CameraManager.instance.LerpYDamping(true);
        }

		//Hvis spilleren står stille eller går op af, kører dette
		if (m_Rigidbody2D.velocity.y >= 0f && !CameraManager.instance.IsLerpingYDamping && CameraManager.instance.LerpedFromPlayerFalling)
        {
			CameraManager.instance.LerpedFromPlayerFalling = false;

			CameraManager.instance.LerpYDamping(false);
        }
    }

    #endregion
    #region IDC
    private void Awake()
	{
		m_Rigidbody2D = GetComponent<Rigidbody2D>();

		if (OnLandEvent == null)
			OnLandEvent = new UnityEvent();

		if (OnCrouchEvent == null)
			OnCrouchEvent = new BoolEvent();

		if (OnWallEvent == null)
		{
			OnWallEvent = new UnityEvent();
        }
	}

	private void FixedUpdate()
	{
		bool wasGrounded = m_Grounded;
		m_Grounded = false;
        bool wasWalled = m_Walled;
        m_Walled = false;
        m_Rigidbody2D.gravityScale = 3f;

        // The player is grounded if a circlecast to the groundcheck position hits anything designated as ground
        // This can be done using layers instead but Sample Assets will not overwrite your project settings.
        Collider2D[] GroundColis = Physics2D.OverlapCircleAll(m_GroundCheck.position, k_GroundedRadius, m_WhatIsGround);
		for (int i = 0; i < GroundColis.Length; i++)
		{
			if (GroundColis[i].gameObject != gameObject)
			{
				m_Grounded = true;
				if (!wasGrounded)
					OnLandEvent.Invoke();
			}
		}

        Collider2D[] WallColis = Physics2D.OverlapCircleAll(m_WallCheck.position, k_WalledRadius, m_WhatIsGround); //samme check om ground men for vï¿½g
        for (int i = 0; i < WallColis.Length; i++)
        {
            if (WallColis[i].gameObject != gameObject)
            {
                m_Walled = true;
				if (m_Rigidbody2D.velocity.y < 0) //tilfï¿½jet sï¿½tning sï¿½ man falder langsommere nï¿½r man stï¿½r op ad en vï¿½g
				{
                    m_Rigidbody2D.gravityScale = 0.5f;
                }
                if (!wasWalled)
				{
                    OnWallEvent.Invoke();
                }
            }
        }
    }


	public void Move(float move, bool crouch, bool jump, bool Dashing)
	{
		// If crouching, check to see if the character can stand up
		if (!crouch)
		{
			// If the character has a ceiling preventing them from standing up, keep them crouching
			if (Physics2D.OverlapCircle(m_CeilingCheck.position, k_CeilingRadius, m_WhatIsGround))
			{
				crouch = true;
			}
		}

		//only control the player if grounded or airControl is turned on
		if (m_Grounded || m_AirControl)
		{

			// If crouching
			if (crouch)
			{
				if (!m_wasCrouching)
				{
					m_wasCrouching = true;
					OnCrouchEvent.Invoke(true);
				}

				// Reduce the speed by the crouchSpeed multiplier
				move *= m_CrouchSpeed;

				// Disable one of the colliders when crouching
				if (m_CrouchDisableCollider != null)
					m_CrouchDisableCollider.enabled = false;
			} else
			{
				// Enable the collider when not crouching
				if (m_CrouchDisableCollider != null)
					m_CrouchDisableCollider.enabled = true;

				if (m_wasCrouching)
				{
					m_wasCrouching = false;
					OnCrouchEvent.Invoke(false);
				}
			}


            // Move the character by finding the target velocity
            Vector3 targetVelocity = new Vector2(move * 10f, m_Rigidbody2D.velocity.y);
			// And then smoothing it out and applying it to the character
			m_Rigidbody2D.velocity = Vector3.SmoothDamp(m_Rigidbody2D.velocity, targetVelocity, ref m_Velocity, m_MovementSmoothing);
            //m_Rigidbody2D.velocity = Vector3.SmoothDamp(m_Rigidbody2D.velocity, targetVelocity, ref m_Velocity, 0);
            // If the input is moving the player right and the player is facing left...

            //m_Rigidbody2D.velocity = new Vector2(m_Rigidbody2D.velocity.x + (move-move), m_Rigidbody2D.velocity.y);
            //gameObject.transform.position = Vector3.Lerp(gameObject.transform.position, gameObject.transform.position + new Vector3(move*1f,0,0), Lerp);

            if (move > 0 && !m_FacingRight)
			{
				// ... flip the player.
				Flip();
			}
			// Otherwise if the input is moving the player left and the player is facing right...
			else if (move < 0 && m_FacingRight)
			{
				// ... flip the player.
				Flip();
			}
		}
		// If the player should jump...     CHANGE HERE

		// If the player should jump...
		if (JumpCount >= 1 && jump && !m_Walled)
		{
            // Add a vertical force to the player.
            m_Grounded = false;
            m_Rigidbody2D.AddForce(new Vector2(0f, m_JumpForce));
            JumpCount--;
            m_Rigidbody2D.velocity = new Vector2(m_Rigidbody2D.velocity.x, 0); // resetter dens vertikale hastighed
        }
        if (m_Grounded) //reset dash og jump nï¿½r man lander
		{
			DashCount = 1;
			JumpCount = 2;
		}
		if (m_Walled)
		if (m_Grounded && jump)
		{
			DashCount = 1;
		}
		if (DashCount >= 1 && Dashing) //starter dash hvis man trykker pï¿½ dash knappen og dashcount er lig med eller over 1
		{
            m_Rigidbody2D.AddForce(new Vector2(m_Rigidbody2D.transform.localScale.x * m_dashForce, 0)); // tilfï¿½rer en force til playerens rigidbody i retningen den vender pga. flip()
            DashCount--;
        }
		if (m_Walled && jump && !m_Grounded) //nï¿½r man er mod en vï¿½g og ikke pï¿½ jorden laver man et wallhop
		{
			m_Rigidbody2D.AddForce(new Vector2(-m_Rigidbody2D.transform.localScale.x * 300f, m_JumpForce));
		}
		if (m_Walled && m_Grounded && jump) //man stï¿½r i et hjï¿½rne hopper man normalt
		{
            m_Grounded = false;
            m_Rigidbody2D.AddForce(new Vector2(0f, m_JumpForce));
            JumpCount--;
            m_Rigidbody2D.velocity = new Vector2(m_Rigidbody2D.velocity.x, 0);
        }
	}
    #endregion

    public void Flip()
	{
		// Switch the way the player is labelled as facing.
		m_FacingRight = !m_FacingRight;

		// Multiply the player's x local scale by -1.
		Vector3 theScale = transform.localScale;
		theScale.x *= -1;
		transform.localScale = theScale;

        #region MoreCameraStuff
        //Kalder på "Camera Following Object" script CallTurn funktion
        _cameraFollowObject.CallTurn();
        #endregion
    }
}
