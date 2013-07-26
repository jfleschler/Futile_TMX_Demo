using UnityEngine;
using System.Collections;
using System;

public class Player : FContainer {
	
	public enum CharacterState {
        idle,
        walking,
        jumping
    };
	
	private FAnimatedSprite sprite;
	private CharacterState currentState;
	
	public Level currentLevel;
	
	private Vector2 velocity = new Vector2(0, 0);
	private Vector2 desiredPosition;
	private bool onGround = false;
		
	private float didJump;
	private float didJumpDownTimer;
		
	public Player() {
		ListenForUpdate(Update);
				
		sprite = new FAnimatedSprite(new FAnimation("idle","DEMOPlayer",16,16,0,0,1,70,false));
		this.AddChild(sprite);
		
		sprite.addAnimation(new FAnimation("walking","DEMOPlayer",16,16,1,0,4,70,true));
		sprite.addAnimation(new FAnimation("jumping","DEMOPlayer",16,16,2,0,1,70,false));
		
		ChangeState(CharacterState.idle);
	}
	
	public override void HandleAddedToContainer(FContainer container) {
		currentLevel = container as Level;
		base.HandleAddedToContainer(container);
	}
	
	// Use this for initialization
	void Start () {
		
	}
	
	public void ChangeState(CharacterState newState) {
        //Debug.Log("Changing from " + currentState.ToString() + " to " + newState.ToString());
        currentState = newState;
		
        switch (currentState) {
            case CharacterState.idle:
                break;
            case CharacterState.walking:
                break;
            case CharacterState.jumping:
				didJump = 0.8f;
                break;
            default: break;
        }
		
        sprite.play(currentState.ToString());
    }
	
	// Update is called once per frame
	void Update () {
		float hInput = Input.GetAxis("Horizontal");
		float vInput = Input.GetAxis("Vertical");
		
		float forwardStep = 800f * Time.deltaTime;
		float gravityStep = 550f * Time.deltaTime;
		
		// apply gravity and dampining
		velocity.y -= gravityStep;
		velocity.x *= 0.9f;
		
		// movement
		if (hInput == 1) { 			// right
			sprite.scaleX = 1.0f;
            if (currentState != CharacterState.walking && onGround) {
                ChangeState(CharacterState.walking);
            }
            velocity.x += forwardStep;
        } else if (hInput == -1) { 	// left
			sprite.scaleX = -1.0f;
            if (currentState != CharacterState.walking && onGround) {
                ChangeState(CharacterState.walking);
            }
            velocity.x += forwardStep * -1;
        } 
		
		// jump
		float jumpBtn = Input.GetAxis("Jump");
		if ( jumpBtn == 1 && onGround && vInput != -1f && didJump == 0) {
	        velocity.y = 200f;
			onGround = false;
			ChangeState(CharacterState.jumping);
	    } else if (jumpBtn != 1 && velocity.y > 125f) {
	        velocity.y = 125f;
	    }

		// clamp velocity
		if (velocity.x < -120f) velocity.x = -120f;
		if (velocity.x > 120f) velocity.x = 120f;
		if (velocity.y < -250f) velocity.y = -250f;
		if (velocity.y > 450f) velocity.y = 450f;
        
		if (velocity.x > -10 && velocity.x < 10) velocity.x = 0;
		
		Vector2 stepVelocity = new Vector2(velocity.x * Time.deltaTime, velocity.y * Time.deltaTime);
		desiredPosition = this.GetPosition() + stepVelocity;
		
		// resolve collisions
		checkForAndResolveCollisions();
			
		if (onGround && velocity.x == 0 && currentState != CharacterState.idle) {
        	ChangeState(CharacterState.idle);
        }
		
		didJump -= Time.deltaTime;
		if (didJump < 0f) didJump = 0f;
	}
	
