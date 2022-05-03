Grid Movement Guide
======================

This documentation describes how to use the `Grid Movement` component in your project.

Behaviours
----------

-   \[`CameraMovement`\]
-   \[`CameraRotation`\]
-   \[`CameraRotationTrigger`\]
-   \[`PlayerMovement`\]

CameraMovement
------------

This behaviour allows the camera to follow the player depending on how close the player is to wall objects.

### Properties

-   `Target` - Locks the camera to the targets movement
-   `Speed` - The speed at which the camera follows the player
-   `InnerBuffer` - If the camera is within this radius then the camera will not move
-   `OuterBuffer` - If the camera is outside this radius then the camera will move

### Script

In this script we start of by setting the cameras position relative to the player
We then set values for how fast we want the camera to move as well as how far the player needs to be for an object in order for the player to move.

```
        [SerializeField] Transform target = null;
	[SerializeField] float speed = 1.0f;
	[SerializeField] float innerBuffer = 0.1f;
	[SerializeField] float outerBuffer = 1.5f;
	bool moving;
	Vector3 offset;
	void Start() {
		offset = target.position + transform.position;
	}
```

We then update the cameras position every frame, however we allow the camera to lag behind the player a bit in order for the movement to look smoother.

```
	void Update() {
		Vector3 cameraTargetPosition = target.position + offset;
		Vector3 heading = cameraTargetPosition - transform.position;
		float distance = heading.magnitude;
		Vector3 direction = heading / distance;
		if (distance > outerBuffer)
			moving = true;
		if (moving) {
			if (distance > innerBuffer)
				transform.position += direction * Time.deltaTime * speed * Mathf.Max(distance, 1f);
			else {
				transform.position = cameraTargetPosition;
				moving = false;
			}
		}
	}
```

Visually shows the inner and outer buffer while in play mode, white dot shows the camera lagging and the red circle in the middle shows where the camera will be.

```
    void OnDrawGizmos() {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(target.position + offset, innerBuffer);
        Gizmos.DrawWireSphere(target.position + offset, outerBuffer);
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(transform.position, innerBuffer);
    }
}
```

CameraRotation
--------

This behaviour allows the camera to be rotated

### Properties

-   `rotationDirection` - The direction that the camera will be rotated from an enum list
-   `speed` - The speed that the camera will be rotated

### Script

Inside of the Update method we check to see if we need to rotate the camera based on its current rotation
We then create a public method which we can call on from other scripts in order to call and rotate the camera

```
	[SerializeField] RotationDirection rotationDirection;
	[SerializeField] float speed = 360f;
	Quaternion targetRotation = Quaternion.identity;
	public void Update() {
		if (transform.rotation != targetRotation) {
			transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation,
					speed * Time.deltaTime);
		}
	}
	public void RotateTo(Vector3 to) {
		Vector3 relativePos = transform.position + to;
		targetRotation = Quaternion.LookRotation(relativePos - transform.position, Vector3.up);
	}
}
```

CameraRotationTrigger
------------

This behaviour triggers the rotation of the camera.

### Properties

-   `cameraRotator` - The parent object holding the camera
-   `targetDirection` - Direction the camera rotates towards when you enter the trigger
-   `exitDirection` - Direction the camera rotates towards when you exit the trigger
-   
### Script

We make a reference to the camerarotation script so that we can access the RotateTo method
We also make sure that the objects that trigger the rotations are invisible by disabling their meshrenderer at the start of the game.


```
    [SerializeField] GameObject cameraRotator = null;
    [SerializeField] RotationDirection targetDirection = RotationDirection.forward;
    [SerializeField] RotationDirection exitDirection = RotationDirection.forward;
    CameraRotation cameraRotation = null;
    void Start() {
        cameraRotation = cameraRotator.GetComponent<CameraRotation>();
        GetComponent<MeshRenderer>().enabled = false;
    }
```

We have two trigger methods that change the cameras rotation based on if they are inside of it, or have just exited which we setup in the serialized fields.
   
```
    void OnTriggerStay(Collider other) {
        if (other.tag == "Player") {
            cameraRotation.RotateTo(Direction.ToVector(targetDirection));
        }
    }
    void OnTriggerExit(Collider other) {
        if (other.tag == "Player") {
            cameraRotation.RotateTo(Direction.ToVector(exitDirection));
        }
    }
}
```


PlayerMovement
----------

This behaviour allows the player to move via a grid, the player is able to move different heighted terrain as long as there is not a wall nearby.

### Properties

