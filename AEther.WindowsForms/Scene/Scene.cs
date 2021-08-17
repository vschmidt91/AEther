namespace AEther.WindowsForms
{
    public class Scene
    {

        public readonly SceneNode Node;
        public readonly Scene[] Children;

        public Scene(SceneNode node, Scene[] children)
        {
            Node = node;
            Children = children;
        }

        public void Update(float dt, AffineTransform parentTransform)
        {
            var transform = parentTransform * Node.Transform.ToTransform();
            //Node.Update(dt, transform);
            foreach (var child in Children)
            {
                child.Update(dt, transform);
            }
        }

    }
}
