class_name Simulator extends RefCounted

# returns a deep copy of the puzzle's state after the move
# plus a list of events that occured
static func logic_step_directional(initial: PuzzleState, action: Consts.Direction) -> Turn:
	# check if action is legal; we may remove this if the caller does this check for us...
	var new_homebase_idx := is_move_legal(initial, initial.cscientist.homebase, action)
	if new_homebase_idx == -1:
		assert(1 == 0, "illegal move!")
	
	return logic_step(initial, new_homebase_idx)

static func logic_step(initial: PuzzleState, new_scientist_homebase_idx: int) -> Turn:
	
	# idk if we need this check
	assert(initial.status == PuzzleState.GameStatus.IN_PROGRESS)
	
	var after := initial.deep_clone()
	
	# --- PHASE 1: PLAYER MOVES ---
	var phase1_events: Array[Dictionary] = []
	
	# move player in direction specified, eliminating enemies on the way
	phase1_events.append_array(move_player(after, new_scientist_homebase_idx, true))
	
	# if player landed on exit, game is done
	# note that this happens *before* enemies have a chance to react!!!
	if (after.status == PuzzleState.GameStatus.ESCAPED):
		return Turn.new(after, phase1_events)
	
	# --- PHASE 2: APPLY DRIFT ---
	var phase2_events: Array[Dictionary] = []
	if (after.drift_direction != Consts.Direction.ZERO):
		
		# does the player need to drift?
		var drift_dest_idx := is_move_legal(after, after.cscientist.homebase, after.drift_direction)
		if (drift_dest_idx == Vertex.OUTTA_BOUNDS): # if no, then only enemies move and can capture the player
			phase2_events.append_array(drift_alive_enemies(after, true))
			
		else: # if yes, player's drift might eliminate enemies

			# drift enemies first since player can activate drift-change items,
			# breaking the illusion that everyone drifts at the same time
			phase2_events.append_array(drift_alive_enemies(after, false))
			# enemies will temporarily share same tile as player in memory.
			# move_player function only kills enemies on the new tile
			# and does not check for enemies on the old tile
			phase2_events.append_array(move_player(after, drift_dest_idx, true))
				
			# if player landed on exit, game is done
			if (after.status == PuzzleState.GameStatus.ESCAPED):
				return Turn.new(after, phase1_events, phase2_events)
				
	# end if (zero-direction check)
	
	# --- PHASE 3: ENEMIES REACT ---
	var phase3_events: Array[Dictionary] = []
	
	# loop through all alive enemies
	for i in range(after.enemies.size()):
		var enemy = after.enemies[i]
		if (enemy.homebase == Vertex.OUTTA_BOUNDS):
			continue
		
		# switch on enemy type
		if (enemy.type == Enemy.Type.STATIONARY):
			
			# stationary enemy will attempt to move onto player's vertex if player is
			# in front of it, eliminating the player
			var infront_idx = is_move_legal(after, enemy.homebase, enemy.facing)
			if (infront_idx != Vertex.OUTTA_BOUNDS and after.vertices[infront_idx].has_scientist):
				phase3_events.append_array(move_enemy(after, i, infront_idx, true))
				return Turn.new(after, phase1_events, phase2_events, phase3_events)
				
		elif (enemy.type == Enemy.Type.SNIPER):
			
			# snipers can eliminate the player if they are the first thing in their beam.
			# other enemies can body block the beam
			var infront_idx = is_move_legal(after, enemy.homebase, enemy.facing)
			while (infront_idx != Vertex.OUTTA_BOUNDS):
				if (after.vertices[infront_idx].has_scientist):
					after.status = PuzzleState.GameStatus.CAPTURED
					var player_captured_event := {
						"type": Consts.PuzzleEvent.PLAYER_CAPTURED,
						"enemy_from": enemy.homebase,
						"player_location": infront_idx,
						"weapon_used": "laser",
						"new_status": PuzzleState.GameStatus.CAPTURED
					}
					phase3_events.append(player_captured_event)
					return Turn.new(after, phase1_events, phase2_events, phase3_events)
					
				elif (after.vertices[infront_idx].enemies.size() > 0):
					break # body blocked
				
				# check the next vertex along the sniper beam
				infront_idx = is_move_legal(after, infront_idx, enemy.facing)
				
		# end if? what to do about unknown enemy types?
	# end for-enemy loop
	
	return Turn.new(after, phase1_events, phase2_events, phase3_events)

# checks if a move is legal. if yes, returns the index of the node that you would move to.
# if not, returns Vertex.OUTTA_BOUNDS
static func is_move_legal(puzzle: PuzzleState, vertex_idx: int, direction: Consts.Direction) -> int:
	var vertex := puzzle.vertices[vertex_idx]
	if (direction == Consts.Direction.LEFT and vertex.left_vertex != Vertex.OUTTA_BOUNDS):
		return vertex.left_vertex
	elif (direction == Consts.Direction.RIGHT and vertex.right_vertex != Vertex.OUTTA_BOUNDS):
		return vertex.right_vertex
	elif (direction == Consts.Direction.UP and vertex.up_vertex != Vertex.OUTTA_BOUNDS):
		return vertex.up_vertex
	elif (direction == Consts.Direction.DOWN and vertex.down_vertex != Vertex.OUTTA_BOUNDS):
		return vertex.down_vertex
	else:
		return Vertex.OUTTA_BOUNDS
	
