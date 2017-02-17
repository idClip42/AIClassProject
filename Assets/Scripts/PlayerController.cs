using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour 
{
	public float speed = 10;			// The speed at which the player walks,
	public float acceleration = 50;		// The rate of the player's acceleration
	public float jumpSpeed = 15;		// Jump speed
	CharacterController controller;		// The player's character controller
	Vector3 velocity;					// The velocity the Character Controller will move at every frame
	Vector3 vertVelocity;				// The vertical velocity, which won't be limited to one speed
	Animator anim;						// The character model of the current player character
	Camera cam;							// The main camera
	bool playerActive;					// Whether the player is active


	/// <summary>
	/// Start this instance.
	/// </summary>
	void Start () 
	{
		// Gets animator
		controller = GetComponent<CharacterController>();
		if(controller == null) Debug.LogError("Player needs a Character Controller");

		// Initializes velocity
		velocity = Vector3.zero;
		vertVelocity = Vector3.zero;

		// Gets animator
		anim = GetComponentInChildren<Animator>();

		cam = Camera.main;
		if(cam == null) Debug.LogError("Scene needs a 'Main Camera'");

		playerActive = true;
	}

	/// <summary>
	/// Updates at a fixed rate based on Physics
	/// </summary>
	void FixedUpdate () 
	{
		if(playerActive == true)
			MovePlayer();
		Animate();
	}

	/// <summary>
	/// Update this instance.
	/// </summary>
	void Update ()
	{
		// Checks every frame for restart command
		if(Input.GetKeyDown(KeyCode.R))
			SceneManager.LoadScene (SceneManager.GetActiveScene().name);
		// Checks every frame for quit command
		if(Input.GetKeyDown(KeyCode.Escape))
			Application.Quit();

		// Checks every frame for Jump button
		if(controller.isGrounded && Input.GetButtonDown("Jump"))
			vertVelocity += Vector3.up * jumpSpeed;
	}
		
	/// <summary>
	/// Moves the player.
	/// </summary>
	void MovePlayer()
	{
		// Gets the forward and right vectors of the camera
		Vector3 forward = Vector3.ProjectOnPlane(cam.transform.forward, Vector3.up).normalized;
		Vector3 right = Vector3.ProjectOnPlane(cam.transform.right, Vector3.up).normalized;

		// Gets the movement input from the user
		float fwdInput = Input.GetAxisRaw("Vertical");		// Corresponds to the forward direction of the camera
		float rightInput = Input.GetAxisRaw("Horizontal");	// Corresponds to the right direction of the camera

		// Adds input movement to the velocity
		velocity += (forward * fwdInput + right * rightInput) * acceleration * Time.fixedDeltaTime;

		// Limits velocity to set speed
		velocity = Vector3.ClampMagnitude(velocity, speed);

		// Drag
		velocity -= velocity * 0.1f;

		vertVelocity += Physics.gravity * Time.fixedDeltaTime;
		if(vertVelocity.y < Physics.gravity.y) vertVelocity = Physics.gravity;

		// Moves the player with the new velocity
		controller.Move((velocity + vertVelocity) * Time.fixedDeltaTime);
	}
		
	/// <summary>
	/// Animate the player model.
	/// </summary>
	void Animate()
	{
		if(anim == null) 
			return;

		// Turns the player model in the correct direction
		anim.transform.forward = Vector3.Lerp(
			anim.transform.forward,
			velocity,
			0.1f
		);

		// Gives speed to animator
		anim.SetFloat("Speed", velocity.magnitude);
	}

	/// <summary>
	/// Sets whether the player is currently controllable by the user
	/// </summary>
	/// <param name="value">If set to <c>true</c> player is controllable.</param>
	public void SetPlayerActive(bool value)
	{
		playerActive = value;
	}

}
