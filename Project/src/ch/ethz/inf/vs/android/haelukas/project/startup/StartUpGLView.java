package ch.ethz.inf.vs.android.haelukas.project.startup;

import android.content.Context;
import android.opengl.GLSurfaceView;

public class StartUpGLView extends GLSurfaceView {

	public StartUpGLView(Context context) {
		super(context);
		setEGLContextClientVersion(2);
		setRenderer(new StartUpRenderer());
		setRenderMode(GLSurfaceView.RENDERMODE_CONTINUOUSLY);
	}

}
