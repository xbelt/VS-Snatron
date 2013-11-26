package ch.ethz.inf.vs.android.haelukas.project.game.tokens;

import java.util.ArrayList;

import ch.ethz.inf.vs.android.haelukas.project.game.model.PlaneCoordinates;
import ch.ethz.inf.vs.android.haelukas.project.game.tokens.player_decorators.BasicPlayer;

/**
 * A token is an object in the game world: A player, a wall, a power up, anything except the world or plane itself.
 * Anything that can be positioned onto a plane using {@link PlaneCoordinates}.
 * @author Marko
 *
 */
public abstract class AbstractToken implements Token {

	public static int COORD_HISTORY_SIZE = 5;
	
	PlaneCoordinates[] history = new PlaneCoordinates[COORD_HISTORY_SIZE];
	int histPos = 0;
	
	public AbstractToken(PlaneCoordinates startPos) {
		for (int i = 0; i < COORD_HISTORY_SIZE; i++)
		{
			history[i] = startPos;
		}
	}
	
	@Override
	public void setPosition(PlaneCoordinates pos)
	{
		history[++histPos] = pos;
	}

	@Override
	public void remove() {
		// TODO Auto-generated method stub
		
	}
	
	@Override
	public PlaneCoordinates getCurrentPosition()
	{
		return history[histPos];
	}
	
	@Override
	public PlaneCoordinates getPreviousPosition()
	{
		return history[(histPos - 1) % history.length];
	}
	
	@Override
	public PlaneCoordinates getFuturePosition()
	{
		// TODO calculate using current position and orientation
		return null;
	}

	@Override
	public int getHistPos()
	{
		return histPos;
	}

	@Override
	public PlaneCoordinates[] getHistory()
	{
		return history;
	}

	/**
	 * TODO: Move to networking?
	 * The data must denote a state in the future, after the currently known state.
	 * @param remoteHistory
	 * @param remoteHistPos
	 */
	@Override
	public void syncHistory(PlaneCoordinates[] remoteHistory, int remoteHistPos)
	{
		if (histPos == remoteHistPos)
			return;
		
		int max = Math.max(histPos, remoteHistPos) % history.length;
		
		for(int i = histPos+1; i < max; i++) {
			history[i] = remoteHistory[i];
			notifyTokenMoved();
		}
		
		if (remoteHistPos < histPos) {
			for(int i = 0; i < histPos; i++) {
				history[i] = remoteHistory[i];
				notifyTokenMoved();
			}
		}
		
		this.history = remoteHistory.clone();
		this.histPos = remoteHistPos;
	}
	
	/*
	 * Player Change Listeners
	 */
	private final ArrayList<TokenChangeListener> listeners = new ArrayList<BasicPlayer.TokenChangeListener>();
	
	@Override
	public void addTokenMoveListener(TokenChangeListener listener)
	{
		listeners.add(listener);
	}

	@Override
	public void removeTokenMoveListener(TokenChangeListener listener)
	{
		listeners.remove(listener);
	}

	public void notifyTokenMoved()
	{
		for(TokenChangeListener listener : listeners)
			listener.positionChanged(this);
	}
	
	public void notifyRemoved()
	{
		for(TokenChangeListener listener : listeners)
			listener.removed(this);
	}
	
	public interface TokenChangeListener
	{
		/**
		 * @param pos
		 */
		void positionChanged(final Token token);
		void removed(Token token);
	}
}
