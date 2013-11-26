package ch.ethz.inf.vs.android.haelukas.project.game.tokens;

import ch.ethz.inf.vs.android.haelukas.project.game.model.PlaneCoordinates;
import ch.ethz.inf.vs.android.haelukas.project.game.model.plane.Plane;
import ch.ethz.inf.vs.android.haelukas.project.game.tokens.player_decorators.Player;

public class EmptyField extends AbstractToken {

	
	public EmptyField(PlaneCoordinates pos) {
		super(pos);
	}

	/**
	 * Add a wall connection to where we came from, labeled by the player.
	 */
	@Override
	public void visit(Player player) {
		Plane plane = getCurrentPosition().getPlane();
		PlaneCoordinates pos = getCurrentPosition();
		plane.addWall(player, pos.getX(), pos.getY(),
				player.getCurrentPosition().getOrientation().getOpposite());
	}

	/**
	 * Add a wall connection to where we're coming from, labeled by the player.
	 */
	@Override
	public void leave(Player player) {
		Plane plane = getCurrentPosition().getPlane();
		PlaneCoordinates pos = getCurrentPosition();
		plane.addWall(player, pos.getX(), pos.getY(),
				player.getCurrentPosition().getOrientation());
	}

}
