namespace AEther.WindowsForms
{
    public abstract class GraphicsComponent
    {

        public readonly Graphics Graphics;

        public GraphicsComponent(Graphics graphics)
        {
            Graphics = graphics;
        }

    }
}
