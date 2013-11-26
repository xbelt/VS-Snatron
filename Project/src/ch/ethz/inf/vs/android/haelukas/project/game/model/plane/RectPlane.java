package ch.ethz.inf.vs.android.haelukas.project.game.model.plane;

import ch.ethz.inf.vs.android.haelukas.project.game.model.Border;

public class RectPlane extends AbstractPlane {

	private final Border[] borders = new Border[4];
	
	public RectPlane(int planeId, int width, int height) {
		setPlaneId(planeId);
		setHeight(height);
		setWidth(width);
	}

	@Override
	public int getHeightAt(int x) {
		return getHeight();
	}

	@Override
	public int getWidthAt(int y) {
		return getWidth();
	}

	@Override
	public Border getBorder(int i) {
		return borders[i];
	}

	@Override
	public Border getBorder(int x, int y)
	{
		int h = getHeight()+1;
		int w = getWidth()+1;
		
		/*
		 * "Bi" denotes border i, (#/#) the coordinates
		 * 
		 *  (-1/h) -- B2 -- (w/h)
		 *    |      	 	  |
		 *    B3	  	 	  B1
		 *    |			 	  |
		 * (-1/-1) -- B0 -- (w/-1)
		 */
		
		int xloc = (x == -1) ? 1 : (x == w) ? -1 : 0;
		int yloc = (y == -1) ? -1 : (y == h) ? 1 : 0;
		
		if (xloc == yloc)
			return null;
		
		if (xloc == 0)
			return borders[1 + yloc]; // B0 or B2
		else
			return borders[2 + xloc]; // B1 or B3
				
	}
	
	@Override
	public int getNOfBorders() {
		return 4;
	}
	
	/**
	 * @return -1 if border id out of range, otherwise the border size.
	 */
	@Override
	public int getBorderSize(int borderId) {
		if (borderId < 0 || borderId > 3)
			return -1;
		else
			return (borderId % 2 == 0) ? getWidth() : getHeight();
	}

	@Override
	public int getBorderX(int borderId, int borderOffset) {
		// TODO Auto-generated method stub
		return 0;
	}

	@Override
	public int getBorderY(int borderId, int borderOffset) {
		// TODO Auto-generated method stub
		return 0;
	}

	public boolean isOnPlane(int x, int y)
	{
		return 	   0 <= x 
				&& 0 <= y 
				&& x <= getWidth()
				&& y <= getHeight();
	}

	
	@Override
	public Plane getNeighborPlane(int x, int y) {
		// TODO Auto-generated method stub
		return null;
	}
}
