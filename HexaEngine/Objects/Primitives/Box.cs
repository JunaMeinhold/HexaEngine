namespace HexaEngine.Objects.Primitives
{
    using HexaEngine.Core.Graphics;
    using HexaEngine.Graphics;
    using System.Numerics;

    public class Cube : Primitive<Vector3>
    {
        public Cube(IGraphicsDevice device) : base(device)
        {
        }

        protected override (VertexBuffer<Vector3>, IndexBuffer?, InstanceBuffer?) InitializeMesh(IGraphicsDevice device)
        {
            VertexBuffer<Vector3> vertexBuffer = new(device, new Vector3[]
            {
                new Vector3(-1.0f, 1.0f, -1.0f),
                new Vector3(-1.0f, -1.0f, -1.0f),
                new Vector3(1.0f, -1.0f, -1.0f),
                new Vector3(1.0f, 1.0f, -1.0f),
                new Vector3(-1.0f, -1.0f, 1.0f),
                new Vector3(-1.0f, 1.0f, 1.0f),
                new Vector3(1.0f, -1.0f, 1.0f),
                new Vector3(1.0f, 1.0f, 1.0f),
                new Vector3(1.0f, -1.0f, 1.0f)
            });

            IndexBuffer indexBuffer = new(device, new int[]
            {
                0,1,2,2,3,0,
                4,1,0,0,5,4,
                2,6,7,7,3,2,
                4,5,7,7,6,4,
                0,3,7,7,5,0,
                1,4,2,2,4,6,
            });

            return (vertexBuffer, indexBuffer, null);
        }
    }
}