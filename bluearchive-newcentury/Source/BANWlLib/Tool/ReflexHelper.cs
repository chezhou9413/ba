using System;

namespace BANWlLib.Tool
{
    public class ReflexHelper
    {
        /// <summary>
        /// 获取对象的 float? (可空浮点型) 属性值
        /// </summary>
        /// <param name="FieldName">属性名称</param>
        /// <param name="obj">目标对象</param>
        /// <returns>返回 float? 类型的值。如果属性不存在或发生转换错误，则返回 null。</returns>
        public static float? GetModelFloatValue(string FieldName, object obj)
        {
            try
            {
                Type Ts = obj.GetType();
                // 获取属性信息
                var prop = Ts.GetProperty(FieldName);

                // 检查属性是否存在
                if (prop == null)
                {
                    return null;
                }

                object o = prop.GetValue(obj, null);
                if (o == null)
                {
                    return null;
                }

                // 将获取到的值安全地转换为 float
                return Convert.ToSingle(o);
            }
            catch
            {
                // 如果在获取或转换过程中发生任何异常，则返回 null
                return null;
            }
        }

        /// <summary>
        /// 设置对象的 float 属性值
        /// </summary>
        /// <param name="FieldName">属性名称</param>
        /// <param name="Value">要设置的 float 值</param>
        /// <param name="obj">目标对象</param>
        /// <returns>成功返回 true，失败返回 false</returns>
        public static bool SetModelFloatValue(string FieldName, float Value, object obj)
        {
            try
            {
                Type Ts = obj.GetType();
                var prop = Ts.GetProperty(FieldName);

                // 确保属性存在并且是可写的
                if (prop == null || !prop.CanWrite)
                {
                    return false;
                }

                // 将输入的 float 值转换为目标属性的实际类型 (例如，如果目标是 double，会自动转换)
                object v = Convert.ChangeType(Value, prop.PropertyType);

                // 设置属性值
                prop.SetValue(obj, v, null);

                return true;
            }
            catch
            {
                // 如果在获取、转换或设置过程中发生任何异常，则操作失败
                return false;
            }
        }
    }
}
