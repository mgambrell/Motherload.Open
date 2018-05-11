using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace MTS.Engine.Modules.MatrixStack
{

	public class MatrixStack
	{
		public MatrixStack()
		{
			LoadIdentity();
			IsDirty = false;
		}

		public static implicit operator Matrix(MatrixStack ms) { return ms.top; }

		public MatrixStack(Matrix matrix) { LoadMatrix(matrix); }

		public bool IsDirty { get; private set; }

		public void ResetDirty()
		{
			IsDirty = false;
		}

		Matrix top;
		Stack<Matrix> stack = new Stack<Matrix>();

		public Matrix Top { get { return top; } }

		/// <summary>
		/// Resets the matrix stack to an empty identity matrix stack
		/// </summary>
		public void Clear()
		{
			stack.Clear();
			LoadIdentity();
		}

		/// <summary>
		/// Clears the matrix stack and loads the specified value
		/// </summary>
		public void Clear(Matrix value)
		{
			stack.Clear();
			top = value;
		}

		public void LoadMatrix(Matrix value) { top = value; IsDirty = true; }

		public void LoadIdentity() { top = Matrix.Identity; IsDirty = true; }

		public void Pop() { top = stack.Pop(); IsDirty = true; }
		public void Push() { stack.Push(top); IsDirty = true; }

		public void RotateAxis(Vector3 axisRotation, float angle) { top = Matrix.CreateFromAxisAngle(axisRotation, angle) * top; IsDirty = true; }

		public void Scale(Vector3 scale) { top = Matrix.CreateScale(scale) * top; IsDirty = true; }
		public void Scale(Vector2 scale) { top = Matrix.CreateScale(scale.X, scale.Y, 1) * top; IsDirty = true; }
		public void Scale(float x, float y, float z) { top = Matrix.CreateScale(x, y, z) * top; IsDirty = true; }
		public void Scale(float ratio) { Scale(ratio, ratio, ratio); IsDirty = true; }
		public void Scale(float x, float y) { Scale(x, y, 1); IsDirty = true; }

		public void RotateAxis(float x, float y, float z, float angle) { MultiplyMatrix(Matrix.CreateFromAxisAngle(new Vector3(x, y, z), angle)); IsDirty = true; }
		public void RotateY(float angle) { MultiplyMatrix(Matrix.CreateRotationY(angle)); IsDirty = true; }
		public void RotateX(float angle) { MultiplyMatrix(Matrix.CreateRotationX(angle)); IsDirty = true; }
		public void RotateZ(float angle) { MultiplyMatrix(Matrix.CreateRotationZ(angle)); IsDirty = true; }

		public void Translate(Vector2 translate) { Translate(translate.X, translate.Y, 0); IsDirty = true; }
		public void Translate(Vector3 translate) { top = Matrix.CreateTranslation(translate) * top; IsDirty = true; }
		public void Translate(float x, float y, float z) { top = Matrix.CreateTranslation(x, y, z) * top; IsDirty = true; }
		public void Translate(float x, float y) { Translate(x, y, 0); IsDirty = true; }
		public void PreTranslate(float x, float y) { top = top * Matrix.CreateTranslation(x, y, 0); IsDirty = true; }
		public void Translate(Point pt) { Translate(pt.X, pt.Y, 0); IsDirty = true; }

		public void MultiplyMatrix(MatrixStack ms) { MultiplyMatrix(ms.Top); IsDirty = true; }
		public void MultiplyMatrix(Matrix value) { top = value * top; IsDirty = true; }
	}


}