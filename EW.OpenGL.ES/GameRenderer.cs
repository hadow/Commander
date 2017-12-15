using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Java.Nio;
using Android.Opengl;
using Javax.Microedition.Khronos.Opengles;
namespace EW.OpenGLES
{
    public class GameRenderer:Java.Lang.Object,GLSurfaceView.IRenderer
    {
        private float[] mModelMatrix = new float[16];

        private float[] mViewMatrix = new float[16];

        private float[] mProjectionMatrix = new float[16];

        private float[] mMVPMatrix = new float[16];

        private readonly FloatBuffer mTriangle1Vertices;
        private readonly FloatBuffer mTriangle2Vertices;
        private readonly FloatBuffer mTriangle3Vertices;


        private readonly int mBytesPerFloat = 4;


        private int mMVPMatrixHandle;

        private int mColorHandle;

        private int mPositionHandle;


        /// <summary>
        /// Size of the position data in element.
        /// </summary>
        private readonly int mPositionDataSize = 3;

        /// <summary>
        /// Offset of the position data.
        /// </summary>
        private readonly int mPositionOffset = 0;

        /// <summary>
        /// Offset of the color data.
        /// </summary>
        private readonly int mColorOffset = 3;

        /// <summary>
        /// Size of the color data in elements.
        /// </summary>
        private readonly int mColorDataSize = 4;

        private readonly int mStrideBytes ;

        public GameRenderer() {

            mStrideBytes = 7 * mBytesPerFloat;
            float[] triangle1VerticesData = { -0.5f,-0.25f,0.0f,1.0f, 0.0f, 0.0f, 1.0f,

                0.5f, -0.25f, 0.0f,
                0.0f, 0.0f, 1.0f, 1.0f,

                0.0f, 0.559016994f, 0.0f,
                0.0f, 1.0f, 0.0f, 1.0f};

            float[] triangle2VerticesData = { 
                // X, Y, Z, 
				// R, G, B, A
	            -0.5f, -0.25f, 0.0f,
                1.0f, 1.0f, 0.0f, 1.0f,

                0.5f, -0.25f, 0.0f,
                0.0f, 1.0f, 1.0f, 1.0f,

                0.0f, 0.559016994f, 0.0f,
                1.0f, 0.0f, 1.0f, 1.0f};

            float[] triangle3VerticesData = { 
                // X, Y, Z, 
				// R, G, B, A
	            -0.5f, -0.25f, 0.0f,
                1.0f, 1.0f, 1.0f, 1.0f,

                0.5f, -0.25f, 0.0f,
                0.5f, 0.5f, 0.5f, 1.0f,

                0.0f, 0.559016994f, 0.0f,
                0.0f, 0.0f, 0.0f, 1.0f};

            //Initialize the buffers.
            mTriangle1Vertices = ByteBuffer.AllocateDirect(triangle1VerticesData.Length * mBytesPerFloat).Order(ByteOrder.NativeOrder()).AsFloatBuffer();

            mTriangle2Vertices = ByteBuffer.AllocateDirect(triangle2VerticesData.Length * mBytesPerFloat).Order(ByteOrder.NativeOrder()).AsFloatBuffer();

            mTriangle3Vertices = ByteBuffer.AllocateDirect(triangle3VerticesData.Length * mBytesPerFloat).Order(ByteOrder.NativeOrder()).AsFloatBuffer();

            mTriangle1Vertices.Put(triangle1VerticesData).Position(0);
            mTriangle2Vertices.Put(triangle2VerticesData).Position(0);
            mTriangle3Vertices.Put(triangle3VerticesData).Position(0);

        }
        
