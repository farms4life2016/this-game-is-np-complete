extends Node3D

@export var lvl_idx: int = 0

var vertices: Array[Node3D]
var edges: Array[Node3D]
var enemies: Array[Node3D]
var player: Node3D
var tabletop: Node3D

var initial: PuzzleState

const tabletop_prefab := preload("res://visuals/tabletop.tscn")
const vertex_prefab := preload("res://visuals/vertex.tscn")

const SCALE_FACTOR := 5

# Called when the node enters the scene tree for the first time.
func _ready() -> void:
	
	initial = Levels.LEVELS[lvl_idx]
	
	# create the tabletop
	tabletop = tabletop_prefab.instantiate()
	tabletop.scale = Vector3(SCALE_FACTOR * initial.base, 1, SCALE_FACTOR * initial.height)
	tabletop.position = SCALE_FACTOR * Vector3(initial.base / 2.0 - 0.5, 0, initial.height / -2.0 + 0.5)
	add_child(tabletop)
	
	for vv in initial.vertices:
		var vertex: Node3D = vertex_prefab.instantiate()
		vertex.position = SCALE_FACTOR * Vector3(vv.logical_location.x, 0, vv.logical_location.y)
		print(vertex)
		vertices.append(vertex)
		add_child(vertex)
	
	pass # Replace with function body.


# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta: float) -> void:
	pass
