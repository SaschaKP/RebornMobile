

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace ClassicUO.Renderer
{
    internal class MatrixEffect : Effect
    {
        private Matrix _projectionMatrix = new Matrix(0f, //(float)( 2.0 / (double)viewport.Width ) is the actual value we will use
                                                      0.0f, 0.0f, 0.0f, 0.0f, 0f, //(float)( -2.0 / (double)viewport.Height ) is the actual value we will use
                                                      0.0f, 0.0f, 0.0f, 0.0f, 1.0f, 0.0f, -1.0f, 1.0f, 0.0f, 1.0f);


        public MatrixEffect(GraphicsDevice graphicsDevice, byte[] effectCode) : base(graphicsDevice, effectCode)
        {
            MatrixTransform = Parameters["MatrixTransform"];
        }

        protected MatrixEffect(Effect cloneSource) : base(cloneSource)
        {
        }



        public EffectParameter MatrixTransform { get; }

        public EffectPass this[int index] => CurrentTechnique.Passes[index];



        public virtual void ApplyStates()
        {
            Viewport viewport = GraphicsDevice.Viewport;
            _projectionMatrix.M11 = (float) (2.0 / viewport.Width);
            _projectionMatrix.M22 = (float) (-2.0 / viewport.Height);

            Matrix idendity = Matrix.Identity;
            Matrix.Multiply(ref idendity, ref _projectionMatrix, out var matrixTransform);

            MatrixTransform.SetValue(matrixTransform);

            foreach (EffectPass pa in CurrentTechnique.Passes) pa.Apply();
        }
    }
}