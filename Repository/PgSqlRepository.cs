using Dapper;
using Easy.Common.NetCore.Exceptions;
using Easy.Common.NetCore.Helpers;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;

namespace Easy.Common.NetCore.Repository
{
    /// <summary>
    /// Dapper读写数据仓储
    /// </summary>
    public class PgSqlRepository<T> : IRepository<T> where T : EntityPrimary
    {
        private readonly string _tableName = typeof(T).Name;

        /// <summary>
        /// 获取主键记录
        /// </summary>
        /// <param name="id">主键值</param>
        /// <param name="tableIndex">分表Id</param>
        public T Get(int id, string connectionStringName = "default", string tableIndex = "")
        {
            if (id <= 0) return default;

            string connectionString = RepositoryCenter.GetConnectionString(connectionStringName);

            tableIndex = (tableIndex ?? string.Empty).Trim();

            using (IDbConnection connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();

                return connection.QuerySingleOrDefault<T>($"SELECT * FROM public.{_tableName + tableIndex} WHERE Id = @Id", new { Id = id });
            }
        }

        /// <summary>
        /// 插入数据
        /// </summary>
        /// <param name="model">model</param>
        /// <param name="tableIndex">分表Id</param>
        public int Insert(T model, string connectionStringName = "default", string tableIndex = "")
        {
            CheckHelper.NotNull(model, "model");

            string connectionString = RepositoryCenter.GetConnectionString(connectionStringName);

            tableIndex = (tableIndex ?? string.Empty).Trim();

            var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            CheckHelper.ArrayNotHasNull(properties, "properties");


            //排除主键Id字段（数据库自增）
            string nameSql = string.Join(",", properties.Where(x => !string.Equals(x.Name, "Id", StringComparison.OrdinalIgnoreCase))
                                   .Select(x => $"{x.Name}").OrderBy(x => x));

            string valueSql = string.Join(",", properties.Where(x => !string.Equals(x.Name, "Id", StringComparison.OrdinalIgnoreCase))
                                   .Select(x => $"@{x.Name}").OrderBy(x => x));

            string sql = $"INSERT INTO public.{_tableName + tableIndex}({nameSql}) VALUES({valueSql}) RETURNING id";

            using (IDbConnection connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();

                int newId = connection.Query<int>(sql, model).FirstOrDefault();

                if (newId <= 0) throw new RepositoryException("插入数据失败！");

                return newId;
            }
        }

        /// <summary>
        /// 批量插入数据
        /// </summary>
        /// <param name="modelList">modelList</param>
        /// <param name="tableIndex">分表Id</param>
        public int InsertBulk(IList<T> modelList, string connectionStringName = "default", string tableIndex = "")
        {
            CheckHelper.ArrayNotHasNull(modelList, "modelList");

            string connectionString = RepositoryCenter.GetConnectionString(connectionStringName);

            tableIndex = (tableIndex ?? string.Empty).Trim();

            var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            CheckHelper.ArrayNotHasNull(properties, "properties");


            //排除主键Id字段（数据库自增）
            string nameSql = string.Join(",", properties.Where(x => !string.Equals(x.Name, "Id", StringComparison.OrdinalIgnoreCase))
                                   .Select(x => $"{x.Name}").OrderBy(x => x));

            string valueSql = string.Join(",", properties.Where(x => !string.Equals(x.Name, "Id", StringComparison.OrdinalIgnoreCase))
                                    .Select(x => $"@{x.Name}").OrderBy(x => x));

            string sql = $"INSERT INTO public.{_tableName + tableIndex}({nameSql}) VALUES({valueSql})";

            using (IDbConnection connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();

                int count = connection.Execute(sql, modelList);

                if (count != modelList.Count) throw new RepositoryException("批量插入数据失败，请重新加载数据后重试！");

                return count;
            }
        }

        /// <summary>
        /// 更新数据
        /// </summary>
        /// <param name="model">model</param>
        /// <param name="tableIndex">分表Id</param>
        public void Update(T model, string connectionStringName = "default", string tableIndex = "")
        {
            CheckHelper.NotNull(model, "model");
            if (model.ID <= 0) throw new Exception("主键Id必须大于0！");

            string connectionString = RepositoryCenter.GetConnectionString(connectionStringName);

            tableIndex = (tableIndex ?? string.Empty).Trim();

            var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            CheckHelper.ArrayNotHasNull(properties, "properties");


            //排除主键Id字段
            string valueSql = string.Join(",", properties.Where(x => !string.Equals(x.Name, "Id", StringComparison.OrdinalIgnoreCase))
                                    .Select(x =>
                                    {
                                        if (string.Equals(x.Name, "Version", StringComparison.OrdinalIgnoreCase))
                                        {
                                            return $"Version = Version + 1";
                                        }

                                        if (string.Equals(x.Name, "EditDate", StringComparison.OrdinalIgnoreCase))
                                        {
                                            return $"EditDate = now()";
                                        }

                                        return $"{x.Name} = @{x.Name}";
                                    }));

            bool hasVersionField = properties.Where(x => string.Equals(x.Name, "Version", StringComparison.OrdinalIgnoreCase)).Any();

            string versionWhere = hasVersionField ? " AND Version = @Version " : string.Empty;

            string sql = $"UPDATE public.{_tableName + tableIndex} SET {valueSql} WHERE Id = @Id {versionWhere}";

            using (IDbConnection connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();

                int count = connection.Execute(sql, model);

                if (count <= 0) throw new RepositoryException("更新数据失败，请重新加载数据后重试！");
            }
        }

