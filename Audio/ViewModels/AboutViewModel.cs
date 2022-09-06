using Plugin.AudioRecorder;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace Audio.ViewModels
{
    public class AboutViewModel : BaseViewModel
    {
        private readonly AudioRecorderService audioRecorderService = new AudioRecorderService()
        {
            StopRecordingAfterTimeout = false,
            StopRecordingOnSilence = false,
            TotalAudioTimeout = TimeSpan.FromHours(1)
        };

        private readonly AudioPlayer audioPlayer = new AudioPlayer();

        Timer audioTimer;

        string audioText;
        public string AudioText
        {
            get { return audioText; }
            set
            {
                if(audioText != value)
                {
                    audioText = value;
                    OnPropertyChanged();
                }
            }
        }

        Color audioTextColor;
        public Color AudioTextColor
        {
            get { return audioTextColor; }
            set
            {
                if (audioTextColor != value)
                {
                    audioTextColor = value;
                    OnPropertyChanged();
                }
            }
        }


        public AboutViewModel()
        {
            Title = "About";
            AudioText = "Record";
            OpenWebCommand = new Command(async () => await Browser.OpenAsync("https://aka.ms/xamarin-quickstart"));
            AudioCmd = new Command(async () => await UpdateRecording());

            audioTimer = new Timer(GetMicrophoneStream, null, Timeout.Infinite, Timeout.Infinite);
        }

        public ICommand OpenWebCommand { get; }
        public ICommand AudioCmd { get; }

        async Task UpdateRecording()
        {
            try
            {
                if (!audioRecorderService.IsRecording)
                {
                    AudioText = "Record";
                    AudioTextColor = Color.Red;



                    var recordingTask = await audioRecorderService.StartRecording();

                    audioTimer?.Change(1000, 1000);

                    Device.BeginInvokeOnMainThread(async () =>
                    {



                        var audioFile = await recordingTask;

                        if (audioFile != null)
                        {
                            audioTimer?.Change(Timeout.Infinite, Timeout.Infinite);
                            audioPlayer.Play(audioRecorderService.GetAudioFilePath());
                            AudioText = "Stop";
                            AudioTextColor = Color.Green;
                        }
                    });

                }
                else
                {
                    await audioRecorderService.StopRecording();
                }
            }
            catch (Exception ex)
            {

            }
        }

        async void GetMicrophoneStream(object state)
        {
            try
            {
                if (audioRecorderService != null)
                {
                    using (var stream = audioRecorderService.GetAudioFileStream())
                    {
                        if (stream != null && stream.Length > 0)
                        {

                            using (var headerStream = new System.IO.MemoryStream())
                            {
                                await stream.CopyToAsync(headerStream);

                                headerStream.Seek(0, System.IO.SeekOrigin.Begin);

                                //write wav to audio file so it can be played
                                AudioFunctions.WriteWavHeader(headerStream,
                                audioRecorderService.AudioStreamDetails.ChannelCount,
                                audioRecorderService.AudioStreamDetails.SampleRate,
                                audioRecorderService.AudioStreamDetails.BitsPerSample);

                                byte[] buffer = headerStream.ToArray();
                                Console.WriteLine($"audio length = {buffer.Length}");
                            }

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR {ex.Message}");
            }
        }
    }
}