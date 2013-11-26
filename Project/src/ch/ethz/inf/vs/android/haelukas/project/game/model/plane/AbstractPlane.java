package ch.ethz.inf.vs.android.haelukas.project.game.model.plane;

import java.util.HashMap;

import android.annotation.SuppressLint;
import ch.ethz.inf.vs.android.haelukas.project.game.model.Orientation;
import ch.ethz.inf.vs.android.haelukas.project.game.model.PlaneCoordinates;
import ch.ethz.inf.vs.android.haelukas.project.game.tokens.EmptyField;
import ch.ethz.inf.vs.android.haelukas.project.game.tokens.Hole;
import ch.ethz.inf.vs.android.haelukas.project.game.tokens.Token;
import ch.ethz.inf.vs.android.haelukas.project.game.tokens.WallCross;
import ch.ethz.inf.vs.android.haelukas.project.game.tokens.player_decorators.Player;

public abstract class AbstractPlane implements Plane {
	
	private int planeId;
	private int height;
	private int width;
	
	/**
	 * Every object on this plane is stored in this hashmap.
	 * The key is a string composed by {@link #posToMapString(int, int)}, using the x and y coordinates as input.
	 */
	private HashMap<String,Token> tokens;
	
	public AbstractPlane()
	{
		tokens = new HashMap<String, Token>();
	}
	
	@Override
	public int getId()
	{
		return planeId;
	}
	
	protected void setPlaneId(int planeId)
	{
		this.planeId = planeId;
	}

	@Override
	public int getWidth()
	{
		return width;
	}
	
	protected void setWidth(int width)
	{
		this.width = width;
	}

	@Override
	public int getHeight()
	{
		return height;
	}
	
	protected void setHeight(int height)
	{
		this.height = height;
	}
	
	/**
	 * init capacity with a size depending on the world size.
	 * This is a heuristic that estimates the duration of a game.
	 * It can be adapted when
	 * @param width
	 * @param height
	 */
	protected void resizeTokenMap(int width, int height)
	{
		int capacity = (int) (Math.sqrt(width) * Math.sqrt(height));
		HashMap<String, Token> t = new HashMap<String, Token>(capacity);
		
		if (tokens != null)
		{
			t.putAll(tokens);
			tokens.clear();
		}
		tokens = t;
	}

	@Override
	public Token getTokenAt(int x, int y) {
		
		// If coordinate outside the plane, the player will die when visiting a hole
		if (!isOnPlane(x, y))
			return dropOffPlaneHole;
		
		// Fetch token at (x,y) from map
		Token t = tokens.get(posToMapString(x, y));
		if (t != null)
			return t;
		
		// 1. we're on the plane
		// 2. there exists no token
		// => It is a empty field
		return new EmptyField(new PlaneCoordinates(this, x, y));
	}
	
	@Override
	public void addTokenTo(Token t, int x, int y) {
		assert isOnPlane(x, y);
		tokens.put(posToMapString(x, y), t);
	}
	
	@SuppressLint("DefaultLocale")
	private String posToMapString(int x, int y)
	{
		return String.format("%s,%s",Integer.toHexString(x),Integer.toHexString(y));
	}
	
	@Override
	public void addWall(Player player, int x, int y, Orientation direction){
		assert isOnPlane(x, y); // TODO
		
		Token t = getTokenAt(x, y);
		WallCross wallCross = null;
		if (t == null)
		{
			wallCross = new WallCross(new PlaneCoordinates(this, x, y));
			addTokenTo(wallCross, x, y);
		}
		else if (t instanceof WallCross)
		{
			wallCross = (WallCross) t;
		}
		else
		{
			t.remove();
			wallCross = new WallCross(new PlaneCoordinates(this, x, y));
			addTokenTo(wallCross, x, y);
		}
		
		wallCross.setWallOwner(player, direction);
	}
	
	@Override
	public void addHole(int x, int y) {
		assert isOnPlane(x, y);
		Hole hole = new Hole(new PlaneCoordinates(this, x, y));
		addTokenTo(hole, x, y);
	}

	@Override
	public void freeField(int x, int y) {
		assert isOnPlane(x, y);
		String key = posToMapString(x, y);
		Token t = tokens.get(key);
		tokens.remove(key);
		
		if (t == null)
			return;
		
		t.remove();
	}
	
	protected Hole dropOffPlaneHole = new Hole(null);
	

}