        void GLSurfaceView.IRenderer.OnSurfaceCreated(IGL10 gl, Javax.Microedition.Khronos.Egl.EGLConfig config)
        {
            //Set the background clear color to gray
            GLES20.GlClearColor(0.5f, 0.5f, 0.5f, 0.5f);

            //Position the eye behind the origin.
            float eyeX = 0.0f;
            float eyeY = 0.0f;
            float eyeZ = 0.0f;

            //We are looking toward the distance.
            float lookX = 0.0f;
            float lookY = 0.0f;
            float lookZ = -5.0f;

            //Set our up vector,This is where our head wold be pointing were we holding the camera

            float upX = 0.0f;
            float upY = 1.0f;
            float upZ = 0.0f;

            

            Matrix.SetLookAtM(mViewMatrix, 0, eyeX, eyeY, eyeZ, lookX, lookY, lookZ, upX, upY, upZ);

            string vertexShader = "uniform mat4 u_MVPMatrix; \n" +
                "attribute vec4 a_Position;\n" +
                "attribute vec4 a_Color;\n" +
                "varying vec4 v_Color; \n" +
                "void main()    \n" +
                "{  \n" +
                "v_Color = a_Color; \n" +
                "gl_Position = u_MVPMatrix \n" +
                "*a_Position;   \n" +
                "}  \n";

            string fragmentShader = "precision mediump float;   \n" +
                "varying vec4 v_Color;  \n" +
                "void main()    \n" +
                "{  \n" +
                "gl_FragColor=v_Color;  \n" +
                "}";

            int vertexShaderHandle = GLES20.GlCreateShader(GLES20.GlVertexShader);

            if (vertexShaderHandle != 0) {

                //Pass in the shader source.
                GLES20.GlShaderSource(vertexShaderHandle, vertexShader);

                //Compile the shader
                GLES20.GlCompileShader(vertexShaderHandle);

                int[] compileStatus = new int[1];
                GLES20.GlGetShaderiv(vertexShaderHandle, GLES20.GlCompileStatus, compileStatus, 0);

                //If the compilation failed,delete the shader
                if (compileStatus[0] == 0)
                {

                    GLES20.GlDeleteShader(vertexShaderHandle);
                    vertexShaderHandle = 0;
                }
            }

            if (vertexShaderHandle == 0)
                throw new ArgumentException("Error creating vertex shader.");

            //Load in the fragment shader shader.
            int fragmentShaderHandle = GLES20.GlCreateShader(GLES20.GlFragmentShader);
            if (fragmentShaderHandle != 0) {
                //Pass in the shader source.

                GLES20.GlShaderSource(fragmentShaderHandle, fragmentShader);

                //Compile the shader.

                GLES20.GlCompileShader(fragmentShaderHandle);

                int[] compileStatus = new int[1];
                GLES20.GlGetShaderiv(fragmentShaderHandle, GLES20.GlCompileStatus, compileStatus, 0);

                if (compileStatus[0] == 0)
                {
                    GLES20.GlDeleteShader(fragmentShaderHandle);
                    fragmentShaderHandle = 0;
                }
            }

            if (fragmentShaderHandle == 0)
                throw new ArgumentException("Error createing fragment shader");

            //Create a program object and store the handle to it.
            int programHandle = GLES20.GlCreateProgram();

            if (programHandle != 0) {

                //Bind the vertex shader to the program.
                GLES20.GlAttachShader(programHandle, vertexShaderHandle);

                //Bind the fragment shader to the program.
                GLES20.GlAttachShader(programHandle, fragmentShaderHandle);

                //Bind attributes.
                GLES20.GlBindAttribLocation(programHandle, 0, "a_Position");
                GLES20.GlBindAttribLocation(programHandle, 1, "a_Color");


                //Link the two shaders together into a program.
                GLES20.GlLinkProgram(programHandle);

                //Get the link status.

                int[] linkStatus = new int[1];
                GLES20.GlGetProgramiv(programHandle, GLES20.GlLinkStatus, linkStatus, 0);


                //If the link failed ,delete the program
                if (linkStatus[0] == 0)
                {
                    GLES20.GlDeleteProgram(programHandle);
                    programHandle = 0;
                }
            }

            if (programHandle == 0)
                throw new ArgumentException("Error creating program.");

            //Set program handles. These will later be used to pass in values

            mMVPMatrixHandle = GLES20.GlGetUniformLocation(programHandle, "u_MVPMatrix");
            mPositionHandle = GLES20.GlGetAttribLocation(programHandle, "a_Position");
            mColorHandle = GLES20.GlGetAttribLocation(programHandle, "a_Color");

            //Tell OpenGL to use this program when rendering.
            GLES20.GlUseProgram(programHandle);
        }

        void GLSurfaceView.IRenderer.OnSurfaceChanged(IGL10 gl, int width, int height)
        {
            GLES20.GlViewport(0, 0, width, height);

            float ratio = (float)width / height;
            float left = -ratio;
            float right = ratio;
            float bottom = -1.0f;
            float top = 1.0f;
            float near = 1.0f;
            float far = 10.0f;

            Matrix.FrustumM(mProjectionMatrix, 0, left, right, bottom, top, near, far);
        }

        void GLSurfaceView.IRenderer.OnDrawFrame(IGL10 gl)
        {
            GLES20.GlClear(GLES20.GlDepthBufferBit | GLES20.GlColorBufferBit);

            Matrix.SetIdentityM(mModelMatrix, 0);
            Matrix.RotateM(mModelMatrix, 0, 60, 0.0f, 0.0f, 1.0f);
            DrawTriangle(mTriangle1Vertices);
        }


        private void DrawTriangle(FloatBuffer aTriangleBuffer) {

            // Pass in the position information.
            aTriangleBuffer.Position(mPositionOffset);

            GLES20.GlVertexAttribPointer(mPositionHandle, mPositionDataSize, GLES20.GlFloat, false, mStrideBytes, aTriangleBuffer);
            GLES20.GlEnableVertexAttribArray(mPositionHandle);

            //Pass in the color information

            aTriangleBuffer.Position(mColorOffset);
            GLES20.GlVertexAttribPointer(mColorHandle, mColorDataSize, GLES20.GlFloat, false, mStrideBytes, aTriangleBuffer);
            GLES20.GlEnableVertexAttribArray(mColorHandle);

            Matrix.MultiplyMM(mMVPMatrix, 0, mViewMatrix, 0, mModelMatrix, 0);

            Matrix.MultiplyMM(mMVPMatrix, 0, mProjectionMatrix, 0, mMVPMatrix, 0);

            GLES20.GlUniformMatrix4fv(mMVPMatrixHandle, 1, false, mMVPMatrix, 0);
            GLES20.GlDrawArrays(GLES20.GlTriangles, 0, 3);
            
        }
        



    }
}