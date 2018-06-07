using System;
using Yuzu;

namespace Lime
{
	[YuzuCompact]
	public struct Matrix44 : IEquatable<Matrix44>
	{
		public Matrix44(float m11, float m12, float m13, float m14, float m21, float m22, float m23, float m24, float m31,
					  float m32, float m33, float m34, float m41, float m42, float m43, float m44)
		{
			this.M11 = m11;
			this.M12 = m12;
			this.M13 = m13;
			this.M14 = m14;
			this.M21 = m21;
			this.M22 = m22;
			this.M23 = m23;
			this.M24 = m24;
			this.M31 = m31;
			this.M32 = m32;
			this.M33 = m33;
			this.M34 = m34;
			this.M41 = m41;
			this.M42 = m42;
			this.M43 = m43;
			this.M44 = m44;
		}

		[YuzuMember("0")]
		public float M11;

		[YuzuMember("1")]
		public float M12;

		[YuzuMember("2")]
		public float M13;

		[YuzuMember("3")]
		public float M14;

		[YuzuMember("4")]
		public float M21;

		[YuzuMember("5")]
		public float M22;

		[YuzuMember("6")]
		public float M23;

		[YuzuMember("7")]
		public float M24;

		[YuzuMember("8")]
		public float M31;

		[YuzuMember("9")]
		public float M32;

		[YuzuMember("A")]
		public float M33;

		[YuzuMember("B")]
		public float M34;

		[YuzuMember("C")]
		public float M41;

		[YuzuMember("D")]
		public float M42;

		[YuzuMember("E")]
		public float M43;

		[YuzuMember("F")]
		public float M44;

		private static readonly Matrix44 identity = new Matrix44(
			1f, 0f, 0f, 0f,
			0f, 1f, 0f, 0f,
			0f, 0f, 1f, 0f,
			0f, 0f, 0f, 1f
		);

		public Vector3 Backward
		{
			get
			{
				return new Vector3(this.M31, this.M32, this.M33);
			}
			set
			{
				this.M31 = value.X;
				this.M32 = value.Y;
				this.M33 = value.Z;
			}
		}

		public Vector3 Down
		{
			get
			{
				return new Vector3(-this.M21, -this.M22, -this.M23);
			}
			set
			{
				this.M21 = -value.X;
				this.M22 = -value.Y;
				this.M23 = -value.Z;
			}
		}

		public Vector3 Forward
		{
			get
			{
				return new Vector3(-this.M31, -this.M32, -this.M33);
			}
			set
			{
				this.M31 = -value.X;
				this.M32 = -value.Y;
				this.M33 = -value.Z;
			}
		}

		public static Matrix44 Identity
		{
			get { return identity; }
		}

		public float[] ToFloatArray()
		{
			float[] array = {
				M11, M12, M13, M14,
				M21, M22, M23, M24,
				M31, M32, M33, M34,
				M41, M42, M43, M44
			};
			return array;
		}

		public Vector3 Left
		{
			get
			{
				return new Vector3(-this.M11, -this.M12, -this.M13);
			}
			set
			{
				this.M11 = -value.X;
				this.M12 = -value.Y;
				this.M13 = -value.Z;
			}
		}

		public Vector3 Right
		{
			get
			{
				return new Vector3(this.M11, this.M12, this.M13);
			}
			set
			{
				this.M11 = value.X;
				this.M12 = value.Y;
				this.M13 = value.Z;
			}
		}

		public Vector3 Translation
		{
			get
			{
				return new Vector3(this.M41, this.M42, this.M43);
			}
			set
			{
				this.M41 = value.X;
				this.M42 = value.Y;
				this.M43 = value.Z;
			}
		}

		public Vector3 Up
		{
			get
			{
				return new Vector3(this.M21, this.M22, this.M23);
			}
			set
			{
				this.M21 = value.X;
				this.M22 = value.Y;
				this.M23 = value.Z;
			}
		}

		public Vector3 Scale
		{
			get { return GetScale(true); }
		}

		public Quaternion Rotation
		{
			get
			{
				Vector3 scale, translation;
				Quaternion rotation;
				Decompose(out scale, out rotation, out translation);
				return rotation;
			}
		}

		public static Matrix44 CreateFromAxisAngle(Vector3 axis, float angle)
		{
			Matrix44 matrix;
			float x = axis.X;
			float y = axis.Y;
			float z = axis.Z;
			float num2 = Mathf.Sin(angle);
			float num = Mathf.Cos(angle);
			float num11 = x * x;
			float num10 = y * y;
			float num9 = z * z;
			float num8 = x * y;
			float num7 = x * z;
			float num6 = y * z;
			matrix.M11 = num11 + (num * (1f - num11));
			matrix.M12 = (num8 - (num * num8)) + (num2 * z);
			matrix.M13 = (num7 - (num * num7)) - (num2 * y);
			matrix.M14 = 0f;
			matrix.M21 = (num8 - (num * num8)) - (num2 * z);
			matrix.M22 = num10 + (num * (1f - num10));
			matrix.M23 = (num6 - (num * num6)) + (num2 * x);
			matrix.M24 = 0f;
			matrix.M31 = (num7 - (num * num7)) + (num2 * y);
			matrix.M32 = (num6 - (num * num6)) - (num2 * x);
			matrix.M33 = num9 + (num * (1f - num9));
			matrix.M34 = 0f;
			matrix.M41 = 0f;
			matrix.M42 = 0f;
			matrix.M43 = 0f;
			matrix.M44 = 1f;
			return matrix;
		}

