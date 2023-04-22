using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VoidNullEngine.Engine.Utils
{
    public interface INoiseGenerator
    {
        /**
         * <summary>
         * The lower bound of the noise value returned by <see cref="Generate(float, float, float)"/>.
         * </summary>
         */
        public float MinValue { get; }
        /**
         * <summary>
         * The upper bound of the noise value returned by <see cref="Generate(float, float, float)"/>.
         * </summary>
         */
        public float MaxValue { get; }

        /**
         * <summary>
         * Generates noise with the given coordinates.
         * </summary>
         * <returns>
         * A noise value between MinValue and MaxValue.
         * </returns>
         */
        public float Generate(float x, float y = 0, float z = 0);
        /**
         * <summary>
         * Generates normalized noise with the given coordinates.
         * </summary>
         * <returns>
         * A noise value normalized to [0, 1].
         * </returns>
         */
        public float GenerateNormal(float x, float y = 0, float z = 0);
        /** 
         * <summary>
         * Provides the local/fractional component of the provided coordinates. Mainly intended for use by <see cref="OctaveNoiseGenerator"/>.
         * </summary>
         * <returns>
         * A tuple containing each coordinate mapped to local space as a fractionnal value.
         * </returns>
         * <seealso cref="OctaveNoiseGenerator"/>
         */
        public (float,float,float) GetCoordsInLocalSpace(float x, float y, float z);
        /** 
         * <summary>
         * Provides the global/integral component of the provided coordinates. Mainly intended for use by <see cref="OctaveNoiseGenerator"/>.
         * </summary>
         * <returns>
         * A tuple containing each coordinate mapped to global space as an integral value.
         * </returns>
         * <seealso cref="OctaveNoiseGenerator"/>
         */
        public (float, float, float) GetCoordsInGlobalSpace(float x, float y, float z);

        /**
         * <summary>
         * Combines two <see cref="INoiseGenerator"/> into one, additioning their resulting noise.
         * </summary>
         * <returns>
         * A <see cref="CompoundNoiseGenerator"/> representing the addition of both <see cref="INoiseGenerator"/>s.
         * </returns>
         */
        public static CompoundNoiseGenerator operator +(INoiseGenerator left, INoiseGenerator right) =>
            new CompoundNoiseGenerator(left, right, (a, b) => a + b);

        /**
         * <summary>
         * Combines two <see cref="INoiseGenerator"/> into one, subtracting their resulting noise.
         * </summary>
         * <returns>
         * A <see cref="CompoundNoiseGenerator"/> representing the subtraction of both <see cref="INoiseGenerator"/>s.
         * </returns>
         */
        public static CompoundNoiseGenerator operator -(INoiseGenerator left, INoiseGenerator right) =>
            new CompoundNoiseGenerator(left, right, (a, b) => a - b);

        /**
         * <summary>
         * Combines two <see cref="INoiseGenerator"/> into one, multiplying their resulting noise.
         * </summary>
         * <returns>
         * A <see cref="CompoundNoiseGenerator"/> representing the multiplication of both <see cref="INoiseGenerator"/>s.
         * </returns>
         */
        public static CompoundNoiseGenerator operator *(INoiseGenerator left, INoiseGenerator right) =>
            new CompoundNoiseGenerator(left, right, (a, b) => a * b);
    }
}
