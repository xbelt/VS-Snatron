package ch.ethz.inf.vs.android.haelukas.project.game.model.world;

import ch.ethz.inf.vs.android.haelukas.project.game.model.plane.Plane;

/**
 * Implements common features from the World interface
 * @author Marko
 *
 */
public abstract class AbstractWorld implements World {
		
	public AbstractWorld() {
	}
	
	protected abstract Plane[] getPlanes();

}
