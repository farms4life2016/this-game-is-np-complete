class_name Vertex extends Resource

var uuid: int
var logical_location: Vector2i

# store raw uuid, lookup actual vertex in array.
# then, -1 represents "no vertex"
var left_vertex: int
var right_vertex: int
var up_vertex: int
var down_vertex: int

# represents the drift-changing item's direction
# left/right/up/down is self-explanatory
# zero vector means cancel drift
var has_item: bool
var item: Consts.Direction

# stores uuid of enemies on this vertex, assume sorted
var enemies: Array[int]

var has_scientist: bool

var is_exit: bool

func deep_clone() -> Vertex:
	var ans: Vertex = Vertex.new()
	ans.uuid = uuid
	ans.logical_location = Vector2i(logical_location)
	ans.left_vertex = left_vertex
	ans.right_vertex = right_vertex
	ans.up_vertex = up_vertex
	ans.down_vertex = down_vertex
	ans.has_item = has_item
	ans.item = item
	ans.enemies = enemies.duplicate(true)
	ans.has_scientist = has_scientist
	ans.is_exit = is_exit
	return ans
	
