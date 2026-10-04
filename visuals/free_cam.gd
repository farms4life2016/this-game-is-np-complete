extends Camera3D
## Editor-style free camera (Godot 4).
## Hold RMB: look around + WASD (forward/back/strafe) + Q/E (down/up).
## Mouse wheel (while RMB held): change speed. Shift: boost.

@export var move_speed: float = 8.0          # metres per second
@export var mouse_sensitivity: float = 0.003  # radians per pixel

var _yaw: float = 0.0
var _pitch: float = 0.0
var _looking: bool = false


func _ready() -> void:
	# Initialise from the camera's current rotation so it doesn't snap
	_yaw = rotation.y
	_pitch = rotation.x


func _unhandled_input(event: InputEvent) -> void:
	if event is InputEventMouseButton:
		match event.button_index:
			MOUSE_BUTTON_RIGHT:
				_looking = event.pressed
				Input.mouse_mode = Input.MOUSE_MODE_CAPTURED if _looking else Input.MOUSE_MODE_VISIBLE

	elif event is InputEventMouseMotion and _looking:
		_yaw -= event.relative.x * mouse_sensitivity
		_pitch = clampf(_pitch - event.relative.y * mouse_sensitivity, deg_to_rad(-89.0), deg_to_rad(89.0))
		rotation = Vector3(_pitch, _yaw, 0.0)  # no roll


func _process(delta: float) -> void:
	if not _looking:
		return

	# x = strafe, y = forward/back (forward is -1, matching Camera3D's -Z)
	var planar := Input.get_vector("move_left", "move_right", "move_forward", "move_backward")
	var dir := Vector3(planar.x, 0.0, planar.y)

	# Rotate into world space so forward follows where the camera is facing
	var velocity := global_transform.basis * dir

	# Up/down in world space, independent of look direction
	velocity += Vector3.UP * Input.get_axis("move_down", "move_up")

	if velocity == Vector3.ZERO:
		return

	global_position += velocity.normalized() * move_speed * delta
