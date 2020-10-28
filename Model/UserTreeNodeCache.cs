namespace Easy.Common.NetCore.Model
{
    /// <summary>
    /// 该对象只用于缓存（而UserTreeNode对象可以呈现上下级关系）
    /// </summary>
    public class UserTreeNodeCache
    {
        public int UserId { get; set; }

        public string UserName { get; set; }

        public int ParentId { get; set; }

        public string ParentName { get; set; }
    }
}