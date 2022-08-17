using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genshin__.lib.json
{
    internal class ProgramJson
    {
        public string updateTime { get; set; } // 上次获取游戏信息时间戳
        public string launcher { get; set; } // 启动器安装路径
        public string game { get; set; } // 游戏本体安装路径
        public LauncherInfo launcherInfo { get; set; } // 启动器初始参数信息
        public GameInfo gameInfo { get; set; } // 游戏本体初始参数信息
    }

    internal class LauncherInfo
    {
        public string cps { get; set; } // 启动器CPS
        public string sub_channel { get; set; } // 启动器子频道
        public string channel { get; set; } // 启动器频道
        public string game_install_path { get; set; } // 游戏安装路径
        public string game_dynamic_bg_name { get; set; } // 启动器动态背景
        public string game_start_name { get; set; } // 游戏启动程序名称
    }

    internal class GameInfo
    {
        public string cps { get; set; } // 游戏CPS
        public string sub_channel { set; get; } // 游戏子频道
        public string channel { set; get; } // 游戏频道
        public string game_version { get; set; } // 游戏版本
        public string plugin_sdk_version { get; set; } // 游戏SDK版本（未知用途保存用于故障恢复，保证游戏正常运行）
        public string plugin_5_version { get; set; } // 游戏插件版本（未知用途保存用于故障恢复，保证游戏正常运行）
    }
}
