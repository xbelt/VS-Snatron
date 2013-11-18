package ch.ethz.inf.vs.android.haelukas.project.startup;

public class Rectangle implements Drawable{
	Triangle bottomLeft = new Triangle(new float[] {
			-0.5f,  0.5f, 0.0f,   // top left
            -0.5f, -0.5f, 0.0f,   // bottom left
             0.5f, -0.5f, 0.0f,   // bottom right
            });
	Triangle topRight = new Triangle(new float[] { 
 			 0.5f,  0.5f, 0.0f, // top right
			-0.5f,  0.5f, 0.0f, // top left
			 0.5f, -0.5f, 0.0f  // bottom right
			 });
	
	
	public Rectangle(float x, float y) {
	}


	@Override
	public void draw(float[] mMVPMatrix) {
		bottomLeft.draw(mMVPMatrix);
		topRight.draw(mMVPMatrix);
	}
}