		public static void CreateFromAxisAngle(ref Vector3 axis, float angle, out Matrix44 result)
		{
			float x = axis.X;
			float y = axis.Y;
			float z = axis.Z;
			float num2 = Mathf.Sin(angle);
			float num = Mathf.Cos(angle);
			float num11 = x * x;
			float num10 = y * y;
			float num9 = z * z;
			float num8 = x * y;
			float num7 = x * z;
			float num6 = y * z;
			result.M11 = num11 + (num * (1f - num11));
			result.M12 = (num8 - (num * num8)) + (num2 * z);
			result.M13 = (num7 - (num * num7)) - (num2 * y);
			result.M14 = 0f;
			result.M21 = (num8 - (num * num8)) - (num2 * z);
			result.M22 = num10 + (num * (1f - num10));
			result.M23 = (num6 - (num * num6)) + (num2 * x);
			result.M24 = 0f;
			result.M31 = (num7 - (num * num7)) + (num2 * y);
			result.M32 = (num6 - (num * num6)) - (num2 * x);
			result.M33 = num9 + (num * (1f - num9));
			result.M34 = 0f;
			result.M41 = 0f;
			result.M42 = 0f;
			result.M43 = 0f;
			result.M44 = 1f;
		}

		public static Matrix44 CreateLookAt(Vector3 cameraPosition, Vector3 cameraTarget, Vector3 cameraUpVector)
		{
			Vector3 vector3_1 = (cameraPosition - cameraTarget).Normalized;
			Vector3 vector3_2 = Vector3.CrossProduct(cameraUpVector, vector3_1).Normalized;
			Vector3 vector1 = Vector3.CrossProduct(vector3_1, vector3_2);
			Matrix44 matrix;
			matrix.M11 = vector3_2.X;
			matrix.M12 = vector1.X;
			matrix.M13 = vector3_1.X;
			matrix.M14 = 0.0f;
			matrix.M21 = vector3_2.Y;
			matrix.M22 = vector1.Y;
			matrix.M23 = vector3_1.Y;
			matrix.M24 = 0.0f;
			matrix.M31 = vector3_2.Z;
			matrix.M32 = vector1.Z;
			matrix.M33 = vector3_1.Z;
			matrix.M34 = 0.0f;
			matrix.M41 = -Vector3.DotProduct(vector3_2, cameraPosition);
			matrix.M42 = -Vector3.DotProduct(vector1, cameraPosition);
			matrix.M43 = -Vector3.DotProduct(vector3_1, cameraPosition);
			matrix.M44 = 1f;
			return matrix;
		}

		public static void CreateLookAt(ref Vector3 cameraPosition, ref Vector3 cameraTarget, ref Vector3 cameraUpVector, out Matrix44 result)
		{
			Vector3 vector = (cameraPosition - cameraTarget).Normalized;
			Vector3 vector2 = Vector3.CrossProduct(cameraUpVector, vector).Normalized;
			Vector3 vector3 = Vector3.CrossProduct(vector, vector2);
			result.M11 = vector2.X;
			result.M12 = vector3.X;
			result.M13 = vector.X;
			result.M14 = 0f;
			result.M21 = vector2.Y;
			result.M22 = vector3.Y;
			result.M23 = vector.Y;
			result.M24 = 0f;
			result.M31 = vector2.Z;
			result.M32 = vector3.Z;
			result.M33 = vector.Z;
			result.M34 = 0f;
			result.M41 = -Vector3.DotProduct(vector2, cameraPosition);
			result.M42 = -Vector3.DotProduct(vector3, cameraPosition);
			result.M43 = -Vector3.DotProduct(vector, cameraPosition);
			result.M44 = 1f;
		}

		public static Matrix44 CreateOrthographic(float width, float height, float zNear, float zFar)
		{
			var maxX = width * 0.5f;
			var maxY = height * 0.5f;
			var minX = -maxX;
			var minY = -maxY;
			return CreateOrthographicOffCenter(minX, maxX, minY, maxY, zNear, zFar);
		}

		public static Matrix44 CreateOrthographicOffCenter(
			float left, float right, float bottom, float top, float zNear, float zFar)
		{
			// If the viewport has zero size, project everything into a single point.
			if (right == left || top == bottom || zNear == zFar)
				return new Matrix44();

			Matrix44 matrix;
			matrix.M11 = (float)(2.0 / ((double)right - (double)left));
			matrix.M12 = 0.0f;
			matrix.M13 = 0.0f;
			matrix.M14 = 0.0f;

			matrix.M21 = 0.0f;
			matrix.M22 = (float)(2.0 / ((double)top - (double)bottom));
			matrix.M23 = 0.0f;
			matrix.M24 = 0.0f;

			matrix.M31 = 0.0f;
			matrix.M32 = 0.0f;
			matrix.M33 = (float)(1.0 / ((double)zNear - (double)zFar));
			matrix.M34 = 0.0f;

			matrix.M41 = (float)(((double)left + (double)right) / ((double)left - (double)right));
			matrix.M42 = (float)(((double)top + (double)bottom) / ((double)bottom - (double)top));
			matrix.M43 = (float)(((double)zNear + (double)zFar) / ((double)zNear - (double)zFar));
			matrix.M44 = 1.0f;
			return matrix;
		}

		public static Matrix44 CreatePerspective(float width, float height, float zNear, float zFar)
		{
			var maxX = width * 0.5f;
			var maxY = height * 0.5f;
			var minX = -maxX;
			var minY = -maxY;
			return CreatePerspectiveOffCenter(minX, maxX, minY, maxY, zNear, zFar);
		}

		public static Matrix44 CreatePerspectiveFieldOfView(float vFov, float aspectRatio, float zNear, float zFar)
		{
			var maxY = zNear * (float)(Math.Tan((double)(vFov * 0.5)));
			var minY = -maxY;
			var maxX = maxY * aspectRatio;
			var minX = minY * aspectRatio;
			return CreatePerspectiveOffCenter(minX, maxX, minY, maxY, zNear, zFar);
		}

