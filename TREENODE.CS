#region 어셈블리 System.Windows.Forms, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089
// C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.8\System.Windows.Forms.dll
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion

using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Security.Permissions;
using System.Text;

namespace System.Windows.Forms
{
    //
    // 요약:
    //     System.Windows.Forms.TreeView의 노드를 나타냅니다.
    [Serializable]
    [TypeConverter(typeof(TreeNodeConverter))]
    [DefaultProperty("Text")]
    public class TreeNode : MarshalByRefObject, ICloneable, ISerializable
    {
        internal class TreeNodeImageIndexer : ImageList.Indexer
        {
            public enum ImageListType
            {
                Default,
                State
            }

            private TreeNode owner;

            private ImageListType imageListType;

            public override ImageList ImageList
            {
                get
                {
                    if (owner.TreeView != null)
                    {
                        if (imageListType == ImageListType.State)
                        {
                            return owner.TreeView.StateImageList;
                        }

                        return owner.TreeView.ImageList;
                    }

                    return null;
                }
                set
                {
                }
            }

            public TreeNodeImageIndexer(TreeNode node, ImageListType imageListType)
            {
                owner = node;
                this.imageListType = imageListType;
            }
        }

        private const int SHIFTVAL = 12;

        private const int CHECKED = 8192;

        private const int UNCHECKED = 4096;

        private const int ALLOWEDIMAGES = 14;

        internal const int MAX_TREENODES_OPS = 200;

        internal OwnerDrawPropertyBag propBag;

        internal IntPtr handle;

        internal string text;

        internal string name;

        private const int TREENODESTATE_isChecked = 1;

        private BitVector32 treeNodeState;

        private TreeNodeImageIndexer imageIndexer;

        private TreeNodeImageIndexer selectedImageIndexer;

        private TreeNodeImageIndexer stateImageIndexer;

        private string toolTipText = "";

        private ContextMenu contextMenu;

        private ContextMenuStrip contextMenuStrip;

        internal bool nodesCleared;

        internal int index;

        internal int childCount;

        internal TreeNode[] children;

        internal TreeNode parent;

        internal TreeView treeView;

        private bool expandOnRealization;

        private bool collapseOnRealization;

        private TreeNodeCollection nodes;

        private object userData;

        private static readonly int insertMask = 35;

        internal TreeNodeImageIndexer ImageIndexer
        {
            get
            {
                if (imageIndexer == null)
                {
                    imageIndexer = new TreeNodeImageIndexer(this, TreeNodeImageIndexer.ImageListType.Default);
                }

                return imageIndexer;
            }
        }

        internal TreeNodeImageIndexer SelectedImageIndexer
        {
            get
            {
                if (selectedImageIndexer == null)
                {
                    selectedImageIndexer = new TreeNodeImageIndexer(this, TreeNodeImageIndexer.ImageListType.Default);
                }

                return selectedImageIndexer;
            }
        }

        internal TreeNodeImageIndexer StateImageIndexer
        {
            get
            {
                if (stateImageIndexer == null)
                {
                    stateImageIndexer = new TreeNodeImageIndexer(this, TreeNodeImageIndexer.ImageListType.State);
                }

                return stateImageIndexer;
            }
        }

        //
        // 요약:
        //     트리 노드의 배경색을 가져오거나 설정합니다.
        //
        // 반환 값:
        //     트리 노드의 배경 System.Drawing.Color입니다. 기본값은 System.Drawing.Color.Empty입니다.
        [SRCategory("CatAppearance")]
        [SRDescription("TreeNodeBackColorDescr")]
        public Color BackColor
        {
            get
            {
                if (propBag == null)
                {
                    return Color.Empty;
                }

                return propBag.BackColor;
            }
            set
            {
                Color backColor = BackColor;
                if (value.IsEmpty)
                {
                    if (propBag != null)
                    {
                        propBag.BackColor = Color.Empty;
                        RemovePropBagIfEmpty();
                    }

                    if (!backColor.IsEmpty)
                    {
                        InvalidateHostTree();
                    }
                }
                else
                {
                    if (propBag == null)
                    {
                        propBag = new OwnerDrawPropertyBag();
                    }

                    propBag.BackColor = value;
                    if (!value.Equals(backColor))
                    {
                        InvalidateHostTree();
                    }
                }
            }
        }

        //
        // 요약:
        //     트리 노드의 경계를 가져옵니다.
        //
        // 반환 값:
        //     트리 노드의 경계를 나타내는 System.Drawing.Rectangle입니다.
        [Browsable(false)]
        public unsafe Rectangle Bounds
        {
            get
            {
                TreeView treeView = TreeView;
                if (treeView == null || treeView.IsDisposed)
                {
                    return Rectangle.Empty;
                }

                NativeMethods.RECT lParam = default(NativeMethods.RECT);
                *(IntPtr*)(&lParam.left) = Handle;
                if ((int)UnsafeNativeMethods.SendMessage(new HandleRef(treeView, treeView.Handle), 4356, 1, ref lParam) == 0)
                {
                    return Rectangle.Empty;
                }

                return Rectangle.FromLTRB(lParam.left, lParam.top, lParam.right, lParam.bottom);
            }
        }

        internal unsafe Rectangle RowBounds
        {
            get
            {
                TreeView treeView = TreeView;
                NativeMethods.RECT lParam = default(NativeMethods.RECT);
                *(IntPtr*)(&lParam.left) = Handle;
                if (treeView == null || treeView.IsDisposed)
                {
                    return Rectangle.Empty;
                }

                if ((int)UnsafeNativeMethods.SendMessage(new HandleRef(treeView, treeView.Handle), 4356, 0, ref lParam) == 0)
                {
                    return Rectangle.Empty;
                }

                return Rectangle.FromLTRB(lParam.left, lParam.top, lParam.right, lParam.bottom);
            }
        }

        internal bool CheckedStateInternal
        {
            get
            {
                return treeNodeState[1];
            }
            set
            {
                treeNodeState[1] = value;
            }
        }

        internal bool CheckedInternal
        {
            get
            {
                return CheckedStateInternal;
            }
            set
            {
                CheckedStateInternal = value;
                if (!(handle == IntPtr.Zero))
                {
                    TreeView treeView = TreeView;
                    if (treeView != null && treeView.IsHandleCreated && !treeView.IsDisposed)
                    {
                        NativeMethods.TV_ITEM lParam = default(NativeMethods.TV_ITEM);
                        lParam.mask = 24;
                        lParam.hItem = handle;
                        lParam.stateMask = 61440;
                        lParam.state |= (value ? 8192 : 4096);
                        UnsafeNativeMethods.SendMessage(new HandleRef(treeView, treeView.Handle), NativeMethods.TVM_SETITEM, 0, ref lParam);
                    }
                }
            }
        }

        //
        // 요약:
        //     트리 노드가 선택되어 있는지 여부를 나타내는 값을 가져오거나 설정합니다.
        //
        // 반환 값:
        //     트리 노드가 선택되어 있으면 true이고, 그렇지 않으면 false입니다.
        [SRCategory("CatBehavior")]
        [SRDescription("TreeNodeCheckedDescr")]
        [DefaultValue(false)]
        public bool Checked
        {
            get
            {
                return CheckedInternal;
            }
            set
            {
                TreeView treeView = TreeView;
                if (treeView != null)
                {
                    if (!treeView.TreeViewBeforeCheck(this, TreeViewAction.Unknown))
                    {
                        CheckedInternal = value;
                        treeView.TreeViewAfterCheck(this, TreeViewAction.Unknown);
                    }
                }
                else
                {
                    CheckedInternal = value;
                }
            }
        }

        //
        // 요약:
        //     이 트리 노드와 연결된 바로 가기 메뉴를 가져옵니다.
        //
        // 반환 값:
        //     트리 노드와 연결된 System.Windows.Forms.ContextMenu입니다.
        [SRCategory("CatBehavior")]
        [DefaultValue(null)]
        [SRDescription("ControlContextMenuDescr")]
        public virtual ContextMenu ContextMenu
        {
            get
            {
                return contextMenu;
            }
            set
            {
                contextMenu = value;
            }
        }