        /// <summary>
        /// 软删除数据
        /// </summary>
        /// <param name="id">主键值</param>
        /// <param name="tableIndex">分表Id</param>
        public void DeleteByTag(int id, string connectionStringName = "default", string tableIndex = "")
        {
            if (id <= 0) throw new ArgumentException("Id不能为空");

            string connectionString = RepositoryCenter.GetConnectionString(connectionStringName);

            tableIndex = (tableIndex ?? string.Empty).Trim();

            using (IDbConnection connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();

                int count = connection.Execute($"UPDATE public.{_tableName + tableIndex} SET IsDel = '1',Version = Version + 1 WHERE Id = @Id", new { Id = id });

                if (count <= 0) throw new RepositoryException("软删除数据失败，请重新加载数据后重试！");
            }
        }

        /// <summary>
        /// 物理删除数据
        /// </summary>
        /// <param name="id">主键值</param>
        /// <param name="tableIndex">分表Id</param>
        public void DeleteFromDB(int id, string connectionStringName = "default", string tableIndex = "")
        {
            if (id <= 0) throw new ArgumentException("Id不能为空");

            string connectionString = RepositoryCenter.GetConnectionString(connectionStringName);

            tableIndex = (tableIndex ?? string.Empty).Trim();

            using (IDbConnection connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();

                int count = connection.Execute($"DELETE FROM public.{_tableName + tableIndex} WHERE Id = @Id", new { Id = id });

                if (count <= 0) throw new RepositoryException("物理删除数据失败！");
            }
        }

        /// <summary>
        /// 分页查询
        /// </summary>
        /// <param name="search">条件</param>
        /// <returns>分页数据</returns>
        public PageResult<T> SearchPageResult(SearchBase search, string connectionStringName = "default")
        {
            CheckHelper.NotNull(search, "search");

            string connectionString = RepositoryCenter.GetConnectionString(connectionStringName);

            string tableIndex = (search.TableIndex ?? string.Empty).Trim();

            var param = new
            {
                StartIndex = search.StartIndex,
                EndIndex = search.EndIndex,
                BeginTime = search.BeginTime,
                EndTime = search.EndTime,
            };

            var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            CheckHelper.ArrayNotHasNull(properties, "properties");

            bool hasCreateDateField = properties.Where(x => string.Equals(x.Name, "CreateDate", StringComparison.OrdinalIgnoreCase)).Any();
            bool hasIsDelDateField = properties.Where(x => string.Equals(x.Name, "IsDel", StringComparison.OrdinalIgnoreCase)).Any();

            string timeWhere = string.Empty;
            if (hasCreateDateField)
            {
                if (search.BeginTime.HasValue)
                {
                    timeWhere += " AND CreateDate >= @BeginTime ";
                }

                if (search.EndTime.HasValue)
                {
                    timeWhere += " AND CreateDate < @EndTime ";
                }
            }

            string isDelWhere = hasIsDelDateField ? " AND IsDel = '0' " : string.Empty;

            using (IDbConnection connection = new NpgsqlConnection(connectionString))
            {
                connection.Open();

                int totalCount = connection.QuerySingleOrDefault<int>($"SELECT COUNT(*) FROM public.{_tableName + tableIndex} WHERE 1 = 1{isDelWhere}{timeWhere}", param);

                if (totalCount <= 0)
                {
                    return new PageResult<T> { TotalCount = 0, Results = new List<T>() };
                }

                string sql = $"SELECT * FROM (SELECT ROW_NUMBER() OVER (ORDER BY Id DESC) AS RowNumber,* FROM public.{_tableName + tableIndex} WHERE 1 = 1{isDelWhere}{timeWhere}) AS Temp WHERE Temp.RowNumber > @StartIndex AND Temp.RowNumber <= @EndIndex";

                var results = connection.Query<T>(sql, param).ToList();

                return new PageResult<T>
                {
                    TotalCount = totalCount,
                    Results = results
                };
            }
        }
    }
}