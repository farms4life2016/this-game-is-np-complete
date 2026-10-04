extends Node

var leveljson = {
	"name": "Hello World",
	"version": "ASCII schema v1.0",
	"base": 4,
	"height": 3,
	"drift": "zero",
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
	draw_puzzle(p1)

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
	for ee in puzzle.enemies:
		var home := puzzle.vertices[ee.homebase]
		var idx := home.enemies.bsearch(ee.uuid)
		# assert(home.enemies[idx] == ee.uuid)
		
		var ee_mi: MeshInstance3D = MeshInstance3D.new()
		ee_mi.mesh = SphereMesh.new()
		var ee_mat: StandardMaterial3D = StandardMaterial3D.new()
		if (ee.type == Enemy.EnemyType.STATIONARY):
			ee_mat.albedo_color = Color.AQUA
		elif (ee.type == Enemy.EnemyType.SNIPER):
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
	

	
static func parse_json(input_dict: Dictionary) -> PuzzleState:
	var p: PuzzleState = PuzzleState.new()
	var base: int = input_dict["base"]
	var height: int = input_dict["height"]
	var ascii: Array = input_dict["ascii"]
	var defn: Dictionary = input_dict["definitions"]
	
	var vertex_counter: int = 0
	var enemy_counter: int = 0
	
	# parse the drift direction
	var drift_dir: String = input_dict["drift"]
	p.drift_direction = Consts.string2direction[drift_dir]
	
	# assumption: the puzzle starts out in_progress state
	p.status = PuzzleState.GameStatus.IN_PROGRESS
	p.turn = 0
	
	var previous_row: Dictionary[int, int] = {}
	
	# convert ascii into graph
	for h in range(0, 2*height, 2):
		
		var row_str: String = ascii[h]
		var current_row: Dictionary[int, int] = {}
		
		for b in range(0, 2*base, 2):
			var vertex_char: String = row_str[b]
			
			# empty space means no vertex here!
			if vertex_char == ' ':
				continue
			
			# otherwise, construct vertex from its definiton
			var new_vertex: Vertex = Vertex.new()
			var additional_info: Dictionary = defn[vertex_char]
			
			# location is determined as cartesian coords. (0,0) in lower left corner of 2d array
			@warning_ignore_start("integer_division") # b and h are guarenteed even ints! stfu!
			new_vertex.logical_location = Vector2i(b / 2, h / 2 - height + 1)
			@warning_ignore_restore("integer_division")
			
			# assign a unique uuid by incrementing counter
			new_vertex.uuid = vertex_counter
			current_row.set(b, new_vertex.uuid)
			vertex_counter += 1
			
			# create item
			if (additional_info.has("item")):
				var value: String = additional_info["item"]
				# TODO: replace these hardcoded magic strings with CONSTANTS 
				if (value == "none"):
					new_vertex.has_item = false
					new_vertex.item = Consts.Direction.ZERO
				else:
					new_vertex.has_item = true
					new_vertex.item = Consts.string2direction[value]
					
			else: # default if omitted: no item
				new_vertex.has_item = false
				new_vertex.item = Consts.Direction.ZERO
			
			# create enemies
			if (additional_info.has("enemies")):
				var enemies: Array = additional_info["enemies"]
				for enemy in enemies:
					var new_enemy: Enemy = Enemy.new()
					var type: String = enemy["type"]
					var facing: String = enemy["facing"]
					
					# assign a uuid
					new_enemy.uuid = enemy_counter
					enemy_counter += 1
					
					# parse type and facing direction (str -> enum)
					new_enemy.type = Enemy.string2enemy_type[type]
					new_enemy.facing = Consts.string2direction[facing]
					
					# add enemy to the vertex and puzzle state
					new_enemy.homebase = new_vertex.uuid
					new_vertex.enemies.append(new_enemy.uuid)
					p.enemies.append(new_enemy)
					
			else: # default if omitted: no enemies
				new_vertex.enemies = []
				
			# create player. we will assume there is only one in the input!
			if (additional_info.has("player")):
				var has_player: bool = additional_info["player"]
				if (has_player):
					var scientist: CScientist = CScientist.new()
					scientist.homebase = new_vertex.uuid
					new_vertex.has_scientist = true
					p.cscientist = scientist
				else:
					new_vertex.has_scientist = false
					
			else: # default if omitted: no scientist here!
				new_vertex.has_scientist = false
			# NOTE: if someone DOES add two scientists to the board,
			#       then this parser will break on has_scientist
			
			# set exit status
			if (additional_info.has("exit")):
				var exit_bool: bool = additional_info["exit"]
				new_vertex.is_exit = exit_bool
				# equivalent to:
				#if (exit_bool):
					#new_vertex.is_exit = true
				#else:
					#new_vertex.is_exit = false
					
			else: # default if omitted: not an exit
				new_vertex.is_exit = false
			
			# now construct the edges. check for left and up neighbours only,
			# but make it two-way. this guarentees all edges covered
			new_vertex.left_vertex = Consts.OUTTA_BOUNDS
			new_vertex.right_vertex = Consts.OUTTA_BOUNDS
			new_vertex.up_vertex = Consts.OUTTA_BOUNDS
			new_vertex.down_vertex = Consts.OUTTA_BOUNDS
			# i have no idea if -1 will help me catch indexouttabounds, cuz this is Python
			
			if (b > 0): # check left. missing edges are left blank
				if (row_str[b-1] != ' '):
					# take advantage of the fact that uuids increment by 1 from left to right
					# so your left neighbour has uuid one less than your own uuid
					var left_uuid := new_vertex.uuid - 1
					new_vertex.left_vertex = left_uuid
					p.vertices[left_uuid].right_vertex = new_vertex.uuid
			
			if (h > 0): # check up.
				if (ascii[h-1][b] != ' '):
					# we need to store extra information to get nodes in the previous row
					var up_uuid := previous_row[b]
					new_vertex.up_vertex = up_uuid
					p.vertices[up_uuid].down_vertex = new_vertex.uuid
			
			p.vertices.append(new_vertex)
		
		# end for-b
		
		# update previous row
		previous_row = current_row
	
	# end for-h
			
	return p
