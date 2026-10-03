class_name PuzzleState extends Resource

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
