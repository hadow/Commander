using System;
using System.Drawing;
using Android.Graphics;
using EW.Graphics;
namespace EW
{
	public enum WindowMode
	{
		Windowed,
		Fullscreen,
		PseudoFullscreen,
	}
	public enum TextureScaleFilter { Nearest, Linear }

	public enum PrimitiveType
	{
		PointList,
		LineList,
		TriangleList,
	}

	public enum BlendMode : byte
	{
		None,
		Alpha,
		Additive,
		Subtractive,
		Multiply,
		Multiplicative,
		DoubleMultiplicative
	}

	public interface IPlatform
	{

		IGraphicsDevice CreateGraphics(Size size, WindowMode windowMode);
		ISoundEngine CreateSound(string device);

	}

	/// <summary>
	/// Graphics device.
	/// </summary>
	public interface IGraphicsDevice : IDisposable
	{
		IVertexBuffer<Vertex> CreateVertexBuffer(int length);
		ITexture CreateTexture(Bitmap bitmap);
		ITexture CreateTexture();
		IFrameBuffer CreateFrameBuffer(Size s);
		IShader CreateShader(string name);

		Size WindowSize { get; }
		void Clear();
		void Present();
		Bitmap TakeScreenShot();
		void PumpInput(IInputHandler inputHandler);
		string GetClipboardText();
		bool SetClipboardText(string text);
		void DrawPrimitives(PrimitiveType type, int firstVertex, int numVertices);

		void EnableScissor(int left, int top, int width, int height);
		void DisableScissor();

		void EnableDepthBuffer();
		void DisableDepthBuffer();
		void ClearDepthBuffer();

		void SetBlendMode(BlendMode mode);

		void GrabWindowMouseFocus();
		void ReleaseWindowMouseFocus();

		string GLVersion { get; }

	}



	public interface ITexture : IDisposable
	{
		void SetData(Bitmap bitmap);
		void SetData(uint[,] colors);
		void SetData(byte[] colors, int width, int height);
		byte[] GetData();
		Size Size { get; }
		TextureScaleFilter ScaleFilter { get; set; }
	}

	public interface IFrameBuffer : IDisposable
	{

		void Bind();
		void Unbind();
		ITexture Texture { get; }
	}

	public interface IVertexBuffer<T> : IDisposable
	{
		void Bind();
		void SetData(T[] vertices, int length);
		void SetData(T[] vertices, int start, int length);
		void SetData(IntPtr data, int start, int length);

	}




	public interface IShader
	{

		void SetBool(string name, bool value);
		void SetVec(string name, float x);
		void SetVec(string name, float x, float y);
		void SetVec(string name, float x, float y, float z);
		void SetVec(string name, float[] vec, int length);
		void SetTexture(string param, ITexture texture);
		void SetMatrix(string param, float[] mtx);
		void Render(Action a);


	}
}
	


