﻿using System;
using System.Configuration;
using System.Windows;
using System.Windows.Controls;
using GGXrdWakeupDPUtil.Library;

namespace GGXrdWakeupDPUtil
{
    /// <summary>
    /// Logique d'interaction pour Window1.xaml
    /// </summary>
    public partial class Window1
    {
        public Window1()
        {
            InitializeComponent();

            InputTextBox.TextChanged += inputTextBox_TextChanged;
        }

        private readonly ReversalTool _reversalTool = new ReversalTool();
        private readonly UpdateManager _updateManager = new UpdateManager();
        private readonly bool _autoUpdate = ConfigurationManager.AppSettings.Get("AutoUpdate") == "1";

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _reversalTool.DummyChanged += _reversalTool_DummyChanged;
            _reversalTool.ReversalLoopErrorOccured += _reversalTool_ReversalLoopErrorOccured;
            _reversalTool.RandomBurstlLoopErrorOccured += _reversalTool_RandomBurstlLoopErrorOccured;
            LogManager.Instance.LineReceived += LogManager_LineReceived;




            if (_autoUpdate)
            {
                UpdateProcess();
            }

            try
            {
                _reversalTool.AttachToProcess();
            }
            catch (Exception exception)
            {
                string message =
                    $"Guilty Gear not found open!  Remember, be in training mode paused when you open this program.  This program will now close.";
                LogManager.Instance.WriteLine(message);
                MessageBox.Show($"{message}{Environment.NewLine}{exception.Message}");
                Application.Current.Shutdown();
                return;
            }



            RefreshBurstInfo();


        }




        private void Window_Closed(object sender, EventArgs e)
        {
            _reversalTool?.Dispose();
        }

        #region Reversal tool events
        private void _reversalTool_ReversalLoopErrorOccured(Exception ex)
        {
            StopReversal();
        }

        private void _reversalTool_DummyChanged(NameWakeupData dummy)
        {
            SetDummyName(dummy.CharName);
        }
        #endregion


        #region Reversal

        private void enableButton_Click(object sender, RoutedEventArgs e)
        {
            int slotNumber = 0;

            if (Slot1R.IsChecked != null && Slot1R.IsChecked.Value)
            {
                slotNumber = 1;
            }
            else if (Slot2R.IsChecked != null && Slot2R.IsChecked.Value)
            {
                slotNumber = 2;
            }
            else if (Slot3R.IsChecked != null && Slot3R.IsChecked.Value)
            {
                slotNumber = 3;
            }

            var slotInput = _reversalTool.SetInputInSlot(slotNumber, InputTextBox.Text);



            _reversalTool.StartReversalLoop(slotInput);

            EnableButton.IsEnabled = false;
            DisableButton.IsEnabled = true;
            InputTextBox.IsEnabled = false;
            BurstTabItem.IsEnabled = false;

            Slot1R.IsEnabled = false;
            Slot2R.IsEnabled = false;
            Slot3R.IsEnabled = false;
        }

        private void disableButton_Click(object sender, RoutedEventArgs e)
        {
            StopReversal();
        }



