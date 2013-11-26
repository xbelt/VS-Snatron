package ch.ethz.inf.vs.android.haelukas.project.game.model;

/**
 * Player orientation relative to Plane orientation
 * Plane orientation relative to World orientation
 * ...
 * @author Marko
 */
public enum Orientation {
	UP, DOWN, LEFT, RIGHT;
	
	public int getNextX(int oldX)
	{
		return (this == LEFT) ? oldX - 1 : (this == RIGHT) ? oldX + 1 : oldX;
	}

	public int getNextY(int oldY)
	{
		return (this == DOWN) ? oldY - 1 : (this == UP) ? oldY + 1 : oldY;
	}
	
	public Orientation getOpposite()
	{
		switch (this)
		{
		case DOWN: return UP;
		case LEFT: return RIGHT;
		case RIGHT: return LEFT;
		case UP: return DOWN;
		}
		return null;
	}
	
	public boolean isParallel(Orientation other)
	{
		return this == other || this.getOpposite() == other;
	}
}
