﻿using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MPServer.Models
{
    [Table("HearBeat")]
    public class HeartBeat
    {
        [Key]
        public int HeartBeatId { get; set; }

        /// <summary>
        /// 收到心跳时间
        /// </summary>
        [Required]
        [Display(Name = "收到心跳时间")]
        public DateTime HeartBeatTime { get; set; }

        /// <summary>
        /// 发送设备
        /// </summary>
        [Required]
        [Display(Name = "发送设备")]
        public string Device { get; set; }

        /// <summary>
        /// 发送设备IP
        /// </summary>
        [Required]
        [Display(Name = "发送设备IP")]
        public string IPAddress { get; set; }
    }

    public class HeartBeatModel
    {
        /// <summary>
        /// 发送设备
        /// </summary>
        [Required]
        public string Device { get; set; }
    }
}
