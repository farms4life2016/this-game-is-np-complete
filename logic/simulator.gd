class_name Simulator extends RefCounted

func logic_step(initial: PuzzleState, action: Consts.Direction):
	var after := initial.deep_clone()
	
#func move_enemy(newbase: Vertex):
	## remove self from homebase enemies list
	## and add self to newbase enemies list.
	## keep arrays sorted!
	#var idx = homebase.enemies.bsearch(uuid)
	#homebase.enemies.remove_at(idx)
	#
	#idx = newbase.enemies.bsearch(uuid)
	#newbase.enemies.insert(idx, uuid)

# move player to a another vertex. this assumes that new_base_idx is valid!
# also eliminates all enemies on new tile, uses up item, and checks win condition
func move_player(puzzle: PuzzleState, new_base_idx: int) -> Array[Dictionary]:
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
		"type": "player_moved",
		"old_homebase": old_home.uuid,
		"new_homebase": new_home.uuid
	}
	event_list.append(player_moved_event)
	
	# eliminate all enemies on that vertex
	for uuid in new_home.enemies:
		
		# remove from puzzle's enemy list
		puzzle.enemies[uuid].homebase = Enemy.DEAD_CHAT
		
		var enemy_eliminated_event := {
			"type": "enemy_eliminated",
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
			"type": "item_consumed",
			"location": new_home.uuid,
			"prev_drift": old_drift,
			"current_drift": puzzle.drift_direction
		}
		event_list.append(item_consumed_event)
	
	# check win-condition
	if (new_home.is_exit):
		puzzle.status = PuzzleState.GameStatus.ESCAPED
		var game_status_change_event := {
			"type": "game_status_change",
			"location": new_home.uuid,
			"new_status": PuzzleState.GameStatus.ESCAPED
		}
		event_list.append(game_status_change_event)
	
	return event_list
