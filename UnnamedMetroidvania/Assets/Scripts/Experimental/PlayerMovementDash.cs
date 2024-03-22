using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;
using UnityEngine.SocialPlatforms;

public class NewPlayerMovement : MonoBehaviour
{
    //Scriptable object which holds all the player's movement parameters. If you don't want to use it
    //just paste in all the parameters, though you will need to manuly change all references in this script
    public PlayerDataWithDash Data;

    #region COMPONENTS
    public Rigidbody2D RB { get; private set; }
    #endregion

    #region STATE PARAMETERS
    //Variables control the various actions the player can perform at any time.
    //These are fields which are public allowing for other sctipts to read them
    //but can only be privately written to.
    public bool IsFacingRight { get; private set; } //samme som gamle script, sat som den vej man bevæger sig, og ændre x-scale
    public bool IsJumping { get; private set; } //man er hoppende hvis, y-velocity er positiv og man ikke er på jorden og false når man er på jorden
    public bool IsWallJumping { get; private set; } //man er wallhoppende, hvis man lige har gjort det, og fjerne det igen når timeren er over
    public bool IsDashing { get; private set; } //SKRIV HER
    public bool IsSliding { get; private set; } //SKRIV HER


    //Timers (also all fields, could be private and a method returning a bool could be used)
    public float LastOnGroundTime { get; private set; } //bliver sat til cayote time når man er på jorden og tæller ned når man forlader
    public float LastOnWallTime { get; private set; } //SKRIV HER
    public float LastOnWallRightTime { get; private set; } //bliver sat til cayote time når man er på højre væg og tæller ned når man forlader
    public float LastOnWallLeftTime { get; private set; } //bliver sat til cayote time når man er på venstre væg og tæller ned når man forlader

    //Jump
    private bool _isJumpCut;
    private bool _isJumpFalling; //er sand når man ikke er på jorden og ens y-velocity er negativ
    private int JumpsLeft;

    //Wall Jump
    private float _wallJumpStartTime;
    private int _lastWallJumpDir;

    //Dash
    private int _dashesLeft;
    private bool _dashRefilling;
    private Vector2 _lastDashDir;
    private bool _isDashAttacking; //true mens dash
    private float LastDash = 2;
    private bool DashedOff;

    //Combat
    private float Cooldown;
    private GameObject Sword;
    private Rigidbody2D enemyRB;
    private bool ignore;
    private float IFramesCD;
    private bool hit;

    #endregion

    #region INPUT PARAMETERS
    private Vector2 _moveInput;

    public float LastPressedJumpTime { get; private set; }
    public float LastPressedDashTime { get; private set; }
    #endregion

    #region CHECK PARAMETERS
    //Set all of these up in the inspector
    [Header("Checks")]
    [SerializeField] private Transform _groundCheckPoint;
    //Size of groundCheck depends on the size of your character generally you want them slightly small than width (for ground) and height (for the wall check)
    [SerializeField] private Vector2 _groundCheckSize = new Vector2(0.49f, 0.03f);
    [Space(5)]
    [SerializeField] private Transform _frontWallCheckPoint;
    [SerializeField] private Transform _backWallCheckPoint;
    [SerializeField] private Vector2 _wallCheckSize = new Vector2(0.5f, 1f);
    #endregion

    #region LAYERS & TAGS
    [Header("Layers & Tags")]
    [SerializeField] private LayerMask _groundLayer;
    [SerializeField] private LayerMask _enemyCheck;
    #endregion

    private void Awake()
    {
        RB = GetComponent<Rigidbody2D>();
        Sword = this.gameObject.transform.GetChild(2).gameObject;
    }

    private void Start()
    {
        SetGravityScale(Data.gravityScale);
        IsFacingRight = true;
    }

