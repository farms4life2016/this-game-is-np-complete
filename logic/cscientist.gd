class_name CScientist extends Resource
# represents the player, a computer scientist

var homebase: int

func deep_clone() -> CScientist:
	var ans: CScientist = CScientist.new()
	ans.homebase = homebase
	return ans

#func move(newbase: Vertex):
	## remove self from homebase
	## and add self to newbase
	#homebase.has_scientist = false
	#newbase.has_scientist = true
