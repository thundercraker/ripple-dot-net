using System;
using System.Diagnostics;
using System.Linq;
using Ripple.Core.Binary;
using Ripple.Core.Hashing;
using Ripple.Core.Types;

namespace Ripple.Core.ShaMapTree
{
    public class ShaMapInner : ShaMapNode
    {
        public int Depth;
        internal int SlotBits = 0;
        internal int Version = 0;
        internal bool DoCoW;
        protected internal ShaMapNode[] Branches = new ShaMapNode[16];

        public ShaMapInner(int depth) : this(false, depth, 0)
        {
        }

        public ShaMapInner(bool isCopy, int depth, int version)
        {
            this.DoCoW = isCopy;
            this.Depth = depth;
            this.Version = version;
        }

        protected internal virtual ShaMapInner Copy(int version)
        {
            ShaMapInner copy = MakeInnerOfSameClass(Depth);
            Array.Copy(Branches, 0, copy.Branches, 0, Branches.Length);
            copy.SlotBits = SlotBits;
            copy.CachedHash = CachedHash;
            copy.Version = version;
            DoCoW = true;

            return copy;
        }

        protected internal virtual ShaMapInner MakeInnerOfSameClass(int depth)
        {
            return new ShaMapInner(true, depth, Version);
        }

        protected internal virtual ShaMapInner MakeInnerChild()
        {
            int childDepth = Depth + 1;
            if (childDepth >= 64)
            {
                throw new Exception();
            }
            return new ShaMapInner(DoCoW, childDepth, Version);
        }

        // Descend into the tree, find the leaf matching this index
        // and if the tree has it.
        protected internal virtual void SetLeaf(ShaMapLeaf leaf)
        {
            if (leaf.Version == -1)
            {
                leaf.Version = Version;
            }
            SetBranch(leaf.Index, leaf);
        }

        private void RemoveBranch(Hash256 index)
        {
            RemoveBranch(SelectBranch(index));
        }

        //public virtual void WalkLeaves(LeafWalker leafWalker)
        //{
        //    foreach (ShaMapNode branch in Branches)
        //    {
        //        if (branch != null)
        //        {
        //            if (branch.Inner)
        //            {
        //                branch.AsInner().WalkLeaves(leafWalker);
        //            }
        //            else if (branch.Leaf)
        //            {
        //                leafWalker.OnLeaf(branch.AsLeaf());
        //            }
        //        }
        //    }
        //}

        //public virtual void WalkTree(TreeWalker treeWalker)
        //{
        //    treeWalker.OnInner(this);
        //    foreach (ShaMapNode branch in Branches)
        //    {
        //        if (branch != null)
        //        {
        //            if (branch.Leaf)
        //            {
        //                ShaMapLeaf ln = branch.AsLeaf();
        //                treeWalker.OnLeaf(ln);
        //            }
        //            else if (branch.Inner)
        //            {
        //                ShaMapInner childInner = branch.AsInner();
        //                childInner.WalkTree(treeWalker);
        //            }
        //        }
        //    }

        //}

        //public virtual void WalkHashedTree(HashedTreeWalker walker)
        //{
        //    walker.OnInner(Hash(), this);

        //    foreach (ShaMapNode branch in Branches)
        //    {
        //        if (branch != null)
        //        {
        //            if (branch.Leaf)
        //            {
        //                ShaMapLeaf ln = branch.AsLeaf();
        //                walker.OnLeaf(branch.Hash(), ln);
        //            }
        //            else if (branch.Inner)
        //            {
        //                ShaMapInner childInner = branch.AsInner();
        //                childInner.WalkHashedTree(walker);
        //            }
        //        }
        //    }
        //}

        /// <returns> the `only child` leaf or null if other children </returns>
        public virtual ShaMapLeaf OnlyChildLeaf()
        {
            ShaMapLeaf leaf = null;
            var leaves = 0;

            foreach (var branch in Branches.Where(branch => branch != null))
            {
                if (branch.Inner)
                {
                    leaf = null;
                    break;
                }
                if (++leaves == 1)
                {
                    leaf = branch.AsLeaf();
                }
                else
                {
                    leaf = null;
                    break;
                }
            }
            return leaf;
        }

        public virtual bool RemoveLeaf(Hash256 index)
        {
            PathToIndex path = PathToIndex(index);
            if (!path.HasMatchedLeaf()) return false;
            var top = path.DirtyOrCopyInners();
            top.RemoveBranch(index);
            path.CollapseOnlyLeafChildInners();
            return true;
        }

        public virtual IShaMapItem<object> GetItem(Hash256 index)
        {
            return GetLeaf(index)?.Item;
        }

