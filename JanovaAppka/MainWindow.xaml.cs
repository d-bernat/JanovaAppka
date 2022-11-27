using System;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Threading;
using Flurl.Http;
using Timer = System.Timers.Timer;

namespace JanovaAppka
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    // ReSharper disable once RedundantExtendsListEntry
    public partial class MainWindow : Window
    {
        private bool Hit { get; set; }

        private Timer Timer { get; set; }

        private int Counter { get; set; }
        private int TotalAmount { get; set; }

        private Random _random = new Random();

        private string[] _badMessages = new[]
        {
            "Zatiaľ nič ...",
            "Trt s makom ...",
            "Vôbec nič ...",
            "Ale že totálna ničota ...",
            "Celé zle, discusy nie sa ..."
        };
        public MainWindow()
        {
            InitializeComponent();

            MainButton.Content = "Nahodiť udičku";
        }

        private void OnClick(object sender, RoutedEventArgs e)
        {
            Hit = !Hit;

            if (Hit)
            {
                SetTimer();

            }
            else
            {
                Timer.Stop();
                Timer.Dispose();
                TimeToRun.Text = string.Empty;
                Total.Text = "Počet pokusov: 0";
                Counter = 0;
                TotalAmount = 0;
                Warning.Visibility = Visibility.Hidden;
            }


            MainButton.Content = Hit ? "Koniec lovu" : "Nahodiť udičku";
        }

        private async Task<bool> GetResponse()
        {
            Timer.Stop();
            var responsePowered =
                await "https://www.segelflug.de/osclass/index.php?page=search&sCategory=153".GetStringAsync();

            var response18 =
                await "https://www.segelflug.de/osclass/index.php?page=search&sCategory=139".GetStringAsync();
            Timer.Start();
            return responsePowered.Contains("discus 2ct", StringComparison.CurrentCultureIgnoreCase)
                || response18.Contains("discus 2ct", StringComparison.CurrentCultureIgnoreCase);
            
        }

        private void SetTimer()
        {
            // Create a timer with a two second interval.
            Timer = new Timer(1000);
            // Hook up the Elapsed event for the timer. 
            Timer.Elapsed += OnTimedEvent;
            Timer.AutoReset = true;
            Timer.Enabled = true;
        }

        private void OnTimedEvent(Object source, ElapsedEventArgs e)
        {
            if (Counter < 30)
            {
                ++Counter;
                TimeToRun.Dispatcher.Invoke(DispatcherPriority.Send,
                    new UpdateTextHandler(UpdateTimeToRun), "Ďalší pokus v sekundách: " + (30 - Counter));

            }
            else
            {
                try
                {
                    var result = GetResponse().Result;
                    
                    Status.Dispatcher.Invoke(DispatcherPriority.Send,
                        new UpdateTextHandler(UpdateStatus), "Status: ok");

                    Warning.Dispatcher.Invoke(DispatcherPriority.Send,
                        new UpdateWarningVisibilityHandler(UpdateWarningVisibility), result);

                    Warning.Dispatcher.Invoke(DispatcherPriority.Send,
                        new UpdateNothingTextHandler(UpdateNothingText), _badMessages[GetRandomNumber()]);

                    Warning.Dispatcher.Invoke(DispatcherPriority.Send,
                        new UpdateNothingVisibilityHandler(UpdateNothingVisibility), !result);

                    if (result)
                    {
                        Dispatcher.Invoke(DispatcherPriority.Send, new RestoreWindowHandler(Restore));
                    }
                }
                catch
                {
                    Timer.Start();

                    Status.Dispatcher.Invoke(DispatcherPriority.Send,
                        new UpdateTextHandler(UpdateStatus), "Status: chyba");
                    Status.Dispatcher.Invoke(DispatcherPriority.Send,
                        new UpdateWarningVisibilityHandler(UpdateWarningVisibility), false);
                    Status.Dispatcher.Invoke(DispatcherPriority.Send,
                        new UpdateNothingTextHandler(UpdateNothingText), "Asi problém s internetom, skús www.segelflug.de v prehliadači...");
                    Status.Dispatcher.Invoke(DispatcherPriority.Send,
                        new UpdateNothingVisibilityHandler(UpdateNothingVisibility), true);
                }


                TimeToRun.Dispatcher.Invoke(DispatcherPriority.Send,
                    new UpdateTextHandler(UpdateTimeToRun), "Ďalší pokus v sekundách: 0");

                Counter = 0;
                Total.Dispatcher.Invoke(DispatcherPriority.Send,
                    new UpdateTextHandler(UpdateTotal), "Počet pokusov: " + ++TotalAmount);
            }


        }

        private delegate void UpdateTextHandler(string updatedText);

        private delegate void UpdateWarningVisibilityHandler(bool visibility);

        private delegate void UpdateNothingVisibilityHandler(bool visibility);

        private delegate void UpdateNothingTextHandler(string text);

        private delegate void RestoreWindowHandler();

        private void UpdateTimeToRun(string updatedText)

        {

            TimeToRun.Text = updatedText;

        }

        private void UpdateTotal(string updatedText)

        {

            Total.Text = updatedText;

        }

        private void UpdateStatus(string updatedText)

        {

            Status.Text = updatedText;

        }

        private void UpdateWarningVisibility(bool visible)
        {
            Warning.Visibility = visible ? Visibility.Visible : Visibility.Hidden;
        }

        private void UpdateNothingVisibility(bool visible)
        {
            Nothing.Visibility = visible ? Visibility.Visible : Visibility.Hidden;
        }

        private void UpdateNothingText(string text)
        {
            Nothing.Text = text;
        }


        private void Restore()
        {
            this.WindowState = WindowState.Normal;
        }
        private int GetRandomNumber() => _random.Next(0, 4);

    }
}
