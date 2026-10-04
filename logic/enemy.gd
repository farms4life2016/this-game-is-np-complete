class_name Enemy extends Resource

var uuid: int

var facing: Consts.Direction

enum Type {
	STATIONARY,
	SNIPER
}

const string2type = {
	"stationary": Type.STATIONARY,
	"sniper": Type.SNIPER
}

var type: Type

# use Vertex.OUTTA_BOUNDS to represent lazy-deleted enemies
# if lazy deletion isn't working, use a dict instead. or some other data structure...
var homebase: int

func deep_clone() -> Enemy:
	var ans: Enemy = Enemy.new()
	ans.uuid = uuid
	ans.facing = facing
	ans.type = type
	ans.homebase = homebase
	return ans
