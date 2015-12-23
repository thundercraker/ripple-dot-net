using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Ripple.Core.Types;

namespace Ripple.Core.ShaMap
{
    public class PathToIndex
    {
        public Hash256 Index;
        public ShaMapLeaf Leaf;

        private LinkedList<ShaMapInner> _inners;
        private ShaMapInner[] _dirtied;
        private bool _matched;

        public virtual bool HasLeaf()
        {
            return Leaf != null;
        }
        public virtual bool LeafMatchedIndex()
        {
            return _matched;
        }
        public virtual bool CopyLeafOnUpdate()
        {
            return Leaf.Version != _dirtied[0].Version;
        }

        internal virtual int Size()
        {
            return _inners.Count;
        }

        public virtual ShaMapInner Top()
        {
            return _dirtied[_dirtied.Length - 1];
        }

        // returns the
        public virtual ShaMapInner DirtyOrCopyInners()
        {
            if (MaybeCopyOnWrite())
            {
                var ix = 0;
                // We want to make a uniformly accessed array of the inners
                _dirtied = new ShaMapInner[_inners.Count];
                // from depth 0 to 1, to 2, to 3, don't be fooled by the api
                IEnumerator<ShaMapInner> it = _inners.GetEnumerator();

                // This is actually the root which COULD be the top of the stack
                // Think about it ;)
                var top = it.Current;
                _dirtied[ix++] = top;
                top.Invalidate();

                while (it.MoveNext())
                {
                    var next = it.Current;
                    var doCopies = next.Version != top.Version;

                    if (doCopies)
                    {
                        var copy = next.Copy(top.Version);
                        copy.Invalidate();
                        top.SetBranch(Index, copy);
                        next = copy;
                    }
                    else
                    {
                        next.Invalidate();
                    }
                    top = next;
                    _dirtied[ix++] = top;
                }
                return top;
            }
            CopyInnersToDirtiedArray();
            return _inners.Last.Value;
        }

        public virtual bool HasMatchedLeaf()
        {
            return HasLeaf() && LeafMatchedIndex();
        }

        public virtual void CollapseOnlyLeafChildInners()
        {
            Debug.Assert(_dirtied != null);
            ShaMapLeaf onlyChild = null;

            for (var i = _dirtied.Length - 1; i >= 0; i--)
            {
                var next = _dirtied[i];
                if (onlyChild != null)
                {
                    next.SetLeaf(onlyChild);
                }
                onlyChild = next.OnlyChildLeaf();
                if (onlyChild == null)
                {
                    break;
                }
            }
        }

        private void CopyInnersToDirtiedArray()
        {
            var ix = 0;
            _dirtied = new ShaMapInner[_inners.Count];
            IEnumerator<ShaMapInner> descending = _inners.GetEnumerator();
            while (descending.MoveNext())
            {
                ShaMapInner next = descending.Current;
                _dirtied[ix++] = next;
                next.Invalidate();
            }
        }

        private bool MaybeCopyOnWrite()
        {
            return _inners.Last().DoCoW;
        }

        public PathToIndex(ShaMapInner root, Hash256 index)
        {
            Index = index;
            MakeStack(root, index);
        }

        private void MakeStack(ShaMapInner root, Hash256 index)
        {
            _inners = new LinkedList<ShaMapInner>();
            var top = root;

            while (true)
            {
                _inners.AddLast(top);
                var existing = top.GetBranch(index);
                if (existing == null)
                {
                    break;
                }
                if (existing.Leaf)
                {
                    Leaf = existing.AsLeaf();
                    _matched = Leaf.Index.Equals(index);
                    break;
                }
                if (existing.Inner)
                {
                    top = existing.AsInner();
                }
            }
        }

        public virtual ShaMapLeaf InvalidatedPossiblyCopiedLeafForUpdating()
        {
            Debug.Assert(_matched);
            if (_dirtied == null)
            {
                DirtyOrCopyInners();
            }
            var theLeaf = Leaf;

            if (CopyLeafOnUpdate())
            {
                theLeaf = Leaf.Copy();
                Top().SetLeaf(theLeaf);
            }
            theLeaf.Invalidate();
            return theLeaf;
        }
    }
}