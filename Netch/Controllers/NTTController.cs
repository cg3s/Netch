﻿using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Netch.Forms;
using Netch.Models;
using Netch.Utils;

namespace Netch.Controllers
{
    public class NTTController
    {
        /// <summary>
        ///		进程实例
        /// </summary>
        public Process Instance;

        /// <summary>
        ///		当前状态
        /// </summary>
        public State State = State.Waiting;

        /// <summary>
        /// 启动NatTypeTester
        /// </summary>
        /// <returns></returns>
        public (bool, string, string, string) Start()
        {
            Thread.Sleep(1000);
            MainForm.Instance.NatTypeStatusText($"{i18N.Translate("Starting NatTester")}");
            try
            {
                if (!File.Exists("bin\\NTT.exe"))
                {
                    return (false, null, null, null);
                }

                Instance = MainController.GetProcess();
                Instance.StartInfo.FileName = "bin\\NTT.exe";

                Instance.StartInfo.Arguments = $" {Global.Settings.STUN_Server} {Global.Settings.STUN_Server_Port}";

                Instance.OutputDataReceived += OnOutputDataReceived;
                Instance.ErrorDataReceived += OnOutputDataReceived;

                State = State.Starting;
                Instance.Start();
                Instance.BeginOutputReadLine();
                Instance.BeginErrorReadLine();
                Instance.WaitForExit();

                var result = File.ReadAllText("logging\\NTT.log").Split('#');
                var natType = result[0];
                var localEnd = result[1];
                var publicEnd = result[2];
                MainForm.Instance.NatTypeStatusText(natType);

                return (true, natType, localEnd, publicEnd);
            }
            catch (Exception)
            {
                Logging.Info("NTT 进程出错");
                Stop();
                return (false, null, null, null);
            }
        }

        /// <summary>
        ///		停止
        /// </summary>
        public void Stop()
        {
            try
            {
                if (Instance != null && !Instance.HasExited)
                {
                    Instance.Kill();
                    Instance.WaitForExit();
                }
            }
            catch (Exception e)
            {
                Logging.Info(e.ToString());
            }
        }

        public void OnOutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(e.Data))
            {
                if (File.Exists("logging\\NTT.log"))
                {
                    File.Delete("logging\\NTT.log");
                }

                File.AppendAllText("logging\\NTT.log", $"{e.Data}\r\n");
            }
        }
    }
}
