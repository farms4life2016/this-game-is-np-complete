class_name PuzzleState extends Resource

var base: int
var height: int

# sorted by uuid
var vertices: Array[Vertex]

var enemies: Array[Enemy]

var cscientist: CScientist

var drift_direction: Consts.Direction

var turn: int

enum GameStatus {
	ESCAPED,
	CAPTURED,
	IN_PROGRESS
}

var status: GameStatus

func deep_clone() -> PuzzleState:
	var ans: PuzzleState = PuzzleState.new()
	for vv in vertices:
		ans.vertices.append(vv.deep_clone())
	for ee in enemies:
		ans.enemies.append(ee.deep_clone())
	ans.cscientist = cscientist.deep_clone()
	ans.drift_direction = drift_direction
	ans.turn = turn
	ans.status = status
	ans.base = base
	ans.height = height
	return ans
