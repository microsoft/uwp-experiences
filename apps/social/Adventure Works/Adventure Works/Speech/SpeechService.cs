using Adventure_Works.CognitiveServices;
using Microsoft.Toolkit.Uwp.UI;
using Microsoft.Toolkit.Uwp.UI.Animations;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Media.Capture;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.Media.SpeechRecognition;
using Windows.Media.SpeechSynthesis;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace Adventure_Works.Speech
{
    public class SpeechService
    {
        private SpeechRecognizer _speechRecognizer;
        private SpeechRecognizer _continousSpeechRecognizer;
        private SpeechSynthesizer _speechSynthesizer;

        private bool _speechInit = false;
        private bool _listening = false;

        private DispatcherTimer _keyTimer;
        private bool _gamepadViewDown = false;

        private MediaPlayer _player;

        private AdventureWorksAideView _view;

        private static SpeechService _instance;
        public static SpeechService Instance => _instance ?? (_instance = new SpeechService());

        private SpeechService()
        {

        }

        public async Task Initialize()
        {
            if (_speechInit == true || !(await CheckForMicrophonePermission()))
                return;

            _continousSpeechRecognizer = new SpeechRecognizer();
            _continousSpeechRecognizer.Constraints.Add(new SpeechRecognitionListConstraint(new List<String>() { "Adventure Works" }, "start"));
            var result = await _continousSpeechRecognizer.CompileConstraintsAsync();

            if (result.Status != SpeechRecognitionResultStatus.Success)
            {
                return;
            }

            _speechRecognizer = new SpeechRecognizer();
            result = await _speechRecognizer.CompileConstraintsAsync();
            _speechRecognizer.HypothesisGenerated += _speechRecognizer_HypothesisGenerated;

            if (result.Status != SpeechRecognitionResultStatus.Success)
            {
                return;
            }

            Window.Current.CoreWindow.KeyDown += CoreWindow_KeyDown;
            Window.Current.CoreWindow.KeyUp += CoreWindow_KeyUp;

            _continousSpeechRecognizer.ContinuousRecognitionSession.ResultGenerated += ContinuousRecognitionSession_ResultGenerated;
            await _continousSpeechRecognizer.ContinuousRecognitionSession.StartAsync(SpeechContinuousRecognitionMode.Default);

            var frame = (Window.Current.Content as Frame);
            if (frame != null)
            {
                frame.Navigating += Frame_Navigating;
                frame.Navigated += Frame_Navigated;
            }

            _keyTimer = new DispatcherTimer();
            _keyTimer.Interval = TimeSpan.FromSeconds(1);
            _keyTimer.Tick += _keyTimer_Tick;

            _speechInit = true;
        }

        private void _keyTimer_Tick(object sender, object e)
        {
            _keyTimer.Stop();
            if (!_listening)
            {
                WakeUpAndListen();
            }
        }

        private void CoreWindow_KeyDown(Windows.UI.Core.CoreWindow sender, Windows.UI.Core.KeyEventArgs args)
        {
            if (!_listening && !_gamepadViewDown && args.VirtualKey == Windows.System.VirtualKey.GamepadView)
            {
                _gamepadViewDown = true;
                _keyTimer.Start();
            }
        }


        private void CoreWindow_KeyUp(Windows.UI.Core.CoreWindow sender, Windows.UI.Core.KeyEventArgs args)
        {
            if (!_listening && args.VirtualKey == Windows.System.VirtualKey.Q)
            {
                if (Window.Current.CoreWindow.GetKeyState(Windows.System.VirtualKey.Control).HasFlag(CoreVirtualKeyStates.Down))
                {
                    WakeUpAndListen();
                }
            }

            if (args.VirtualKey == Windows.System.VirtualKey.GamepadView)
            {
                _gamepadViewDown = false;
                _keyTimer.Stop();
            }
        }

        
        private void ContinuousRecognitionSession_ResultGenerated(SpeechContinuousRecognitionSession sender, SpeechContinuousRecognitionResultGeneratedEventArgs args)
        {
            if (args.Result.Confidence == SpeechRecognitionConfidence.High || args.Result.Confidence == SpeechRecognitionConfidence.Medium)
            {
                Helpers.RunOnCoreDispatcherIfPossible( () => WakeUpAndListen(), false);
            }
        }

        private void _speechRecognizer_HypothesisGenerated(SpeechRecognizer sender, SpeechRecognitionHypothesisGeneratedEventArgs args)
        {
            if (_view != null)
            {
                Helpers.RunOnCoreDispatcherIfPossible(() => _view.Text = args.Hypothesis.Text.ToLower(), false);
            }
        }


        private async Task WakeUpAndListen()
        {
            _listening = true;
            try
            {
                await _continousSpeechRecognizer.ContinuousRecognitionSession.CancelAsync();
            }
            catch (Exception ex)
            {

            }
            ShowUI();
            await SpeakAsync("hey!");

            int retry = 3;

            while (true)
            {
                _view.State = AdventureWorksAideState.Listening;
                _view.Text = "";

                var spokenText = await ListenForText();
                if (string.IsNullOrWhiteSpace(spokenText) ||
                    spokenText.ToLower().Contains("cancel") ||
                    spokenText.ToLower().Contains("never mind"))
                {
                    break;
                }
                else
                {
                    _view.State = AdventureWorksAideState.Thinking;
                    _view.Text = $"\"{spokenText.ToLower()}\"";
                    var state = await LUISAPI.Instance.HandleIntent(spokenText);
                    if (!state.Success)
                    {
                        _view.Text = "don't know that yet";
                        await Task.Delay(1000);

                        if (--retry < 1)
                            break;
                    }
                    else
                    {
                        _view.State = AdventureWorksAideState.Speaking;
                        if (!string.IsNullOrWhiteSpace(state.TextResponse))
                        {
                            _view.Text = state.TextResponse;
                        }
                        else
                        {
                            _view.Text = "check this out";
                        }

                        if (!string.IsNullOrWhiteSpace(state.SpeechRespose))
                        {
                            await SpeakAsync(state.SpeechRespose);
                        }
                        else
                        {
                            await Task.Delay(1000);
                        }

                        retry = 3;

                    }
                }
            }

            await _continousSpeechRecognizer.ContinuousRecognitionSession.StartAsync();
            await HideUI();
            _listening = false;
        }

        private async Task<string> ListenForText()
        {
            string result = null;

            try
            {
                SpeechRecognitionResult speechRecognitionResult = await _speechRecognizer.RecognizeAsync();
                if (speechRecognitionResult.Status == SpeechRecognitionResultStatus.Success)
                {
                    result = speechRecognitionResult.Text;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            return result;
        }

        private async Task SpeakAsync(string toSpeak)
        {
            if (_speechSynthesizer == null)
            {
                _speechSynthesizer = new SpeechSynthesizer();
                var voice = SpeechSynthesizer.AllVoices.Where(v => v.Gender == VoiceGender.Female && v.Language.Contains("en")).FirstOrDefault();
                if (voice != null)
                {
                    _speechSynthesizer.Voice = voice;
                }
            }
            var syntStream = await _speechSynthesizer.SynthesizeTextToStreamAsync(toSpeak);

            if (_player == null)
            {
                _player = new MediaPlayer();
            }

            var taskSource = new TaskCompletionSource<object>();
            TypedEventHandler<MediaPlayer, object> mediaEnded = null;
            mediaEnded += (s, e) =>
            {
                _player.MediaEnded -= mediaEnded;
                taskSource.SetResult(null);
            };

            _player.MediaEnded += mediaEnded;
            _player.Source = MediaSource.CreateFromStream(syntStream, syntStream.ContentType);
            _player.Play();

            await taskSource.Task;
        }


        private AdventureWorksAideView CreateUI()
        {
            if (_view != null)
            {
                return _view;
            }

            _view = new AdventureWorksAideView()
            {
                VerticalAlignment = VerticalAlignment.Top,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(20),
                Opacity = 0,
            };

            return _view;
        }

        private async Task ShowUI()
        {
            var currentPage = Window.Current.Content as Frame;
            if (currentPage == null)
            {
                return;
            }

            var root = currentPage.FindDescendant<Grid>();
            if (root == null)
            {
                return;
            }

            CreateUI();

            _view.Text = "";

            try
            {
                root.Children.Add(_view);
            }
            catch (Exception)
            {
                _view = null;
                CreateUI();
                root.Children.Add(_view);
            }
            await _view.Show();
        }

        private async Task HideUI()
        {
            var currentPage = Window.Current.Content as Frame;
            if (currentPage == null)
            {
                return;
            }

            var root = currentPage.FindDescendant<Grid>();
            if (root == null || !root.Children.Contains(_view))
            {
                return;
            }

            await _view.Hide();

            root.Children.Remove(_view);
        }

        private async void Frame_Navigated(object sender, Windows.UI.Xaml.Navigation.NavigationEventArgs e)
        {
            await Task.Delay(500);
            if (_listening)
            {
                var root = (sender as Frame).FindDescendant<Grid>();
                if (root != null && !root.Children.Contains(_view))
                {
                    root.Children.Add(_view);
                }
            }
        }

        private void Frame_Navigating(object sender, Windows.UI.Xaml.Navigation.NavigatingCancelEventArgs e)
        {
            if (_listening)
            {
                var root = (sender as Frame).FindDescendant<Grid>();
                if (root != null && root.Children.Contains(_view))
                {
                    root.Children.Remove(_view);
                }
            }
        }

        private async Task<bool> CheckForMicrophonePermission()
        {
            try
            {
                // Request access to the microphone 
                MediaCaptureInitializationSettings settings = new MediaCaptureInitializationSettings();
                settings.StreamingCaptureMode = StreamingCaptureMode.Audio;
                settings.MediaCategory = MediaCategory.Speech;
                MediaCapture capture = new MediaCapture();

                await capture.InitializeAsync(settings);
            }
            catch (UnauthorizedAccessException)
            {
                // The user has turned off access to the microphone. If this occurs, we should show an error, or disable
                // functionality within the app to ensure that further exceptions aren't generated when 
                // recognition is attempted.
                return false;
            }
            
            return true;
        }
    }
}
