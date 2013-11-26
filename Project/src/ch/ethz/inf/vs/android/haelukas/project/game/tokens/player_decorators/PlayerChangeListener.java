package ch.ethz.inf.vs.android.haelukas.project.game.tokens.player_decorators;


public interface PlayerChangeListener
{
	void playerInitialized(Player p);
	void playerKilled(Player p);
}