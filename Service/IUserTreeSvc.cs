using Easy.Common.NetCore.Model;
using System;
using System.Collections.Generic;

namespace Easy.Common.NetCore.Service
{
    public interface IUserTreeSvc
    {
        Func<int, UserTreeNodeCache> GetUserFunc { get; set; }

        /// <summary>
        /// 添加用户树缓存
        /// </summary>
        bool SetUserTreeNode(UserTreeNodeCache cacheModel);

        /// <summary>
        /// 把指定的孩子UserId添加到指定父亲的孩子集合缓存里
        /// </summary>
        /// <param name="parentId">父亲用户Id</param>
        /// <param name="childUserId">孩子用户Id</param>
        void AddToPartOfParentCache(int parentId, int childUserId);

        /// <summary>
        /// 把指定的孩子UserId从指定父亲的孩子集合缓存里移除
        /// </summary>
        /// <param name="parentId">父亲用户Id</param>
        /// <param name="childUserId">孩子用户Id</param>
        void RemoveMemberFromPartOfParentCache(int parentId, int childUserId);

        /// <summary>
        /// 移除指定UserId的孩子集合缓存
        /// </summary>
        void RemovePartOfParentCache(int userId);

        /// <summary>
        /// 检查指定孩子用户Id是否存在于他的父节点缓存集合中
        /// </summary>
        /// <param name="parentId">父亲用户Id</param>
        /// <param name="childUserId">孩子用户Id</param>
        bool IsInPartOfParentCache(int parentId, int childUserId);

        /// <summary>
        /// 获取用户树缓存 
        /// </summary>
        UserTreeNodeCache GetUserTreeNode(int userId);

        /// <summary>
        /// 移除用户树节点缓存
        /// </summary>
        void RemoveUserTreeNode(int userId);

        /// <summary>
        /// 传入的2个用户Id，判断是否是上下级关系（所有下级）
        /// </summary>
        /// <param name="childUserId">下级代理用户Id</param>
        /// <param name="parentId">上级代理用户Id</param>
        /// <returns>是否属于上下级关系</returns>
        bool IsChildren(int childUserId, int parentId);

        /// <summary>
        /// 从指定的用户Id【从下往上】构建用户【树形结构】
        /// </summary>
        /// <returns>参数【bottomUserId】对应的节点</returns>
        UserTreeNode BuildUserTree_BottomToTop(int bottomUserId, Action<UserTreeNode> action = null);

        /// <summary>
        /// 从指定的用户Id【从上往下】构建【用户树形结构】
        /// </summary>
        /// <returns>参数【topUserId】对应的节点</returns>
        UserTreeNode BuildUserTree_TopToBottom(int topUserId);

        /// <summary>
        /// 查询出指定用户Id下的【所有】下级用户列表（默认包括自己）
        /// </summary>
        List<UserTreeNodeCache> GetChildrenNodeList(int userId, bool isContainSelf = true);

        /// <summary>
        /// 查询指定用户Id下的【所有】用户Id（默认包括自己 ,格式： 1,2,3,4）
        /// </summary>
        string GetChildrenIdsStr(int userId, bool isContainSelf = true);

        /// <summary>
        /// 查找指定用户的【直接】孩子UserId集合（不包括自己）
        /// </summary>
        List<int> FindDirectChildUserId(int parentId);

        /// <summary>
        /// 查找指定用户的【直接】孩子个数（不包括自己）
        /// </summary>
        long FindDirectChildCount(int parentId);

        /// <summary>
        /// 查找指定用户的【直接】孩子信息集合（不包括自己）
        /// </summary>
        List<UserTreeNodeCache> FindDirectChildren(int parentId);

        /// <summary>
        /// 根据指定用户Id获取其树中最顶层的用户信息
        /// </summary>
        /// <param name="userId">指定用户Id</param>
        /// <returns>树中最顶层的用户信息</returns>
        UserTreeNode GetTopUserTreeNode(int userId);
    }
}