        private void inputTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            //TODO add watermark
            CheckValidInput();
        }

        private void CheckValidInput()
        {
            var text = InputTextBox.Text;

            if (string.IsNullOrEmpty(text))
            {
                ErrorTextBlock.Visibility = Visibility.Hidden;
                EnableButton.IsEnabled = false;
            }



            bool validInput = _reversalTool != null && _reversalTool.CheckValidInput(text);

            if (validInput)
            {
                ErrorTextBlock.Visibility = Visibility.Hidden;
                EnableButton.IsEnabled = true;
            }
            else
            {
                ErrorTextBlock.Visibility = Visibility.Visible;
                ErrorTextBlock.Text = "Invalid Input";
                EnableButton.IsEnabled = false;
            }
        }



        private void SetDummyName(string dummyName)
        {
            Dispatcher.Invoke(() =>
            {
                DummyTextBlock.Text = $"Current Dummy: {dummyName}";
            });
        }

        private void StopReversal()
        {
            Dispatcher.Invoke(() =>
            {
                EnableButton.IsEnabled = true;
                DisableButton.IsEnabled = false;
                InputTextBox.IsEnabled = true;
                BurstTabItem.IsEnabled = true;

                Slot1R.IsEnabled = true;
                Slot2R.IsEnabled = true;
                Slot3R.IsEnabled = true;
            });

            _reversalTool.StopReversalLoop();

        }
        #endregion

        #region Burst
        private void enableBurstButton_Click(object sender, RoutedEventArgs e)
        {
            int slotNumber = 0;

            if (Slot1RBurst.IsChecked != null && Slot1RBurst.IsChecked.Value)
            {
                slotNumber = 1;
            }
            else if (Slot2RBurst.IsChecked != null && Slot2RBurst.IsChecked.Value)
            {
                slotNumber = 2;
            }
            else if (Slot3RBurst.IsChecked != null && Slot3RBurst.IsChecked.Value)
            {
                slotNumber = 3;
            }

            int min = NumericUpDownMinBurst.Value;
            int max = NumericUpDownMaxBurst.Value;
            int burstPercentage = (int)BurstSlider.Value;

            _reversalTool.StartRandomBurstLoop(min, max, slotNumber, burstPercentage);



            EnableBurstButton.IsEnabled = false;
            DisableBurstButton.IsEnabled = true;
            NumericUpDownMinBurst.IsEnabled = false;
            NumericUpDownMaxBurst.IsEnabled = false;
            BurstSlider.IsEnabled = false;
            ReversalTabItem.IsEnabled = false;

            Slot1RBurst.IsEnabled = false;
            Slot2RBurst.IsEnabled = false;
            Slot3RBurst.IsEnabled = false;


        }

        private void disableBurstButton_Click(object sender, RoutedEventArgs e)
        {
            StopBurst();
        }
        private void StopBurst()
        {
            Dispatcher.Invoke(() =>
            {
                EnableBurstButton.IsEnabled = true;
                DisableBurstButton.IsEnabled = false;
                NumericUpDownMinBurst.IsEnabled = true;
                NumericUpDownMaxBurst.IsEnabled = true;
                BurstSlider.IsEnabled = true;
                ReversalTabItem.IsEnabled = true;

                Slot1RBurst.IsEnabled = true;
                Slot2RBurst.IsEnabled = true;
                Slot3RBurst.IsEnabled = true;
            });

            _reversalTool.StopRandomBurstLoop(); ;

        }
        private void AppendLog(string message)
        {
            Dispatcher.Invoke(() =>
            {
                LogTextBox.AppendText($"{message}{Environment.NewLine}");
            });
        }
        private void NumericUpDownMinBurst_ValueChanged(object sender, RoutedEventArgs e)
        {
            if (NumericUpDownMinBurst != null && NumericUpDownMaxBurst != null)
            {
                NumericUpDownMaxBurst.Minimum = NumericUpDownMinBurst.Value;
            }


            RefreshBurstInfo();
        }
        private void NumericUpDownMaxBurst_ValueChanged(object sender, RoutedEventArgs e)
        {
            if (NumericUpDownMinBurst != null && NumericUpDownMaxBurst != null)
            {
                NumericUpDownMinBurst.Maximum = NumericUpDownMaxBurst.Value;
            }

            RefreshBurstInfo();
        }
        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            RefreshBurstInfo();
        }


        private void RefreshBurstInfo()
        {

            if (NumericUpDownMinBurst != null && NumericUpDownMaxBurst != null && BurstSlider != null)
            {
                int min = NumericUpDownMinBurst.Value;
                int max = NumericUpDownMaxBurst.Value;
                int burstPercentage = (int)BurstSlider.Value;

                string text;

                if (burstPercentage == 100)
                {
                    text = $"The dummy will burst randomly between {min} and {max} hit combo";
                }
                else
                {
                    text = min == max ?
                        $"- The dummy will burst randomly at {min} hit combo ({burstPercentage}% chance)" :
                        $"- The dummy will burst randomly between {min} and {max} hit combo ({burstPercentage}% chance)";
                    text += Environment.NewLine;
                    text += $"- The dummy won't burst at all ({100 - burstPercentage}% chance)";
                }

                Dispatcher.Invoke(() =>
                {
                    BurstInfoTextBlock.Text = text;
                    BurstPercentageLabel.Content = $"{burstPercentage}%";
                });
            }
        }

        private void _reversalTool_RandomBurstlLoopErrorOccured(Exception ex)
        {
            StopBurst();
        }
        private void LogManager_LineReceived(object sender, string e)
        {
            AppendLog(e);
        }



        #endregion

        #region Menu
        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            UpdateProcess(confirm: true);
        }

        #endregion

        private void UpdateProcess(bool confirm = false)
        {
            string currentVersion = ConfigurationManager.AppSettings.Get("CurrentVersion");

            LogManager.Instance.WriteLine($"Current Version is {currentVersion}");
            try
            {
                this._updateManager.CleanOldFiles();
                var latestVersion = this._updateManager.CheckUpdates();

                if (latestVersion != null)
                {
                    if (!confirm || MessageBox.Show("A new version is available\r\bDo you want do download it?", "New version available", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                    {
                        LogManager.Instance.WriteLine($"Found new version : v{latestVersion.Version}");
                        bool downloadSuccess = this._updateManager.DownloadUpdate(latestVersion);

                        if (downloadSuccess)
                        {
                            bool installSuccess = this._updateManager.InstallUpdate();

                            if (installSuccess)
                            {
                                this._updateManager.SaveVersion(latestVersion.Version);
                                this._updateManager.RestartApplication();
                            }
                        }
                    }

                }
                else
                {
                    LogManager.Instance.WriteLine("No updates");

                    if (confirm)
                    {
                        MessageBox.Show("Your version is up to date");
                    }
                }
            }
            catch (Exception ex)
            {
                LogManager.Instance.WriteException(ex);
            }
        }
    }
}
