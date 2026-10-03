class_name PuzzleState extends Resource

# sorted by uuid
var vertices: Array[Vertex]

var enemies: Array[Enemy]

var cscientist: CScientist

var drift_direction: Vector2i

var turn: int

enum GameStatus {
	win,
	loss,
	in_progress
}

var status: GameStatus
