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
var item: Vector2i

# stores uuid of enemies on this vertex, assume sorted
var enemies: Array[int]

var has_scientist: bool

var is_exit: bool
