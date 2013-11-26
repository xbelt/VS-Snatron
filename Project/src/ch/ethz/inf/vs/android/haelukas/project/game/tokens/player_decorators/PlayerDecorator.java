package ch.ethz.inf.vs.android.haelukas.project.game.tokens.player_decorators;

import ch.ethz.inf.vs.android.haelukas.project.game.model.PlaneCoordinates;

/**
 * This is a very basic decorator for Players.
 * It delegates all methods to the decoree without modifying any properties.
 * Subclasses must only override methods which they like to modify.
 * @author Marko
 *
 */
public abstract class PlayerDecorator extends AbstractPlayer {
	
	private AbstractPlayer decoree;

	public PlayerDecorator(AbstractPlayer p) {
		super();
		this.decoree = p;
	}
	
	@Override
	public int getHistPos()
	{
		return decoree.getHistPos();
	}
	
	@Override
	public PlaneCoordinates[] getHistory()
	{
		return decoree.getHistory();
	}
	
	@Override
	public PlaneCoordinates getCurrentPosition()
	{
		return decoree.getCurrentPosition();
	}
	
	@Override
	public void initPlayerAt(PlaneCoordinates startPos)
	{
		decoree.initPlayerAt(startPos);
	}
	
	@Override
	public void kill()
	{
		decoree.kill();
	}
	
	@Override
	public boolean isAlive()
	{
		return decoree.isAlive();
	}

	@Override
	public void addTokenMoveListener(TokenChangeListener listener)
	{
		decoree.addTokenMoveListener(listener);
	}
	
	@Override
	public void removeTokenMoveListener(TokenChangeListener listener)
	{
		decoree.removeTokenMoveListener(listener);
	}

	
	@Override
	public float getSpeed() {
		return decoree.getSpeed();
	}

	@Override
	public float getAcceleration() {
		return decoree.getAcceleration();
	}
}
