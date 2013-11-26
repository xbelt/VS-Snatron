package ch.ethz.inf.vs.android.haelukas.project.game.model.world;

import ch.ethz.inf.vs.android.haelukas.project.game.model.plane.Plane;
import ch.ethz.inf.vs.android.haelukas.project.game.model.plane.RectPlane;

/**
 * This world consists of a single rectangle, a plane only.
 * @author Marko
 *
 */
public class RectWorld extends AbstractWorld {
	
	private Plane[] planes;
	
	public RectWorld(int width, int height) {
		planes = new Plane[1];
		planes[0] = new RectPlane(1, width, height);
	}

	@Override
	protected Plane[] getPlanes()
	{
		return planes;
	}

}