        public virtual bool AddItem(Hash256 index, IShaMapItem<object> item)
        {
            return AddLeaf(new ShaMapLeaf(index, item));
        }

        public virtual bool UpdateItem(Hash256 index, IShaMapItem<object> item)
        {
            return UpdateLeaf(new ShaMapLeaf(index, item));
        }

        public virtual bool HasLeaf(Hash256 index)
        {
            return PathToIndex(index).HasMatchedLeaf();
        }

        public virtual ShaMapLeaf GetLeaf(Hash256 index)
        {
            PathToIndex stack = PathToIndex(index);
            return stack.HasMatchedLeaf() ? stack.Leaf : null;
        }

        public virtual bool AddLeaf(ShaMapLeaf leaf)
        {
            PathToIndex stack = PathToIndex(leaf.Index);
            if (stack.HasMatchedLeaf())
            {
                return false;
            }
            var top = stack.DirtyOrCopyInners();
            top.AddLeafToTerminalInner(leaf);
            return true;
        }

        public virtual bool UpdateLeaf(ShaMapLeaf leaf)
        {
            PathToIndex stack = PathToIndex(leaf.Index);
            if (!stack.HasMatchedLeaf()) return false;
            ShaMapInner top = stack.DirtyOrCopyInners();
            // Why not update in place? Because of structural sharing
            top.SetLeaf(leaf);
            return true;
        }

        public virtual PathToIndex PathToIndex(Hash256 index)
        {
            return new PathToIndex(this, index);
        }

        /// <summary>
        /// This should only be called on the deepest inners, as it
        /// does not do any dirtying. </summary>
        /// <param name="leaf"> to add to inner </param>
        internal virtual void AddLeafToTerminalInner(ShaMapLeaf leaf)
        {
            var branch = GetBranch(leaf.Index);
            if (branch == null)
            {
                SetLeaf(leaf);
            }
            else if (branch.Inner)
            {
                throw new Exception();
            }
            else if (branch.Leaf)
            {
                var inner = MakeInnerChild();
                SetBranch(leaf.Index, inner);
                inner.AddLeafToTerminalInner(leaf);
                inner.AddLeafToTerminalInner(branch.AsLeaf());
            }
        }

        protected internal virtual void SetBranch(Hash256 index, ShaMapNode node)
        {
            SetBranch(SelectBranch(index), node);
        }

        protected internal virtual ShaMapNode GetBranch(Hash256 index)
        {
            return GetBranch(index.Nibblet(Depth));
        }

        public virtual ShaMapNode GetBranch(int i)
        {
            return Branches[i];
        }

        public virtual ShaMapNode Branch(int i)
        {
            return Branches[i];
        }

        protected internal virtual int SelectBranch(Hash256 index)
        {
            return index.Nibblet(Depth);
        }

        public virtual bool HasLeaf(int i)
        {
            return Branches[i].Leaf;
        }
        public virtual bool HasInner(int i)
        {
            return Branches[i].Inner;
        }
        public virtual bool HasNone(int i)
        {
            return Branches[i] == null;
        }

        private void SetBranch(int slot, ShaMapNode node)
        {
            SlotBits = SlotBits | (1 << slot);
            Branches[slot] = node;
            Invalidate();
        }

        private void RemoveBranch(int slot)
        {
            Branches[slot] = null;
            SlotBits = SlotBits & ~(1 << slot);
        }
        public virtual bool Empty()
        {
            return SlotBits == 0;
        }

        public override bool Inner => true;
        public override bool Leaf => false;

        internal override HashPrefix Prefix()
        {
            return HashPrefix.InnerNode;
        }

        public override void ToBytesSink(IBytesSink sink)
        {
            foreach (var branch in Branches)
            {
                if (branch != null)
                {
                    branch.Hash().ToBytes(sink);
                }
                else
                {
                    Hash256.Zero.ToBytes(sink);
                }
            }
        }

        public override Hash256 Hash()
        {
            if (Empty())
            {
                // empty inners have a hash of all Zero
                // it's only valid for a root node to be empty
                // any other inner node, must contain at least a
                // single leaf
                Debug.Assert(Depth == 0);
                return Hash256.Zero;
            }
            // hash the hashPrefix() and toBytesSink
            return base.Hash();
        }

        public virtual ShaMapLeaf GetLeafForUpdating(Hash256 leaf)
        {
            PathToIndex path = PathToIndex(leaf);
            if (path.HasMatchedLeaf())
            {
                return path.InvalidatedPossiblyCopiedLeafForUpdating();
            }
            return null;
        }

        public virtual int BranchCount()
        {
            return Branches.Count(branch => branch != null);
        }
    }
}