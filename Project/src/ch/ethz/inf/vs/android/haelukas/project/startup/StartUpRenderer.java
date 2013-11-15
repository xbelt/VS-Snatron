package ch.ethz.inf.vs.android.haelukas.project.startup;

import javax.microedition.khronos.egl.EGLConfig;
import javax.microedition.khronos.opengles.GL10;

import android.opengl.GLES20;
import android.opengl.GLSurfaceView.Renderer;

public class StartUpRenderer implements Renderer {

	@Override
	public void onSurfaceCreated(GL10 arg0, EGLConfig arg1) {
		GLES20.glClearColor(0f, 0f, 1f, 1f);
	}

	@Override
	public void onDrawFrame(GL10 arg0) {
		 GLES20.glClear(GLES20.GL_COLOR_BUFFER_BIT);
	}

	@Override
	public void onSurfaceChanged(GL10 arg0, int width, int height) {
		GLES20.glViewport(0, 0, width, height);
	}
}
