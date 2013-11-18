package ch.ethz.inf.vs.android.haelukas.project.startup;

import android.opengl.GLES20;
import android.opengl.Matrix;

public class Grid implements Drawable{
	private Line x = new Line(
		new float[] {
			0f, 0f, 0f,
			1f, 0f, 0f
		}, 
		new float[] {
			1f, 0f, 0f, 1f
		}
	);
	private Line y = new Line(
			new float[] {
					0f, 0f, 0f,
					0f, 1f, 0f
			}, 
			new float[] {
					0f, 1f, 0f, 1f
			}
			);
	private Line z = new Line(
			new float[] {
					0f, 0f, 0f,
					0f, 0f, 1f
			}, 
			new float[] {
					0f, 0f, 1f, 1f
			}
			);
	private float[] mMMatrix = new float[16];
	private float rotation = 0;

	@Override
	public void draw(float[] mvpMatrix) {
		Matrix.setRotateM(mMMatrix , 0, rotation++, 0, 1, 1);
		Matrix.multiplyMM(mvpMatrix, 0, mvpMatrix, 0, mMMatrix, 0);
		x.draw(mvpMatrix);
		y.draw(mvpMatrix);
		z.draw(mvpMatrix);
	}
}
