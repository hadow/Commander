using System;
using OpenTK;
using OpenTK.Graphics.ES20;

namespace ratest
{
	public class Text
	{
		const int UNIFORM_PROJECTION = 0;
		const int UNIFORM_LIGHT = 1;
		const int UNIFORM_VIEW = 2;
		const int UNIFORM_NORMAL_MATRIX = 3;
		const int UNIFORM_COUNT = 4;
		int[] uniforms = new int[UNIFORM_COUNT];
		const int ATTRIB_VERTEX = 0;
		const int ATTRIB_NORMAL = 1;
		const int ATTRIB_COUNT = 2;

		int vbo, vbi;

		public float xAngle = (float)-Math.PI / 2, yAngle = 0;
		public float xAcc, yAcc;
		public float xSign = 1, ySign = 1;
		float xInc = .0033f, yInc = .01f;
		int Width, Height;

		internal void InitModel()
		{
			GL.GenBuffers(1, out vbo);
			GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
			GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(TextModel.vertices.Length * sizeof(float)), TextModel.vertices, BufferUsage.StaticDraw);
			GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

			GL.GenBuffers(1, out vbi);
			GL.BindBuffer(BufferTarget.ElementArrayBuffer, vbi);
			GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(TextModel.faceIndexes.Length * sizeof(ushort)), TextModel.faceIndexes, BufferUsage.StaticDraw);
			GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
		}

		internal void Start()
		{
			depthCounter = 0;
			textureDepth = new int[24];
			GL.Enable(EnableCap.DepthTest);
			GL.Enable(EnableCap.CullFace);
			GL.CullFace(CullFaceMode.Back);
		}

		internal void DrawModel()
		{
			// Update attribute values.
			GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
			GL.VertexAttribPointer(ATTRIB_VERTEX, 3, VertexAttribPointerType.Float, false, sizeof(float) * 8, IntPtr.Zero);
			GL.EnableVertexAttribArray(ATTRIB_VERTEX);

			GL.VertexAttribPointer(ATTRIB_NORMAL, 3, VertexAttribPointerType.Float, false, sizeof(float) * 8, new IntPtr(sizeof(float) * 3));
			GL.EnableVertexAttribArray(ATTRIB_NORMAL);

			GL.BindBuffer(BufferTarget.ElementArrayBuffer, vbi);
			GL.DrawElements(BeginMode.Triangles, TextModel.faceIndexes.Length, DrawElementsType.UnsignedShort, IntPtr.Zero);

			GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
			GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
		}

		internal Matrix4 view = new Matrix4();
		internal Matrix4 normalMatrix = new Matrix4();
		internal Matrix4 projection = new Matrix4();
		internal Matrix4 textProjection = new Matrix4();

		int program;
		int depthCounter;
		int[] textureDepth = new int[24];
		int[] textureFinalIndex = { 12, 13, 14, 15, 16, 17, 6, 7, 9, 10, 11 };

		internal void RenderFrame()
		{
			if (depthCounter >= 20 && depthCounter < 12 * 20)
			{
				int offset = depthCounter / 20 - 1;
				textureDepth[textureFinalIndex[offset]] = offset + 1;
			}
			// Replace the implementation of this method to do your own custom drawing.
			GL.ClearColor(0.2f, 0.2f, 0.2f, 1.0f);
			GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
			CheckGLError();

			// Use shader program.
			GL.UseProgram(program);
			CheckGLError();

			// Update uniform value.
			GL.UniformMatrix4(uniforms[UNIFORM_PROJECTION], false, ref projection);
			GL.UniformMatrix4(uniforms[UNIFORM_VIEW], false, ref view);
			GL.UniformMatrix4(uniforms[UNIFORM_NORMAL_MATRIX], false, ref normalMatrix);
			GL.Uniform3(uniforms[UNIFORM_LIGHT], 25f, 25f, 28f);
			CheckGLError();

			DrawModel();
			CheckGLError();

			// Validate program before drawing. This is a good check, but only really necessary in a debug build.
#if DEBUG
			if (!ValidateProgram (program)) {
				Console.WriteLine ($"Failed to validate program {program:x}\nGL Error: {GL.GetErrorCode()}");
				throw new Exception ("Invalid shaders program");
			}
#endif

			depthCounter++;
		}

		string LoadResource(string name)
		{
			return new System.IO.StreamReader(System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(name)).ReadToEnd();
		}

		internal void LoadShaders()
		{
			LoadShaders(LoadResource("ratest.Shaders.Shader.vsh"), LoadResource("ratest.Shaders.Shader.fsh"));
		}

		internal bool LoadShaders(string vertShaderSource, string fragShaderSource)
		{
			Console.WriteLine("load shaders");
			int vertShader, fragShader;

			// Create shader program.
			program = GL.CreateProgram();

			// Create and compile vertex shader.
			if (!CompileShader(ShaderType.VertexShader, vertShaderSource, out vertShader))
			{
				Console.WriteLine("Failed to compile vertex shader");
				return false;
			}
			// Create and compile fragment shader.
			if (!CompileShader(ShaderType.FragmentShader, fragShaderSource, out fragShader))
			{
				Console.WriteLine("Failed to compile fragment shader");
				return false;
			}

			// Attach vertex shader to program.
			GL.AttachShader(program, vertShader);

			// Attach fragment shader to program.
			GL.AttachShader(program, fragShader);

			// Bind attribute locations.
			// This needs to be done prior to linking.
			GL.BindAttribLocation(program, ATTRIB_VERTEX, "position");
			GL.BindAttribLocation(program, ATTRIB_NORMAL, "normal");

			// Link program.
			if (!LinkProgram(program))
			{
				Console.WriteLine($"Failed to link program: {program:x}");

				if (vertShader != 0)
					GL.DeleteShader(vertShader);

				if (fragShader != 0)
					GL.DeleteShader(fragShader);

				if (program != 0)
				{
					GL.DeleteProgram(program);
					program = 0;
				}
				return false;
			}

			// Get uniform locations.
			uniforms[UNIFORM_PROJECTION] = GL.GetUniformLocation(program, "projection");
			uniforms[UNIFORM_VIEW] = GL.GetUniformLocation(program, "view");
			uniforms[UNIFORM_NORMAL_MATRIX] = GL.GetUniformLocation(program, "normalMatrix");
			uniforms[UNIFORM_LIGHT] = GL.GetUniformLocation(program, "light");

			// Release vertex and fragment shaders.
			if (vertShader != 0)
			{
				GL.DetachShader(program, vertShader);
				GL.DeleteShader(vertShader);
			}

			if (fragShader != 0)
			{
				GL.DetachShader(program, fragShader);
				GL.DeleteShader(fragShader);
			}

			return true;
		}

		internal void DestroyShaders()
		{
			if (program != 0)
			{
				GL.DeleteProgram(program);
				program = 0;
			}
		}

		static bool CompileShader(ShaderType type, string src, out int shader)
		{
			shader = GL.CreateShader(type);
			GL.ShaderSource(shader, src);
			GL.CompileShader(shader);

#if DEBUG
			int logLength = 0;
			GL.GetShader (shader, ShaderParameter.InfoLogLength, out logLength);
			if (logLength > 0)
				Console.WriteLine ($"Shader compile log:\n{GL.GetShaderInfoLog (shader)}");
#endif

			int status = 0;
			GL.GetShader(shader, ShaderParameter.CompileStatus, out status);
			if (status == 0)
			{
				GL.DeleteShader(shader);
				return false;
			}

			return true;
		}

		internal static bool LinkProgram(int prog)
		{
			GL.LinkProgram(prog);

#if DEBUG
			int logLength = 0;
			GL.GetProgram (prog, ProgramParameter.InfoLogLength, out logLength);
			if (logLength > 0)
				Console.WriteLine ($"Program link log:\n{GL.GetProgramInfoLog (prog)}");
#endif
			int status = 0;
			GL.GetProgram(prog, ProgramParameter.LinkStatus, out status);
			if (status == 0)
				return false;

			return true;
		}

		static void CheckGLError()
		{
			ErrorCode code = GL.GetErrorCode();
			if (code != ErrorCode.NoError)
				Console.WriteLine($"GL Error {code}");
		}

		static bool ValidateProgram(int prog)
		{
			GL.ValidateProgram(prog);
			CheckGLError();

			int logLength = 0;
			GL.GetProgram(prog, ProgramParameter.InfoLogLength, out logLength);
			CheckGLError();
			if (logLength > 0)
			{
				var infoLog = new System.Text.StringBuilder(logLength);
				GL.GetProgramInfoLog(prog, logLength, out logLength, infoLog);
			}

			int status = 0;
			GL.GetProgram(prog, ProgramParameter.LinkStatus, out status);
			CheckGLError();
			if (status == 0)
				return false;

			return true;
		}

		internal void SetupProjection(int width, int height)
		{
			Matrix4 model = Matrix4.Mult(Matrix4.CreateRotationX(-xAngle), Matrix4.CreateRotationZ(-yAngle));

			float aspect = (float)width / height;
			if (aspect > 1)
			{
				Matrix4 scale = Matrix4.Scale(16 * aspect);
				model = Matrix4.Mult(model, scale);
			}
			view = Matrix4.Mult(model, Matrix4.LookAt(0, -70, 5, 0, 10, 0, 0, 1, 0));
			projection = Matrix4.CreatePerspectiveFieldOfView(OpenTK.MathHelper.DegreesToRadians(42.0f), aspect, 1.0f, 200.0f);
			projection = Matrix4.Mult(view, projection);
			normalMatrix = Matrix4.Invert(view);
			normalMatrix.Transpose();

			Width = width;
			Height = height;
		}

		public void UpdateWorld()
		{
			xAngle += xSign * (xInc + xAcc * xAcc);
			yAngle += ySign * (yInc + yAcc * yAcc);
			SetupProjection(Width, Height);
			xAcc = System.Math.Max(0, xAcc - 0.001f);
			yAcc = System.Math.Max(0, yAcc - 0.001f);
		}
	}
}