	private void checkForAndResolveCollisions() {
		
		onGround = false;
		
		float vInput = Input.GetAxis("Vertical");
		float hInput = Input.GetAxis("Horizontal");
		float jumpBtn = Input.GetAxis("Jump");
		
		int lengthI = currentLevel.Grid_Collision.GetLength(0);
	    int lengthJ = currentLevel.Grid_Collision.GetLength(1);
		
		int tileSize = currentLevel.tileWidth;
		
		int P_X = Math.Abs((int)((desiredPosition.x) / currentLevel.tileWidth));
		int P_Y = Math.Abs((int)((desiredPosition.y) / currentLevel.tileWidth));
		
	    int Min_X = P_X - 1;
	    int Max_X = P_X + 1;
	    int Min_Y = P_Y - 1;
	    int Max_Y = P_Y + 1;
	
	    if (Min_X < 0) Min_X = 0;
	    if (Max_X > lengthJ) Max_X = lengthJ;
	    if (Min_Y < 0) Min_Y = 0;
	    if (Max_Y > lengthI) Max_Y = lengthI;
	
	    Rect gridRect;
	    Rect playerRect;
		
		for (int i = Min_Y; i <= Max_Y; i++) {
            for (int j = Min_X; j <= Max_X; j++) {
				
				playerRect = new Rect(desiredPosition.x + 2, desiredPosition.y, 12, 16);
		
				if (currentLevel.Grid_Collision[i, j] == 1) {
                    gridRect = new Rect((j * tileSize) + 8, (-i * tileSize) - 8, tileSize, tileSize);
                    
					if (RXRectExtensions.CheckIntersect(playerRect,gridRect)) {
                        Vector2 intersection = RectIntersection(playerRect, gridRect);
												
						if (i > P_Y && j == P_X) {					// below
							desiredPosition.y += intersection.y;
							velocity.y = 0f;
							onGround = true;
						} else if (i < P_Y && j == P_X) {			// above
							//Debug.Log("above");
							desiredPosition.y -= intersection.y;
							velocity.y = 0f;
						} else if (i == P_Y && j < P_X) {			// left
							//Debug.Log("left");
							desiredPosition.x += intersection.x;
							velocity.x = 0f;
						} else if (i == P_Y && j > P_X) {			// right
							//Debug.Log("right");
							desiredPosition.x -= intersection.x;
							velocity.x = 0f;
						} else {
							if (intersection.x > intersection.y) {
								//tile is diagonal, but resolving collision vertially
								if (intersection.y != 0) {
									if (i < P_Y) {
										desiredPosition.y -= intersection.y;
									} else {
										desiredPosition.y += intersection.y;
										onGround = true;
									}
								}
							} else {
								if (intersection.x != 0) {
									if (j > P_X) {
										desiredPosition.x -= intersection.x;
									} else {
										desiredPosition.x += intersection.x;
									}
								}
							}
						}				
					}
				// one way
				} else if (currentLevel.Grid_Collision[i, j] == 3) {
					
                    if (velocity.y < 0) {
						gridRect = new Rect(j * tileSize + 8, (-i * tileSize) - 8, tileSize, tileSize);
                    
	                    if (RXRectExtensions.CheckIntersect(playerRect,gridRect)) {
							Vector2 intersection = RectIntersection(playerRect, gridRect);

	                       	if (i > P_Y && j == P_X) {					// below
								if (vInput == -1 && jumpBtn == 1) {
									didJumpDownTimer = 10f;
								}  
								if (didJumpDownTimer == 0 && intersection.y < 5f) {
									desiredPosition.y += intersection.y;
									velocity.y = 0f;
									onGround = true;
								}
							}
	                    }
					}
                }
            }
        }
		
		didJumpDownTimer -= 1f;
		if (didJumpDownTimer < 0f) didJumpDownTimer = 0f;

		// assign new position
		this.SetPosition(desiredPosition);	
	}
	
	public Vector2 RectIntersection(Rect rectA, Rect rectB) {
	
		float leftX   = Math.Max( rectA.x, rectB.x );
		float rightX  = Math.Min( rectA.x+rectA.width, rectB.x+rectB.width );
		float topY    = Math.Max( Math.Abs(rectA.y), Math.Abs(rectB.y) );
		float bottomY = Math.Min( Math.Abs(rectA.y)+rectA.height, Math.Abs(rectB.y)+rectB.height );
		
		return new Vector2(rightX - leftX,  bottomY - topY );
	}
}
