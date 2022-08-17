using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genshin__.lib.json
{
    internal class MainConfig
    {
        public FpsSettingInfo fps { get; set; } // FPS相关
        public StartResolutionSettingInfo startResolution { get; set; } // 启动分辨率相关
        public LauncherBackgroudImage launchImage { get; set; } // 启动器图片相关
        public bool gameInfoDisplay { get; set; } // 左上信息展示气泡
        public bool popupAlert { get; set; } // 弹窗提示
    }

    internal class LauncherBackgroudImage
    {
        public bool enable { get; set; } // 开启自定义启动器图片
        public string path { get; set; } // 自定义图片路径
    }

    internal class FpsSettingInfo
    {
        public bool unlock { get; set; } // 是否解锁
        public int fpsLimit { get; set; } // 解锁最大FPS
    }

    internal class StartResolutionSettingInfo
    {
        public bool enable { get; set; } // 启动自定义
        public bool fullScreen { get; set; } // 全屏 -screen-fullscreen 0 1
        public bool popupWindow { get; set; } // 无边框窗口 -popupwindow
        public int height { get; set; } // 分辨率高 -screen-height 1080
        public int width { get; set; } // 分辨率宽 -screen-width 1920
    }
}
