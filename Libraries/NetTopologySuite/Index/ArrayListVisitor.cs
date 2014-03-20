using System.Collections;
using System.Collections.Generic;

namespace GisSharpBlog.NetTopologySuite.Index
{
    /// <summary>
    /// 
    /// </summary>
    public class ArrayListVisitor : IItemVisitor
    {
        private List<object> items = new List<object>();
        
        /// <summary>
        /// 
        /// </summary>
        public ArrayListVisitor() { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        public void VisitItem(object item)
        {
            items.Add(item);
        }

        /// <summary>
        /// 
        /// </summary>
        public IList Items
        {
            get
            {
                return items;
            }
        }
    }
}
