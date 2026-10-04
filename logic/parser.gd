class_name Parser extends RefCounted

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
