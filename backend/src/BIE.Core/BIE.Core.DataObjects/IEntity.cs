using System;

namespace BIE.Core.DataObjects
{
    public interface IEntity
    {
        long Key { get; }
        void Copy(IEntity entity);
    }

    public static class Helper
    {
        public static void Copy(Type type, object source, object target)
        {
            var properties = (type).GetProperties();

            foreach (var p in properties)
            {
                p.SetValue(target, p.GetValue(source, null));
            }
        }

        public static void Copy<T>(object source, object target)
        {
            Helper.Copy(typeof(T), source, target);
        }
    }
}
