Setup
1. Import your sprite sheets

Add your sprite sheets into Unity and make sure:

Sprite Mode is set to Multiple
You slice the sprites in the Sprite Editor
2. Create the player

Create a GameObject (for example: Player) and add:

UniversalCharacterController

The required components (Rigidbody2D and BoxCollider2D) are added automatically.

3. Assign animations

In the inspector, assign a sprite sheet to each animation:

Idle
Walk
Run
Jump
Attack

Adjust frame rates if needed.

4. Setup ground check

Create an empty GameObject as a child of the player (for example: GroundCheck).

Position it at the character’s feet.

In the inspector:

Assign this object to the groundCheck field
Set a Ground layer and assign it to your ground objects
Assign that layer to the groundLayer field
Usage

Once everything is set up, the controller handles movement and animation automatically.

A/D or Arrow Keys → Move
Space → Jump

The animator updates based on:

Horizontal movement
Direction
Grounded state

No extra scripting is needed.