		public static Matrix44 CreatePerspectiveOffCenter(float left, float right, float bottom, float top, float zNear, float zFar)
		{
			Matrix44 matrix;
			matrix.M11 = (2f * zNear) / (right - left);
			matrix.M12 = 0f;
			matrix.M13 = 0f;
			matrix.M14 = 0f;

			matrix.M21 = 0f;
			matrix.M22 = (2f * zNear) / (top - bottom);
			matrix.M23 = 0f;
			matrix.M24 = 0f;

			matrix.M31 = (left + right) / (right - left);
			matrix.M32 = (top + bottom) / (top - bottom);
			matrix.M33 = -(zFar + zNear) / (zFar - zNear);
			matrix.M34 = -1f;

			matrix.M41 = 0f;
			matrix.M42 = 0f;
			matrix.M43 = -2f * (zFar * zNear) / (zFar - zNear);
			matrix.M44 = 0f;
			return matrix;
		}

		public static Matrix44 CreateRotationX(float radians)
		{
			Matrix44 returnMatrix = Matrix44.Identity;

			var val1 = (float)Math.Cos(radians);
			var val2 = (float)Math.Sin(radians);

			returnMatrix.M22 = val1;
			returnMatrix.M23 = val2;
			returnMatrix.M32 = -val2;
			returnMatrix.M33 = val1;

			return returnMatrix;
		}

		public static void CreateRotationX(float radians, out Matrix44 result)
		{
			result = Matrix44.Identity;

			var val1 = (float)Math.Cos(radians);
			var val2 = (float)Math.Sin(radians);

			result.M22 = val1;
			result.M23 = val2;
			result.M32 = -val2;
			result.M33 = val1;
		}

		public static Matrix44 CreateRotationY(float radians)
		{
			Matrix44 returnMatrix = Matrix44.Identity;

			var val1 = (float)Math.Cos(radians);
			var val2 = (float)Math.Sin(radians);

			returnMatrix.M11 = val1;
			returnMatrix.M13 = -val2;
			returnMatrix.M31 = val2;
			returnMatrix.M33 = val1;

			return returnMatrix;
		}

		public static void CreateRotationY(float radians, out Matrix44 result)
		{
			result = Matrix44.Identity;

			var val1 = (float)Math.Cos(radians);
			var val2 = (float)Math.Sin(radians);

			result.M11 = val1;
			result.M13 = -val2;
			result.M31 = val2;
			result.M33 = val1;
		}

		public static Matrix44 CreateRotationZ(float radians)
		{
			Matrix44 returnMatrix = Matrix44.Identity;

			var val1 = (float)Math.Cos(radians);
			var val2 = (float)Math.Sin(radians);

			returnMatrix.M11 = val1;
			returnMatrix.M12 = val2;
			returnMatrix.M21 = -val2;
			returnMatrix.M22 = val1;

			return returnMatrix;
		}

		public static void CreateRotationZ(float radians, out Matrix44 result)
		{
			result = Matrix44.Identity;

			var val1 = (float)Math.Cos(radians);
			var val2 = (float)Math.Sin(radians);

			result.M11 = val1;
			result.M12 = val2;
			result.M21 = -val2;
			result.M22 = val1;
		}

		public static Matrix44 CreateRotation(Quaternion quaternion)
		{
			Matrix44 result;
			CreateRotation(ref quaternion, out result);
			return result;
		}

		public static void CreateRotation(ref Quaternion quaternion, out Matrix44 result)
		{
			float num9 = quaternion.X * quaternion.X;
			float num8 = quaternion.Y * quaternion.Y;
			float num7 = quaternion.Z * quaternion.Z;
			float num6 = quaternion.X * quaternion.Y;
			float num5 = quaternion.Z * quaternion.W;
			float num4 = quaternion.Z * quaternion.X;
			float num3 = quaternion.Y * quaternion.W;
			float num2 = quaternion.Y * quaternion.Z;
			float num = quaternion.X * quaternion.W;
			result.M11 = 1f - (2f * (num8 + num7));
			result.M12 = 2f * (num6 + num5);
			result.M13 = 2f * (num4 - num3);
			result.M14 = 0f;
			result.M21 = 2f * (num6 - num5);
			result.M22 = 1f - (2f * (num7 + num9));
			result.M23 = 2f * (num2 + num);
			result.M24 = 0f;
			result.M31 = 2f * (num4 + num3);
			result.M32 = 2f * (num2 - num);
			result.M33 = 1f - (2f * (num8 + num9));
			result.M34 = 0f;
			result.M41 = 0f;
			result.M42 = 0f;
			result.M43 = 0f;
			result.M44 = 1f;
		}

		public static Matrix44 CreateScale(float x, float y, float z) => CreateScale(new Vector3(x, y, z));

		public static Matrix44 CreateScale(Vector3 scales)
		{
			Matrix44 result;
			result.M11 = scales.X;
			result.M12 = 0;
			result.M13 = 0;
			result.M14 = 0;
			result.M21 = 0;
			result.M22 = scales.Y;
			result.M23 = 0;
			result.M24 = 0;
			result.M31 = 0;
			result.M32 = 0;
			result.M33 = scales.Z;
			result.M34 = 0;
			result.M41 = 0;
			result.M42 = 0;
			result.M43 = 0;
			result.M44 = 1;
			return result;
		}

		public static Matrix44 CreateTranslation(float x, float y, float z) => CreateTranslation(new Vector3(x, y, z));

