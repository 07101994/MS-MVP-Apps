﻿namespace MVP.App.UWP.Cortana
{
    using System;
    using System.Threading.Tasks;

    using Windows.ApplicationModel.AppService;
    using Windows.ApplicationModel.Background;
    using Windows.ApplicationModel.VoiceCommands;

    public sealed partial class VoiceCommandService : IBackgroundTask
    {
        private VoiceCommandServiceConnection voiceServiceConnection;

        private BackgroundTaskDeferral serviceDeferral;

        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            this.serviceDeferral = taskInstance.GetDeferral();
            taskInstance.Canceled += this.OnTaskCanceled;

            var triggerDetails = taskInstance.TriggerDetails as AppServiceTriggerDetails;

            if (triggerDetails != null && triggerDetails.Name == nameof(VoiceCommandService))
            {
                try
                {
                    this.voiceServiceConnection =
                        VoiceCommandServiceConnection.FromAppServiceTriggerDetails(triggerDetails);

                    this.voiceServiceConnection.VoiceCommandCompleted += this.OnVoiceCommandCompleted;

                    var voiceCommand = await this.voiceServiceConnection.GetVoiceCommandAsync();

                    if (voiceCommand.CommandName.ToLower().Contains("contribution"))
                    {
                        await this.HandleContributionCommand(voiceCommand);
                    }
                    else
                    {
                        await this.LaunchAppForVoiceAsync();
                    }
                }
                catch (Exception ex)
                {
#if DEBUG
                    System.Diagnostics.Debug.WriteLine(ex.Message);
#endif
                }
            }
        }

        private async Task LaunchAppForVoiceAsync()
        {
            var message = new VoiceCommandUserMessage { SpokenMessage = "Opening MVP community app" };
            var response = VoiceCommandResponse.CreateResponse(message);
            response.AppLaunchArgument = string.Empty;

            await this.voiceServiceConnection.RequestAppLaunchAsync(response);
        }

        private void OnVoiceCommandCompleted(VoiceCommandServiceConnection sender, VoiceCommandCompletedEventArgs args)
        {
            this.serviceDeferral?.Complete();
        }

        private void OnTaskCanceled(IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason)
        {
            this.serviceDeferral?.Complete();
        }
    }
}