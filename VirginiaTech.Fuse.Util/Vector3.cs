using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VirginiaTech.Fuse.Util
{
    public enum Axis
    {
        X, Y, Z
    }

    public class Vector3
    {
        public readonly double X;
        public readonly double Y;
        public readonly double Z;

        /// <summary>
        /// Build a new representation of a 3D vector. Structs are
        /// a value type so these can be put on the stack.
        /// </summary>
        /// <param name="x">The x coordinate.</param>
        /// <param name="y">The y coordinate.</param>
        /// <param name="z">The z coordinate.</param>
        public Vector3(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        /// <summary>
        /// Invert the magnitude.
        /// </summary>
        /// <returns>A vector with opposite magnitude and equal direction.</returns>
        public Vector3 invert()
        {
            return new Vector3(-X, -Y, -Z);
        }

        /// <summary>
        /// Add two vectors.
        /// </summary>
        /// <param name="other">The other vector.</param>
        /// <returns>The sum of the vectors.</returns>
        public Vector3 add(Vector3 other)
        {
            return new Vector3(
                this.X + other.X,
                this.Y + other.Y,
                this.Z + other.Z);
        }

        /// <summary>
        /// Subtract one vector from another.
        /// </summary>
        /// <param name="other">The amount to subtract by.</param>
        /// <returns>The difference of the two vectors.</returns>
        public Vector3 subtract(Vector3 other)
        {
            return this.add(other.invert());
        }

        /// <summary>
        /// Return the magnitude.
        /// </summary>
        /// <returns>The magnitude of the vector.</returns>
        public double magnitude()
        {
            return Math.Sqrt(Math.Pow(X, 2) + Math.Pow(Y, 2) + Math.Pow(Z, 2));
        }

        /// <summary>
        /// Returns the dot product or scalar product of two vectors.
        /// </summary>
        /// <param name="other">The other vector.</param>
        /// <returns>The dot product.</returns>
        public double dotProduct(Vector3 other)
        {
            return (this.X * other.X) + (this.Y * other.Y) + (this.Z * other.Z);
        }

        /// <summary>
        /// Chops off the x coordinate of a vector. 
        /// This is useful for 2D vector math.
        /// </summary>
        /// <returns>the same vector, but with the x coordinate set to zero.</returns>
        public Vector3 dropX()
        {
            return new Vector3(0, this.Y, this.Z);
        }

        /// <summary>
        /// Chops off the y coordinate of a vector. 
        /// This is useful for 2D vector math.
        /// </summary>
        /// <returns>the same vector, but with the y coordinate set to zero.</returns>
        public Vector3 dropY()
        {
            return new Vector3(this.X, 0, this.Z);
        }

        /// <summary>
        /// Chops off the z coordinate of a vector. 
        /// This is useful for 2D vector math.
        /// </summary>
        /// <returns>the same vector, but with the z coordinate set to zero.</returns>
        public Vector3 dropZ()
        {
            return new Vector3(this.X, this.Y, 0);
        }

        /// <summary>
        /// Chop of a coordiante from a vector.
        /// This is useful for 2D vector math.
        /// </summary>
        /// <returns>the same vector, but with a coordnate set to zero</returns>
        public Vector3 drop(Axis axis)
        {
            switch (axis)
            {
                case Axis.X:
                    return dropX();
                case Axis.Y:
                    return dropY();
                case Axis.Z:
                    return dropZ();
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Get the angle between two vectors with this vector
        /// as the center point.
        /// </summary>
        /// <param name="vecA">The end of the first ray.</param>
        /// <param name="vecB">The end of the second ray.</param>
        /// <returns>The angle in radians.</returns>
        public double angleAround(Vector3 vecA, Vector3 vecB)
        {
            Vector3 normalVecA = vecA.subtract(this);
            Vector3 normalVecB = vecB.subtract(this);
            return Math.Acos(normalVecA.dotProduct(normalVecB) / (normalVecA.magnitude() * normalVecB.magnitude()));
        }
    }
}