		public static Matrix44 CreateTranslation(Vector3 position)
		{
			Matrix44 result;
			result.M11 = 1;
			result.M12 = 0;
			result.M13 = 0;
			result.M14 = 0;
			result.M21 = 0;
			result.M22 = 1;
			result.M23 = 0;
			result.M24 = 0;
			result.M31 = 0;
			result.M32 = 0;
			result.M33 = 1;
			result.M34 = 0;
			result.M41 = position.X;
			result.M42 = position.Y;
			result.M43 = position.Z;
			result.M44 = 1;
			return result;
		}

		public static Matrix44 CreateReflection(Plane plane)
		{
			var x = plane.Normal.X;
			var y = plane.Normal.Y;
			var z = plane.Normal.Z;
			var x2 = -2.0f * x;
			var y2 = -2.0f * y;
			var z2 = -2.0f * z;
			Matrix44 result;
			result.M11 = (x2 * x) + 1.0f;
			result.M12 = y2 * x;
			result.M13 = z2 * x;
			result.M14 = 0.0f;
			result.M21 = x2 * y;
			result.M22 = (y2 * y) + 1.0f;
			result.M23 = z2 * y;
			result.M24 = 0.0f;
			result.M31 = x2 * z;
			result.M32 = y2 * z;
			result.M33 = (z2 * z) + 1.0f;
			result.M34 = 0.0f;
			result.M41 = x2 * plane.D;
			result.M42 = y2 * plane.D;
			result.M43 = z2 * plane.D;
			result.M44 = 1.0f;
			return result;
		}

		public float CalcDeterminant()
		{
			float num22 = this.M11;
			float num21 = this.M12;
			float num20 = this.M13;
			float num19 = this.M14;
			float num12 = this.M21;
			float num11 = this.M22;
			float num10 = this.M23;
			float num9 = this.M24;
			float num8 = this.M31;
			float num7 = this.M32;
			float num6 = this.M33;
			float num5 = this.M34;
			float num4 = this.M41;
			float num3 = this.M42;
			float num2 = this.M43;
			float num = this.M44;
			float num18 = (num6 * num) - (num5 * num2);
			float num17 = (num7 * num) - (num5 * num3);
			float num16 = (num7 * num2) - (num6 * num3);
			float num15 = (num8 * num) - (num5 * num4);
			float num14 = (num8 * num2) - (num6 * num4);
			float num13 = (num8 * num3) - (num7 * num4);
			return (((
				(num22 * (((num11 * num18) - (num10 * num17)) + (num9 * num16))) -
				(num21 * (((num12 * num18) - (num10 * num15)) + (num9 * num14)))) +
				(num20 * (((num12 * num17) - (num11 * num15)) + (num9 * num13)))) -
				(num19 * (((num12 * num16) - (num11 * num14)) + (num10 * num13)
			)));
		}

		public bool Equals(Matrix44 other)
		{
			return ((((((this.M11 == other.M11) && (this.M22 == other.M22)) && ((this.M33 == other.M33) &&
				(this.M44 == other.M44))) && (((this.M12 == other.M12) && (this.M13 == other.M13)) &&
				((this.M14 == other.M14) && (this.M21 == other.M21)))) && ((((this.M23 == other.M23) &&
				(this.M24 == other.M24)) && ((this.M31 == other.M31) && (this.M32 == other.M32))) &&
				(((this.M34 == other.M34) && (this.M41 == other.M41)) && (this.M42 == other.M42)))) &&
				(this.M43 == other.M43));
		}

		public override bool Equals(object obj)
		{
			bool flag = false;
			if (obj is Matrix44) {
				flag = this.Equals((Matrix44)obj);
			}
			return flag;
		}

		public override int GetHashCode()
		{
			return (((((((((((((((M11.GetHashCode() + M12.GetHashCode()) + M13.GetHashCode()) + M14.GetHashCode()) +
				M21.GetHashCode()) + M22.GetHashCode()) + M23.GetHashCode()) + M24.GetHashCode()) +
				M31.GetHashCode()) + M32.GetHashCode()) + M33.GetHashCode()) + M34.GetHashCode()) +
				M41.GetHashCode()) + M42.GetHashCode()) + M43.GetHashCode()) + M44.GetHashCode());
		}

		public static Matrix44 Invert(Matrix44 matrix)
		{
			Invert(ref matrix, out matrix);
			return matrix;
		}

		public Matrix44 CalcInverted()
		{
			Matrix44 result;
			Invert(ref this, out result);
			return result;
		}

