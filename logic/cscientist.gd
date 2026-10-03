class_name CScientist extends Resource
# represents the player, a computer scientist

var homebase: Vertex

func move(newbase: Vertex):
	# remove self from homebase
	# and add self to newbase
	homebase.has_scientist = false
	newbase.has_scientist = true
