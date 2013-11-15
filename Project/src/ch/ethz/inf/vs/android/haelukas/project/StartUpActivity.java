package ch.ethz.inf.vs.android.haelukas.project;

import ch.ethz.inf.vs.android.haelukas.project.startup.StartUpGLView;
import android.opengl.GLSurfaceView;
import android.os.Bundle;
import android.app.Activity;
import android.view.Menu;

public class StartUpActivity extends Activity {

	private GLSurfaceView startUpGLView;
	
	@Override
	protected void onCreate(Bundle savedInstanceState) {
		super.onCreate(savedInstanceState);
		
		startUpGLView = new StartUpGLView(this);
		setContentView(startUpGLView);
	}

	@Override
	public boolean onCreateOptionsMenu(Menu menu) {
		// Inflate the menu; this adds items to the action bar if it is present.
		getMenuInflater().inflate(R.menu.start_up, menu);
		return true;
	}

}
