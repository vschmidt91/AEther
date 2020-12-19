using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;

namespace AEther.WindowsForms
{
    public static class SceneExt
    {

        public static Tree<(SceneNode, AffineTransform)> WithWorldTransform(this Tree<SceneNode> scene, AffineTransform? parentTransform = default)
        {
            return (scene, parentTransform ?? AffineTransform.Identity).UnfoldToTree(parent =>
            {
                var (node, transform) = parent;
                var worldTransform = node.Item.Transform.ToTransform() * transform;
                var item = (node.Item, worldTransform);
                var children = node.Children.Select(c => (c, worldTransform));
                return (item, children);
            });
        }

    }
}
