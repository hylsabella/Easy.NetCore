using System.Collections.Generic;

namespace Easy.Common.NetCore.Model
{
    public class UserTreeNode
    {
        public int UserId { get; set; }

        public string UserName { get; set; }

        public int ParentId { get; set; }

        public string ParentName { get; set; }

        public UserTreeNode ParentNode { get; set; }

        public IList<UserTreeNode> ChildrenNode { get; set; } = new List<UserTreeNode>();
    }
}