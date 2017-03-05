﻿namespace MVP.App.UWP.Cortana
{
    using System;
    using System.Threading.Tasks;

    using Windows.ApplicationModel.VoiceCommands;

    public sealed partial class VoiceCommandService
    {
        private async Task ReportResultAsync(VoiceReportResult result, string message)
        {
            var progress = new VoiceCommandUserMessage();
            progress.DisplayMessage = progress.SpokenMessage = message;

            var response = VoiceCommandResponse.CreateResponse(progress);

            switch (result)
            {
                case VoiceReportResult.Progress:
                    await this.voiceServiceConnection.ReportProgressAsync(response);
                    break;
                case VoiceReportResult.Fail:
                    await this.voiceServiceConnection.ReportFailureAsync(response);
                    break;
                case VoiceReportResult.Success:
                    await this.voiceServiceConnection.ReportSuccessAsync(response);
                    break;
            }
        }
    }
}