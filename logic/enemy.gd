class_name Enemy extends Resource

var uuid: int

var facing: Consts.Direction

enum EnemyType {
	STATIONARY,
	SNIPER
}

const string2enemy_type = {
	"stationary": EnemyType.STATIONARY,
	"sniper": EnemyType.SNIPER
}

var type: EnemyType

var homebase: int

func deep_clone() -> Enemy:
	var ans: Enemy = Enemy.new()
	ans.uuid = uuid
	ans.facing = facing
	ans.type = type
	ans.homebase = homebase
	return ans

#func move(newbase: Vertex):
	## remove self from homebase enemies list
	## and add self to newbase enemies list.
	## keep arrays sorted!
	#var idx = homebase.enemies.bsearch(uuid)
	#homebase.enemies.remove_at(idx)
	#
	#idx = newbase.enemies.bsearch(uuid)
	#newbase.enemies.insert(idx, uuid)
