using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace MPServer.Models
{
    [Table("Message")]
    public class Message
    {
        [Key]
        public int MessageId { get; set; }

        /// <summary>
        /// 收到短信时间
        /// </summary>
        [Required]
        [Display(Name = "收到短信时间")]
        public DateTime MessageTime { get; set; }

        /// <summary>
        /// 短信内容
        /// </summary>
        [Required]
        [Display(Name = "短信内容")]
        public string Content { get; set; }

        /// <summary>
        /// 短信加密用盐
        /// </summary>
        [Required]
        public string Salt { get; set; }
    }
}
