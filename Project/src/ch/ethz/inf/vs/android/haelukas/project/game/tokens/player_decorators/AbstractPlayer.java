package ch.ethz.inf.vs.android.haelukas.project.game.tokens.player_decorators;

import ch.ethz.inf.vs.android.haelukas.project.game.model.Orientation;
import ch.ethz.inf.vs.android.haelukas.project.game.model.PlaneCoordinates;
import ch.ethz.inf.vs.android.haelukas.project.game.tokens.AbstractToken;

public abstract class AbstractPlayer extends AbstractToken implements Player {
	
	private boolean isAlive;
	
	public AbstractPlayer() {
		super(new PlaneCoordinates(null, -1, -1));
	}
	
	@Override
	public void initPlayerAt(PlaneCoordinates startPos)
	{
		this.isAlive = true;
		this.setPosition(startPos);
	}
	
	@Override
	public void kill()
	{
		this.isAlive = false;
	}
	
	@Override
	public boolean isAlive()
	{
		return isAlive;
	}
	
	/**
	 * Two players are allowed to be on the same field, as long as they are moving parallel to each other.
	 * Only if they cross each other's path, both will die.
	 */
	@Override
	public void visit(Player p)
	{
		// TODO define what happens when two players hit each other.
		//Orientation o1 = getPosition().getOrientation();
		//Orientation o2 = p.getPosition().getOrientation();
	}
	
	/**
	 * A player cannot be left, since both players should already be dead.
	 */
	@Override
	public void leave(Player p)
	{
		// TODO define what happens when a player leaves a field with another player.
		//if
		//kill();
		//p.kill();
	}
	
	/*
	 * Event listeners
	 */
}