    private void Update()
    {
        #region TIMERS
        LastOnGroundTime -= Time.deltaTime;
        LastOnWallTime -= Time.deltaTime;
        LastOnWallRightTime -= Time.deltaTime;
        LastOnWallLeftTime -= Time.deltaTime;
        LastDash -= Time.deltaTime;
        Cooldown -= Time.deltaTime;
        LastPressedJumpTime -= Time.deltaTime;
        LastPressedDashTime -= Time.deltaTime;
        IFramesCD -= Time.deltaTime;

        if (LastDash < 0 && DashedOff || LastOnWallTime > 0)
        {
            _dashesLeft = 1;
            DashedOff = false;
        }
        else if (LastOnGroundTime < 0 && JumpsLeft == 1)
            JumpsLeft--;
        #endregion

        #region DOUBLE JUMP
        if (LastOnGroundTime > 0)
        {
            JumpsLeft = 1;
            DashedOff = true;
        }
        #endregion

        #region SWORD CHECK
        if (Input.GetButtonDown("Fire1") && Cooldown <= 0f) //Attack efter hvad ens attack speed er
        {
            startAttack(); //kørere startattack metoden
            Cooldown = Data.AttackSpeed; //resetter cooldown til attackspeed
            Invoke("endAttack", Data.ActiveFrames); //fjerne sværet efter en bestemt mængde tid
        }
        #endregion

        #region INPUT HANDLER
        _moveInput.x = Input.GetAxisRaw("Horizontal");
        _moveInput.y = Input.GetAxisRaw("Vertical");

        if (_moveInput.x != 0)
            CheckDirectionToFace(_moveInput.x > 0);

        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.C) || Input.GetKeyDown(KeyCode.J)) //tjekker for jump input
        {
            OnJumpInput(); //starter buffer
        }

        if (Input.GetKeyUp(KeyCode.Space) || Input.GetKeyUp(KeyCode.C) || Input.GetKeyUp(KeyCode.J)) //tjekker for hvornår man releaser jump button
        {
            OnJumpUpInput(); //starter buffer
        }

        if (Input.GetKeyDown(KeyCode.X) || Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.K)) //tjekker input for dash
        {
            OnDashInput(); //starter buffer
        }
        #endregion

        #region COLLISION CHECKS
        if (!IsDashing && !IsJumping)
        {
            //Ground Check
            if (Physics2D.OverlapBox(_groundCheckPoint.position, _groundCheckSize, 0, _groundLayer) && !IsJumping) //checks if set box overlaps with ground
            {
                LastOnGroundTime = Data.coyoteTime; //if so sets the lastGrounded to coyoteTime
            }

            //Right Wall Check
            if (((Physics2D.OverlapBox(_frontWallCheckPoint.position, _wallCheckSize, 0, _groundLayer) && IsFacingRight)
                || (Physics2D.OverlapBox(_backWallCheckPoint.position, _wallCheckSize, 0, _groundLayer) && !IsFacingRight)) && !IsWallJumping)
                LastOnWallRightTime = Data.coyoteTime;

            //Right Wall Check
            if (((Physics2D.OverlapBox(_frontWallCheckPoint.position, _wallCheckSize, 0, _groundLayer) && !IsFacingRight)
                || (Physics2D.OverlapBox(_backWallCheckPoint.position, _wallCheckSize, 0, _groundLayer) && IsFacingRight)) && !IsWallJumping)
                LastOnWallLeftTime = Data.coyoteTime;

            //Two checks needed for both left and right walls since whenever the play turns the wall checkPoints swap sides
            LastOnWallTime = Mathf.Max(LastOnWallLeftTime, LastOnWallRightTime);
        }
        /*if (Physics2D.OverlapBox(_frontWallCheckPoint.position, _wallCheckSize, 0, _enemyCheck))
        {
            hit = true;
        }*/
        #endregion

        #region JUMP CHECKS
        if (IsJumping && RB.velocity.y < 0) //parameter for at tjekke om man kan hoppe eller er hoppende
        {
            IsJumping = false;

            if (!IsWallJumping)
                _isJumpFalling = true;
        }

        if (IsWallJumping && Time.time - _wallJumpStartTime > Data.wallJumpTime) //wallJumpstart bliver sat til time.time senere så det fungerer som en timer
        {
            IsWallJumping = false;
        }

        if (LastOnGroundTime > 0 && !IsJumping && !IsWallJumping) //LastOnGroundTime bliver sat til cayote time når man er på jorden
        {
            _isJumpCut = false;

            if (!IsJumping)
                _isJumpFalling = false; //Man er ikke faldende
        }

        if (!IsDashing)
        {
            //Jump
            if (CanJump() && LastPressedJumpTime > 0) //når man trykker på jump starter en buffer og den kører en method til at tjekke om man kan hoppe
            {
                IsJumping = true;
                IsWallJumping = false;
                _isJumpCut = false;
                _isJumpFalling = false;
                Jump();
            }
            //WALL JUMP
            else if (CanWallJump() && LastPressedJumpTime > 0) //hvis man ikke kan hoppe, så tjekker den for wallhop
            {
                IsWallJumping = true;
                IsJumping = false;
                _isJumpCut = false;
                _isJumpFalling = false;

                _wallJumpStartTime = Time.time;
                _lastWallJumpDir = (LastOnWallRightTime > 0) ? -1 : 1; //if else statement der retunerer -1 eller 1

                WallJump(_lastWallJumpDir);
            }
        }
        #endregion

        #region DASH CHECKS
        if (CanDash() && LastPressedDashTime > 0 && _dashesLeft >= 0) //dasher hvis man kan
        {
            //Freeze game for split second. Adds juiciness and a bit of forgiveness over directional input
            Sleep(Data.dashSleepTime);

            //If not direction pressed, dash forward
           // if (_moveInput != Vector2.zero)  UDKOMMENTERE, da det giver et 8D dash
             //   _lastDashDir = _moveInput;
           // else
                _lastDashDir = IsFacingRight ? Vector2.right : Vector2.left;


            IsDashing = true;
            IsJumping = false;
            IsWallJumping = false;
            _isJumpCut = false;
            _dashesLeft--;
            LastDash = Data.dashRefillTime;

            StartCoroutine(nameof(StartDash), _lastDashDir);
        }
        #endregion

        #region SLIDE CHECKS 
        if (CanSlide() && ((LastOnWallLeftTime > 0 && _moveInput.x < 0) || (LastOnWallRightTime > 0 && _moveInput.x > 0))) //hvis man har været på en væg i lang nok tid uden at bevæge sig, begge sider. Wallcling sætter sig fast på en væg
        {
            JumpsLeft = 1;
            IsSliding = true;
        }
        else
            IsSliding = false;
        #endregion

        #region GRAVITY 
        if (!_isDashAttacking) 
        {
            //Higher gravity if we've released the jump input or are falling
            if (IsSliding) //wallcling
            {
                SetGravityScale(0);
            }
            else if (RB.velocity.y < 0 && _moveInput.y < 0) //Fast fall
            {
                //Much higher gravity if holding down
                SetGravityScale(Data.gravityScale * Data.fastFallGravityMult);
                //Caps maximum fall speed, so when falling over large distances we don't accelerate to insanely high speeds
                RB.velocity = new Vector2(RB.velocity.x, Mathf.Max(RB.velocity.y, -Data.maxFastFallSpeed));
            }
            else if (_isJumpCut) //giver slip på hop knap
            {
                //Higher gravity if jump button released
                SetGravityScale(Data.gravityScale * Data.jumpCutGravityMult);
                RB.velocity = new Vector2(RB.velocity.x, Mathf.Max(RB.velocity.y, -Data.maxFallSpeed));
            }
            else if ((IsJumping || IsWallJumping || _isJumpFalling) && Mathf.Abs(RB.velocity.y) < Data.jumpHangTimeThreshold) //jump hang
            {
                SetGravityScale(Data.gravityScale * Data.jumpHangGravityMult);
            }
            else if (RB.velocity.y < 0) //normalt fald
            {
                //Higher gravity if falling
                SetGravityScale(Data.gravityScale * Data.fallGravityMult);
                //Caps maximum fall speed, so when falling over large distances we don't accelerate to insanely high speeds
                RB.velocity = new Vector2(RB.velocity.x, Mathf.Max(RB.velocity.y, -Data.maxFallSpeed));
            }
            else //normal tyngdekraft
            {
                //Default gravity if standing on a platform or moving upwards
                SetGravityScale(Data.gravityScale);
            }
        }
        else //igen tyngdekraft ved dash
        {
            //No gravity when dashing (returns to normal once initial dashAttack phase over)
            SetGravityScale(0);
        }
        #endregion

        #region IFRAMES
        if (IFramesCD <= 0)
        {
            ignore = false;
        }
        Physics2D.IgnoreLayerCollision(7, 8, ignore); //ignorere collision, mellem lag 7 og 8 som er player og enemy
        #endregion

    }

    private void FixedUpdate()
    {
        #region LERP
        //Handle Run
        if (!IsDashing)
        {
            if (IsWallJumping)
                Run(Data.wallJumpRunLerp); //ændre hastigheden mens man wallhopper
            else
                Run(1);
        }
        else if (_isDashAttacking) //ændre hastighed ved dash
        {
            Run(Data.dashEndRunLerp);
        }

        //Handle Slide
        if (IsSliding) //starter slide
            Slide();
        #endregion

        #region KNOCKBACK
        /*if (hit)
        {
            //Laver en normalvektor og scaler den op så spilleren tager knockback
            Vector2 dir = new Vector2(enemyRB.position.x - RB.position.x, enemyRB.position.y - RB.position.y);
            Vector2 force = new Vector2(dir.normalized.x, dir.normalized.y * (Data.runMaxSpeed / Data.maxFallSpeed));
            Debug.Log(dir);
            Debug.Log(force);
            //RB.velocity = Vector2.zero;
            //RB.AddForce(-dir.normalized * Data.KnockbackForce, ForceMode2D.Impulse);
            RB.AddForce(-force * Data.KnockbackForce, ForceMode2D.Impulse);
            hit = false
        }*/
        #endregion
    }

    #region INPUT CALLBACKS
    //Methods which whandle input detected in Update()
    public void OnJumpInput()
    {
        LastPressedJumpTime = Data.jumpInputBufferTime;
    }

    public void OnJumpUpInput()
    {
        if (CanJumpCut() || CanWallJumpCut())
            _isJumpCut = true;
    }

    public void OnDashInput()
    {
        LastPressedDashTime = Data.dashInputBufferTime;
    }
    #endregion

    #region GENERAL METHODS
    public void SetGravityScale(float scale) //ændre tyngdekraft efter scale
    {
        RB.gravityScale = scale;
    }

    private void Sleep(float duration) //klader på preform sleep som gør det umuligt at bevæge sig i en bestemt mængde tid, laver en corutine
    {
        //Method used so we don't need to call StartCoroutine everywhere
        //nameof() notation means we don't need to input a string directly.
        //Removes chance of spelling mistakes and will improve error messages if any
        StartCoroutine(nameof(PerformSleep), duration);
    }

    private IEnumerator PerformSleep(float duration) //corutine der starter sleepmode
    {
        Time.timeScale = 0;
        yield return new WaitForSecondsRealtime(duration); //Must be Realtime since timeScale with be 0 
        Time.timeScale = 1;
    }
    #endregion

    //MOVEMENT METHODS
    #region RUN METHODS
    private void Run(float lerpAmount) //kaldt fra fixed
    {
        //Calculate the direction we want to move in and our desired velocity
        float targetSpeed = _moveInput.x * Data.runMaxSpeed;
        //We can reduce our control using Lerp() this smooths changes to our direction and speed
        targetSpeed = Mathf.Lerp(RB.velocity.x, targetSpeed, lerpAmount); //kan måske fjerne den for ikke at have acceleration eller sæt lerp til 1

        #region Calculate AccelRate
        float accelRate;

        //Gets an acceleration value based on if we are accelerating (includes turning) 
        //or trying to decelerate (stop). As well as applying a multiplier if we're air borne.
        if (LastOnGroundTime > 0)
            accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? Data.runAccelAmount : Data.runDeccelAmount; //acceleration på jorden
        else
            accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? Data.runAccelAmount * Data.accelInAir : Data.runDeccelAmount * Data.deccelInAir; //acceleration i luften, hvis den aboslutte værdi af ens targetspeed
        #endregion

        #region Add Bonus Jump Apex Acceleration
        //Increase are acceleration and maxSpeed when at the apex of their jump, makes the jump feel a bit more bouncy, responsive and natural
        if ((IsJumping || IsWallJumping || _isJumpFalling) && Mathf.Abs(RB.velocity.y) < Data.jumpHangTimeThreshold) //hvis man er i luften og ens y-velocity er lav, accelerere man hurtigere
        {
            accelRate *= Data.jumpHangAccelerationMult;
            targetSpeed *= Data.jumpHangMaxSpeedMult;
        }
        #endregion

        #region Conserve Momentum
        //We won't slow the player down if they are moving in their desired direction but at a greater speed than their maxSpeed
        //hvis momentum er true og x-velociry er størrer end targetspeed og ens targetspeed er samme retning som ens x-velocity og ens targetspeed er størrer end 0.01 og man er på jorden.
        if (Data.doConserveMomentum && Mathf.Abs(RB.velocity.x) > Mathf.Abs(targetSpeed) && Mathf.Sign(RB.velocity.x) == Mathf.Sign(targetSpeed) && Mathf.Abs(targetSpeed) > 0.01f && LastOnGroundTime < 0)
        {
            //Prevent any deceleration from happening, or in other words conserve are current momentum
            //You could experiment with allowing for the player to slightly increae their speed whilst in this "state"
            accelRate = 0;
        }
        #endregion

        //Calculate difference between current velocity and desired velocity
        float speedDif = targetSpeed - RB.velocity.x;
        //Calculate force along x-axis to apply to thr player

        float movement = speedDif * accelRate;

        //Convert this to a vector and apply to rigidbody
        RB.AddForce(movement * Vector2.right, ForceMode2D.Force);

        /*
		 * For those interested here is what AddForce() will do
		 * RB.velocity = new Vector2(RB.velocity.x + (Time.fixedDeltaTime  * speedDif * accelRate) / RB.mass, RB.velocity.y);
		 * Time.fixedDeltaTime is by default in Unity 0.02 seconds equal to 50 FixedUpdate() calls per second
		*/
    }

    private void Turn() //simpel :)
    {
        //stores scale and flips the player along the x axis, 
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;

        IsFacingRight = !IsFacingRight;
    }
    #endregion

    #region JUMP METHODS
    private void Jump() //hopper når man kan
    {
        //Ensures we can't call Jump multiple times from one press
        LastPressedJumpTime = 0;
        LastOnGroundTime = 0; //resetter buffer og cayote timer
        JumpsLeft--;

        #region Perform Jump
        //We increase the force applied if we are falling
        //This means we'll always feel like we jump the same amount 
        //(setting the player's Y velocity to 0 beforehand will likely work the same, but I find this more elegant :D)
        float force = Data.jumpForce;
        //if (RB.velocity.y < 0)
        //  force -= RB.velocity.y;
        RB.velocity = new Vector2(RB.velocity.x, 0);

        RB.AddForce(Vector2.up * force, ForceMode2D.Impulse);
        #endregion
    }

    private void WallJump(int dir)
    {
        //Ensures we can't call Wall Jump multiple times from one press
        LastPressedJumpTime = 0;
        LastOnGroundTime = 0;
        LastOnWallRightTime = 0;
        LastOnWallLeftTime = 0;

        #region Perform Wall Jump
        Vector2 force = new Vector2(Data.wallJumpForce.x, Data.wallJumpForce.y);
        force.x *= dir; //apply force in opposite direction of wall

        if (Mathf.Sign(RB.velocity.x) != Mathf.Sign(force.x))
            force.x -= RB.velocity.x;

        if (RB.velocity.y < 0) //checks whether player is falling, if so we subtract the velocity.y (counteracting force of gravity). This ensures the player always reaches our desired jump force or greater
            force.y -= RB.velocity.y;

        //Unlike in the run we want to use the Impulse mode.
        //The default mode will apply are force instantly ignoring masss
        RB.AddForce(force, ForceMode2D.Impulse);
        #endregion
    }
    #endregion

    #region DASH METHODS
    //Dash Coroutine
    private IEnumerator StartDash(Vector2 dir)
    {
        //Overall this method of dashing aims to mimic Celeste, if you're looking for
        // a more physics-based approach try a method similar to that used in the jump
        hit = false;
        LastOnGroundTime = 0;
        LastPressedDashTime = 0; //reset Dash buffer og coyotetimer

        float startTime = Time.time;
        float noHit = Time.time;

        _dashesLeft--;
        _isDashAttacking = true;

        SetGravityScale(0);

        //We keep the player's velocity at the dash speed during the "attack" phase (in celeste the first 0.15s)
        while (Time.time - startTime <= Data.dashAttackTime && !hit)
        {
            RB.velocity = dir.normalized * Data.dashSpeed;
            //Pauses the loop until the next frame, creating something of a Update loop. 
            //This is a cleaner implementation opposed to multiple timers and this coroutine approach is actually what is used in Celeste :D
            yield return null;
        }
        /*if (noHit == Data.dashAttackTime)
        {
            hit = false;
        }*/
        startTime = Time.time;

        _isDashAttacking = false;

        //Begins the "end" of our dash where we return some control to the player but still limit run acceleration (see Update() and Run())
        SetGravityScale(Data.gravityScale);
        RB.velocity = Data.dashEndSpeed * dir.normalized;

        while (Time.time - startTime <= Data.dashEndTime)
        {
            yield return null;
        }

        //Dash over
        IsDashing = false;
    }
    #endregion

    #region OTHER MOVEMENT METHODS
    private void Slide()
    {
        //Works the same as the Run but only in the y-axis
        //THis seems to work fine, buit maybe you'll find a better way to implement a slide into this system
        //float speedDif = Data.slideSpeed - RB.velocity.y;
        //float movement = speedDif * Data.slideAccel;
        //So, we clamp the movement here to prevent any over corrections (these aren't noticeable in the Run)
        //The force applied can't be greater than the (negative) speedDifference * by how many times a second FixedUpdate() is called. For more info research how force are applied to rigidbodies.
        //movement = Mathf.Clamp(movement, -Mathf.Abs(speedDif) * (1 / Time.fixedDeltaTime), Mathf.Abs(speedDif) * (1 / Time.fixedDeltaTime));

        //RB.AddForce(movement * Vector2.up);
        SetGravityScale(0.3f);
        RB.velocity = new Vector2(RB.velocity.x, Mathf.Max(RB.velocity.y, -3));
    }
    #endregion

    #region CHECK METHODS
    public void CheckDirectionToFace(bool isMovingRight)
    {
        if (isMovingRight != IsFacingRight)
            Turn();
    }

    private bool CanJump()
    {
        return JumpsLeft >= 0 && !IsJumping && !IsSliding;
    }

    private bool CanWallJump()
    {
        return LastPressedJumpTime > 0 && LastOnWallTime > 0 && LastOnGroundTime <= 0 && (!IsWallJumping ||
             (LastOnWallRightTime > 0 && _lastWallJumpDir == 1) || (LastOnWallLeftTime > 0 && _lastWallJumpDir == -1));
    }

    private bool CanJumpCut()
    {
        return IsJumping && RB.velocity.y > 0;
    }

    private bool CanWallJumpCut()
    {
        return IsWallJumping && RB.velocity.y > 0;
    }

    private bool CanDash()
    {
        if (!IsDashing && _dashesLeft < Data.dashAmount && LastOnGroundTime > 0)
        {
            //StartCoroutine(nameof(RefillDash), 1);
        }

        return _dashesLeft > 0;
    }

    public bool CanSlide()
    {
        if (LastOnWallTime > 0 && !IsJumping && !IsWallJumping && !IsDashing && LastOnGroundTime <= 0) //hvis bufferen er gået og man ikke er midt i hop eller wallhop eller dasher eller er på jorden
            return true;
        else
            return false;
    }
    #endregion

    #region EDITOR METHODS
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(_groundCheckPoint.position, _groundCheckSize);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(_frontWallCheckPoint.position, _wallCheckSize);
        Gizmos.DrawWireCube(_backWallCheckPoint.position, _wallCheckSize);
    }
    #endregion

    #region SWORD METHOD
    void startAttack()
    {
        //A_Sword.Invoke(); //invoke kører starter et event, som gør sværet aktivt
        Sword.SetActive(true);
    }
    void endAttack()
    {
        //D_Sword.Invoke(); //starter event, som deaktivere sværet
        Sword.SetActive(false);
    }
    #endregion

    #region KNOCKBACK CHECK
    private void OnCollisionEnter2D(Collision2D collision)
    {
        enemyRB = collision.gameObject.GetComponent<Rigidbody2D>();
        //Hvis man har Iframes kører dette ikke
        if (IFramesCD <= 0f)
        {
            //Hvis spilleren rør en "enemy" tager de skade og får invincivility frames
            if (collision.gameObject.tag == "Enemy")
            {
                //Debug.Log("Hit");
                hit = true;
                RB.velocity = Vector2.zero;
                //Laver en normalvektor og scaler den op så spilleren tager knockback
                Vector2 dir = new Vector2(enemyRB.position.x - RB.position.x, enemyRB.position.y - RB.position.y);
                Vector2 force = new Vector2(dir.normalized.x, dir.normalized.y * (Data.runMaxSpeed / Data.maxFallSpeed));
                //RB.velocity = Vector2.zero;
                //RB.AddForce(-dir.normalized * Data.KnockbackForce, ForceMode2D.Impulse);
                RB.AddForce(-force * Data.KnockbackForce, ForceMode2D.Impulse);
                //StartCoroutine(nameof(StartDash), -dir);

                //starter i frames
                Data.PHP--;
                IFramesCD = Data.IFrames;
                ignore = true;
            }
        }
    }
    #endregion 
}