static func drift_alive_enemies(puzzle: PuzzleState, eliminate_player: bool) -> Array[Dictionary]:
	var event_list: Array[Dictionary] = []
	
	# loop through all alive enemies
	for i in range(puzzle.enemies.size()):
		var enemy := puzzle.enemies[i]
		if (enemy.homebase == Vertex.OUTTA_BOUNDS):
			continue
		
		# get drift destination and move enemy. otherwise stay put
		var enemy_drift_dest_idx := is_move_legal(puzzle, enemy.homebase, puzzle.drift_direction)
		if (enemy_drift_dest_idx != Vertex.OUTTA_BOUNDS):
			event_list.append_array(move_enemy(puzzle, i, enemy_drift_dest_idx, eliminate_player))
	# end for-i
	
	return event_list
	
# moves an enemy to another vertex. this assumes that new_base_idx is valid AND enemy is alive!
static func move_enemy(puzzle: PuzzleState, enemy_idx: int, new_base_idx: int, eliminate_player: bool) -> Array[Dictionary]:
	var event_list: Array[Dictionary] = []
	
	var enemy := puzzle.enemies[enemy_idx]
	var vertices := puzzle.vertices
	var old_home := vertices[enemy.homebase]
	var new_home := vertices[new_base_idx]
	
	# remove self from homebase enemies list
	# and add self to newbase enemies list.
	# keep arrays sorted!
	var idx = old_home.enemies.bsearch(enemy.uuid)
	assert(old_home.enemies[idx] == enemy.uuid)
	old_home.enemies.remove_at(idx)
	
	idx = new_home.enemies.bsearch(enemy.uuid)
	new_home.enemies.insert(idx, enemy.uuid)
	
	# update your own homebase
	enemy.homebase = new_base_idx
	
	# return an event?
	var enemy_moved_event := {
		"type": Consts.PuzzleEvent.ENEMY_MOVED,
		"old_homebase": old_home.uuid,
		"new_homebase": new_home.uuid
	}
	event_list.append(enemy_moved_event)
	
	if (eliminate_player):
		
		# elimiate the player if they are on the new vertex
		if (new_home.has_scientist):
			puzzle.status = PuzzleState.GameStatus.CAPTURED
			var player_captured_event := {
				"type": Consts.PuzzleEvent.PLAYER_CAPTURED,
				"enemy_from": old_home.uuid,
				"player_location": new_home.uuid,
				"weapon_used": "movement",
				"new_status": PuzzleState.GameStatus.CAPTURED
			}
			event_list.append(player_captured_event)
	
	return event_list

# move player to a another vertex. this assumes that new_base_idx is valid!
# also eliminates all enemies on new tile, uses up item, and checks win condition
static func move_player(puzzle: PuzzleState, new_base_idx: int, eliminate_enemies: bool) -> Array[Dictionary]:
	var event_list: Array[Dictionary] = []
	
	var scientist := puzzle.cscientist
	var vertices := puzzle.vertices
	var old_home := vertices[scientist.homebase]
	var new_home := vertices[new_base_idx]
	
	# remove self from homebase and add self to newbase
	old_home.has_scientist = false
	new_home.has_scientist = true
	
	# update own home base within self
	scientist.homebase = new_base_idx
	
	# return an event?
	var player_moved_event := {
		"type": Consts.PuzzleEvent.PLAYER_MOVED,
		"old_homebase": old_home.uuid,
		"new_homebase": new_home.uuid
	}
	event_list.append(player_moved_event)
	
	if (eliminate_enemies):
		
		# eliminate all enemies on that vertex
		for uuid in new_home.enemies:
			
			# remove from puzzle's enemy list
			puzzle.enemies[uuid].homebase = Vertex.OUTTA_BOUNDS
			
			var enemy_eliminated_event := {
				"type": Consts.PuzzleEvent.ENEMY_ELIMINATED,
				"player_from": old_home.uuid,
				"killed_at": new_home.uuid
			}
			event_list.append(enemy_eliminated_event)
		
		new_home.enemies.clear()  # remove all enemies ids from vertex too
	
	# use-up any items
	if (new_home.has_item):
		var old_drift := puzzle.drift_direction
		puzzle.drift_direction = new_home.item
		new_home.has_item = false
		
		var item_consumed_event := {
			"type": Consts.PuzzleEvent.ITEM_CONSUMED,
			"location": new_home.uuid,
			"prev_drift": old_drift,
			"current_drift": puzzle.drift_direction
		}
		event_list.append(item_consumed_event)
	
	# check win-condition
	if (new_home.is_exit):
		puzzle.status = PuzzleState.GameStatus.ESCAPED
		var player_escaped_event := {
			"type": Consts.PuzzleEvent.PLAYER_ESCAPED,
			"location": new_home.uuid,
			"new_status": PuzzleState.GameStatus.ESCAPED
		}
		event_list.append(player_escaped_event)
	
	return event_list