-   `Move Speed` - Speed at which the player moves between grids
-   `Ray Length` - Determines the length of the ray cast, if an object collides with the raycast then the player cannot move in that direction
-   `RayOffsetX` - Used to see if the player can move on the X axis
-   `RayOffsetY` - Used to see if the player can move on the Y axis
-   `RayOffsetZ` - Used to see if the player can move on the Z axis
-   `Camera Rotator` - Reference to the parent object holding the camera
-   `Walkable Mask` - Layer that makes objects walkable on by the Player
-   `Collidable Mask` - Layer that makes objects unwalkable on by the Player
-   `Max Fall Cast Distance` - Distance the game checks to see if there is a walkable floor from a drop
-   `Fall Speed` - Speed at which the player falls  

### Script

By using serialized fields we can customise the restrictions of our players movement which can depend on the size of the player/objects.

```
    [SerializeField] float moveSpeed = 0.25f;
    [SerializeField] float rayLength = 1.4f;
    [SerializeField] float rayOffsetX = 0.5f;
    [SerializeField] float rayOffsetY = 0.5f;
    [SerializeField] float rayOffsetZ = 0.5f;
    Vector3 targetPosition;
    Vector3 startPosition;
    bool moving;
    Vector3 xOffset;
    Vector3 yOffset;
    Vector3 zOffset;
    Vector3 zAxisOriginA;
    Vector3 zAxisOriginB;
    Vector3 xAxisOriginA;
    Vector3 xAxisOriginB;
    [SerializeField] Transform cameraRotator = null;
    [SerializeField]  LayerMask walkableMask = 0;
    [SerializeField]  LayerMask collidableMask = 0;
    [SerializeField] float maxFallCastDistance = 100f;
    [SerializeField] float fallSpeed = 30f;
    bool falling;
    float targetFallHeight;
```

Similarly to the camera we update the positions of our raycasts every update, in order to see when the raycasts hit an object we use debugs for all the axis in order to get a visual representation.

```
    void Update() {
        yOffset = transform.position + Vector3.up * rayOffsetY;
        zOffset = Vector3.forward * rayOffsetZ;
        xOffset = Vector3.right * rayOffsetX;
        zAxisOriginA = yOffset + xOffset;
        zAxisOriginB = yOffset - xOffset;
        xAxisOriginA = yOffset + zOffset;
        xAxisOriginB = yOffset - zOffset;
        Debug.DrawLine(
                zAxisOriginA,
                zAxisOriginA + Vector3.forward * rayLength,
                Color.red,
                Time.deltaTime);
        Debug.DrawLine(
                zAxisOriginB,
                zAxisOriginB + Vector3.forward * rayLength,
                Color.red,
                Time.deltaTime);
        Debug.DrawLine(
                zAxisOriginA,
                zAxisOriginA + Vector3.back * rayLength,
                Color.red,
                Time.deltaTime);
        Debug.DrawLine(
                zAxisOriginB,
                zAxisOriginB + Vector3.back * rayLength,
                Color.red,
                Time.deltaTime);
        Debug.DrawLine(
                xAxisOriginA,
                xAxisOriginA + Vector3.left * rayLength,
                Color.red,
                Time.deltaTime);
        Debug.DrawLine(
                xAxisOriginB,
                xAxisOriginB + Vector3.left * rayLength,
                Color.red,
                Time.deltaTime);
        Debug.DrawLine(
                xAxisOriginA,
                xAxisOriginA + Vector3.right * rayLength,
                Color.red,
                Time.deltaTime);
        Debug.DrawLine(
                xAxisOriginB,
                xAxisOriginB + Vector3.right * rayLength,
                Color.red,
                Time.deltaTime);
```

Inside the falling if statement we check to see if there is a floor within our fall height, if this is the case then we set the new transforms for the player, however if our player is moving on solid ground then the new transforms will be created from the position we push the player in and where they end up. In order to check if we are falling we use raycasts to check if the player is currently grounded or not.


```
        if (falling) {
            if (transform.position.y <= targetFallHeight) {
                float x = Mathf.Round(transform.position.x);
                float y = Mathf.Round(targetFallHeight);
                float z = Mathf.Round(transform.position.z);
                transform.position = new Vector3(x, y, z);
                falling = false;
                return;
            }
            transform.position += Vector3.down * fallSpeed * Time.deltaTime;
            return;
        } else if (moving) {
            if (Vector3.Distance(startPosition, transform.position) > 1f) {
                float x = Mathf.Round(targetPosition.x);
                float y = Mathf.Round(targetPosition.y);
                float z = Mathf.Round(targetPosition.z);
                transform.position = new Vector3(x, y, z);
                moving = false;
                return;
            }
            transform.position += (targetPosition - startPosition) * moveSpeed * Time.deltaTime;
            return;
        } else {
            RaycastHit[] hits = Physics.RaycastAll(
                    transform.position + Vector3.up * 0.5f,
                    Vector3.down,
                    maxFallCastDistance,
                    walkableMask
            );
```    

