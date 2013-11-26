package ch.ethz.inf.vs.android.haelukas.project.game.model;

import ch.ethz.inf.vs.android.haelukas.project.game.model.plane.Plane;

/**
 * This uniquely defines a logical and integral coordinate in our game model.
 * 
 * Note: The player speed is limited by 1/(2*max_sync_time)
 * => If |delta_location| > 1 since the last sync, then something went wrong.
 * 
 * @author Marko
 */
public class PlaneCoordinates {

	private Plane plane;
	private int x;
	private int y;
	private Orientation orientation;
	
	public PlaneCoordinates(Plane plane, int x, int y) {
		this.plane = plane;
		this.x = x;
		this.y = y;
	}

	public Plane getPlane() {
		return plane;
	}

	public int getX() {
		return x;
	}
	
	public int getY() {
		return y;
	}

	public void setPos(Plane plane, int x, int y) {
		assert plane == null || plane.isOnPlane(x, y);
		this.plane = plane;
		this.x = x;
		this.y = y;
	}
	
	public Orientation getOrientation(){
		return orientation;
	}
	
	public void setOrientation(Orientation orientation){
		this.orientation = orientation;
	}
	
	public boolean isLegalMove(Orientation o)
	{
		if (plane == null)
			return false;
		int nX = o.getNextX(x);
		int nY = o.getNextY(y);
		return plane.isOnPlane(nX, nY)
				|| plane.getNeighborPlane(nX, nY) != null;
	}
	
	/**
	 * @param o must denote a legal move relative to the current coordinate.
	 * @see #isLegalMove(Orientation)
	 */
	public void move(Orientation o)
	{
		assert isLegalMove(o);
		int nX = o.getNextX(x);
		int nY = o.getNextY(y);
		
		Plane neighbor = plane.getNeighborPlane(nX, nY);
		
		if (neighbor == null) { // since it is a legal move, we remain on this plane
			this.x = nX;
			this.y = nY;
		}else { // There must exist a border too
			Border border = plane.getBorder(nX, nY);
		}
	}
	
}
