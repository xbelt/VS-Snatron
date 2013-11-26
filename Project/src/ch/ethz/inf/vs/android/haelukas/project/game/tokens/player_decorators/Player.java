package ch.ethz.inf.vs.android.haelukas.project.game.tokens.player_decorators;

import ch.ethz.inf.vs.android.haelukas.project.game.model.PlaneCoordinates;
import ch.ethz.inf.vs.android.haelukas.project.game.tokens.Token;

public interface Player extends Token{

	public void initPlayerAt(PlaneCoordinates startPos);

	public void kill();

	public boolean isAlive();
	
	public float getSpeed();
	
	public float getAcceleration();

}