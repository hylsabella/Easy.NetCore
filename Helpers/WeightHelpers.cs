using System;
using System.Collections.Generic;
using System.Linq;

namespace Easy.Common.NetCore.Helpers
{
    public class WeightObject<T>
    {
        public T Object { get; set; }

        public int Weight { get; set; }
    }

    public static class WeightHelpers
    {
        public static WeightObject<T> ConvertToWeightObject<T>(T model, int weight)
        {
            if (weight < 0) throw new Exception("weight不能小于0");

            return new WeightObject<T>
            {
                Object = model,
                Weight = weight,
            };
        }

        public static List<T> GetRandomList<T>(List<WeightObject<T>> srcList, int topCount)
        {
            if (topCount <= 0) throw new Exception("topCount必须大于0");
            if (srcList is { Count: <= 0 }) throw new Exception("srcList不能为空");

            if (srcList.Count < topCount)
            {
                topCount = srcList.Count;
            }

            //权重总和
            int totalWeights = srcList.Sum(x => x.Weight + 1);

            var randomlist = new List<KeyValuePair<int, int>>();

            for (int listIndex = 0; listIndex < srcList.Count; listIndex++)
            {
                int randomWeight = srcList[listIndex].Weight + 1 + RandomHelper.GetRandom(0, totalWeights);

                randomlist.Add(new KeyValuePair<int, int>(listIndex, randomWeight));
            }

            //倒序排序
            randomlist.Sort((kvp1, kvp2) => kvp2.Value - kvp1.Value);

            //取最前面几个
            var resultList = randomlist.Take(topCount).Select(x => srcList[x.Key].Object).ToList();

            return resultList;
        }
    }
}
