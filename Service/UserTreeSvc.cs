using Easy.Common.NetCore.Cache;
using Easy.Common.NetCore.Exceptions;
using Easy.Common.NetCore.Model;
using Easy.Common.NetCore.Setting;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Easy.Common.NetCore.Service
{
    public class UserTreeSvc : IUserTreeSvc
    {
        private readonly IEasyCache _easyCache;
        private readonly static Logger logger = LogManager.GetCurrentClassLogger();
        private readonly static int redisdbIndex = 1;//用户树缓存数据保存在redis的db1中
        private static readonly TimeSpan _expiresTimeSpan = TimeSpan.FromDays(1);

        public Func<int, UserTreeNodeCache> GetUserFunc { get; set; }

        public UserTreeSvc(IEasyCache easyCache)
        {
            this._easyCache = easyCache;
        }

        #region 【添加】和【获取】用户树缓存 

        /// <summary>
        /// 添加【用户树节点】缓存 
        /// </summary>
        public bool SetUserTreeNode(UserTreeNodeCache cacheModel)
        {
            if (GetUserFunc == null) throw new FException("您未指定查询用户信息的方法");
            if (cacheModel.UserId <= 0) throw new FException("您必须要传入【用户Id】");
            if (cacheModel.ParentId < 0) throw new FException("您的推荐人【用户Id】不能为空");
            if (string.IsNullOrWhiteSpace(cacheModel.UserName)) throw new FException("您必须要传入【用户名】");
            if (cacheModel.ParentId > 0 && string.IsNullOrWhiteSpace(cacheModel.ParentName))
            {
                throw new FException("您必须要传入推荐人【用户名】");
            }

            //把该用户加到他父亲下面的孩子集合
            this.AddToPartOfParentCache(cacheModel.ParentId, cacheModel.UserId);

            //把自己的信息添加到【用户树节点缓存】（如果修改了用户信息，那么需要重置该缓存，以保证数据正确）
            string selfKey = CommonCacheKeys.Build_UserTree_Key(cacheModel.UserId);

            return this._easyCache.Set(selfKey, cacheModel, _expiresTimeSpan, db: redisdbIndex);
        }

        /// <summary>
        /// 获取【用户树节点】缓存 
        /// </summary>
        public UserTreeNodeCache GetUserTreeNode(int userId)
        {
            if (GetUserFunc == null) throw new FException("您未指定查询用户信息的方法");
            if (userId <= 0) return null;

            string key = CommonCacheKeys.Build_UserTree_Key(userId);

            return this._easyCache.Get(key, () =>
            {
                //读不到缓存从数据库读
                UserTreeNodeCache cacheModel = this.GetUserFormDB(userId);
                return cacheModel;

            }, _expiresTimeSpan, db: redisdbIndex);
        }

        /// <summary>
        /// 移除用户树节点缓存
        /// </summary>
        public void RemoveUserTreeNode(int userId)
        {
            if (GetUserFunc == null) throw new FException("您未指定查询用户信息的方法");
            if (userId <= 0) return;

            string selfKey = CommonCacheKeys.Build_UserTree_Key(userId);

            this._easyCache.Remove(selfKey, db: redisdbIndex);
        }

        /// <summary>
        /// 把指定的孩子UserId添加到指定父亲的孩子集合缓存里
        /// </summary>
        /// <param name="parentId">父亲用户Id</param>
        /// <param name="childUserId">孩子用户Id</param>
        public void AddToPartOfParentCache(int parentId, int childUserId)
        {
            if (GetUserFunc == null) throw new FException("您未指定查询用户信息的方法");

            string partOfKey = CommonCacheKeys.Build_UserTree_PartOf_Puid_Key(parentId);

            this._easyCache.SetAdd(partOfKey, childUserId.ToString(), db: redisdbIndex);

            //为了防止数据没有添加到redis，这里做一次检查，并且再补偿一次，记下记录以便跟踪。
            if (!IsInPartOfParentCache(parentId, childUserId))
            {
                logger.Fatal($"PartOfParentCache：出现添加异常。parentId={parentId}，childUserId={childUserId}");

                this._easyCache.SetAdd(partOfKey, childUserId.ToString(), db: redisdbIndex);
            }
        }

        /// <summary>
        /// 把指定的孩子UserId从指定父亲的孩子集合缓存里移除
        /// </summary>
        /// <param name="parentId">父亲用户Id</param>
        /// <param name="childUserId">孩子用户Id</param>
        public void RemoveMemberFromPartOfParentCache(int parentId, int childUserId)
        {
            if (GetUserFunc == null) throw new FException("您未指定查询用户信息的方法");

            string partOfKey = CommonCacheKeys.Build_UserTree_PartOf_Puid_Key(parentId);

            this._easyCache.SetRemove(partOfKey, childUserId.ToString(), db: redisdbIndex);
        }

        /// <summary>
        /// 移除指定UserId的孩子集合缓存
        /// </summary>
        public void RemovePartOfParentCache(int userId)
        {
            if (GetUserFunc == null) throw new FException("您未指定查询用户信息的方法");

            string partOfKey = CommonCacheKeys.Build_UserTree_PartOf_Puid_Key(userId);

            this._easyCache.Remove(partOfKey, db: redisdbIndex);
        }

        /// <summary>
        /// 检查指定孩子用户Id是否存在于他的父节点缓存集合中
        /// </summary>
        /// <param name="parentId">父亲用户Id</param>
        /// <param name="childUserId">孩子用户Id</param>
        public bool IsInPartOfParentCache(int parentId, int childUserId)
        {
            if (GetUserFunc == null) throw new FException("您未指定查询用户信息的方法");

            string partOfKey = CommonCacheKeys.Build_UserTree_PartOf_Puid_Key(parentId);

            return this._easyCache.IsInSet(partOfKey, childUserId.ToString(), db: redisdbIndex);
        }

        /// <summary>
        /// 从数据库查询用户信息
        /// </summary>
        private UserTreeNodeCache GetUserFormDB(int userId)
        {
            if (GetUserFunc == null) throw new FException("您未指定查询用户信息的方法");

            //查询用户信息
            var userTreeCacheNode = GetUserFunc.Invoke(userId);

            if (userTreeCacheNode == null) throw new FException("您传入的【用户Id】未能找到指定的用户");

            return userTreeCacheNode;
        }

        #endregion

        #region 【从下往上】构建用户树

        /// <summary>
        /// 从指定的用户Id【从下往上】构建用户【树形结构】
        /// </summary>
        /// <returns>参数【bottomUserId】对应的节点</returns>
        public UserTreeNode BuildUserTree_BottomToTop(int bottomUserId, Action<UserTreeNode> action = null)
        {
            if (GetUserFunc == null) throw new FException("您未指定查询用户信息的方法");
            if (bottomUserId <= 0) throw new FException("您必须要传入【用户Id】");

            //获取自己的节点信息
            var bottomCacheModel = this.GetUserTreeNode(bottomUserId);

            var bottomTreeNode = new UserTreeNode
            {
                UserId = bottomCacheModel.UserId,
                UserName = bottomCacheModel.UserName,
                ParentId = bottomCacheModel.ParentId,
                ParentName = bottomCacheModel.ParentName,
            };

            //执行附加任务
            if (action != null)
            {
                action.Invoke(bottomTreeNode);
            }

            //开始遍历上级节点
            this.Recursion_BuildUserTree_BottomToTop(ref bottomTreeNode, action);

            return bottomTreeNode;
        }

        /// <summary>
        /// 递归【从下往上】构建
        /// </summary>
        private void Recursion_BuildUserTree_BottomToTop(ref UserTreeNode currTreeNode, Action<UserTreeNode> action = null)
        {
            if (GetUserFunc == null) throw new FException("您未指定查询用户信息的方法");
            if (currTreeNode.UserId <= 0) throw new FException("您必须要传入【用户Id】");
            if (currTreeNode.ParentId < 0) throw new FException("您传入的上级【用户Id】未能找到指定的用户");

            //如果没有父级节点，直接返回
            if (currTreeNode.ParentId <= 0)
            {
                return;
            }

            var parentCacheModel = this.GetUserTreeNode(currTreeNode.ParentId);

            var parentNode = new UserTreeNode
            {
                UserId = parentCacheModel.UserId,
                UserName = parentCacheModel.UserName,
                ParentId = parentCacheModel.ParentId,
                ParentName = parentCacheModel.ParentName,
            };

            currTreeNode.ParentNode = parentNode;

            parentNode.ChildrenNode.Add(currTreeNode);

            //执行附加任务
            if (action != null)
            {
                action.Invoke(parentNode);
            }

            this.Recursion_BuildUserTree_BottomToTop(ref parentNode, action);
        }

        #endregion

        #region 【从上往下】构建用户树

        /// <summary>
        /// 从指定的用户Id【从上往下】构建【用户树形结构】
        /// </summary>
        /// <returns>参数【topUserId】对应的节点</returns>
        public UserTreeNode BuildUserTree_TopToBottom(int topUserId)
        {
            if (GetUserFunc == null) throw new FException("您未指定查询用户信息的方法");
            if (topUserId <= 0) throw new FException("您必须要传入【用户Id】");

            //获取自己的节点信息
            var topCacheModel = this.GetUserTreeNode(topUserId);

            var topTreeNode = new UserTreeNode
            {
                UserId = topCacheModel.UserId,
                UserName = topCacheModel.UserName,
                ParentId = topCacheModel.ParentId,
                ParentName = topCacheModel.ParentName,
            };

            //如果存在父级节点，那么添加父级节点
            if (topCacheModel.ParentId > 0)
            {
                var parentCacheModel = this.GetUserTreeNode(topCacheModel.ParentId);

                var parentNode = new UserTreeNode
                {
                    UserId = parentCacheModel.UserId,
                    UserName = parentCacheModel.UserName,
                    ParentId = parentCacheModel.ParentId,
                    ParentName = parentCacheModel.ParentName,
                };

                parentNode.ChildrenNode.Add(topTreeNode);

                topTreeNode.ParentNode = parentNode;
            }

            //开始遍历下级节点
            this.Recursion_BuildUserTree_TopToBottom(ref topTreeNode);

            return topTreeNode;
        }

        /// <summary>
        /// 递归【从上往下】构建
        /// </summary>
        private void Recursion_BuildUserTree_TopToBottom(ref UserTreeNode topTreeNode)
        {
            if (GetUserFunc == null) throw new FException("您未指定查询用户信息的方法");
            if (topTreeNode.UserId <= 0) throw new FException("您必须要传入【用户Id】");

            //查询直接孩子的缓存Key
            var childUserIdList = this.FindDirectChildUserId(topTreeNode.UserId);

            //如果没有孩子，直接返回
            if (childUserIdList.Count <= 0)
            {
                return;
            }

            //遍历孩子
            foreach (int childUserId in childUserIdList)
            {
                var cacheNode = this.GetUserTreeNode(childUserId);

                var childTreeNode = new UserTreeNode
                {
                    UserId = cacheNode.UserId,
                    UserName = cacheNode.UserName,
                    ParentId = cacheNode.ParentId,
                    ParentName = cacheNode.ParentName,
                };

                childTreeNode.ParentNode = topTreeNode;

                topTreeNode.ChildrenNode.Add(childTreeNode);

                this.Recursion_BuildUserTree_TopToBottom(ref childTreeNode);
            }
        }

        #endregion

        #region 用户相关查询

        /// <summary>
        /// 查询指定用户Id下的【所有】用户列表（默认包括自己）
        /// </summary>
        public List<UserTreeNodeCache> GetChildrenNodeList(int userId, bool isContainsSelf = true)
        {
            if (GetUserFunc == null) throw new FException("您未指定查询用户信息的方法");

            var result = new List<UserTreeNodeCache>();

            //开始遍历下级节点
            this.Recursion_GetChildrenNodeList(userId, ref result, isContainsSelf);

            return result;
        }

        /// <summary>
        /// 递归获取孩子节点
        /// </summary>
        /// <param name="currUserId">当前用户Id</param>
        /// <param name="userTreeList">结果集</param>
        /// <param name="isContainsSelf">是否包含当前用户</param>
        private void Recursion_GetChildrenNodeList(int currUserId, ref List<UserTreeNodeCache> userTreeList, bool isContainsSelf = true)
        {
            if (GetUserFunc == null) throw new FException("您未指定查询用户信息的方法");
            if (currUserId <= 0) throw new FException("您必须要传入【用户Id】");

            //先找出自己的树节点
            var selfTreeNode = this.GetUserTreeNode(currUserId);

            //加入列表中
            if (isContainsSelf)
            {
                userTreeList.Add(selfTreeNode);
            }

            //查询直接孩子的缓存Key
            var childUserIdList = this.FindDirectChildUserId(currUserId);

            //没有孩子
            if (childUserIdList.Count <= 0)
            {
                return;
            }

            //遍历孩子
            foreach (int childUserId in childUserIdList)
            {
                var childTreeNode = this.GetUserTreeNode(childUserId);

                this.Recursion_GetChildrenNodeList(childTreeNode.UserId, ref userTreeList, true);
            }
        }

        /// <summary>
        /// 查询指定用户Id下的【所有】用户Id（默认包括自己，格式： 1,2,3,4）
        /// </summary>
        public string GetChildrenIdsStr(int userId, bool isContainsSelf = true)
        {
            if (GetUserFunc == null) throw new FException("您未指定查询用户信息的方法");

            var nodeList = GetChildrenNodeList(userId, isContainsSelf);

            return string.Join(",", nodeList.Select(x => x.UserId));
        }

        /// <summary>
        /// 查找指定用户的【直接】孩子UserId集合（不包括自己）
        /// </summary>
        public List<int> FindDirectChildUserId(int parentId)
        {
            if (GetUserFunc == null) throw new FException("您未指定查询用户信息的方法");

            //再找出直接孩子的树节点
            string partOfParentKey = CommonCacheKeys.Build_UserTree_PartOf_Puid_Key(parentId);

            //【模式匹配】找出直接孩子的树节点Key
            var childUserIdList = this._easyCache.GetSetList(partOfParentKey, db: redisdbIndex) ?? new List<string>();

            return childUserIdList.Select(x => int.Parse(x)).ToList();
        }

        /// <summary>
        /// 查找指定用户的【直接】孩子个数（不包括自己）
        /// </summary>
        public long FindDirectChildCount(int parentId)
        {
            if (GetUserFunc == null) throw new FException("您未指定查询用户信息的方法");

            //再找出直接孩子的树节点
            string partOfParentKey = CommonCacheKeys.Build_UserTree_PartOf_Puid_Key(parentId);

            //【模式匹配】找出直接孩子的树节点Key
            long count = this._easyCache.GetSetLength(partOfParentKey, db: redisdbIndex);

            return count;
        }

        /// <summary>
        /// 查找指定用户的【直接】孩子信息集合（不包括自己）
        /// </summary>
        public List<UserTreeNodeCache> FindDirectChildren(int parentId)
        {
            if (GetUserFunc == null) throw new FException("您未指定查询用户信息的方法");

            var childUserIdList = FindDirectChildUserId(parentId);

            var userTreeList = new List<UserTreeNodeCache>();

            foreach (int childUserId in childUserIdList)
            {
                var userTreeNode = this.GetUserTreeNode(childUserId);

                userTreeList.Add(userTreeNode);
            }

            return userTreeList;
        }

        /// <summary>
        /// 传入的2个用户Id，判断是否是上下级关系（所有下级）
        /// </summary>
        /// <param name="childUserId">下级代理用户Id</param>
        /// <param name="parentId">上级代理用户Id</param>
        /// <returns>是否属于上下级关系</returns>
        public bool IsChildren(int childUserId, int parentId)
        {
            if (GetUserFunc == null) throw new FException("您未指定查询用户信息的方法");
            if (childUserId <= 0) throw new FException("您必须要传入【用户Id】");
            if (parentId <= 0) throw new FException("您传入的上级【用户Id】未能找到指定的用户");
            if (childUserId == parentId) { return true; }

            var cacheModel = this.GetUserTreeNode(childUserId);

            //ParentId == 0，那么该childUserId已经是顶级代理了，那么他肯定不属于parentId，直接返回false
            if (cacheModel.ParentId == 0)
            {
                return false;
            }

            //如果缓存中的ParentId与传入的parentId参数相同，那么说明他们是上下级关系
            //否则继续查找，直到找到顶级元素为止
            if (cacheModel.ParentId == parentId)
            {
                return true;
            }
            else
            {
                return this.IsChildren(cacheModel.ParentId, parentId);
            }
        }

        /// <summary>
        /// 根据指定用户Id获取其树中最顶层的用户信息
        /// </summary>
        /// <param name="userId">指定用户Id</param>
        /// <returns>树中最顶层的用户信息</returns>
        public UserTreeNode GetTopUserTreeNode(int userId)
        {
            if (GetUserFunc == null) throw new FException("您未指定查询用户信息的方法");
            if (userId <= 0) throw new FException("您必须要传入【用户Id】");

            var topUserTreeNode = this.BuildUserTree_BottomToTop(userId);

            return topUserTreeNode;
        }

        #endregion
    }
}