		public static void Invert(ref Matrix44 matrix, out Matrix44 result)
		{
			float num1 = matrix.M11;
			float num2 = matrix.M12;
			float num3 = matrix.M13;
			float num4 = matrix.M14;
			float num5 = matrix.M21;
			float num6 = matrix.M22;
			float num7 = matrix.M23;
			float num8 = matrix.M24;
			float num9 = matrix.M31;
			float num10 = matrix.M32;
			float num11 = matrix.M33;
			float num12 = matrix.M34;
			float num13 = matrix.M41;
			float num14 = matrix.M42;
			float num15 = matrix.M43;
			float num16 = matrix.M44;
			float num17 = (float)((double)num11 * (double)num16 - (double)num12 * (double)num15);
			float num18 = (float)((double)num10 * (double)num16 - (double)num12 * (double)num14);
			float num19 = (float)((double)num10 * (double)num15 - (double)num11 * (double)num14);
			float num20 = (float)((double)num9 * (double)num16 - (double)num12 * (double)num13);
			float num21 = (float)((double)num9 * (double)num15 - (double)num11 * (double)num13);
			float num22 = (float)((double)num9 * (double)num14 - (double)num10 * (double)num13);
			float num23 = (float)((double)num6 * (double)num17 - (double)num7 * (double)num18 + (double)num8 * (double)num19);
			float num24 = (float)-((double)num5 * (double)num17 - (double)num7 * (double)num20 + (double)num8 * (double)num21);
			float num25 = (float)((double)num5 * (double)num18 - (double)num6 * (double)num20 + (double)num8 * (double)num22);
			float num26 = (float)-((double)num5 * (double)num19 - (double)num6 * (double)num21 + (double)num7 * (double)num22);
			float num27 = (float)(1.0 / ((double)num1 * (double)num23 + (double)num2 * (double)num24 + (double)num3 * (double)num25 + (double)num4 * (double)num26));
			result.M11 = num23 * num27;
			result.M21 = num24 * num27;
			result.M31 = num25 * num27;
			result.M41 = num26 * num27;
			result.M12 = (float)-((double)num2 * (double)num17 - (double)num3 * (double)num18 + (double)num4 * (double)num19) * num27;
			result.M22 = (float)((double)num1 * (double)num17 - (double)num3 * (double)num20 + (double)num4 * (double)num21) * num27;
			result.M32 = (float)-((double)num1 * (double)num18 - (double)num2 * (double)num20 + (double)num4 * (double)num22) * num27;
			result.M42 = (float)((double)num1 * (double)num19 - (double)num2 * (double)num21 + (double)num3 * (double)num22) * num27;
			float num28 = (float)((double)num7 * (double)num16 - (double)num8 * (double)num15);
			float num29 = (float)((double)num6 * (double)num16 - (double)num8 * (double)num14);
			float num30 = (float)((double)num6 * (double)num15 - (double)num7 * (double)num14);
			float num31 = (float)((double)num5 * (double)num16 - (double)num8 * (double)num13);
			float num32 = (float)((double)num5 * (double)num15 - (double)num7 * (double)num13);
			float num33 = (float)((double)num5 * (double)num14 - (double)num6 * (double)num13);
			result.M13 = (float)((double)num2 * (double)num28 - (double)num3 * (double)num29 + (double)num4 * (double)num30) * num27;
			result.M23 = (float)-((double)num1 * (double)num28 - (double)num3 * (double)num31 + (double)num4 * (double)num32) * num27;
			result.M33 = (float)((double)num1 * (double)num29 - (double)num2 * (double)num31 + (double)num4 * (double)num33) * num27;
			result.M43 = (float)-((double)num1 * (double)num30 - (double)num2 * (double)num32 + (double)num3 * (double)num33) * num27;
			float num34 = (float)((double)num7 * (double)num12 - (double)num8 * (double)num11);
			float num35 = (float)((double)num6 * (double)num12 - (double)num8 * (double)num10);
			float num36 = (float)((double)num6 * (double)num11 - (double)num7 * (double)num10);
			float num37 = (float)((double)num5 * (double)num12 - (double)num8 * (double)num9);
			float num38 = (float)((double)num5 * (double)num11 - (double)num7 * (double)num9);
			float num39 = (float)((double)num5 * (double)num10 - (double)num6 * (double)num9);
			result.M14 = (float)-((double)num2 * (double)num34 - (double)num3 * (double)num35 + (double)num4 * (double)num36) * num27;
			result.M24 = (float)((double)num1 * (double)num34 - (double)num3 * (double)num37 + (double)num4 * (double)num38) * num27;
			result.M34 = (float)-((double)num1 * (double)num35 - (double)num2 * (double)num37 + (double)num4 * (double)num39) * num27;
			result.M44 = (float)((double)num1 * (double)num36 - (double)num2 * (double)num38 + (double)num3 * (double)num39) * num27;
		}

		public static Matrix44 Lerp(Matrix44 value1, Matrix44 value2, float amount)
		{
			var result = new Matrix44();
			result.M11 = value1.M11 + ((value2.M11 - value1.M11) * amount);
			result.M12 = value1.M12 + ((value2.M12 - value1.M12) * amount);
			result.M13 = value1.M13 + ((value2.M13 - value1.M13) * amount);
			result.M14 = value1.M14 + ((value2.M14 - value1.M14) * amount);
			result.M21 = value1.M21 + ((value2.M21 - value1.M21) * amount);
			result.M22 = value1.M22 + ((value2.M22 - value1.M22) * amount);
			result.M23 = value1.M23 + ((value2.M23 - value1.M23) * amount);
			result.M24 = value1.M24 + ((value2.M24 - value1.M24) * amount);
			result.M31 = value1.M31 + ((value2.M31 - value1.M31) * amount);
			result.M32 = value1.M32 + ((value2.M32 - value1.M32) * amount);
			result.M33 = value1.M33 + ((value2.M33 - value1.M33) * amount);
			result.M34 = value1.M34 + ((value2.M34 - value1.M34) * amount);
			result.M41 = value1.M41 + ((value2.M41 - value1.M41) * amount);
			result.M42 = value1.M42 + ((value2.M42 - value1.M42) * amount);
			result.M43 = value1.M43 + ((value2.M43 - value1.M43) * amount);
			result.M44 = value1.M44 + ((value2.M44 - value1.M44) * amount);
			return result;
		}

		/// <summary>
		/// Decomposes a matrix into a scale, rotation, and translation.
		/// </summary>
		/// <param name="scale">When the method completes, contains the scaling component of the decomposed matrix.</param>
		/// <param name="rotation">When the method completes, contains the rtoation component of the decomposed matrix.</param>
		/// <param name="translation">When the method completes, contains the translation component of the decomposed matrix.</param>
		/// <remarks>
		/// This method is designed to decompose an SRT transformation matrix only.
		/// </remarks>
		public void Decompose(out Vector3 scale, out Quaternion rotation, out Vector3 translation)
		{
			Matrix44 rotationMatrix;
			Decompose(out scale, out rotationMatrix, out translation);
			Quaternion.CreateFromRotationMatrix(ref rotationMatrix, out rotation);
		}

