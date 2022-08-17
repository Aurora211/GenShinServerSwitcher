using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genshin__.lib.json
{
    internal class ServerJson
    {
        public string updateTime { get; set; } // 服务器云配置更新日期
        public GameSetting[] gameSettings { get; set; } // 服务器配置列表
    }

    internal class GameSetting
    {
        public string comment { get; set; } // 配置名称
        public GameSettingInfo info { get; set; } // 设置信息
    }

    internal class GameSettingInfo
    {
        public string cps { get; set; } // 游戏CPS
        public string sub_channel { set; get; } // 游戏子频道
        public string channel { set; get; } // 游戏频道
        public string game_version { get; set; } // 游戏版本
        public FileRequirement[] fileRequirements { get; set; } // 转换需要的文件列表
    }

    internal class FileRequirement
    {
        public string method { get; set; } // 应执行操作
        public string fileName { get; set; } // 需要的文件
        public string filePath { get; set; } // 文件应存放那里
        public string fileMd5 { get; set; } // 文件MD5
    }
}