        //
        // 요약:
        //     이 트리 노드와 연결된 바로 가기 메뉴를 가져오거나 설정합니다.
        //
        // 반환 값:
        //     트리 노드와 연결된 System.Windows.Forms.ContextMenuStrip입니다.
        [SRCategory("CatBehavior")]
        [DefaultValue(null)]
        [SRDescription("ControlContextMenuDescr")]
        public virtual ContextMenuStrip ContextMenuStrip
        {
            get
            {
                return contextMenuStrip;
            }
            set
            {
                contextMenuStrip = value;
            }
        }

        //
        // 요약:
        //     트리 노드 컬렉션에 있는 첫째 자식 트리 노드를 가져옵니다.
        //
        // 반환 값:
        //     System.Windows.Forms.TreeNode 컬렉션에 있는 첫째 자식 System.Windows.Forms.TreeNode.Nodes입니다.
        [Browsable(false)]
        public TreeNode FirstNode
        {
            get
            {
                if (childCount == 0)
                {
                    return null;
                }

                return children[0];
            }
        }

        private TreeNode FirstVisibleParent
        {
            get
            {
                TreeNode treeNode = this;
                while (treeNode != null && treeNode.Bounds.IsEmpty)
                {
                    treeNode = treeNode.Parent;
                }

                return treeNode;
            }
        }

        //
        // 요약:
        //     트리 노드의 전경색을 가져오거나 설정합니다.
        //
        // 반환 값:
        //     트리 노드의 전경 System.Drawing.Color입니다.
        [SRCategory("CatAppearance")]
        [SRDescription("TreeNodeForeColorDescr")]
        public Color ForeColor
        {
            get
            {
                if (propBag == null)
                {
                    return Color.Empty;
                }

                return propBag.ForeColor;
            }
            set
            {
                Color foreColor = ForeColor;
                if (value.IsEmpty)
                {
                    if (propBag != null)
                    {
                        propBag.ForeColor = Color.Empty;
                        RemovePropBagIfEmpty();
                    }

                    if (!foreColor.IsEmpty)
                    {
                        InvalidateHostTree();
                    }
                }
                else
                {
                    if (propBag == null)
                    {
                        propBag = new OwnerDrawPropertyBag();
                    }

                    propBag.ForeColor = value;
                    if (!value.Equals(foreColor))
                    {
                        InvalidateHostTree();
                    }
                }
            }
        }

        //
        // 요약:
        //     루트 트리 노드에서 현재 트리 노드까지의 경로를 가져옵니다.
        //
        // 반환 값:
        //     루트 트리 노드에서 현재 트리 노드까지의 경로입니다.
        //
        // 예외:
        //   T:System.InvalidOperationException:
        //     System.Windows.Forms.TreeView에 노드가 없는 경우
        [Browsable(false)]
        public string FullPath
        {
            get
            {
                TreeView treeView = TreeView;
                if (treeView != null)
                {
                    StringBuilder stringBuilder = new StringBuilder();
                    GetFullPath(stringBuilder, treeView.PathSeparator);
                    return stringBuilder.ToString();
                }

                throw new InvalidOperationException(SR.GetString("TreeNodeNoParent"));
            }
        }

        //
        // 요약:
        //     트리 노드의 핸들을 가져옵니다.
        //
        // 반환 값:
        //     트리 노드 핸들입니다.
        [Browsable(false)]
        public IntPtr Handle
        {
            get
            {
                if (handle == IntPtr.Zero)
                {
                    TreeView.CreateControl();
                }

                return handle;
            }
        }

