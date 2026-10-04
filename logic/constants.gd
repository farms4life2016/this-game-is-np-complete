class_name Consts extends RefCounted

enum Direction {
	LEFT,
	RIGHT,
	UP,
	DOWN,
	ZERO
}

const string2direction = {
	"left": Direction.LEFT,
	"right": Direction.RIGHT,
	"up": Direction.UP,
	"down": Direction.DOWN,
	"zero": Direction.ZERO
}

# converts a direction to a rotation for the Y-axis
# assumes that at zero rotation, the arrow points to the right
const direction2rotation = {
	Direction.RIGHT: 0,
	Direction.UP: PI/2,
	Direction.LEFT: PI,
	Direction.DOWN: -PI/2
}
const OUTTA_BOUNDS = -1
