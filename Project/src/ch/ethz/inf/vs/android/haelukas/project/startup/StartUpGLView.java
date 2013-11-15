package ch.ethz.inf.vs.android.haelukas.project.startup;

import android.content.Context;
import android.opengl.GLSurfaceView;

public class StartUpGLView extends GLSurfaceView {

	public StartUpGLView(Context context) {
		super(context);
		setEGLContextClientVersion(2);
		setRenderMode(GLSurfaceView.RENDERMODE_WHEN_DIRTY);
		setRenderer(new StartUpRenderer());
	}

}
