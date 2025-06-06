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
        public LoggerService() { }

        public async Task GetDeviceLog(SirisDeviceManager.Model.Device device)
        {
            string address = $"http://{device.Ip}:5100";

            while(true)
            {
                using var channel = GrpcChannel.ForAddress(address);
                var client = new Logger.LoggerClient(channel);
                var request = new LogRequest();

                try
                {
                    using var call = client.WatchLogs(request);

                    await AppSessionManager.Instance.AddLog(device.SerialNumber, $"[INFO] Connected to {address}");

                    await foreach (var log in call.ResponseStream.ReadAllAsync())
                    {
                        await AppSessionManager.Instance.AddLog(device.SerialNumber, $"[{log.Timestamp}] {log.Source} [{log.Level}] - {log.Message}");
                    }

                    break;
                }
                catch (Exception ex)
                {
                    await AppSessionManager.Instance.AddLog(device.SerialNumber, $"[ERROR] Connection failed: {ex.Message}");
                    await Task.Delay(TimeSpan.FromSeconds(5));
                }
            }
        }

        public async Task ParseLog(SirisDeviceManager.Model.Device device, string log)
        {
            try
            {
                //Parse device state: connected/disconnected
                GetDeviceState(device, log);

                //Parse device rasprun version: "v0.0"
                GetVersion(device, log);

                //Parse running streaming session: true/false 
                GetRunningState(device, log);

                //Parse download session files state: any/error/compleated/downloading 
                GetDownloadState(device, log);

                //Parse session state: any/waiting/running_files/running_streaming
                GetSessionState(device, log);

                //Parse recent session ID
                GetSessionId(device, log);

                // Parse reboot schedule and reboot countdown
                GetRebootState(device, log);

                await Task.Delay(10);
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        #region AUX PARSE FUNCTIONS

        private void GetRebootState(Device device, string log)
        {
            if (string.IsNullOrWhiteSpace(log))
                return;

            log = log.Trim();

            // [INFO] Scheduling reboot in {rebootDelay.TotalMinutes:F2} minutes (Session {session.Id})
            var scheduleMatch = Regex.Match(log, @"Scheduling reboot in ([\d.]+) minutes");
            if (scheduleMatch.Success)
            {
                if (double.TryParse(scheduleMatch.Groups[1].Value, out double minutes))
                {
                    device.IsRebootScheduled = true;
                    device.RebootCountDown = TimeSpan.FromMinutes(minutes);
                }
                return;
            }

            // [INFO] Rebooting in {remaining.Minutes:D2}:{remaining.Seconds:D2} (Session {session.Id})
            var rebootingMatch = Regex.Match(log, @"Rebooting in (\d{2}):(\d{2})");
            if (rebootingMatch.Success)
            {
                if (int.TryParse(rebootingMatch.Groups[1].Value, out int minutes) &&
                    int.TryParse(rebootingMatch.Groups[2].Value, out int seconds))
                {
                    device.IsRebootScheduled = true;
                    device.RebootCountDown = new TimeSpan(0, minutes, seconds);
                }
                return;
            }

            // [INFO] Session {session.Id} start time changed and is now outside the reboot window ({newTimeToStart.TotalMinutes:F2} minutes). Canceling reboot.
            // [INFO] Reboot canceled for session {session.Id}.
            // [INFO] {session.Id}, was cancelled canceling reboot.
            // [INFO] All reboot tasks canceled.
            if (Regex.IsMatch(log, @"(Canceling reboot|Reboot canceled|was cancelled canceling reboot|All reboot tasks canceled)", RegexOptions.IgnoreCase))
            {
                device.IsRebootScheduled = false;
                device.RebootCountDown = TimeSpan.Zero;
                return;
            }

            // [INFO] Session {session.Id} start time changed but remains within valid reboot window ({newTimeToStart.TotalMinutes:F2} minutes). Keeping reboot.
            var keepMatch = Regex.Match(log, @"remains within valid reboot window \(([\d.]+) minutes\)");
            if (keepMatch.Success)
            {
                if (double.TryParse(keepMatch.Groups[1].Value, out double minutes))
                {
                    device.IsRebootScheduled = true;
                    device.RebootCountDown = TimeSpan.FromMinutes(minutes);
                }
                return;
            }

            // [INFO] Rebooting now for session {session.Id}!
            if (Regex.IsMatch(log, @"Rebooting now", RegexOptions.IgnoreCase))
            {
                device.IsRebootScheduled = false;
                device.RebootCountDown = TimeSpan.Zero;
                return;
            }

            // Nenhuma correspondência -> não altera o estado
        }

        private void GetVersion(SirisDeviceManager.Model.Device device, string log)
        {
            if (log.Contains("[VERSION]"))
            {
                var match = Regex.Match(log, @"\[VERSION\]\s*-\s*(V[\d\.]+)");
                if (match.Success)
                    device.Version = match.Groups[1].Value;
            }
        }

        private void GetRunningState(SirisDeviceManager.Model.Device device, string log)
        {
            var regex = new Regex(@"\[.*\]\s+RaspRun\s+\[RUNNING\]\s+-\s+Running session .*", RegexOptions.IgnoreCase);
            if (regex.IsMatch(log))
                device.IsRunning = true;

            var endedRegex = new Regex(@"\[.*\]\s+RaspRun\s+\[INFO\]\s+-\s+Session .* has ended externally\. Killing playback\.", RegexOptions.IgnoreCase);
            if (endedRegex.IsMatch(log))
                device.IsRunning = false;

            var endRegex2 = new Regex(@"\[INFO\]\s+Session .* has ended\. Stopping (outro|stream) loop\.", RegexOptions.IgnoreCase);
            var endRegex3 = new Regex(@"Stream playback ended early\. Restarting for session .*", RegexOptions.IgnoreCase);
            if (endRegex2.IsMatch(log) || endRegex3.IsMatch(log))
                device.IsRunning = false;

            if (log.Contains("Stream playback ended") || log.Contains("Playback complete"))
                device.IsRunning = false;
        }

        private void GetSessionId(SirisDeviceManager.Model.Device device, string log)
        {
            var regex = new Regex(@"\[.*\]\s+RaspRun\s+\[RUNNING\]\s+-\s+Running session (files|streaming) (\S+)\s+-", RegexOptions.IgnoreCase);
            var match = regex.Match(log);

            if (match.Success)
            {
                device.SessionId = match.Groups[2].Value;
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

            var regexFiles = new Regex(@"\[.*\]\s+RaspRun\s+\[RUNNING\]\s+-\s+Running session files .* - .*", RegexOptions.IgnoreCase);
            var regexStream = new Regex(@"\[.*\]\s+RaspRun\s+\[RUNNING\]\s+-\s+Running session streaming .* - .*", RegexOptions.IgnoreCase);

            if (regexFiles.IsMatch(log))
                device.SessionState = SessionState.SESSION_EXIST_RUNNING_FILES;
            
            else if (regexStream.IsMatch(log))
                device.SessionState = SessionState.SESSION_EXIST_RUNNING_STREAM;
            
            // Caso: [RUNNING] - Starting session
            var regexVLC = new Regex(@"Starting VLC for session .* with .* item\(s\)", RegexOptions.IgnoreCase);
            var regexMPV = new Regex(@"\[INFO\] Stream playback started via MPV for session .*", RegexOptions.IgnoreCase);
            if (regexVLC.IsMatch(log) || regexMPV.IsMatch(log))
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
            var sessionEnded2 = new Regex(@"Stream playback ended early", RegexOptions.IgnoreCase);
            if (sessionEndedRegex.IsMatch(log) || sessionEnded2.IsMatch(log))
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

        private void GetDeviceState(SirisDeviceManager.Model.Device device, string log)
        {
            if (log.Contains("Connection failed") || log.Contains("Connected to"))
                device.IsConnected = false;

            if (log.Contains("[RUNNING]") || log.Contains("[SYNC]") || log.Contains("[SYNC]"))
                device.IsConnected = true;

            if (log.Contains("[INFO]") && !log.Contains("[INFO] Connected to"))
                device.IsConnected = true;
        }

        #endregion
    }
}
