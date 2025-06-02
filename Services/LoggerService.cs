using FluentFTP.Helpers;
using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.VisualBasic.Logging;
using RaspRun.Grpc;
using SirisDeviceManager.Model;
using SirisDeviceManager.View.UserControls.Device.Resume;
using SixLabors.ImageSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;


namespace SirisDeviceManager.Services
{
    public class LoggerService
    {
        private static readonly Regex RebootRegex = new(@"Rebooting in (?<time>\d+:\d+)", RegexOptions.Compiled);
        private static readonly Regex FileDownloadRegex = new(@"Downloading missing file: (?<file>.+?) ", RegexOptions.Compiled);
        private static readonly Regex FilePlayRegex = new(@"Started playing .*?: (?<file>.+?) ", RegexOptions.Compiled);
        private static readonly Regex StreamRestartRegex = new(@"Stream playback ended early", RegexOptions.Compiled);

        public LoggerService() { }

        public async Task GetDeviceLog(SirisDeviceManager.Model.Device device)
        {
            string address = $"http://{device.Ip}:5100";
            
            using var channel = GrpcChannel.ForAddress(address);
            var client = new Logger.LoggerClient(channel);
            var request = new LogRequest();
           
            try
            {
                using var call = client.WatchLogs(request);

                await AppSessionManager.Instance.AddLog(device.SerialNumber, $"[INFO] Connected to {address}");

                await foreach (var log in call.ResponseStream.ReadAllAsync())
                    await AppSessionManager.Instance.AddLog(device.SerialNumber, $"[{log.Timestamp}] {log.Source} [{log.Level}] - {log.Message}");
            
            }catch(Exception ex)
            {
                await AppSessionManager.Instance.AddLog(device.SerialNumber, $"[ERROR] Connection failed: {ex.Message}");
            }
        }

        public async Task ParseLog(SirisDeviceManager.Model.Device device, string log)
        {
            try
            {
                device.IsConnected = GetDeviceState(log);

                if (log.Contains("[VERSION]"))
                {
                    var match = Regex.Match(log, @"\[VERSION\]\s*-\s*(V[\d\.]+)");
                    if (match.Success)
                        device.Version = match.Groups[1].Value;
                }

                GetDownloadState(device, log);

                GetSessionState(device, log);

                await Task.Delay(10);
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void GetSessionState(SirisDeviceManager.Model.Device device, string log)
        {
            if (string.IsNullOrWhiteSpace(log))
                device.SessionState = SessionState.SESSION_ANY;

            // Caso: [INFO] - Found: {int} sessions of this device
            var foundSessionsRegex = new Regex(@"\[INFO\]\s+-\s+Found:\s+(\d+)\s+sessions of this device", RegexOptions.IgnoreCase);
            var matchFound = foundSessionsRegex.Match(log);
            if (matchFound.Success && int.TryParse(matchFound.Groups[1].Value, out int count))
            {
                if (count == 0)
                    device.SessionState = SessionState.SESSION_ANY;
                else
                    device.SessionState = SessionState.SESSION_EXIST_WAITING;
            }

            // Caso: [RUNNING] - Queued: {some file name}
            var runningQueuedRegex = new Regex(@"\[RUNNING\]\s+-\s+Queued:", RegexOptions.IgnoreCase);
            if (runningQueuedRegex.IsMatch(log))
                device.SessionState = SessionState.SESSION_EXIST_RUNNING_FILES;

            // Caso: [INFO] - Finished playing: {some file name}
            var finishedPlayingRegex = new Regex(@"\[INFO\]\s+-\s+Finished playing:", RegexOptions.IgnoreCase);
            if (finishedPlayingRegex.IsMatch(log))
                device.SessionState = SessionState.SESSION_ANY;

            // Caso: Starting streaming playback
            var streamingStartRegex = new Regex(@"Starting streaming playback for session", RegexOptions.IgnoreCase);
            if (streamingStartRegex.IsMatch(log))
                device.SessionState = SessionState.SESSION_EXIST_RUNNING_STREAM;

            // Caso: Sessão terminou
            var sessionEndedRegex = new Regex(@"Session\s+\d+\s+has ended\. Stopping stream loop", RegexOptions.IgnoreCase);
            if (sessionEndedRegex.IsMatch(log))
                device.SessionState = SessionState.SESSION_ANY;
        }

        private void GetDownloadState(SirisDeviceManager.Model.Device device, string log)
        {
            if (string.IsNullOrWhiteSpace(log))
                device.DownloadState = DownloadState.DOWNLOAD_ANY;

            var downloadingRegex = new Regex(@"\[DOWNLOAD\] - Downloading missing file:", RegexOptions.IgnoreCase);
            if (downloadingRegex.IsMatch(log))
                device.DownloadState = DownloadState.DOWNLOADING;

            var completedRegex = new Regex(@"\[INFO\]\s+-\s+\[INFO\]\s+Download completed for fileId=", RegexOptions.IgnoreCase);
            if (completedRegex.IsMatch(log))
                device.DownloadState = DownloadState.DOWLOAD_COMPLEATED;

            var errorRegex = new Regex(@"\[(WARNING|ERROR)\].*(Delete failed|Download error|Failed to delete)", RegexOptions.IgnoreCase);
            if (errorRegex.IsMatch(log))
                device.DownloadState = DownloadState.DOWNLOAD_ERROR;

            var foundSessionsRegex = new Regex(@"\[INFO\]\s+-\s+Found:\s+(\d+)\s+sessions of this device", RegexOptions.IgnoreCase);
            var match = foundSessionsRegex.Match(log);
            if (match.Success && int.TryParse(match.Groups[1].Value, out int sessionCount) && sessionCount == 0)
                device.DownloadState = DownloadState.DOWNLOAD_ANY;

            if (Regex.IsMatch(log, @"\[SYNC\]\s*-\s*File already exists:", RegexOptions.IgnoreCase))
                device.DownloadState = DownloadState.DOWLOAD_COMPLEATED;
        }

        private bool GetDeviceState(string log)
        {
            if (log.Contains("Connection failed"))
                return false;

            return true;
        }
    }
}
