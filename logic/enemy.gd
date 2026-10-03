class_name Enemy extends Resource

var uuid: int

var facing: Vector2i

enum EnemyType {
	STATIONARY,
	SNIPER
}

var type: EnemyType

var homebase: Vertex

func move(newbase: Vertex):
	# remove self from homebase enemies list
	# and add self to newbase enemies list.
	# keep arrays sorted!
	var idx = homebase.enemies.bsearch(uuid)
	homebase.enemies.remove_at(idx)
	
	idx = newbase.enemies.bsearch(uuid)
	newbase.enemies.insert(idx, uuid)
