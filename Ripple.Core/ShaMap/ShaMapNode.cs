using System.CodeDom;
using Ripple.Core.Binary;
using Ripple.Core.Hashing;
using Ripple.Core.Types;

namespace Ripple.Core.ShaMap
{
    public abstract class ShaMapNode
    {
        protected Hash256 CachedHash;

        public abstract bool Leaf { get; }
        public abstract bool Inner { get; }

        public virtual ShaMapLeaf AsLeaf()
        {
            return (ShaMapLeaf)this;
        }

        public virtual ShaMapInner AsInner()
        {
            return (ShaMapInner)this;
        }

        internal abstract HashPrefix Prefix();
        public abstract void ToBytesSink(IBytesSink sink);

        public virtual void Invalidate()
        {
            CachedHash = null;
        }
        public virtual Hash256 Hash()
        {
            return CachedHash ?? (CachedHash = CreateHash());
        }

        public virtual Hash256 CreateHash()
        {
            var half = new Sha512(Prefix().Bytes());
            ToBytesSink(half);
            return new Hash256(half.Finish256());
        }
        ///// <summary>
        ///// Walk any leaves, possibly this node itself, if it's terminal.
        ///// </summary>
        //public virtual void WalkAnyLeaves(LeafWalker leafWalker)
        //{
        //    if (Leaf)
        //    {
        //        leafWalker.OnLeaf(AsLeaf());
        //    }
        //    else
        //    {
        //        AsInner().WalkLeaves(leafWalker);
        //    }
        //}
    }
}

