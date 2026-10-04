extends Node

var leveljson = {
	"name": "Hello World",
	"version": "ASCII schema v1.0",
	"base": 4,
	"height": 3,
	"drift": "right",
	"ascii": [
		"X   O-E",
		"|   | |",
		"O-O-O-B",
		"| | |  ",
        "P-O-L  "
	],
	"definitions": {
		"O": {
			"item": "none",
			"enemies": [],
			"player": false,
			"exit": false
		},
		"P": {
			"player": true
		},
		"E": {
			"exit": true
		},
		"B": {
			"enemies": [
				{
					"type": "stationary",
					"facing": "left"
				},
				{
					"type": "sniper",
					"facing": "up"
				}
			]
		},
		"L": {
			"item": "left"
		},
		"X": {
			"item": "zero"
		}
	}
}

const ARROW = preload("res://testing/arrow.tscn")

func _ready() -> void:
	# run stuff here
	var p1: PuzzleState = Parser.parse_json(leveljson)
	print(leveljson, "\n", p1)
	# draw_puzzle(p1)
	var p2 := Simulator.logic_step_directional(p1, Consts.Direction.RIGHT)
	
	print(p2.puzzle)
	print(p2.phase1_events)
	print(p2.phase2_events)
	print(p2.phase3_events)
	draw_puzzle(p2.puzzle)
	
	#p2 = Simulator.logic_step_directional(p2[Consts.Turn.PUZZLE], Consts.Direction.RIGHT)
	

# this code can be optimized since all edges are rectilinear & same length
func draw_edge(v1: Vector2, v2: Vector2, c: Color, thickness: float, yoffset: float) -> MeshInstance3D:
	var dir := v2 - v1
	var length := dir.length()

	# A flat rectangle: length along local X, thickness along local Z
	var plane := PlaneMesh.new()
	plane.size = Vector2(length, thickness)

	var mat := StandardMaterial3D.new()
	mat.albedo_color = c
	mat.cull_mode = BaseMaterial3D.CULL_DISABLED  # visible from above and below
	plane.material = mat

	var mi := MeshInstance3D.new()
	mi.mesh = plane

	# Center on the midpoint of the edge (tiny y offset avoids z-fighting
	# with anything else drawn at y = 0)
	var mid := (v1 + v2) * 0.5
	mi.position = Vector3(mid.x, yoffset, mid.y)

	mi.rotation.y = atan2(-dir.y, dir.x)

	add_child(mi)
	return mi

# this function is "static", i.e. freely moved into any other script
func draw_puzzle(puzzle: PuzzleState) -> void:
	
	for vv in puzzle.vertices:
		# draw vertex locations
		var mi: MeshInstance3D = MeshInstance3D.new()
		mi.mesh = TorusMesh.new()
		var mat: StandardMaterial3D = StandardMaterial3D.new()
		if (vv.is_exit): # exit indicator
			mat.albedo_color = Color.YELLOW
		else:
			mat.albedo_color = Color.BLACK
		mi.material_override = mat
		mi.position = Vector3(vv.logical_location.x, 0, vv.logical_location.y)
		mi.scale = Vector3(0.2, 0.1, 0.2)
		add_child(mi)
		
		if (vv.has_scientist):
			var player_mi: MeshInstance3D = MeshInstance3D.new()
			player_mi.mesh = SphereMesh.new()
			var player_mat: StandardMaterial3D = StandardMaterial3D.new()
			player_mat.albedo_color = Color.BISQUE
			player_mi.material_override = player_mat
			player_mi.position = Vector3(vv.logical_location.x, 0.2, vv.logical_location.y)
			player_mi.scale = Vector3(0.2, 0.2, 0.2)
			add_child(player_mi)
			
		if (vv.has_item):
			var arrow: Node3D = ARROW.instantiate()
			arrow.position = Vector3(vv.logical_location.x, 0.2, vv.logical_location.y)
			var arrow_direction: Consts.Direction = vv.item
			if (arrow_direction != Consts.Direction.ZERO):
				arrow.rotation.y = Consts.direction2rotation[arrow_direction]
			else:
				arrow.rotation.z = PI/2
			add_child(arrow)
			
		# draw edges: left and up edges vs. right and down edges
		if (vv.left_vertex != -1):
			var neighbour := puzzle.vertices[vv.left_vertex]
			var start := Vector2(vv.logical_location)
			var end := Vector2(neighbour.logical_location)
			draw_edge(start, end, Color.CHARTREUSE, 0.05, 0.01)
		if (vv.up_vertex != -1):
			var neighbour := puzzle.vertices[vv.up_vertex]
			var start := Vector2(vv.logical_location)
			var end := Vector2(neighbour.logical_location)
			draw_edge(start, end, Color.CHARTREUSE, 0.05, 0.01)
		
		# realistically we only need to draw the edge from one direction
		# im just drawing the other direction to confirm bidirectional connection
		if (vv.right_vertex != -1):
			var neighbour := puzzle.vertices[vv.right_vertex]
			var start := Vector2(vv.logical_location)
			var end := Vector2(neighbour.logical_location)
			draw_edge(start, end, Color.NAVY_BLUE, 0.15, 0.005)
		if (vv.down_vertex != -1):
			var neighbour := puzzle.vertices[vv.down_vertex]
			var start := Vector2(vv.logical_location)
			var end := Vector2(neighbour.logical_location)
			draw_edge(start, end, Color.NAVY_BLUE, 0.15, 0.005)

	# end for vv loop

	# more optimized enemy drawing code
	# loop through every enemy, get their homebase, check their index within the vertex's array
	for ee in puzzle.enemies:
		var home := puzzle.vertices[ee.homebase]
		var idx := home.enemies.bsearch(ee.uuid)
		# assert(home.enemies[idx] == ee.uuid)
		
		var ee_mi: MeshInstance3D = MeshInstance3D.new()
		ee_mi.mesh = SphereMesh.new()
		var ee_mat: StandardMaterial3D = StandardMaterial3D.new()
		if (ee.type == Enemy.Type.STATIONARY):
			ee_mat.albedo_color = Color.AQUA
		elif (ee.type == Enemy.Type.SNIPER):
			ee_mat.albedo_color = Color.DARK_GREEN
		ee_mi.material_override = ee_mat
		ee_mi.position = Vector3(home.logical_location.x, 0.2 + idx * 0.2, home.logical_location.y)
		ee_mi.scale = Vector3(0.2, 0.2, 0.2)
		add_child(ee_mi)
		
	# draw the drift direction
	var drift_arrow: Node3D = ARROW.instantiate()
	drift_arrow.position = Vector3(-1, 0.2, 1)
	var drift_dir: Consts.Direction = puzzle.drift_direction
	if (drift_dir != Consts.Direction.ZERO):
		drift_arrow.rotation.y = Consts.direction2rotation[drift_dir]
	else:
		drift_arrow.rotation.z = PI/2
	add_child(drift_arrow)