In this if statement we check to see if the raycasts have hit anything, this will loop so that we can check whether the player is currently falling or not.

``` 
            if (hits.Length > 0) {
                int topCollider = 0;
                for (int i = 0; i < hits.Length; i++) {
                    if (hits[topCollider].collider.bounds.max.y < hits[i].collider.bounds.max.y)
                        topCollider = i;
                }
                if (hits[topCollider].distance > 1f) {
                    targetFallHeight = transform.position.y - hits[topCollider].distance + 0.5f;
                    falling = true;
                }
            } else {
                targetFallHeight = -Mathf.Infinity;
                falling = true;
            }
        }
```

Allows the player to move in WASD directions as well having the ability to move up elevated terrain.

```
        if (Input.GetKeyDown(KeyCode.W)) {
            if (CanMove(Vector3.forward)) {
                targetPosition = transform.position + cameraRotator.transform.forward;
                startPosition = transform.position;
                moving = true;
            } else if (CanMoveUp(Vector3.forward)) {
                targetPosition = transform.position + cameraRotator.transform.forward + Vector3.up;
                startPosition = transform.position;
                moving = true;
            }
        } else if (Input.GetKeyDown(KeyCode.S)) {
            if (CanMove(Vector3.back)) {
                targetPosition = transform.position - cameraRotator.transform.forward;
                startPosition = transform.position;
                moving = true;
            } else if (CanMoveUp(Vector3.back)) {
                targetPosition = transform.position - cameraRotator.transform.forward + Vector3.up;
                startPosition = transform.position;
                moving = true;
            }
        } else if (Input.GetKeyDown(KeyCode.A)) {
            if (CanMove(Vector3.left)) {
                targetPosition = transform.position - cameraRotator.transform.right;
                startPosition = transform.position;
                moving = true;
            } else if (CanMoveUp(Vector3.left)) {
                targetPosition = transform.position - cameraRotator.transform.right + Vector3.up;
                startPosition = transform.position;
                moving = true;
            }
        } else if (Input.GetKeyDown(KeyCode.D)) {
            if (CanMove(Vector3.right)) {
                targetPosition = transform.position + cameraRotator.transform.right;
                startPosition = transform.position;
                moving = true;
            } else if (CanMoveUp(Vector3.right)) {
                targetPosition = transform.position + cameraRotator.transform.right + Vector3.up;
                startPosition = transform.position;
                moving = true;
            }
        }
    }
    
```

This portion of the script allows us to check if the player is able to move or not, this is dependent on whether there is an object hitting the raycasts or not
We also use these raycasts to see if there is an object blocking the player from moving to a higher piece of terrain.

```
    bool CanMove(Vector3 direction) {
        if (direction.z != 0) {
            if (Physics.Raycast(zAxisOriginA, direction, rayLength)) return false;
            if (Physics.Raycast(zAxisOriginB, direction, rayLength)) return false;
        }
        else if (direction.x != 0) {
            if (Physics.Raycast(xAxisOriginA, direction, rayLength)) return false;
            if (Physics.Raycast(xAxisOriginB, direction, rayLength)) return false;
        }
        return true;
    }
    bool CanMoveUp(Vector3 direction) {
        if (Physics.Raycast(transform.position + Vector3.up * 0.5f, Vector3.up, 1f, collidableMask))
            return false;
        if (Physics.Raycast(transform.position + Vector3.up * 1.5f, direction, 1f, collidableMask))
            return false;
        if (Physics.Raycast(transform.position + Vector3.up * 0.5f, direction, 1f, walkableMask))
            return true;
        return false;
    }
    
```

If the player steps onto an object without the walkable layer mask then the game pushes the player onto the nearest object with a walkable layer mask.

```
    void OnCollisionEnter(Collision other) {
        if (falling && (1 << other.gameObject.layer & walkableMask) == 0) {
            Vector3 direction = Vector3.zero;
            Vector3[] directions = { Vector3.forward, Vector3.right, Vector3.back, Vector3.left };
            for (int i = 0; i < 4; i++) {
                if (Physics.OverlapSphere(transform.position + directions[i], 0.1f).Length == 0) {
                    direction = directions[i];
                    break;
                }
            }
            transform.position += direction;
        }
    }
}
```
