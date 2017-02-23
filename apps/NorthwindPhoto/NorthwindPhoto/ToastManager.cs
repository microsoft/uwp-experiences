// ******************************************************************
// Copyright (c) Microsoft. All rights reserved.
// This code is licensed under the MIT License (MIT).
// THE CODE IS PROVIDED “AS IS”, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
// INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
// IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM,
// DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
// TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH
// THE CODE OR THE USE OR OTHER DEALINGS IN THE CODE.
// ******************************************************************

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Data.Xml.Dom;
using Windows.Foundation.Metadata;
using Windows.UI.Notifications;
using Microsoft.Toolkit.Uwp.Notifications;

namespace NorthwindPhoto
{
    public static class ToastManager
    {
        private static string s_tag;
        private static readonly ToastNotifier s_toastNotifier;

        static ToastManager()
        {
            s_toastNotifier = ToastNotificationManager.CreateToastNotifier();
        }

        public static async Task CreateProgressToastAsync(string title, string status)
        {
            if (!ApiInformation.IsApiContractPresent("Windows.Foundation.UniversalApiContract", 4))
                return;

            // TODO: 8 Toast notificiation with binding
            var xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(
                File.ReadAllText(Path.Combine(Package.Current.InstalledLocation.Path, @"Assets\Toasts\Upload.xml")));

            // Dictionary with all the elements to databind.
            var data = new Dictionary<string, string>
            {
                {"title", title},
                {"status", status},
                {"progressValue", "0"},
                {"progressValueStringOverride", "Calculating..."}
            };

            // Unique tag to reference the toast by
            s_tag = Guid.NewGuid().ToString();

            var toastNotification = new ToastNotification(xmlDocument)
            {
                Tag = s_tag,
                Data = new NotificationData(data)
            };

            s_toastNotifier.Show(toastNotification);

            await BeginUploadImage(title);
        }

        private static async Task BeginUploadImage(string title)
        {
            double progress = 0;
            while (progress < 1)
            {
                var uploadProgress = await UploadImageChunk();
                progress = uploadProgress.Item1;

                // Update the data, just the parts that have changed.
                var data = new Dictionary<string, string>
                {
                    {"progressValue", progress.ToString()},
                    {"progressValueStringOverride", $"{uploadProgress.Item2} seconds"}
                };

                // Using the unique tag we created to identify toast.
                s_toastNotifier.Update(new NotificationData(data), s_tag);
            }

            SendCompletedToast(title);
        }

        /// <summary>
        /// Completed toast. Instead of using XAML you can use strongly typed objects
        /// </summary>
        private static void SendCompletedToast(string title)
        {
            var toastContent = new ToastContent
            {
                Visual = new ToastVisual
                {
                    BindingGeneric = new ToastBindingGeneric
                    {
                        Children =
                        {
                            new AdaptiveText
                            {
                                Text = "Upload completed!"
                            },
                            new AdaptiveText
                            {
                                Text = title
                            }
                        }
                    }
                }
            };

            var notification = new ToastNotification(toastContent.GetXml())
            {
                Tag = s_tag
            };

            s_toastNotifier.Show(notification);
        }

        #region upload helper

        private static async Task<Tuple<double, int>> UploadImageChunk()
        {
            if (count > 5)
                count = 0;

            await Task.Delay(1000);

            var progress = 100d / 5 * count / 100;

            var tuple = new Tuple<double, int>(progress, 5 - count);
            count += 1;
            return tuple;
        }

        private static int count;

        #endregion
    }
}