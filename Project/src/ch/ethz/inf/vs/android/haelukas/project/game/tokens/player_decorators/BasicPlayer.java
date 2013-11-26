package ch.ethz.inf.vs.android.haelukas.project.game.tokens.player_decorators;

import ch.ethz.inf.vs.android.haelukas.project.game.model.PlayerAction;

public class BasicPlayer extends AbstractPlayer {
	
	int playerId;
	
	float speed = 0.5f;
	/**
	 * how far away the player is from its last visited coordinate, on the way to the next.
	 * This must lie in the interval [0,1).
	 */
	float progress = 0.0f;
	
	PlayerAction lastAction = PlayerAction.NONE;
	
	public BasicPlayer(int playerId) {
		super();
	}

	
	@Override
	public float getSpeed() {
		// TODO Auto-generated method stub
		return 0;
	}

	@Override
	public float getAcceleration() {
		// TODO Auto-generated method stub
		return 0;
	}
	
}
