package ch.ethz.inf.vs.android.haelukas.project.game.tokens;

import ch.ethz.inf.vs.android.haelukas.project.game.model.Orientation;
import ch.ethz.inf.vs.android.haelukas.project.game.model.PlaneCoordinates;
import ch.ethz.inf.vs.android.haelukas.project.game.tokens.player_decorators.Player;

/**
 * Note: we do not need a wall object, it is enough to mark the four directions of each point in our coordinate system.
 * This is also more space efficient.
 * @author Marko
 *
 */
public class WallCross extends AbstractToken {

	private Player upWallOwner;
	private Player downWallOwner;
	private Player leftWallOwner;
	private Player rightWallOwner;
	
	public WallCross(PlaneCoordinates startPos) {
		super(startPos);
	}

	@Override
	public void visit(Player player) {
		
		// repaint wall, in direction where we came from
		
		setWallOwner(player, player.getPreviousPosition().getOrientation().getOpposite());

		// TODO define deadly crossing.
		// If crossing is deadly : kill player
	}

	@Override
	public void leave(Player player) {
		// TODO If player still alive, repaint the wall to player's color

		setWallOwner(player, player.getPreviousPosition().getOrientation());

		// TODO leaving a cross is not deadly, is it?
	}

	public void setWallOwner(Player player, Orientation direction)
	{
		switch(direction)
		{
		case DOWN: setDownWallOwner(player); break;
		case LEFT: setLeftWallOwner(player); break;
		case RIGHT: setRightWallOwner(player); break;
		case UP: setUpWallOwner(player); break;
		}
	}
 
 	public Player getUpWallOwner() {
		return upWallOwner;
	}

	public void setUpWallOwner(Player upWallOwner) {
		this.upWallOwner = upWallOwner;
	}

	public Player getDownWallOwner() {
		return downWallOwner;
	}

	public void setDownWallOwner(Player downWallOwner) {
		this.downWallOwner = downWallOwner;
	}

	public Player getLeftWallOwner() {
		return leftWallOwner;
	}

	public void setLeftWallOwner(Player leftWallOwner) {
		this.leftWallOwner = leftWallOwner;
	}

	public Player getRightWallOwner() {
		return rightWallOwner;
	}

	public void setRightWallOwner(Player rightWallOwner) {
		this.rightWallOwner = rightWallOwner;
	}

}
