extends Node

var leveljson = {
	"name": "Hello World",
	"version": "ASCII schema v1.0",
	"base": 4,
	"height": 3,
	"drift": "none",
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
			"item": "cancel"
		}
	}
}

func _ready() -> void:
	# run stuff here
	var puzzle: PuzzleState = parse_json(leveljson)
	print(leveljson, "\n", puzzle)
	
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
	if (drift_dir == "left"):
		p.drift_direction = Vector2i.LEFT
	elif (drift_dir == "right"):
		p.drift_direction = Vector2i.RIGHT
	elif (drift_dir == "up"):
		p.drift_direction = Vector2i.UP
	elif (drift_dir == "down"):
		p.drift_direction = Vector2i.DOWN
	elif (drift_dir == "none"):
		p.drift_direction = Vector2i.ZERO
	else:
		assert(1 == 0, "Invalid value for drift_dir: " + drift_dir)
	
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
			new_vertex.logical_location = Vector2i(b / 2, h - h / 2 - 1)
			
			# assign a unique uuid by incrementing counter
			new_vertex.uuid = vertex_counter
			vertex_counter += 1
			
			# create item
			if (additional_info.has("item")):
				var value: String = additional_info["item"]
				# TODO: replace these hardcoded magic strings with CONSTANTS 
				if (value == "none"):
					new_vertex.has_item = false
					new_vertex.item = Vector2i.ZERO
				elif (value == "left"):
					new_vertex.has_item = true
					new_vertex.item = Vector2i.LEFT
				elif (value == "right"):
					new_vertex.has_item = true
					new_vertex.item = Vector2i.RIGHT
				elif (value == "up"):
					new_vertex.has_item = true
					new_vertex.item = Vector2i.UP
				elif (value == "down"):
					new_vertex.has_item = true
					new_vertex.item = Vector2i.DOWN
				elif (value == "cancel"):
					new_vertex.has_item = true
					new_vertex.item = Vector2i.ZERO
				else:
					assert(1 == 0, "Invalid value for item: " + value)
					
			else: # default if omitted: no item
				new_vertex.has_item = false
				new_vertex.item = Vector2i.ZERO
			
			# create enemies
			if (additional_info.has("enemies")):
				var enemies: Array = additional_info["enemies"]
				for i in range(enemies.size()):
					var new_enemy: Enemy = Enemy.new()
					var type: String = enemies[i]["type"]
					var facing: String = enemies[i]["facing"]
					
					# assign a uuid
					new_enemy.uuid = enemy_counter
					enemy_counter += 1
					
					# parse type
					if (type == "stationary"):
						new_enemy.type = Enemy.EnemyType.STATIONARY
					elif (type == "sniper"):
						new_enemy.type = Enemy.EnemyType.SNIPER
					else:
						assert(1 == 0, "Invalid value for enemy.type: " + type)
					
					# parse facing direction
					if (facing == "left"):
						new_enemy.facing = Vector2i.LEFT
					elif (facing == "right"):
						new_enemy.facing = Vector2i.RIGHT
					elif (facing == "up"):
						new_enemy.facing = Vector2i.UP
					elif (facing == "down"):
						new_enemy.facing = Vector2i.DOWN
					else:
						assert(1 == 0, "Invalid value for enemy.facing: " + facing)
						
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
