using System;
using Yuzu;

namespace Lime
{
	[YuzuCompact]
	public struct Matrix44 : IEquatable<Matrix44>
	{
		public Matrix44(
			float m11, float m12, float m13, float m14,
			float m21, float m22, float m23, float m24,
			float m31, float m32, float m33, float m34,
			float m41, float m42, float m43, float m44)
		{
			M11 = m11; M12 = m12; M13 = m13; M14 = m14;
			M21 = m21; M22 = m22; M23 = m23; M24 = m24;
			M31 = m31; M32 = m32; M33 = m33; M34 = m34;
			M41 = m41; M42 = m42; M43 = m43; M44 = m44;
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
				return new Vector3(M31, M32, M33);
			}
			set
			{
				M31 = value.X;
				M32 = value.Y;
				M33 = value.Z;
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
				M21 = -value.X;
				M22 = -value.Y;
				M23 = -value.Z;
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
				M31 = -value.X;
				M32 = -value.Y;
				M33 = -value.Z;
			}
		}

		public static Matrix44 Identity => identity;

		public float[] ToFloatArray() =>
			new float[] {
				M11, M12, M13, M14,
				M21, M22, M23, M24,
				M31, M32, M33, M34,
				M41, M42, M43, M44
			};

		public Vector3 Left
		{
			get
			{
				return new Vector3(-M11, -M12, -M13);
			}
			set
			{
				M11 = -value.X;
				M12 = -value.Y;
				M13 = -value.Z;
			}
		}

		public Vector3 Right
		{
			get
			{
				return new Vector3(M11, M12, M13);
			}
			set
			{
				M11 = value.X;
				M12 = value.Y;
				M13 = value.Z;
			}
		}

		public Vector3 Translation
		{
			get
			{
				return new Vector3(M41, M42, M43);
			}
			set
			{
				M41 = value.X;
				M42 = value.Y;
				M43 = value.Z;
			}
		}

		public Vector3 Up
		{
			get
			{
				return new Vector3(M21, M22, M23);
			}
			set
			{
				M21 = value.X;
				M22 = value.Y;
				M23 = value.Z;
			}
		}

		public Vector3 Scale => GetScale(true);

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

		public static void CreateLookAt(
			ref Vector3 cameraPosition, ref Vector3 cameraTarget, ref Vector3 cameraUpVector, out Matrix44 result)
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

			return new Matrix44 {
				M11 = (float)(2.0 / ((double)right - (double)left)),
				M12 = 0.0f,
				M13 = 0.0f,
				M14 = 0.0f,

				M21 = 0.0f,
				M22 = (float)(2.0 / ((double)top - (double)bottom)),
				M23 = 0.0f,
				M24 = 0.0f,

				M31 = 0.0f,
				M32 = 0.0f,
				M33 = (float)(1.0 / ((double)zNear - (double)zFar)),
				M34 = 0.0f,

				M41 = (float)(((double)left + (double)right) / ((double)left - (double)right)),
				M42 = (float)(((double)top + (double)bottom) / ((double)bottom - (double)top)),
				M43 = (float)(((double)zNear + (double)zFar) / ((double)zNear - (double)zFar)),
				M44 = 1.0f,
			};
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

