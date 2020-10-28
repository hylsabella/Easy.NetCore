using System.Collections.Generic;

namespace Easy.Common.NetCore.Helpers
{
    /// <summary>
    /// 银行卡助手类
    /// </summary>
    public static class BankCardHelper
    {

        /// <summary>
        /// 银行卡号格式检测
        /// Luhn校验规则：16位银行卡号（19位通用）:
        /// 1.将未带校验位的 15（或18）位卡号从右依次编号 1 到 15（18），位于奇数位号上的数字乘以 2。
        /// 2.将奇位乘积的个十位全部相加，再加上所有偶数位上的数字。
        /// 3.将加法和加上校验位能被 10 整除。
        /// </summary>
        public static bool CheckBankCardNo(string bankCardNo)
        {
            if (string.IsNullOrWhiteSpace(bankCardNo))
            {
                return false;
            }

            bankCardNo = bankCardNo.Replace(" ", string.Empty);

            if (bankCardNo.Length < 16 || bankCardNo.Length > 19)
            {
                return false;
            }

            //开头6位
            var strBin = "10,18,30,35,37,40,41,42,43,44,45,46,47,48,49,50,51,52,53,54,55,56,58,60,62,65,68,69,84,87,88,94,95,98,99";
            if (strBin.IndexOf(bankCardNo.Substring(0, 2)) < 0)
            {
                return false;
            }

            bool isBankCard = false;

            var lastNum = int.Parse(bankCardNo.Substring(bankCardNo.Length - 1, 1));//取出最后一位（与luhn进行比较）

            string first15Num = bankCardNo.Substring(0, bankCardNo.Length - 1);//前15或18位
            List<string> newArr = new List<string>();
            for (var i = first15Num.Length - 1; i > -1; i--)
            {    //前15或18位倒序存进数组
                newArr.Add(first15Num.Substring(i, 1));
            }

            List<int> arrJiShu = new List<int>();  //奇数位*2的积 <9
            List<int> arrJiShu2 = new List<int>(); //奇数位*2的积 >9

            List<string> arrOuShu = new List<string>();  //偶数位数组

            for (var j = 0; j < newArr.Count; j++)
            {
                if ((j + 1) % 2 == 1)
                {
                    if (int.Parse(newArr[j]) * 2 < 9)//奇数位
                    {
                        arrJiShu.Add(int.Parse(newArr[j]) * 2);
                    }
                    else
                    {
                        arrJiShu2.Add(int.Parse(newArr[j]) * 2);
                    }
                }
                else//偶数位
                {
                    arrOuShu.Add(newArr[j]);
                }
            }

            List<int> jishu_child1 = new List<int>();//奇数位*2 >9 的分割之后的数组个位数
            List<int> jishu_child2 = new List<int>();//奇数位*2 >9 的分割之后的数组十位数
            for (var h = 0; h < arrJiShu2.Count; h++)
            {
                jishu_child1.Add(arrJiShu2[h] % 10);
                jishu_child2.Add(arrJiShu2[h] / 10);
            }

            var sumJiShu = 0; //奇数位*2 < 9 的数组之和
            var sumOuShu = 0; //偶数位数组之和
            var sumJiShuChild1 = 0; //奇数位*2 >9 的分割之后的数组个位数之和
            var sumJiShuChild2 = 0; //奇数位*2 >9 的分割之后的数组十位数之和

            for (var m = 0; m < arrJiShu.Count; m++)
            {
                sumJiShu += arrJiShu[m];
            }

            for (var n = 0; n < arrOuShu.Count; n++)
            {
                sumOuShu += int.Parse(arrOuShu[n]);
            }

            for (var p = 0; p < jishu_child1.Count; p++)
            {
                sumJiShuChild1 += jishu_child1[p];
                sumJiShuChild2 += jishu_child2[p];
            }

            //计算总和
            int sumTotal = sumJiShu + sumOuShu + sumJiShuChild1 + sumJiShuChild2;

            //计算luhn值
            var k = sumTotal % 10 == 0 ? 10 : sumTotal % 10;
            var luhn = 10 - k;

            if (lastNum == luhn)
            {
                isBankCard = true;
            }

            return isBankCard;
        }
    }
}