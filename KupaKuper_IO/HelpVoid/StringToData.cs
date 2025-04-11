
namespace KupaKuper_IO.HelpVoid
{
    public static class StringToData
    {
        /// <summary>
        /// 将字符串列表里的字符串尝试转换成指定类型, 如果不能转换的为默认值(0)
        /// </summary>
        /// <typeparam name="T">要转换的目标类型</typeparam>
        /// <param name="strings">待转换的字符串列表</param>
        /// <returns>转换后的指定类型列表</returns>
        public static List<T> StringTo<T>(List<string> strings)
        {
            var result = new List<T>();
            foreach (var str in strings)
            {
                if (typeof(T) == typeof(int))
                {
                    result.Add((T)(object)StringToInt(str));
                }
                else if (typeof(T) == typeof(double))
                {
                    result.Add((T)(object)StringToDouble(str));
                }
                else if (typeof(T) == typeof(float))
                {
                    result.Add((T)(object)StringToFloat(str));
                }
                else if (typeof(T) == typeof(long))
                {
                    result.Add((T)(object)StringToLong(str));
                }
                else
                {
                    throw new NotSupportedException($"不支持的类型: {typeof(T)}");
                }
            }
            return result;
        }

        /// <summary>
        /// 将字符串转换为 int 类型，如果转换失败则返回 0
        /// </summary>
        /// <param name="str">待转换的字符串</param>
        /// <returns>转换后的 int 值</returns>
        private static int StringToInt(string str)
        {
            if (int.TryParse(str, out int intValue))
            {
                return intValue;
            }
            return 0;
        }

        /// <summary>
        /// 将字符串转换为 double 类型，如果转换失败则返回 0
        /// </summary>
        /// <param name="str">待转换的字符串</param>
        /// <returns>转换后的 double 值</returns>
        private static double StringToDouble(string str)
        {
            if (double.TryParse(str, out double doubleValue))
            {
                return doubleValue;
            }
            return 0;
        }

        /// <summary>
        /// 将字符串转换为 float 类型，如果转换失败则返回 0
        /// </summary>
        /// <param name="str">待转换的字符串</param>
        /// <returns>转换后的 float 值</returns>
        private static float StringToFloat(string str)
        {
            if (float.TryParse(str, out float floatValue))
            {
                return floatValue;
            }
            return 0;
        }

        /// <summary>
        /// 将字符串转换为 long 类型，如果转换失败则返回 0
        /// </summary>
        /// <param name="str">待转换的字符串</param>
        /// <returns>转换后的 long 值</returns>
        private static long StringToLong(string str)
        {
            if (long.TryParse(str, out long longValue))
            {
                return longValue;
            }
            return 0;
        }
    }
}
