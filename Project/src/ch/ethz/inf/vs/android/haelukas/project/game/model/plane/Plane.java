package ch.ethz.inf.vs.android.haelukas.project.game.model.plane;

import ch.ethz.inf.vs.android.haelukas.project.game.model.Border;
import ch.ethz.inf.vs.android.haelukas.project.game.model.Orientation;
import ch.ethz.inf.vs.android.haelukas.project.game.model.PlaneCoordinates;
import ch.ethz.inf.vs.android.haelukas.project.game.model.world.World;
import ch.ethz.inf.vs.android.haelukas.project.game.tokens.Token;
import ch.ethz.inf.vs.android.haelukas.project.game.tokens.player_decorators.Player;

/**
 * A {@link World} consists of one or more connected {@link Plane}s.
 * @author Marko
 */
public interface Plane {
	/**
	 * The plane's id identifies it uniquely in a world.
	 * It is part of a {@link PlaneCoordinates}.
	 * @return
	 */
	int getId();
	
	/**
	 * The y-coordinates on this plane lie in the interval [0,{@link #getHeight()}].
	 * @return
	 */
	int getHeight();
	
	/**
	 * @param x
	 * @return The height of the slice of the plane defined by the x-coordinate
	 */
	int getHeightAt(int x);
	
	/**
	 * The x-coordinates on this plane lie in the interval [0,{@link #getWidth()}]
	 * @return
	 */
	int getWidth();
	
	/**
	 * @param y
	 * @return The width of the slice of the plane defined by the y-coordinate
	 */
	int getWidthAt(int y);
	
	/**
	 * @param i, where 0 <= i <= {@link #getNOfBorders()}
	 * @return The border with index i
	 */
	Border getBorder(int i);
	
	/**
	 * @param x
	 * @param y
	 * @return If (x,y) lies 1 step outside of this plane, return the border which provides a transition from this plane to some other plane. Null if (x,y) is not one step outside of this plane.
	 */
	Border getBorder(int x, int y);
	
	/**
	 * 
	 * @return the number of borders
	 */
	int getNOfBorders();
	
	/**
	 * S, the size of the specified border.
	 * That means that this plane has S transitions to the other plane connected to the border.
	 * @param borderId
	 * @return S
	 */
	int getBorderSize(int borderId);
	
	/**
	 * @param borderId
	 * @param borderOffset = i
	 * @return the x-coordinate of the i'th transition of the border
	 */
	int getBorderX(int borderId, int borderOffset);

	/**
	 * @param borderId
	 * @param borderOffset = i
	 * @return the y-coordinate of the i'th transition of the border
	 */
	int getBorderY(int borderId, int borderOffset);

	/**
	 * @param playerId
	 * @param x
	 * @param y
	 * @param direction, where the wall is connected.
	 */
	void addWall(Player player, int x, int y, Orientation direction);
	
	/**
	 * Put a hole into the plane at position (x,y)
	 * @param x
	 * @param y
	 */
	void addHole(int x, int y);
	
	/**
	 * If the field at (x,y) has either a wall or a hole,
	 * remove that obstacle and make the field walkable again.
	 * @param x
	 * @param y
	 */
	void freeField(int x, int y);
	
	/**
	 * @param x
	 * @param y
	 * @return whether the coordinates denote a point on this shape.
	 */
	boolean isOnPlane(int x, int y);
	
	/**
	 * (x,y) are supposed to denote a point on the border, just outside this plane
	 * @param x
	 * @param y
	 * @return The other plane, if there is one at (x,y) or null.
	 */
	Plane getNeighborPlane(int x, int y);
	
	/**
	 * @param x
	 * @param y
	 * @return The type of the field at (x,y)
	 */
	Token getTokenAt(int x, int y);

	/**
	 * Add the token t to this plane at (x,y)
	 * @param t the Token to add
	 * @param x
	 * @param y
	 */
	void addTokenTo(Token t, int x, int y);
}