		/// <summary>
		/// Decomposes a matrix into a scale, rotation, and translation.
		/// </summary>
		/// <param name="scale">When the method completes, contains the scaling component of the decomposed matrix.</param>
		/// <param name="rotation">When the method completes, contains the rtoation component of the decomposed matrix.</param>
		/// <param name="translation">When the method completes, contains the translation component of the decomposed matrix.</param>
		/// <remarks>
		/// This method is designed to decompose an SRT transformation matrix only.
		/// </remarks>
		public void Decompose(out Vector3 scale, out Matrix44 rotation, out Vector3 translation)
		{
			//Source: Unknown
			//References: http://www.gamedev.net/community/forums/topic.asp?topic_id=441695

			//Get the translation.
			translation.X = this.M41;
			translation.Y = this.M42;
			translation.Z = this.M43;

			//Scaling is the length of the rows.
			scale.X = (float)Math.Sqrt((M11 * M11) + (M12 * M12) + (M13 * M13));
			scale.Y = (float)Math.Sqrt((M21 * M21) + (M22 * M22) + (M23 * M23));
			scale.Z = (float)Math.Sqrt((M31 * M31) + (M32 * M32) + (M33 * M33));

			//If any of the scaling factors are zero, than the rotation matrix can not exist.
			if (
				Math.Abs(scale.X) < Mathf.ZeroTolerance ||
				Math.Abs(scale.Y) < Mathf.ZeroTolerance ||
				Math.Abs(scale.Z) < Mathf.ZeroTolerance
			) {
				rotation = Identity;
				return;
			}

			// Calculate an perfect orthonormal matrix (no reflections)
			var at = new Vector3(M31 / scale.Z, M32 / scale.Z, M33 / scale.Z);
			var up = Vector3.CrossProduct(at, new Vector3(M11 / scale.X, M12 / scale.X, M13 / scale.X));
			var right = Vector3.CrossProduct(up, at);

			rotation = Identity;
			rotation.Right = right;
			rotation.Up = up;
			rotation.Backward = at;

			// In case of reflexions
			scale.X = Vector3.DotProduct(right, Right) > 0.0f ? scale.X : -scale.X;
			scale.Y = Vector3.DotProduct(up, Up) > 0.0f ? scale.Y : -scale.Y;
			scale.Z = Vector3.DotProduct(at, Backward) > 0.0f ? scale.Z : -scale.Z;
		}

		public static bool operator ==(Matrix44 matrix1, Matrix44 matrix2)
		{
			return
				matrix1.M11 == matrix2.M11 &&
				matrix1.M12 == matrix2.M12 &&
				matrix1.M13 == matrix2.M13 &&
				matrix1.M14 == matrix2.M14 &&
				matrix1.M21 == matrix2.M21 &&
				matrix1.M22 == matrix2.M22 &&
				matrix1.M23 == matrix2.M23 &&
				matrix1.M24 == matrix2.M24 &&
				matrix1.M31 == matrix2.M31 &&
				matrix1.M32 == matrix2.M32 &&
				matrix1.M33 == matrix2.M33 &&
				matrix1.M34 == matrix2.M34 &&
				matrix1.M41 == matrix2.M41 &&
				matrix1.M42 == matrix2.M42 &&
				matrix1.M43 == matrix2.M43 &&
				matrix1.M44 == matrix2.M44;
		}

		public static bool operator !=(Matrix44 matrix1, Matrix44 matrix2)
		{
			return
				matrix1.M11 != matrix2.M11 ||
				matrix1.M12 != matrix2.M12 ||
				matrix1.M13 != matrix2.M13 ||
				matrix1.M14 != matrix2.M14 ||
				matrix1.M21 != matrix2.M21 ||
				matrix1.M22 != matrix2.M22 ||
				matrix1.M23 != matrix2.M23 ||
				matrix1.M24 != matrix2.M24 ||
				matrix1.M31 != matrix2.M31 ||
				matrix1.M32 != matrix2.M32 ||
				matrix1.M33 != matrix2.M33 ||
				matrix1.M34 != matrix2.M34 ||
				matrix1.M41 != matrix2.M41 ||
				matrix1.M42 != matrix2.M42 ||
				matrix1.M43 != matrix2.M43 ||
				matrix1.M44 != matrix2.M44;
		}

