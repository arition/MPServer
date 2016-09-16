using System;

namespace MessageClient.Models
{
    public class Message
    {
        public int MessageId { get; set; }

        /// <summary>
        /// 收到短信时间
        /// </summary>
        public DateTime MessageTime { get; set; }

        /// <summary>
        /// 短信内容
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// 短信加密用盐
        /// </summary>
        public string Salt { get; set; }
    }
}
