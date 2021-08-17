namespace AEther.WindowsForms
{
    public class Node : GraphicsComponent
    {

        public AffineTransform LocalTransform = AffineTransform.Identity;
        public AffineTransform WorldTransform = AffineTransform.Identity;

        public Node(Graphics graphics)
            : base(graphics)
        { }

        public virtual void Update(AffineTransform parentTransform, float dt)
        {

            WorldTransform = parentTransform * LocalTransform;

        }

    }
}