        //
        // 요약:
        //     트리 노드를 선택하지 않은 상태일 때 표시되는 이미지의 이미지 목록 인덱스 값을 가져오거나 설정합니다.
        //
        // 반환 값:
        //     할당된 System.Windows.Forms.ImageList에서 이미지의 위치를 나타내는 인덱스(0부터 시작) 값입니다.
        [Localizable(true)]
        [SRCategory("CatBehavior")]
        [SRDescription("TreeNodeImageIndexDescr")]
        [TypeConverter(typeof(TreeViewImageIndexConverter))]
        [Editor("System.Windows.Forms.Design.ImageIndexEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor))]
        [RefreshProperties(RefreshProperties.Repaint)]
        [DefaultValue(-1)]
        [RelatedImageList("TreeView.ImageList")]
        public int ImageIndex
        {
            get
            {
                return ImageIndexer.Index;
            }
            set
            {
                ImageIndexer.Index = value;
                UpdateNode(2);
            }
        }

        //
        // 요약:
        //     트리 노드가 선택되지 않은 상태일 때 이 트리 노드와 연결된 이미지의 키를 가져오거나 설정합니다.
        //
        // 반환 값:
        //     트리 노드가 선택되지 않은 상태일 때 이 트리 노드와 연결된 이미지의 키입니다.
        [Localizable(true)]
        [SRCategory("CatBehavior")]
        [SRDescription("TreeNodeImageKeyDescr")]
        [TypeConverter(typeof(TreeViewImageKeyConverter))]
        [DefaultValue("")]
        [Editor("System.Windows.Forms.Design.ImageIndexEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor))]
        [RefreshProperties(RefreshProperties.Repaint)]
        [RelatedImageList("TreeView.ImageList")]
        public string ImageKey
        {
            get
            {
                return ImageIndexer.Key;
            }
            set
            {
                ImageIndexer.Key = value;
                UpdateNode(2);
            }
        }

        //
        // 요약:
        //     트리 노드 컬렉션에 있는 트리 노드의 위치를 가져옵니다.
        //
        // 반환 값:
        //     System.Windows.Forms.TreeNode.Nodes 컬렉션에 있는 트리 노드의 위치를 나타내는 인덱스(0부터 시작하는) 값입니다.
        [SRCategory("CatBehavior")]
        [SRDescription("TreeNodeIndexDescr")]
        public int Index => index;

        //
        // 요약:
        //     트리 노드가 편집 가능한 상태에 있는지 여부를 나타내는 값을 가져옵니다.
        //
        // 반환 값:
        //     트리 노드가 편집 가능한 상태이면 true이고, 그렇지 않으면 false입니다.
        [Browsable(false)]
        public bool IsEditing
        {
            get
            {
                TreeView treeView = TreeView;
                if (treeView != null)
                {
                    return treeView.editNode == this;
                }

                return false;
            }
        }

        //
        // 요약:
        //     트리 노드가 확장된 상태에 있는지 여부를 나타내는 값을 가져옵니다.
        //
        // 반환 값:
        //     트리 노드가 확장된 상태이면 true이고, 그렇지 않으면 false입니다.
        [Browsable(false)]
        public bool IsExpanded
        {
            get
            {
                if (handle == IntPtr.Zero)
                {
                    return expandOnRealization;
                }

                return (State & 0x20) != 0;
            }
        }

        //
        // 요약:
        //     트리 노드가 선택 상태에 있는지 여부를 나타내는 값을 가져옵니다.
        //
        // 반환 값:
        //     트리 노드가 선택된 상태이면 true이고, 그렇지 않으면 false입니다.
        [Browsable(false)]
        public bool IsSelected
        {
            get
            {
                if (handle == IntPtr.Zero)
                {
                    return false;
                }

                return (State & 2) != 0;
            }
        }

        //
        // 요약:
        //     트리 노드의 전체 또는 일부가 표시되는지 여부를 나타내는 값을 가져옵니다.
        //
        // 반환 값:
        //     트리 노드의 전체 또는 일부가 표시되면 true이고, 그렇지 않으면 false입니다.
        [Browsable(false)]
        public unsafe bool IsVisible
        {
            get
            {
                if (handle == IntPtr.Zero)
                {
                    return false;
                }

                TreeView treeView = TreeView;
                if (treeView.IsDisposed)
                {
                    return false;
                }

                NativeMethods.RECT lParam = default(NativeMethods.RECT);
                *(IntPtr*)(&lParam.left) = Handle;
                bool flag = (int)UnsafeNativeMethods.SendMessage(new HandleRef(treeView, treeView.Handle), 4356, 1, ref lParam) != 0;
                if (flag)
                {
                    Size clientSize = treeView.ClientSize;
                    flag = lParam.bottom > 0 && lParam.right > 0 && lParam.top < clientSize.Height && lParam.left < clientSize.Width;
                }

                return flag;
            }
        }

        //
        // 요약:
        //     마지막 자식 트리 노드를 가져옵니다.
        //
        // 반환 값:
        //     마지막 자식 트리 노드를 나타내는 System.Windows.Forms.TreeNode입니다.
        [Browsable(false)]
        public TreeNode LastNode
        {
            get
            {
                if (childCount == 0)
                {
                    return null;
                }

                return children[childCount - 1];
            }
        }

        //
        // 요약:
        //     System.Windows.Forms.TreeView 컨트롤에 있는 트리 노드의 0부터 시작하는 깊이를 가져옵니다.
        //
        // 반환 값:
        //     System.Windows.Forms.TreeView 컨트롤에 있는 트리 노드의 0부터 시작하는 깊이입니다.
        [Browsable(false)]
        public int Level
        {
            get
            {
                if (Parent == null)
                {
                    return 0;
                }

                return Parent.Level + 1;
            }
        }

        //
        // 요약:
        //     다음 형제 트리 노드를 가져옵니다.
        //
        // 반환 값:
        //     다음 형제 트리 노드를 나타내는 System.Windows.Forms.TreeNode입니다.
        [Browsable(false)]
        public TreeNode NextNode
        {
            get
            {
                if (index + 1 < parent.Nodes.Count)
                {
                    return parent.Nodes[index + 1];
                }

                return null;
            }
        }

        //
        // 요약:
        //     다음의 표시되는 트리 노드를 가져옵니다.
        //
        // 반환 값:
        //     다음의 표시되는 트리 노드를 나타내는 System.Windows.Forms.TreeNode입니다.
        [Browsable(false)]
        public TreeNode NextVisibleNode
        {
            get
            {
                TreeView treeView = TreeView;
                if (treeView == null || treeView.IsDisposed)
                {
                    return null;
                }

                TreeNode firstVisibleParent = FirstVisibleParent;
                if (firstVisibleParent != null)
                {
                    IntPtr intPtr = UnsafeNativeMethods.SendMessage(new HandleRef(treeView, treeView.Handle), 4362, 6, firstVisibleParent.Handle);
                    if (intPtr != IntPtr.Zero)
                    {
                        return treeView.NodeFromHandle(intPtr);
                    }
                }

                return null;
            }
        }

        //
        // 요약:
        //     트리 노드의 레이블에 텍스트를 표시하기 위해 사용된 글꼴을 가져오거나 설정합니다.
        //
        // 반환 값:
        //     트리 노드의 레이블에 텍스트를 표시하기 위해 사용된 System.Drawing.Font입니다.
        [Localizable(true)]
        [SRCategory("CatAppearance")]
        [SRDescription("TreeNodeNodeFontDescr")]
        [DefaultValue(null)]
        public Font NodeFont
        {
            get
            {
                if (propBag == null)
                {
                    return null;
                }

                return propBag.Font;
            }
            set
            {
                Font nodeFont = NodeFont;
                if (value == null)
                {
                    if (propBag != null)
                    {
                        propBag.Font = null;
                        RemovePropBagIfEmpty();
                    }

                    if (nodeFont != null)
                    {
                        InvalidateHostTree();
                    }
                }
                else
                {
                    if (propBag == null)
                    {
                        propBag = new OwnerDrawPropertyBag();
                    }

                    propBag.Font = value;
                    if (!value.Equals(nodeFont))
                    {
                        InvalidateHostTree();
                    }
                }
            }
        }

        //
        // 요약:
        //     현재 트리 노드에 할당된 System.Windows.Forms.TreeNode 개체의 컬렉션을 가져옵니다.
        //
        // 반환 값:
        //     현재 트리 노드에 할당된 트리 노드를 나타내는 System.Windows.Forms.TreeNodeCollection입니다.
        [ListBindable(false)]
        [Browsable(false)]
        public TreeNodeCollection Nodes
        {
            get
            {
                if (nodes == null)
                {
                    nodes = new TreeNodeCollection(this);
                }

                return nodes;
            }
        }

        //
        // 요약:
        //     현재 트리 노드의 부모 트리 노드를 가져옵니다.
        //
        // 반환 값:
        //     현재 트리 노드의 부모를 나타내는 System.Windows.Forms.TreeNode입니다.
        [Browsable(false)]
        public TreeNode Parent
        {
            get
            {
                TreeView treeView = TreeView;
                if (treeView != null && parent == treeView.root)
                {
                    return null;
                }

                return parent;
            }
        }

        //
        // 요약:
        //     이전 형제 트리 노드를 가져옵니다.
        //
        // 반환 값:
        //     이전 형제 트리 노드를 나타내는 System.Windows.Forms.TreeNode입니다.
        [Browsable(false)]
        public TreeNode PrevNode
        {
            get
            {
                int num = index;
                int fixedIndex = parent.Nodes.FixedIndex;
                if (fixedIndex > 0)
                {
                    num = fixedIndex;
                }

                if (num > 0 && num <= parent.Nodes.Count)
                {
                    return parent.Nodes[num - 1];
                }

                return null;
            }
        }

        //
        // 요약:
        //     이전에 표시되는 트리 노드를 가져옵니다.
        //
        // 반환 값:
        //     이전에 표시되는 트리 노드를 나타내는 System.Windows.Forms.TreeNode입니다.
        [Browsable(false)]
        public TreeNode PrevVisibleNode
        {
            get
            {
                TreeNode firstVisibleParent = FirstVisibleParent;
                TreeView treeView = TreeView;
                if (firstVisibleParent != null)
                {
                    if (treeView == null || treeView.IsDisposed)
                    {
                        return null;
                    }

                    IntPtr intPtr = UnsafeNativeMethods.SendMessage(new HandleRef(treeView, treeView.Handle), 4362, 7, firstVisibleParent.Handle);
                    if (intPtr != IntPtr.Zero)
                    {
                        return treeView.NodeFromHandle(intPtr);
                    }
                }

                return null;
            }
        }

        //
        // 요약:
        //     트리 노드를 선택한 상태일 때 표시되는 이미지의 이미지 목록 인덱스 값을 가져오거나 설정합니다.
        //
        // 반환 값:
        //     System.Windows.Forms.ImageList에서 이미지의 위치를 나타내는 인덱스(0부터 시작) 값입니다.
        [Localizable(true)]
        [SRCategory("CatBehavior")]
        [SRDescription("TreeNodeSelectedImageIndexDescr")]
        [TypeConverter(typeof(TreeViewImageIndexConverter))]
        [DefaultValue(-1)]
        [RefreshProperties(RefreshProperties.Repaint)]
        [Editor("System.Windows.Forms.Design.ImageIndexEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor))]
        [RelatedImageList("TreeView.ImageList")]
        public int SelectedImageIndex
        {
            get
            {
                return SelectedImageIndexer.Index;
            }
            set
            {
                SelectedImageIndexer.Index = value;
                UpdateNode(32);
            }
        }

        //
        // 요약:
        //     트리 노드가 선택된 상태일 때 표시되는 이미지의 키를 가져오거나 설정합니다.
        //
        // 반환 값:
        //     트리 노드가 선택된 상태일 때 표시되는 이미지의 키입니다.
        [Localizable(true)]
        [SRCategory("CatBehavior")]
        [SRDescription("TreeNodeSelectedImageKeyDescr")]
        [TypeConverter(typeof(TreeViewImageKeyConverter))]
        [DefaultValue("")]
        [RefreshProperties(RefreshProperties.Repaint)]
        [Editor("System.Windows.Forms.Design.ImageIndexEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor))]
        [RelatedImageList("TreeView.ImageList")]
        public string SelectedImageKey
        {
            get
            {
                return SelectedImageIndexer.Key;
            }
            set
            {
                SelectedImageIndexer.Key = value;
                UpdateNode(32);
            }
        }

        internal int State
        {
            get
            {
                if (handle == IntPtr.Zero)
                {
                    return 0;
                }

                TreeView treeView = TreeView;
                if (treeView == null || treeView.IsDisposed)
                {
                    return 0;
                }

                NativeMethods.TV_ITEM lParam = default(NativeMethods.TV_ITEM);
                lParam.hItem = Handle;
                lParam.mask = 24;
                lParam.stateMask = 34;
                UnsafeNativeMethods.SendMessage(new HandleRef(treeView, treeView.Handle), NativeMethods.TVM_GETITEM, 0, ref lParam);
                return lParam.state;
            }
        }

        //
        // 요약:
        //     부모 System.Windows.Forms.TreeNode의 System.Windows.Forms.TreeView 속성이 false로 설정된
        //     경우 System.Windows.Forms.TreeView.CheckBoxes의 상태를 나타내는 데 사용되는 이미지의 키를 가져오거나 설정합니다.
        //
        // 반환 값:
        //     System.Windows.Forms.TreeNode의 상태를 나타내는 데 사용되는 이미지의 키입니다.
        [Localizable(true)]
        [SRCategory("CatBehavior")]
        [SRDescription("TreeNodeStateImageKeyDescr")]
        [TypeConverter(typeof(ImageKeyConverter))]
        [DefaultValue("")]
        [Editor("System.Windows.Forms.Design.ImageIndexEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor))]
        [RefreshProperties(RefreshProperties.Repaint)]
        [RelatedImageList("TreeView.StateImageList")]
        public string StateImageKey
        {
            get
            {
                return StateImageIndexer.Key;
            }
            set
            {
                if (StateImageIndexer.Key != value)
                {
                    StateImageIndexer.Key = value;
                    if (treeView != null && !treeView.CheckBoxes)
                    {
                        UpdateNode(8);
                    }
                }
            }
        }

        //
        // 요약:
        //     부모 System.Windows.Forms.TreeNode의 System.Windows.Forms.TreeView 속성이 false로 설정된
        //     경우 System.Windows.Forms.TreeView.CheckBoxes의 상태를 나타내는 데 사용되는 이미지의 인덱스를 가져오거나
        //     설정합니다.
        //
        // 반환 값:
        //     System.Windows.Forms.TreeNode의 상태를 나타내는 데 사용되는 이미지의 인덱스입니다.
        //
        // 예외:
        //   T:System.ArgumentOutOfRangeException:
        //     지정된 인덱스가 -1보다 작거나 14보다 큰 경우
        [Localizable(true)]
        [TypeConverter(typeof(NoneExcludedImageIndexConverter))]
        [DefaultValue(-1)]
        [SRCategory("CatBehavior")]
        [SRDescription("TreeNodeStateImageIndexDescr")]
        [Editor("System.Windows.Forms.Design.ImageIndexEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor))]
        [RefreshProperties(RefreshProperties.Repaint)]
        [RelatedImageList("TreeView.StateImageList")]
        public int StateImageIndex
        {
            get
            {
                if (treeView != null && treeView.StateImageList != null)
                {
                    return StateImageIndexer.Index;
                }

                return -1;
            }
            set
            {
                if (value < -1 || value > 14)
                {
                    throw new ArgumentOutOfRangeException("StateImageIndex", SR.GetString("InvalidArgument", "StateImageIndex", value.ToString(CultureInfo.CurrentCulture)));
                }

                StateImageIndexer.Index = value;
                if (treeView != null && !treeView.CheckBoxes)
                {
                    UpdateNode(8);
                }
            }
        }

        //
        // 요약:
        //     트리 노드에 대한 데이터가 들어 있는 개체를 가져오거나 설정합니다.
        //
        // 반환 값:
        //     트리 노드에 대한 데이터가 들어 있는 System.Object입니다. 기본값은 null입니다.
        [SRCategory("CatData")]
        [Localizable(false)]
        [Bindable(true)]
        [SRDescription("ControlTagDescr")]
        [DefaultValue(null)]
        [TypeConverter(typeof(StringConverter))]
        public object Tag
        {
            get
            {
                return userData;
            }
            set
            {
                userData = value;
            }
        }

        //
        // 요약:
        //     트리 노드의 레이블에 표시할 텍스트를 가져오거나 설정합니다.
        //
        // 반환 값:
        //     트리 노드 레이블에 표시되는 텍스트입니다.
        [Localizable(true)]
        [SRCategory("CatAppearance")]
        [SRDescription("TreeNodeTextDescr")]
        public string Text
        {
            get
            {
                if (text != null)
                {
                    return text;
                }

                return "";
            }
            set
            {
                text = value;
                UpdateNode(1);
            }
        }

        //
        // 요약:
        //     마우스 포인터로 System.Windows.Forms.TreeNode를 가리키면 나타나는 텍스트를 가져오거나 설정합니다.
        //
        // 반환 값:
        //     마우스 포인터로 System.Windows.Forms.TreeNode를 가리키면 나타나는 텍스트를 가져옵니다.
        [Localizable(false)]
        [SRCategory("CatAppearance")]
        [SRDescription("TreeNodeToolTipTextDescr")]
        [DefaultValue("")]
        public string ToolTipText
        {
            get
            {
                return toolTipText;
            }
            set
            {
                toolTipText = value;
            }
        }

        //
        // 요약:
        //     트리 노드의 이름을 가져오거나 설정합니다.
        //
        // 반환 값:
        //     트리 노드의 이름을 나타내는 System.String입니다.
        [SRCategory("CatAppearance")]
        [SRDescription("TreeNodeNodeNameDescr")]
        public string Name
        {
            get
            {
                if (name != null)
                {
                    return name;
                }

                return "";
            }
            set
            {
                name = value;
            }
        }

        //
        // 요약:
        //     트리 노드가 할당된 부모 트리 뷰를 가져옵니다.
        //
        // 반환 값:
        //     트리 노드가 할당된 부모 트리 뷰를 나타내는 System.Windows.Forms.TreeView이고, 노드가 트리 뷰에 할당되지 않았으면
        //     null입니다.
        [Browsable(false)]
        public TreeView TreeView
        {
            get
            {
                if (treeView == null)
                {
                    treeView = FindTreeView();
                }

                return treeView;
            }
        }

        //
        // 요약:
        //     System.Windows.Forms.TreeNode 클래스의 새 인스턴스를 초기화합니다.
        public TreeNode()
        {
            treeNodeState = default(BitVector32);
        }

        internal TreeNode(TreeView treeView)
            : this()
        {
            this.treeView = treeView;
        }

        //
        // 요약:
        //     지정된 레이블 텍스트를 사용하여 System.Windows.Forms.TreeNode 클래스의 새 인스턴스를 초기화합니다.
        //
        // 매개 변수:
        //   text:
        //     새 트리 노드의 레이블 System.Windows.Forms.TreeNode.Text입니다.
        public TreeNode(string text)
            : this()
        {
            this.text = text;
        }

        //
        // 요약:
        //     지정된 레이블 텍스트와 자식 트리 노드를 사용하여 System.Windows.Forms.TreeNode 클래스의 새 인스턴스를 초기화합니다.
        //
        // 매개 변수:
        //   text:
        //     새 트리 노드의 레이블 System.Windows.Forms.TreeNode.Text입니다.
        //
        //   children:
        //     자식 System.Windows.Forms.TreeNode 개체로 이루어진 배열입니다.
        public TreeNode(string text, TreeNode[] children)
            : this()
        {
            this.text = text;
            Nodes.AddRange(children);
        }

        //
        // 요약:
        //     트리 노드가 선택 상태 또는 선택하지 않은 상태에 있을 때 표시할 지정된 레이블 텍스트와 이미지를 사용하여 System.Windows.Forms.TreeNode
        //     클래스의 새 인스턴스를 초기화합니다.
        //
        // 매개 변수:
        //   text:
        //     새 트리 노드의 레이블 System.Windows.Forms.TreeNode.Text입니다.
        //
        //   imageIndex:
        //     트리 노드를 선택하지 않은 상태일 때 표시할 System.Drawing.Image의 인덱스 값입니다.
        //
        //   selectedImageIndex:
        //     트리 노드를 선택한 상태일 때 표시할 System.Drawing.Image의 인덱스 값입니다.
        public TreeNode(string text, int imageIndex, int selectedImageIndex)
            : this()
        {
            this.text = text;
            ImageIndexer.Index = imageIndex;
            SelectedImageIndexer.Index = selectedImageIndex;
        }

        //
        // 요약:
        //     트리 노드가 선택 상태 또는 선택하지 않은 상태에 있을 때 표시할 지정된 레이블 텍스트, 자식 트리 노드 및 이미지를 사용하여 System.Windows.Forms.TreeNode
        //     클래스의 새 인스턴스를 초기화합니다.
        //
        // 매개 변수:
        //   text:
        //     새 트리 노드의 레이블 System.Windows.Forms.TreeNode.Text입니다.
        //
        //   imageIndex:
        //     트리 노드를 선택하지 않은 상태일 때 표시할 System.Drawing.Image의 인덱스 값입니다.
        //
        //   selectedImageIndex:
        //     트리 노드를 선택한 상태일 때 표시할 System.Drawing.Image의 인덱스 값입니다.
        //
        //   children:
        //     자식 System.Windows.Forms.TreeNode 개체로 이루어진 배열입니다.
        public TreeNode(string text, int imageIndex, int selectedImageIndex, TreeNode[] children)
            : this()
        {
            this.text = text;
            ImageIndexer.Index = imageIndex;
            SelectedImageIndexer.Index = selectedImageIndex;
            Nodes.AddRange(children);
        }

        //
        // 요약:
        //     지정된 serialization 정보와 컨텍스트를 사용하여 System.Windows.Forms.TreeNode 클래스의 새 인스턴스를 초기화합니다.
        //
        // 매개 변수:
        //   serializationInfo:
        //     클래스를 deserialize할 데이터가 들어 있는 System.Runtime.Serialization.SerializationInfo입니다.
        //
        //   context:
        //     serialize된 스트림의 소스와 대상이 들어 있는 System.Runtime.Serialization.StreamingContext입니다.
        protected TreeNode(SerializationInfo serializationInfo, StreamingContext context)
            : this()
        {
            Deserialize(serializationInfo, context);
        }

        internal int AddSorted(TreeNode node)
        {
            int result = 0;
            string @string = node.Text;
            TreeView treeView = TreeView;
            if (childCount > 0)
            {
                if (treeView.TreeViewNodeSorter == null)
                {
                    CompareInfo compareInfo = Application.CurrentCulture.CompareInfo;
                    if (compareInfo.Compare(children[childCount - 1].Text, @string) <= 0)
                    {
                        result = childCount;
                    }
                    else
                    {
                        int num = 0;
                        int num2 = childCount;
                        while (num < num2)
                        {
                            int num3 = (num + num2) / 2;
                            if (compareInfo.Compare(children[num3].Text, @string) <= 0)
                            {
                                num = num3 + 1;
                            }
                            else
                            {
                                num2 = num3;
                            }
                        }

                        result = num;
                    }
                }
                else
                {
                    IComparer treeViewNodeSorter = treeView.TreeViewNodeSorter;
                    int num = 0;
                    int num2 = childCount;
                    while (num < num2)
                    {
                        int num3 = (num + num2) / 2;
                        if (treeViewNodeSorter.Compare(children[num3], node) <= 0)
                        {
                            num = num3 + 1;
                        }
                        else
                        {
                            num2 = num3;
                        }
                    }

                    result = num;
                }
            }

            node.SortChildren(treeView);
            InsertNodeAt(result, node);
            return result;
        }

        //
        // 요약:
        //     지정된 핸들을 사용하여 지정된 트리 뷰 컨트롤에 할당된 트리 노드를 반환합니다.
        //
        // 매개 변수:
        //   tree:
        //     트리 노드가 들어 있는 System.Windows.Forms.TreeView입니다.
        //
        //   handle:
        //     트리 노드의 핸들입니다.
        //
        // 반환 값:
        //     지정된 핸들을 사용하여 지정된 System.Windows.Forms.TreeNode 컨트롤에 할당된 트리 노드를 나타내는 System.Windows.Forms.TreeView입니다.
        public static TreeNode FromHandle(TreeView tree, IntPtr handle)
        {
            IntSecurity.ControlFromHandleOrLocation.Demand();
            return tree.NodeFromHandle(handle);
        }

        private void SortChildren(TreeView parentTreeView)
        {
            if (childCount <= 0)
            {
                return;
            }

            TreeNode[] array = new TreeNode[childCount];
            if (parentTreeView == null || parentTreeView.TreeViewNodeSorter == null)
            {
                CompareInfo compareInfo = Application.CurrentCulture.CompareInfo;
                for (int i = 0; i < childCount; i++)
                {
                    int num = -1;
                    for (int j = 0; j < childCount; j++)
                    {
                        if (children[j] != null)
                        {
                            if (num == -1)
                            {
                                num = j;
                            }
                            else if (compareInfo.Compare(children[j].Text, children[num].Text) <= 0)
                            {
                                num = j;
                            }
                        }
                    }

                    array[i] = children[num];
                    children[num] = null;
                    array[i].index = i;
                    array[i].SortChildren(parentTreeView);
                }

                children = array;
                return;
            }

            IComparer treeViewNodeSorter = parentTreeView.TreeViewNodeSorter;
            for (int k = 0; k < childCount; k++)
            {
                int num2 = -1;
                for (int l = 0; l < childCount; l++)
                {
                    if (children[l] != null)
                    {
                        if (num2 == -1)
                        {
                            num2 = l;
                        }
                        else if (treeViewNodeSorter.Compare(children[l], children[num2]) <= 0)
                        {
                            num2 = l;
                        }
                    }
                }

                array[k] = children[num2];
                children[num2] = null;
                array[k].index = k;
                array[k].SortChildren(parentTreeView);
            }

            children = array;
        }

        //
        // 요약:
        //     트리 노드 레이블의 편집을 초기화합니다.
        //
        // 예외:
        //   T:System.InvalidOperationException:
        //     System.Windows.Forms.TreeView.LabelEdit이 false로 설정됩니다.
        public void BeginEdit()
        {
            if (handle != IntPtr.Zero)
            {
                TreeView treeView = TreeView;
                if (!treeView.LabelEdit)
                {
                    throw new InvalidOperationException(SR.GetString("TreeNodeBeginEditFailed"));
                }

                if (!treeView.Focused)
                {
                    treeView.FocusInternal();
                }

                UnsafeNativeMethods.SendMessage(new HandleRef(treeView, treeView.Handle), NativeMethods.TVM_EDITLABEL, 0, handle);
            }
        }

        internal void Clear()
        {
            bool flag = false;
            TreeView treeView = TreeView;
            try
            {
                if (treeView != null)
                {
                    treeView.nodesCollectionClear = true;
                    if (treeView != null && childCount > 200)
                    {
                        flag = true;
                        treeView.BeginUpdate();
                    }
                }

                while (childCount > 0)
                {
                    children[childCount - 1].Remove(notify: true);
                }

                children = null;
                if (treeView != null && flag)
                {
                    treeView.EndUpdate();
                }
            }
            finally
            {
                if (treeView != null)
                {
                    treeView.nodesCollectionClear = false;
                }

                nodesCleared = true;
            }
        }

        //
        // 요약:
        //     트리 노드와 이 트리 노드에서 시작하는 전체 하위 트리를 복사합니다.
        //
        // 반환 값:
        //     복제된 System.Object를 나타내는 System.Windows.Forms.TreeNode입니다.
        public virtual object Clone()
        {
            Type type = GetType();
            TreeNode treeNode = null;
            treeNode = ((!(type == typeof(TreeNode))) ? ((TreeNode)Activator.CreateInstance(type)) : new TreeNode(text, ImageIndexer.Index, SelectedImageIndexer.Index));
            treeNode.Text = text;
            treeNode.Name = name;
            treeNode.ImageIndexer.Index = ImageIndexer.Index;
            treeNode.SelectedImageIndexer.Index = SelectedImageIndexer.Index;
            treeNode.StateImageIndexer.Index = StateImageIndexer.Index;
            treeNode.ToolTipText = toolTipText;
            treeNode.ContextMenu = contextMenu;
            treeNode.ContextMenuStrip = contextMenuStrip;
            if (!string.IsNullOrEmpty(ImageIndexer.Key))
            {
                treeNode.ImageIndexer.Key = ImageIndexer.Key;
            }

            if (!string.IsNullOrEmpty(SelectedImageIndexer.Key))
            {
                treeNode.SelectedImageIndexer.Key = SelectedImageIndexer.Key;
            }

            if (!string.IsNullOrEmpty(StateImageIndexer.Key))
            {
                treeNode.StateImageIndexer.Key = StateImageIndexer.Key;
            }

            if (childCount > 0)
            {
                treeNode.children = new TreeNode[childCount];
                for (int i = 0; i < childCount; i++)
                {
                    treeNode.Nodes.Add((TreeNode)children[i].Clone());
                }
            }

            if (propBag != null)
            {
                treeNode.propBag = OwnerDrawPropertyBag.Copy(propBag);
            }

            treeNode.Checked = Checked;
            treeNode.Tag = Tag;
            return treeNode;
        }

        private void CollapseInternal(bool ignoreChildren)
        {
            TreeView treeView = TreeView;
            bool flag = false;
            collapseOnRealization = false;
            expandOnRealization = false;
            if (treeView == null || !treeView.IsHandleCreated)
            {
                collapseOnRealization = true;
                return;
            }

            if (ignoreChildren)
            {
                DoCollapse(treeView);
            }
            else
            {
                if (!ignoreChildren && childCount > 0)
                {
                    for (int i = 0; i < childCount; i++)
                    {
                        if (treeView.SelectedNode == children[i])
                        {
                            flag = true;
                        }

                        children[i].DoCollapse(treeView);
                        children[i].Collapse();
                    }
                }

                DoCollapse(treeView);
            }

            if (flag)
            {
                treeView.SelectedNode = this;
            }

            treeView.Invalidate();
            collapseOnRealization = false;
        }

        //
        // 요약:
        //     System.Windows.Forms.TreeNode를 축소하고 선택적으로 해당 자식을 축소합니다.
        //
        // 매개 변수:
        //   ignoreChildren:
        //     자식 노드를 현재 상태로 두려면 true이고, 자식 노드를 축소하려면 false입니다.
        public void Collapse(bool ignoreChildren)
        {
            CollapseInternal(ignoreChildren);
        }

        //
        // 요약:
        //     트리 노드를 축소합니다.
        public void Collapse()
        {
            CollapseInternal(ignoreChildren: false);
        }

        private void DoCollapse(TreeView tv)
        {
            if (((uint)State & 0x20u) != 0)
            {
                TreeViewCancelEventArgs treeViewCancelEventArgs = new TreeViewCancelEventArgs(this, cancel: false, TreeViewAction.Collapse);
                tv.OnBeforeCollapse(treeViewCancelEventArgs);
                if (!treeViewCancelEventArgs.Cancel)
                {
                    UnsafeNativeMethods.SendMessage(new HandleRef(tv, tv.Handle), 4354, 1, Handle);
                    tv.OnAfterCollapse(new TreeViewEventArgs(this));
                }
            }
        }

        //
        // 요약:
        //     지정된 System.Windows.Forms.TreeNode에서 System.Runtime.Serialization.SerializationInfo의
        //     상태를 로드합니다.
        //
        // 매개 변수:
        //   serializationInfo:
        //     System.Runtime.Serialization.SerializationInfo를 설명하는 System.Windows.Forms.TreeNode입니다.
        //
        //   context:
        //     deserialize하는 동안 스트림의 상태를 나타내는 System.Runtime.Serialization.StreamingContext입니다.
        protected virtual void Deserialize(SerializationInfo serializationInfo, StreamingContext context)
        {
            int num = 0;
            int num2 = -1;
            string text = null;
            int num3 = -1;
            string text2 = null;
            int num4 = -1;
            string text3 = null;
            SerializationInfoEnumerator enumerator = serializationInfo.GetEnumerator();
            while (enumerator.MoveNext())
            {
                SerializationEntry current = enumerator.Current;
                switch (current.Name)
                {
                    case "PropBag":
                        propBag = (OwnerDrawPropertyBag)serializationInfo.GetValue(current.Name, typeof(OwnerDrawPropertyBag));
                        break;
                    case "Text":
                        Text = serializationInfo.GetString(current.Name);
                        break;
                    case "ToolTipText":
                        ToolTipText = serializationInfo.GetString(current.Name);
                        break;
                    case "Name":
                        Name = serializationInfo.GetString(current.Name);
                        break;
                    case "IsChecked":
                        CheckedStateInternal = serializationInfo.GetBoolean(current.Name);
                        break;
                    case "ImageIndex":
                        num2 = serializationInfo.GetInt32(current.Name);
                        break;
                    case "SelectedImageIndex":
                        num3 = serializationInfo.GetInt32(current.Name);
                        break;
                    case "ImageKey":
                        text = serializationInfo.GetString(current.Name);
                        break;
                    case "SelectedImageKey":
                        text2 = serializationInfo.GetString(current.Name);
                        break;
                    case "StateImageKey":
                        text3 = serializationInfo.GetString(current.Name);
                        break;
                    case "StateImageIndex":
                        num4 = serializationInfo.GetInt32(current.Name);
                        break;
                    case "ChildCount":
                        num = serializationInfo.GetInt32(current.Name);
                        break;
                    case "UserData":
                        userData = current.Value;
                        break;
                }
            }

            if (text != null)
            {
                ImageKey = text;
            }
            else if (num2 != -1)
            {
                ImageIndex = num2;
            }

            if (text2 != null)
            {
                SelectedImageKey = text2;
            }
            else if (num3 != -1)
            {
                SelectedImageIndex = num3;
            }

            if (text3 != null)
            {
                StateImageKey = text3;
            }
            else if (num4 != -1)
            {
                StateImageIndex = num4;
            }

            if (num > 0)
            {
                TreeNode[] array = new TreeNode[num];
                for (int i = 0; i < num; i++)
                {
                    array[i] = (TreeNode)serializationInfo.GetValue("children" + i, typeof(TreeNode));
                }

                Nodes.AddRange(array);
            }
        }

        //
        // 요약:
        //     트리 노드 레이블의 편집을 마칩니다.
        //
        // 매개 변수:
        //   cancel:
        //     트리 노드 레이블 텍스트를 편집한 내용이 저장되지 않고 취소되었으면 true이고, 그렇지 않으면 false입니다.
        public void EndEdit(bool cancel)
        {
            TreeView treeView = TreeView;
            if (treeView != null && !treeView.IsDisposed)
            {
                UnsafeNativeMethods.SendMessage(new HandleRef(treeView, treeView.Handle), 4374, cancel ? 1 : 0, 0);
            }
        }

        internal void EnsureCapacity(int num)
        {
            int num2 = num;
            if (num2 < 4)
            {
                num2 = 4;
            }

            if (children == null)
            {
                children = new TreeNode[num2];
            }
            else if (childCount + num > children.Length)
            {
                int num3 = childCount + num;
                if (num == 1)
                {
                    num3 = childCount * 2;
                }

                TreeNode[] destinationArray = new TreeNode[num3];
                Array.Copy(children, 0, destinationArray, 0, childCount);
                children = destinationArray;
            }
        }

        private void EnsureStateImageValue()
        {
            if (treeView != null && treeView.CheckBoxes && treeView.StateImageList != null)
            {
                if (!string.IsNullOrEmpty(StateImageKey))
                {
                    StateImageIndex = (Checked ? 1 : 0);
                    StateImageKey = treeView.StateImageList.Images.Keys[StateImageIndex];
                }
                else
                {
                    StateImageIndex = (Checked ? 1 : 0);
                }
            }
        }

        //
        // 요약:
        //     트리 노드가 표시되고 확장되며 트리 뷰 컨트롤이 충분히 스크롤되는지 확인합니다.
        public void EnsureVisible()
        {
            TreeView treeView = TreeView;
            if (treeView != null && !treeView.IsDisposed)
            {
                UnsafeNativeMethods.SendMessage(new HandleRef(treeView, treeView.Handle), 4372, 0, Handle);
            }
        }

        //
        // 요약:
        //     트리 노드를 확장합니다.
        public void Expand()
        {
            TreeView treeView = TreeView;
            if (treeView == null || !treeView.IsHandleCreated)
            {
                expandOnRealization = true;
                return;
            }

            ResetExpandedState(treeView);
            if (!IsExpanded)
            {
                UnsafeNativeMethods.SendMessage(new HandleRef(treeView, treeView.Handle), 4354, 2, Handle);
            }

            expandOnRealization = false;
        }

        //
        // 요약:
        //     자식 트리 노드를 모두 확장합니다.
        public void ExpandAll()
        {
            Expand();
            for (int i = 0; i < childCount; i++)
            {
                children[i].ExpandAll();
            }
        }

        internal TreeView FindTreeView()
        {
            TreeNode treeNode = this;
            while (treeNode.parent != null)
            {
                treeNode = treeNode.parent;
            }

            return treeNode.treeView;
        }

        private void GetFullPath(StringBuilder path, string pathSeparator)
        {
            if (parent != null)
            {
                parent.GetFullPath(path, pathSeparator);
                if (parent.parent != null)
                {
                    path.Append(pathSeparator);
                }

                path.Append(text);
            }
        }

        //
        // 요약:
        //     자식 트리 노드의 수를 반환합니다.
        //
        // 매개 변수:
        //   includeSubTrees:
        //     결과 수에 이 트리 노드에서 시작하는 하위 트리 노드까지 모두 포함되어 있으면 true이고, 그렇지 않으면 false입니다.
        //
        // 반환 값:
        //     System.Windows.Forms.TreeNode.Nodes 컬렉션에 할당된 자식 트리 노드의 수입니다.
        public int GetNodeCount(bool includeSubTrees)
        {
            int num = childCount;
            if (includeSubTrees)
            {
                for (int i = 0; i < childCount; i++)
                {
                    num += children[i].GetNodeCount(includeSubTrees: true);
                }
            }

            return num;
        }

        internal void InsertNodeAt(int index, TreeNode node)
        {
            EnsureCapacity(1);
            node.parent = this;
            node.index = index;
            for (int num = childCount; num > index; num--)
            {
                (children[num] = children[num - 1]).index = num;
            }

            children[index] = node;
            childCount++;
            node.Realize(insertFirst: false);
            if (TreeView != null && node == TreeView.selectedNode)
            {
                TreeView.SelectedNode = node;
            }
        }

        private void InvalidateHostTree()
        {
            if (treeView != null && treeView.IsHandleCreated)
            {
                treeView.Invalidate();
            }
        }

        internal void Realize(bool insertFirst)
        {
            TreeView treeView = TreeView;
            if (treeView == null || !treeView.IsHandleCreated || treeView.IsDisposed)
            {
                return;
            }

            if (parent != null)
            {
                if (treeView.InvokeRequired)
                {
                    throw new InvalidOperationException(SR.GetString("InvalidCrossThreadControlCall"));
                }

                NativeMethods.TV_INSERTSTRUCT lParam = default(NativeMethods.TV_INSERTSTRUCT);
                lParam.item_mask = insertMask;
                lParam.hParent = parent.handle;
                TreeNode prevNode = PrevNode;
                if (insertFirst || prevNode == null)
                {
                    lParam.hInsertAfter = (IntPtr)(-65535);
                }
                else
                {
                    lParam.hInsertAfter = prevNode.handle;
                }

                lParam.item_pszText = Marshal.StringToHGlobalAuto(text);
                lParam.item_iImage = ((ImageIndexer.ActualIndex == -1) ? treeView.ImageIndexer.ActualIndex : ImageIndexer.ActualIndex);
                lParam.item_iSelectedImage = ((SelectedImageIndexer.ActualIndex == -1) ? treeView.SelectedImageIndexer.ActualIndex : SelectedImageIndexer.ActualIndex);
                lParam.item_mask = 1;
                lParam.item_stateMask = 0;
                lParam.item_state = 0;
                if (treeView.CheckBoxes)
                {
                    lParam.item_mask |= 8;
                    lParam.item_stateMask |= 61440;
                    lParam.item_state |= (CheckedInternal ? 8192 : 4096);
                }
                else if (treeView.StateImageList != null && StateImageIndexer.ActualIndex >= 0)
                {
                    lParam.item_mask |= 8;
                    lParam.item_stateMask = 61440;
                    lParam.item_state = StateImageIndexer.ActualIndex + 1 << 12;
                }

                if (lParam.item_iImage >= 0)
                {
                    lParam.item_mask |= 2;
                }

                if (lParam.item_iSelectedImage >= 0)
                {
                    lParam.item_mask |= 32;
                }

                bool flag = false;
                IntPtr intPtr = UnsafeNativeMethods.SendMessage(new HandleRef(treeView, treeView.Handle), 4367, 0, 0);
                if (intPtr != IntPtr.Zero)
                {
                    flag = true;
                    UnsafeNativeMethods.SendMessage(new HandleRef(treeView, treeView.Handle), 4374, 0, 0);
                }

                handle = UnsafeNativeMethods.SendMessage(new HandleRef(treeView, treeView.Handle), NativeMethods.TVM_INSERTITEM, 0, ref lParam);
                treeView.nodeTable[handle] = this;
                UpdateNode(4);
                Marshal.FreeHGlobal(lParam.item_pszText);
                if (flag)
                {
                    UnsafeNativeMethods.PostMessage(new HandleRef(treeView, treeView.Handle), NativeMethods.TVM_EDITLABEL, IntPtr.Zero, handle);
                }

                SafeNativeMethods.InvalidateRect(new HandleRef(treeView, treeView.Handle), null, erase: false);
                if (parent.nodesCleared && (insertFirst || prevNode == null) && !treeView.Scrollable)
                {
                    UnsafeNativeMethods.SendMessage(new HandleRef(treeView, treeView.Handle), 11, 1, 0);
                    nodesCleared = false;
                }
            }

            for (int num = childCount - 1; num >= 0; num--)
            {
                children[num].Realize(insertFirst: true);
            }

            if (expandOnRealization)
            {
                Expand();
            }

            if (collapseOnRealization)
            {
                Collapse();
            }
        }

        //
        // 요약:
        //     트리 뷰 컨트롤에서 현재 트리 노드를 제거합니다.
        public void Remove()
        {
            Remove(notify: true);
        }

        internal void Remove(bool notify)
        {
            bool isExpanded = IsExpanded;
            for (int i = 0; i < childCount; i++)
            {
                children[i].Remove(notify: false);
            }

            if (notify && parent != null)
            {
                for (int j = index; j < parent.childCount - 1; j++)
                {
                    (parent.children[j] = parent.children[j + 1]).index = j;
                }

                parent.children[parent.childCount - 1] = null;
                parent.childCount--;
                parent = null;
            }

            expandOnRealization = isExpanded;
            TreeView treeView = TreeView;
            if (treeView == null || treeView.IsDisposed)
            {
                return;
            }

            if (handle != IntPtr.Zero)
            {
                if (notify && treeView.IsHandleCreated)
                {
                    UnsafeNativeMethods.SendMessage(new HandleRef(treeView, treeView.Handle), 4353, 0, handle);
                }

                this.treeView.nodeTable.Remove(handle);
                handle = IntPtr.Zero;
            }

            this.treeView = null;
        }

        private void RemovePropBagIfEmpty()
        {
            if (propBag != null && propBag.IsEmpty())
            {
                propBag = null;
            }
        }

        private void ResetExpandedState(TreeView tv)
        {
            NativeMethods.TV_ITEM lParam = default(NativeMethods.TV_ITEM);
            lParam.mask = 24;
            lParam.hItem = handle;
            lParam.stateMask = 64;
            lParam.state = 0;
            UnsafeNativeMethods.SendMessage(new HandleRef(tv, tv.Handle), NativeMethods.TVM_SETITEM, 0, ref lParam);
        }

        private bool ShouldSerializeBackColor()
        {
            return BackColor != Color.Empty;
        }

        private bool ShouldSerializeForeColor()
        {
            return ForeColor != Color.Empty;
        }

        //
        // 요약:
        //     System.Windows.Forms.TreeNode의 상태를 지정된 System.Runtime.Serialization.SerializationInfo에
        //     저장합니다.
        //
        // 매개 변수:
        //   si:
        //     System.Runtime.Serialization.SerializationInfo를 설명하는 System.Windows.Forms.TreeNode입니다.
        //
        //   context:
        //     serialize하는 동안 스트림의 상태를 나타내는 System.Runtime.Serialization.StreamingContext입니다.
        [SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.SerializationFormatter)]
        [SecurityPermission(SecurityAction.InheritanceDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
        protected virtual void Serialize(SerializationInfo si, StreamingContext context)
        {
            if (propBag != null)
            {
                si.AddValue("PropBag", propBag, typeof(OwnerDrawPropertyBag));
            }

            si.AddValue("Text", text);
            si.AddValue("ToolTipText", toolTipText);
            si.AddValue("Name", Name);
            si.AddValue("IsChecked", treeNodeState[1]);
            si.AddValue("ImageIndex", ImageIndexer.Index);
            si.AddValue("ImageKey", ImageIndexer.Key);
            si.AddValue("SelectedImageIndex", SelectedImageIndexer.Index);
            si.AddValue("SelectedImageKey", SelectedImageIndexer.Key);
            if (treeView != null && treeView.StateImageList != null)
            {
                si.AddValue("StateImageIndex", StateImageIndexer.Index);
            }

            if (treeView != null && treeView.StateImageList != null)
            {
                si.AddValue("StateImageKey", StateImageIndexer.Key);
            }

            si.AddValue("ChildCount", childCount);
            if (childCount > 0)
            {
                for (int i = 0; i < childCount; i++)
                {
                    si.AddValue("children" + i, children[i], typeof(TreeNode));
                }
            }

            if (userData != null && userData.GetType().IsSerializable)
            {
                si.AddValue("UserData", userData, userData.GetType());
            }
        }

        //
        // 요약:
        //     트리 노드를 확장된 상태 또는 축소된 상태로 설정/해제합니다.
        public void Toggle()
        {
            if (IsExpanded)
            {
                Collapse();
            }
            else
            {
                Expand();
            }
        }

        //
        // 요약:
        //     현재 개체를 나타내는 문자열을 반환합니다.
        //
        // 반환 값:
        //     현재 개체를 나타내는 문자열입니다.
        public override string ToString()
        {
            return "TreeNode: " + ((text == null) ? "" : text);
        }

        private void UpdateNode(int mask)
        {
            if (handle == IntPtr.Zero)
            {
                return;
            }

            TreeView treeView = TreeView;
            NativeMethods.TV_ITEM lParam = default(NativeMethods.TV_ITEM);
            lParam.mask = 0x10 | mask;
            lParam.hItem = handle;
            if (((uint)mask & (true ? 1u : 0u)) != 0)
            {
                lParam.pszText = Marshal.StringToHGlobalAuto(text);
            }

            if (((uint)mask & 2u) != 0)
            {
                lParam.iImage = ((ImageIndexer.ActualIndex == -1) ? treeView.ImageIndexer.ActualIndex : ImageIndexer.ActualIndex);
            }

            if (((uint)mask & 0x20u) != 0)
            {
                lParam.iSelectedImage = ((SelectedImageIndexer.ActualIndex == -1) ? treeView.SelectedImageIndexer.ActualIndex : SelectedImageIndexer.ActualIndex);
            }

            if (((uint)mask & 8u) != 0)
            {
                lParam.stateMask = 61440;
                if (StateImageIndexer.ActualIndex != -1)
                {
                    lParam.state = StateImageIndexer.ActualIndex + 1 << 12;
                }
            }

            if (((uint)mask & 4u) != 0)
            {
                lParam.lParam = handle;
            }

            UnsafeNativeMethods.SendMessage(new HandleRef(treeView, treeView.Handle), NativeMethods.TVM_SETITEM, 0, ref lParam);
            if (((uint)mask & (true ? 1u : 0u)) != 0)
            {
                Marshal.FreeHGlobal(lParam.pszText);
                if (treeView.Scrollable)
                {
                    treeView.ForceScrollbarUpdate(delayed: false);
                }
            }
        }

        internal void UpdateImage()
        {
            TreeView treeView = TreeView;
            if (!treeView.IsDisposed)
            {
                NativeMethods.TV_ITEM lParam = default(NativeMethods.TV_ITEM);
                lParam.mask = 18;
                lParam.hItem = Handle;
                lParam.iImage = Math.Max(0, (ImageIndexer.ActualIndex >= treeView.ImageList.Images.Count) ? (treeView.ImageList.Images.Count - 1) : ImageIndexer.ActualIndex);
                UnsafeNativeMethods.SendMessage(new HandleRef(treeView, treeView.Handle), NativeMethods.TVM_SETITEM, 0, ref lParam);
            }
        }

        //
        // 요약:
        //     System.Windows.Forms.TreeNode을 serialize하는 데 필요한 데이터로 serialization 정보 개체를 채웁니다.
        //
        // 매개 변수:
        //   si:
        //     System.Runtime.Serialization.SerializationInfo를 serialize할 데이터가 들어 있는 System.Windows.Forms.TreeNode입니다.
        //
        //   context:
        //     이 serialization의 대상 정보가 들어 있는 System.Runtime.Serialization.StreamingContext입니다.
        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
        void ISerializable.GetObjectData(SerializationInfo si, StreamingContext context)
        {
            Serialize(si, context);
        }
    }
}
#if false // 디컴파일 로그
캐시의 '41'개 항목
------------------
확인: 'mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'
단일 어셈블리를 찾았습니다. 'mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'
로드 위치: 'C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.8\mscorlib.dll'
------------------
확인: 'System.Drawing, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
단일 어셈블리를 찾았습니다. 'System.Drawing, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
로드 위치: 'C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.8\System.Drawing.dll'
------------------
확인: 'System.Security, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
'System.Security, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a' 이름으로 찾을 수 없습니다.
------------------
확인: 'System.Xml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'
단일 어셈블리를 찾았습니다. 'System.Xml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'
로드 위치: 'C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.8\System.Xml.dll'
------------------
확인: 'System, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'
단일 어셈블리를 찾았습니다. 'System, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'
로드 위치: 'C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.8\System.dll'
------------------
확인: 'System.Core, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'
단일 어셈블리를 찾았습니다. 'System.Core, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'
로드 위치: 'C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.8\System.Core.dll'
------------------
확인: 'System.Configuration, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
'System.Configuration, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a' 이름으로 찾을 수 없습니다.
------------------
확인: 'Accessibility, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
'Accessibility, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a' 이름으로 찾을 수 없습니다.
------------------
확인: 'System.Deployment, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
단일 어셈블리를 찾았습니다. 'System.Deployment, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
로드 위치: 'C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.8\System.Deployment.dll'
------------------
확인: 'System.Runtime.Serialization.Formatters.Soap, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
'System.Runtime.Serialization.Formatters.Soap, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a' 이름으로 찾을 수 없습니다.
#endif
