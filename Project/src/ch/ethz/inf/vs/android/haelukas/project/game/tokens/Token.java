package ch.ethz.inf.vs.android.haelukas.project.game.tokens;

import ch.ethz.inf.vs.android.haelukas.project.game.model.PlaneCoordinates;
import ch.ethz.inf.vs.android.haelukas.project.game.tokens.AbstractToken.TokenChangeListener;
import ch.ethz.inf.vs.android.haelukas.project.game.tokens.player_decorators.Player;

public interface Token {

	/**
	 * Assign the token a new location and notify listeners.
	 * @param pos
	 */
	public void setPosition(PlaneCoordinates pos);
	
	/**
	 * mark this token as removed (non-active) and notify listeners,
	 * then remove listeners.
	 */
	public void remove();

	/**
	 * @return The field, where the token is now.
	 * @see #getPreviousPosition()
	 * @see #getFuturePosition()
	 */
	public PlaneCoordinates getCurrentPosition();
	
	/**
	 * @return The field, where the token was just before it reached the current position
	 * @see #getCurrentPosition()
	 * @see #getFuturePosition()
	 */
	public PlaneCoordinates getPreviousPosition();
	
	/**
	 * @return The field, which the token will reach next, after current position.
	 * @see #getCurrentPosition()
	 * @see #getPreviousPosition()
	 */
	public PlaneCoordinates getFuturePosition();

	/**
	 * @return The index of current position in the history ring buffer
	 * @see #getHistory()
	 */
	public int getHistPos();

	/**
	 * @return the whole history of the last N plane coordinates. To find the current position, use {@link #getHistPos()}.
	 */
	public PlaneCoordinates[] getHistory();

	/**
	 * The data must denote a state in the future, after the currently known state.
	 * @param remoteHistory
	 * @param remoteHistPos
	 */
	public void syncHistory(PlaneCoordinates[] remoteHistory, int remoteHistPos);

	/**
	 * Visitor Pattern. This is called when a player visits a new field.
	 * This way, a wall with the player's identity can be created, a powerup collected,
	 * a player killed because he runs into a hole or a wall,
	 * or two players killed when both run onto the same field simultaneously, ...
	 * @see #leave(Player)
	 * @param player
	 */
	public void visit(Player player);

	/**
	 * Visitor Pattern. This is called when a player leaves a visited field.
	 * @see #visit(Player)
	 * @param player
	 */
	public void leave(Player player);

	/**
	 * Register the event listener to this object.
	 * @param listener
	 */
	public void addTokenMoveListener(TokenChangeListener listener);

	/**
	 * unregister the event listener from this object.
	 * @param listener
	 */
	public void removeTokenMoveListener(TokenChangeListener listener);

}