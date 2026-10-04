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
	var p1: PuzzleState = parse_json(leveljson)
	print(leveljson, "\n", p1)
	draw_puzzle(p1)
	
	# NOTE: deep copy DOES NOT WORK. you need to write it yourself :(
	
	#var p2: PuzzleState = p1.duplicate_deep(2) # Resource.DeepDuplicateMode.DEEP_DUPLICATE_ALL
	#
	#print(p1, "\n", p1.vertices.size())
	#print(p2, "\n", p2.vertices.size())
	#print(p1 == p2)
	#p2.vertices[0].enemies.append(69)
	#print(p1.vertices[0].enemies, "\n", p2.vertices[0].enemies)
	
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
		
		#for i in range(vv.enemies.size()):
			#var ee := find_enemy_by_uuid(puzzle.enemies, vv.enemies[i])
			#assert(ee != null)
			#var ee_mi: MeshInstance3D = MeshInstance3D.new()
			#ee_mi.mesh = SphereMesh.new()
			#var ee_mat: StandardMaterial3D = StandardMaterial3D.new()
			#if (ee.type == Enemy.EnemyType.STATIONARY):
				#ee_mat.albedo_color = Color.AQUA
			#elif (ee.type == Enemy.EnemyType.SNIPER):
				#ee_mat.albedo_color = Color.DARK_GREEN
			#ee_mi.material_override = ee_mat
			#ee_mi.position = Vector3(vv.logical_location.x, 0.3 + i * 0.2, vv.logical_location.y)
			#ee_mi.scale = Vector3(0.2, 0.2, 0.2)
			#add_child(ee_mi)
			
	# more optimized enemy drawing code?
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

#static func find_enemy_by_uuid(enemies: Array[Enemy], uuid: int) -> Enemy:
	#var lo := 0
	#var hi := enemies.size()
#
	#while lo < hi:
		#var mid := (lo + hi) / 2
		#var enemy := enemies[mid]
#
		#if enemy.uuid < uuid:
			#lo = mid + 1
		#else:
			#hi = mid
#
	#if lo < enemies.size() and enemies[lo].uuid == uuid:
		#return enemies[lo]
#
	#return null
	
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
	
	# convert ascii into graph
	for h in range(0, 2*height, 2):
		var str: String = ascii[h]
		for b in range(0, 2*base, 2):
			var char: String = str[b]
			
			# empty space means no vertex here!
			if char == ' ':
				continue
			
			# otherwise, construct vertex from its definiton
			var new_vertex: Vertex = Vertex.new()
			var additional_info: Dictionary = defn[char]
			
			# location is determined as cartesian coords. (0,0) in lower left corner of 2d array
			new_vertex.logical_location = Vector2i(b / 2, h / 2 - height + 1)
			
			# assign a unique uuid by incrementing counter
			new_vertex.uuid = vertex_counter
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
			
			# now construct the edges...
			
			p.vertices.append(new_vertex)
			
	return p