		public static Matrix44 operator *(Matrix44 matrix1, Matrix44 matrix2)
		{
			var m11 = (((matrix1.M11 * matrix2.M11) + (matrix1.M12 * matrix2.M21)) + (matrix1.M13 * matrix2.M31)) + (matrix1.M14 * matrix2.M41);
			var m12 = (((matrix1.M11 * matrix2.M12) + (matrix1.M12 * matrix2.M22)) + (matrix1.M13 * matrix2.M32)) + (matrix1.M14 * matrix2.M42);
			var m13 = (((matrix1.M11 * matrix2.M13) + (matrix1.M12 * matrix2.M23)) + (matrix1.M13 * matrix2.M33)) + (matrix1.M14 * matrix2.M43);
			var m14 = (((matrix1.M11 * matrix2.M14) + (matrix1.M12 * matrix2.M24)) + (matrix1.M13 * matrix2.M34)) + (matrix1.M14 * matrix2.M44);
			var m21 = (((matrix1.M21 * matrix2.M11) + (matrix1.M22 * matrix2.M21)) + (matrix1.M23 * matrix2.M31)) + (matrix1.M24 * matrix2.M41);
			var m22 = (((matrix1.M21 * matrix2.M12) + (matrix1.M22 * matrix2.M22)) + (matrix1.M23 * matrix2.M32)) + (matrix1.M24 * matrix2.M42);
			var m23 = (((matrix1.M21 * matrix2.M13) + (matrix1.M22 * matrix2.M23)) + (matrix1.M23 * matrix2.M33)) + (matrix1.M24 * matrix2.M43);
			var m24 = (((matrix1.M21 * matrix2.M14) + (matrix1.M22 * matrix2.M24)) + (matrix1.M23 * matrix2.M34)) + (matrix1.M24 * matrix2.M44);
			var m31 = (((matrix1.M31 * matrix2.M11) + (matrix1.M32 * matrix2.M21)) + (matrix1.M33 * matrix2.M31)) + (matrix1.M34 * matrix2.M41);
			var m32 = (((matrix1.M31 * matrix2.M12) + (matrix1.M32 * matrix2.M22)) + (matrix1.M33 * matrix2.M32)) + (matrix1.M34 * matrix2.M42);
			var m33 = (((matrix1.M31 * matrix2.M13) + (matrix1.M32 * matrix2.M23)) + (matrix1.M33 * matrix2.M33)) + (matrix1.M34 * matrix2.M43);
			var m34 = (((matrix1.M31 * matrix2.M14) + (matrix1.M32 * matrix2.M24)) + (matrix1.M33 * matrix2.M34)) + (matrix1.M34 * matrix2.M44);
			var m41 = (((matrix1.M41 * matrix2.M11) + (matrix1.M42 * matrix2.M21)) + (matrix1.M43 * matrix2.M31)) + (matrix1.M44 * matrix2.M41);
			var m42 = (((matrix1.M41 * matrix2.M12) + (matrix1.M42 * matrix2.M22)) + (matrix1.M43 * matrix2.M32)) + (matrix1.M44 * matrix2.M42);
			var m43 = (((matrix1.M41 * matrix2.M13) + (matrix1.M42 * matrix2.M23)) + (matrix1.M43 * matrix2.M33)) + (matrix1.M44 * matrix2.M43);
			var m44 = (((matrix1.M41 * matrix2.M14) + (matrix1.M42 * matrix2.M24)) + (matrix1.M43 * matrix2.M34)) + (matrix1.M44 * matrix2.M44);
			var result = new Matrix44();
			result.M11 = m11;
			result.M12 = m12;
			result.M13 = m13;
			result.M14 = m14;
			result.M21 = m21;
			result.M22 = m22;
			result.M23 = m23;
			result.M24 = m24;
			result.M31 = m31;
			result.M32 = m32;
			result.M33 = m33;
			result.M34 = m34;
			result.M41 = m41;
			result.M42 = m42;
			result.M43 = m43;
			result.M44 = m44;
			return result;
		}

		public static Matrix44 operator *(Matrix44 matrix, float scaleFactor)
		{
			var result = new Matrix44();
			result.M11 = matrix.M11 * scaleFactor;
			result.M12 = matrix.M12 * scaleFactor;
			result.M13 = matrix.M13 * scaleFactor;
			result.M14 = matrix.M14 * scaleFactor;
			result.M21 = matrix.M21 * scaleFactor;
			result.M22 = matrix.M22 * scaleFactor;
			result.M23 = matrix.M23 * scaleFactor;
			result.M24 = matrix.M24 * scaleFactor;
			result.M31 = matrix.M31 * scaleFactor;
			result.M32 = matrix.M32 * scaleFactor;
			result.M33 = matrix.M33 * scaleFactor;
			result.M34 = matrix.M34 * scaleFactor;
			result.M41 = matrix.M41 * scaleFactor;
			result.M42 = matrix.M42 * scaleFactor;
			result.M43 = matrix.M43 * scaleFactor;
			result.M44 = matrix.M44 * scaleFactor;
			return result;
		}

		public static Matrix44 operator +(Matrix44 matrix1, Matrix44 matrix2)
		{
			var result = new Matrix44();
			result.M11 = matrix1.M11 + matrix2.M11;
			result.M12 = matrix1.M12 + matrix2.M12;
			result.M13 = matrix1.M13 + matrix2.M13;
			result.M14 = matrix1.M14 + matrix2.M14;
			result.M21 = matrix1.M21 + matrix2.M21;
			result.M22 = matrix1.M22 + matrix2.M22;
			result.M23 = matrix1.M23 + matrix2.M23;
			result.M24 = matrix1.M24 + matrix2.M24;
			result.M31 = matrix1.M31 + matrix2.M31;
			result.M32 = matrix1.M32 + matrix2.M32;
			result.M33 = matrix1.M33 + matrix2.M33;
			result.M34 = matrix1.M34 + matrix2.M34;
			result.M41 = matrix1.M41 + matrix2.M41;
			result.M42 = matrix1.M42 + matrix2.M42;
			result.M43 = matrix1.M43 + matrix2.M43;
			result.M44 = matrix1.M44 + matrix2.M44;
			return result;
		}

		public static Matrix44 operator -(Matrix44 matrix1, Matrix44 matrix2)
		{
			var result = new Matrix44();
			result.M11 = matrix1.M11 - matrix2.M11;
			result.M12 = matrix1.M12 - matrix2.M12;
			result.M13 = matrix1.M13 - matrix2.M13;
			result.M14 = matrix1.M14 - matrix2.M14;
			result.M21 = matrix1.M21 - matrix2.M21;
			result.M22 = matrix1.M22 - matrix2.M22;
			result.M23 = matrix1.M23 - matrix2.M23;
			result.M24 = matrix1.M24 - matrix2.M24;
			result.M31 = matrix1.M31 - matrix2.M31;
			result.M32 = matrix1.M32 - matrix2.M32;
			result.M33 = matrix1.M33 - matrix2.M33;
			result.M34 = matrix1.M34 - matrix2.M34;
			result.M41 = matrix1.M41 - matrix2.M41;
			result.M42 = matrix1.M42 - matrix2.M42;
			result.M43 = matrix1.M43 - matrix2.M43;
			result.M44 = matrix1.M44 - matrix2.M44;
			return result;
		}

