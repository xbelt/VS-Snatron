package ch.ethz.inf.vs.android.haelukas.project.game.model;

import ch.ethz.inf.vs.android.haelukas.project.game.model.plane.Plane;

/**
 * Each plane of a world has up to N borders if the plane is a N-sided polygon.
 * A Border connects two planes:
 * 	If a player would fall off the edge (the border) of a plane,
 * and if that edge is connected with another plane through a border,
 * then the player continues on the other plane.
 * @author Marko
 */
public class Border {

	private Plane plane1;
	private Plane plane2;
	
	private int borderId1;
	private int borderId2;
	
	public Border(Plane plane1, int borderId1, Plane plane2, int borderId2) {
		assert plane1.getBorderSize(borderId1) == plane2.getBorderSize(borderId2);
		
		this.plane1 = plane1;
		this.plane2 = plane2;
		this.borderId1 = borderId1;
		this.borderId2 = borderId2;
		
	}

	public int getSize()
	{
		return 0;
	}
}