		public static Matrix44 CreatePerspectiveOffCenter(
			float left, float right, float bottom, float top, float zNear, float zFar)
		{
			return new Matrix44 {
				M11 = (2f * zNear) / (right - left),
				M12 = 0f,
				M13 = 0f,
				M14 = 0f,

				M21 = 0f,
				M22 = (2f * zNear) / (top - bottom),
				M23 = 0f,
				M24 = 0f,

				M31 = (left + right) / (right - left),
				M32 = (top + bottom) / (top - bottom),
				M33 = -(zFar + zNear) / (zFar - zNear),
				M34 = -1f,

				M41 = 0f,
				M42 = 0f,
				M43 = -2f * (zFar * zNear) / (zFar - zNear),
				M44 = 0f,
			};
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

		public static Matrix44 CreateScale(Vector3 scales) =>
			new Matrix44 {
				M11 = scales.X, M12 = 0, M13 = 0, M14 = 0,
				M21 = 0, M22 = scales.Y, M23 = 0, M24 = 0,
				M31 = 0, M32 = 0, M33 = scales.Z, M34 = 0,
				M41 = 0, M42 = 0, M43 = 0, M44 = 1,
			};

		public static Matrix44 CreateTranslation(Vector3 position) =>
			new Matrix44 {
				M11 = 1, M12 = 0, M13 = 0, M14 = 0,
				M21 = 0, M22 = 1, M23 = 0, M24 = 0,
				M31 = 0, M32 = 0, M33 = 1, M34 = 0,
				M41 = position.X, M42 = position.Y, M43 = position.Z, M44 = 1,
			};

		public static Matrix44 CreateReflection(Plane plane)
		{
			var x = plane.Normal.X;
			var y = plane.Normal.Y;
			var z = plane.Normal.Z;
			var x2 = -2.0f * x;
			var y2 = -2.0f * y;
			var z2 = -2.0f * z;
			return new Matrix44 {
				M11 = (x2 * x) + 1.0f, M12 = y2 * x, M13 = z2 * x, M14 = 0.0f,
				M21 = x2 * y, M22 = (y2 * y) + 1.0f, M23 = z2 * y, M24 = 0.0f,
				M31 = x2 * z, M32 = y2 * z, M33 = (z2 * z) + 1.0f, M34 = 0.0f,
				M41 = x2 * plane.D, M42 = y2 * plane.D, M43 = z2 * plane.D, M44 = 1.0f,
			};
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

		public bool Equals(Matrix44 other) =>
			// Check main diagonal first to catch the most frequent case.
			M11 == other.M11 && M22 == other.M22 && M33 == other.M33 && M44 == other.M44 &&
			M12 == other.M12 && M13 == other.M13 && M14 == other.M14 &&
			M21 == other.M21 && M23 == other.M23 && M24 == other.M24 &&
			M31 == other.M31 && M32 == other.M32 && M34 == other.M34 &&
			M41 == other.M41 && M42 == other.M42 && M43 == other.M43;

		public override bool Equals(object obj) =>
			obj is Matrix44 && Equals((Matrix44)obj);

		public override int GetHashCode() =>
			M11.GetHashCode() + M12.GetHashCode() + M13.GetHashCode() + M14.GetHashCode() +
			M21.GetHashCode() + M22.GetHashCode() + M23.GetHashCode() + M24.GetHashCode() +
			M31.GetHashCode() + M32.GetHashCode() + M33.GetHashCode() + M34.GetHashCode() +
			M41.GetHashCode() + M42.GetHashCode() + M43.GetHashCode() + M44.GetHashCode();

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

		public static Matrix44 Lerp(Matrix44 a, Matrix44 b, float amount) =>
			new Matrix44 {
				M12 = a.M12 + (b.M12 - a.M12) * amount,
				M11 = a.M11 + (b.M11 - a.M11) * amount,
				M13 = a.M13 + (b.M13 - a.M13) * amount,
				M14 = a.M14 + (b.M14 - a.M14) * amount,

				M21 = a.M21 + (b.M21 - a.M21) * amount,
				M22 = a.M22 + (b.M22 - a.M22) * amount,
				M23 = a.M23 + (b.M23 - a.M23) * amount,
				M24 = a.M24 + (b.M24 - a.M24) * amount,

				M31 = a.M31 + (b.M31 - a.M31) * amount,
				M32 = a.M32 + (b.M32 - a.M32) * amount,
				M33 = a.M33 + (b.M33 - a.M33) * amount,
				M34 = a.M34 + (b.M34 - a.M34) * amount,

				M41 = a.M41 + (b.M41 - a.M41) * amount,
				M42 = a.M42 + (b.M42 - a.M42) * amount,
				M43 = a.M43 + (b.M43 - a.M43) * amount,
				M44 = a.M44 + (b.M44 - a.M44) * amount,
			};

		/// <summary>
		/// Decomposes a matrix into a scale, rotation, and translation.
		/// </summary>
		/// <param name="scale">When the method completes, contains the scaling component of the decomposed matrix.</param>
		/// <param name="rotation">When the method completes, contains the rtoation component of the decomposed matrix.</param>
		/// <param name="translation">When the method completes, contains the translation component of the decomposed matrix.</param>
		/// <remarks>
		/// This method is designed to decompose an SRT transformation matrix only.
		/// </remarks>
		public bool Decompose(out Vector3 scale, out Quaternion rotation, out Vector3 translation)
		{
			Matrix44 rotationMatrix;
			if (Decompose(out scale, out rotationMatrix, out translation)) {
				Quaternion.CreateFromRotationMatrix(ref rotationMatrix, out rotation);
				return true;
			}
			rotation = default(Quaternion);
			return false;
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
		public bool Decompose(out Vector3 scale, out Matrix44 rotation, out Vector3 translation)
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
			if (Math.Abs(scale.X) < Mathf.ZeroTolerance ||
				Math.Abs(scale.Y) < Mathf.ZeroTolerance ||
				Math.Abs(scale.Z) < Mathf.ZeroTolerance) {
				rotation = Matrix44.Identity;
				return false;
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

			return true;
		}

		public static bool operator ==(Matrix44 a, Matrix44 b) =>
			a.M11 == b.M11 && a.M12 == b.M12 && a.M13 == b.M13 && a.M14 == b.M14 &&
			a.M21 == b.M21 && a.M22 == b.M22 && a.M23 == b.M23 && a.M24 == b.M24 &&
			a.M31 == b.M31 && a.M32 == b.M32 && a.M33 == b.M33 && a.M34 == b.M34 &&
			a.M41 == b.M41 && a.M42 == b.M42 && a.M43 == b.M43 && a.M44 == b.M44;

		public static bool operator !=(Matrix44 a, Matrix44 b) =>
			a.M11 != b.M11 || a.M12 != b.M12 || a.M13 != b.M13 || a.M14 != b.M14 ||
			a.M21 != b.M21 || a.M22 != b.M22 || a.M23 != b.M23 || a.M24 != b.M24 ||
			a.M31 != b.M31 || a.M32 != b.M32 || a.M33 != b.M33 || a.M34 != b.M34 ||
			a.M41 != b.M41 || a.M42 != b.M42 || a.M43 != b.M43 || a.M44 != b.M44;

		public static Matrix44 operator *(Matrix44 a, Matrix44 b) =>
			new Matrix44 {
				M11 = a.M11 * b.M11 + a.M12 * b.M21 + a.M13 * b.M31 + a.M14 * b.M41,
				M12 = a.M11 * b.M12 + a.M12 * b.M22 + a.M13 * b.M32 + a.M14 * b.M42,
				M13 = a.M11 * b.M13 + a.M12 * b.M23 + a.M13 * b.M33 + a.M14 * b.M43,
				M14 = a.M11 * b.M14 + a.M12 * b.M24 + a.M13 * b.M34 + a.M14 * b.M44,

				M21 = a.M21 * b.M11 + a.M22 * b.M21 + a.M23 * b.M31 + a.M24 * b.M41,
				M22 = a.M21 * b.M12 + a.M22 * b.M22 + a.M23 * b.M32 + a.M24 * b.M42,
				M23 = a.M21 * b.M13 + a.M22 * b.M23 + a.M23 * b.M33 + a.M24 * b.M43,
				M24 = a.M21 * b.M14 + a.M22 * b.M24 + a.M23 * b.M34 + a.M24 * b.M44,

				M31 = a.M31 * b.M11 + a.M32 * b.M21 + a.M33 * b.M31 + a.M34 * b.M41,
				M32 = a.M31 * b.M12 + a.M32 * b.M22 + a.M33 * b.M32 + a.M34 * b.M42,
				M33 = a.M31 * b.M13 + a.M32 * b.M23 + a.M33 * b.M33 + a.M34 * b.M43,
				M34 = a.M31 * b.M14 + a.M32 * b.M24 + a.M33 * b.M34 + a.M34 * b.M44,

				M41 = a.M41 * b.M11 + a.M42 * b.M21 + a.M43 * b.M31 + a.M44 * b.M41,
				M42 = a.M41 * b.M12 + a.M42 * b.M22 + a.M43 * b.M32 + a.M44 * b.M42,
				M43 = a.M41 * b.M13 + a.M42 * b.M23 + a.M43 * b.M33 + a.M44 * b.M43,
				M44 = a.M41 * b.M14 + a.M42 * b.M24 + a.M43 * b.M34 + a.M44 * b.M44,
			};

		public static Matrix44 operator *(Matrix44 a, float x) =>
			new Matrix44 {
				M11 = a.M11 * x, M12 = a.M12 * x, M13 = a.M13 * x, M14 = a.M14 * x,
				M21 = a.M21 * x, M22 = a.M22 * x, M23 = a.M23 * x, M24 = a.M24 * x,
				M31 = a.M31 * x, M32 = a.M32 * x, M33 = a.M33 * x, M34 = a.M34 * x,
				M41 = a.M41 * x, M42 = a.M42 * x, M43 = a.M43 * x, M44 = a.M44 * x,
			};

		public static Matrix44 operator +(Matrix44 a, Matrix44 b) =>
			new Matrix44 {
				M11 = a.M11 + b.M11, M12 = a.M12 + b.M12, M13 = a.M13 + b.M13, M14 = a.M14 + b.M14,
				M21 = a.M21 + b.M21, M22 = a.M22 + b.M22, M23 = a.M23 + b.M23, M24 = a.M24 + b.M24,
				M31 = a.M31 + b.M31, M32 = a.M32 + b.M32, M33 = a.M33 + b.M33, M34 = a.M34 + b.M34,
				M41 = a.M41 + b.M41, M42 = a.M42 + b.M42, M43 = a.M43 + b.M43, M44 = a.M44 + b.M44,
			};

		public static Matrix44 operator -(Matrix44 a, Matrix44 b) =>
			new Matrix44 {
				M11 = a.M11 - b.M11, M12 = a.M12 - b.M12, M13 = a.M13 - b.M13, M14 = a.M14 - b.M14,
				M21 = a.M21 - b.M21, M22 = a.M22 - b.M22, M23 = a.M23 - b.M23, M24 = a.M24 - b.M24,
				M31 = a.M31 - b.M31, M32 = a.M32 - b.M32, M33 = a.M33 - b.M33, M34 = a.M34 - b.M34,
				M41 = a.M41 - b.M41, M42 = a.M42 - b.M42, M43 = a.M43 - b.M43, M44 = a.M44 - b.M44,
			};

		public static Matrix44 operator -(Matrix44 a) =>
			new Matrix44 {
				M11 = -a.M11, M12 = -a.M12, M13 = -a.M13, M14 = -a.M14,
				M21 = -a.M21, M22 = -a.M22, M23 = -a.M23, M24 = -a.M24,
				M31 = -a.M31, M32 = -a.M32, M33 = -a.M33, M34 = -a.M34,
				M41 = -a.M41, M42 = -a.M42, M43 = -a.M43, M44 = -a.M44,
			};

		public Vector3 TransformVector(Vector3 position) =>
			new Vector3(
				position.X * M11 + position.Y * M21 + position.Z * M31 + M41,
				position.X * M12 + position.Y * M22 + position.Z * M32 + M42,
				position.X * M13 + position.Y * M23 + position.Z * M33 + M43
			);

		public Vector2 TransformVector(Vector2 position) =>
			(Vector2)TransformVector((Vector3)position);

		public Vector4 TransformVector(Vector4 position) =>
			new Vector4(
				position.X * M12 + position.Y * M22 + position.Z * M32 + position.W * M42,
				position.X * M11 + position.Y * M21 + position.Z * M31 + position.W * M41,
				position.X * M13 + position.Y * M23 + position.Z * M33 + position.W * M43,
				position.X * M14 + position.Y * M24 + position.Z * M34 + position.W * M44
			);

		public Vector2 TransformNormal(Vector2 normal) =>
			new Vector2 {
				X = normal.X * M11 + normal.Y * M21,
				Y = normal.X * M12 + normal.Y * M22,
			};

		public Vector3 TransformNormal(Vector3 normal) =>
			new Vector3 {
				X = normal.X * M11 + normal.Y * M21 + normal.Z * M31,
				Y = normal.X * M12 + normal.Y * M22 + normal.Z * M32,
				Z = normal.X * M13 + normal.Y * M23 + normal.Z * M33
			};

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

		public static Vector3 operator *(Vector3 a, Matrix44 b) => b.TransformVector(a);

		public static Vector2 operator *(Vector2 a, Matrix44 b) => b.TransformVector(a);

		public override string ToString() =>
			$"{{M11:{M11} M12:{M12} M13:{M13} M14:{M14}}} " +
			$"{{M21:{M21} M22:{M22} M23:{M23} M24:{M24}}} " +
			$"{{M31:{M31} M32:{M32} M33:{M33} M34:{M34}}} " +
			$"{{M41:{M41} M42:{M42} M43:{M43} M44:{M44}}}";

		public Matrix44 Transpose() => Transpose(this);

		public static Matrix44 Transpose(Matrix44 matrix)
		{
			Matrix44 ret;
			Transpose(ref matrix, out ret);
			return ret;
		}

		public static void Transpose(ref Matrix44 a, out Matrix44 result)
		{
			result.M11 = a.M11; result.M12 = a.M21; result.M13 = a.M31; result.M14 = a.M41;
			result.M21 = a.M12; result.M22 = a.M22; result.M23 = a.M32; result.M24 = a.M42;
			result.M31 = a.M13; result.M32 = a.M23; result.M33 = a.M33; result.M34 = a.M43;
			result.M41 = a.M14; result.M42 = a.M24; result.M43 = a.M34; result.M44 = a.M44;
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

#if UNITY
		public static explicit operator UnityEngine.Matrix4x4(Matrix44 m)
		{
			var r = new UnityEngine.Matrix4x4();
			r.m00 = m.M11;
			r.m01 = m.M21;
			r.m02 = m.M31;
			r.m03 = m.M41;
			r.m10 = m.M12;
			r.m11 = m.M22;
			r.m12 = m.M32;
			r.m13 = m.M42;
			r.m20 = m.M13;
			r.m21 = m.M23;
			r.m22 = m.M33;
			r.m23 = m.M43;
			r.m30 = m.M14;
			r.m31 = m.M24;
			r.m32 = m.M34;
			r.m33 = m.M44;
			return r;
		}
#endif
	}
}