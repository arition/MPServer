using System.Collections.Generic;

namespace MPServer.Models
{
    public class ApiListModel<T>
    {
        /// <summary>
        /// 总计页数
        /// </summary>
        public int MaxPage { get; set; }

        /// <summary>
        /// 当前页数
        /// </summary>
        public int CurrentPage { get; set; }

        /// <summary>
        /// 数据
        /// </summary>
        public IEnumerable<T> Data { get; set; }

        public ApiListModel(int maxPage, int currentPage, IEnumerable<T> data)
        {
            MaxPage = maxPage;
            CurrentPage = currentPage;
            Data = data;
        }
    }
}