		public static Matrix44 operator -(Matrix44 matrix)
		{
			matrix.M11 = -matrix.M11;
			matrix.M12 = -matrix.M12;
			matrix.M13 = -matrix.M13;
			matrix.M14 = -matrix.M14;
			matrix.M21 = -matrix.M21;
			matrix.M22 = -matrix.M22;
			matrix.M23 = -matrix.M23;
			matrix.M24 = -matrix.M24;
			matrix.M31 = -matrix.M31;
			matrix.M32 = -matrix.M32;
			matrix.M33 = -matrix.M33;
			matrix.M34 = -matrix.M34;
			matrix.M41 = -matrix.M41;
			matrix.M42 = -matrix.M42;
			matrix.M43 = -matrix.M43;
			matrix.M44 = -matrix.M44;
			return matrix;
		}

		public Vector3 TransformVector(Vector3 position)
		{
			var result = new Vector3(
				(position.X * M11) + (position.Y * M21) + (position.Z * M31) + M41,
				(position.X * M12) + (position.Y * M22) + (position.Z * M32) + M42,
				(position.X * M13) + (position.Y * M23) + (position.Z * M33) + M43);
			return result;
		}

		public Vector2 TransformVector(Vector2 position)
		{
			return (Vector2)TransformVector((Vector3)position);
		}

		public Vector4 TransformVector(Vector4 position)
		{
			return new Vector4(
				(position.X * M11) + (position.Y * M21) + (position.Z * M31) + (position.W * M41),
				(position.X * M12) + (position.Y * M22) + (position.Z * M32) + (position.W * M42),
				(position.X * M13) + (position.Y * M23) + (position.Z * M33) + (position.W * M43),
				(position.X * M14) + (position.Y * M24) + (position.Z * M34) + (position.W * M44)
			);
		}

		public Vector2 TransformNormal(Vector2 normal)
		{
			return new Vector2 {
				X = normal.X * M11 + normal.Y * M21,
				Y = normal.X * M12 + normal.Y * M22
			};
		}

		public Vector3 TransformNormal(Vector3 normal)
		{
			return new Vector3 {
				X = normal.X * M11 + normal.Y * M21 + normal.Z * M31,
				Y = normal.X * M12 + normal.Y * M22 + normal.Z * M32,
				Z = normal.X * M13 + normal.Y * M23 + normal.Z * M33
			};
		}

		public Vector3 ProjectVector(Vector3 position)
		{
			var result = TransformVector(new Vector4(position, 1));
			return (Vector3)result / result.W;
		}

		public Vector2 ProjectVector(Vector2 position)
		{
			var x = position.X * M11 + position.Y * M21 + M41;
			var y = position.X * M12 + position.Y * M22 + M42;
			var w = position.X * M14 + position.Y * M24 + M44;
			return new Vector2(x / w, y / w);
		}

		public static Vector3 operator *(Vector3 a, Matrix44 b)
		{
			return b.TransformVector(a);
		}

		public static Vector2 operator *(Vector2 a, Matrix44 b)
		{
			return b.TransformVector(a);
		}

		public override string ToString()
		{
			return "{" + String.Format("M11:{0} M12:{1} M13:{2} M14:{3}", M11, M12, M13, M14) + "}"
				+ " {" + String.Format("M21:{0} M22:{1} M23:{2} M24:{3}", M21, M22, M23, M24) + "}"
				+ " {" + String.Format("M31:{0} M32:{1} M33:{2} M34:{3}", M31, M32, M33, M34) + "}"
				+ " {" + String.Format("M41:{0} M42:{1} M43:{2} M44:{3}", M41, M42, M43, M44) + "}";
		}

		public Matrix44 Transpose()
		{
			return Transpose(this);
		}

		public static Matrix44 Transpose(Matrix44 matrix)
		{
			Matrix44 ret;
			Transpose(ref matrix, out ret);
			return ret;
		}

		public static void Transpose(ref Matrix44 matrix, out Matrix44 result)
		{
			result.M11 = matrix.M11;
			result.M12 = matrix.M21;
			result.M13 = matrix.M31;
			result.M14 = matrix.M41;

			result.M21 = matrix.M12;
			result.M22 = matrix.M22;
			result.M23 = matrix.M32;
			result.M24 = matrix.M42;

			result.M31 = matrix.M13;
			result.M32 = matrix.M23;
			result.M33 = matrix.M33;
			result.M34 = matrix.M43;

			result.M41 = matrix.M14;
			result.M42 = matrix.M24;
			result.M43 = matrix.M34;
			result.M44 = matrix.M44;
		}

		public Vector3 GetScale(bool checkReflexion)
		{
			var scale = new Vector3(
				(float)Math.Sqrt(M11 * M11 + M12 * M12 + M13 * M13),
				(float)Math.Sqrt(M21 * M21 + M22 * M22 + M23 * M23),
				(float)Math.Sqrt(M31 * M31 + M32 * M32 + M33 * M33)
			);
			if (!checkReflexion ||
				scale.X < Mathf.ZeroTolerance ||
				scale.Y < Mathf.ZeroTolerance ||
				scale.Z < Mathf.ZeroTolerance) {
				return scale;
			}
			var at = Backward / scale.Z;
			var up = Vector3.CrossProduct(at, Right / scale.X);
			var right = Vector3.CrossProduct(up, at);
			return new Vector3 {
				X = Vector3.DotProduct(right, Right) > 0f ? scale.X : -scale.X,
				Y = Vector3.DotProduct(up, Up) > 0f ? scale.Y : -scale.Y,
				Z = Vector3.DotProduct(at, Backward) > 0f ? scale.Z : -scale.Z
			};
		}
	}
}
