using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using Antlr4.Runtime.Tree.Xpath;
using Gebug64.Unfloader;
using Gebug64.Unfloader.Manage;
using Gebug64.Unfloader.Protocol.Gebug;
using Gebug64.Unfloader.Protocol.Gebug.Message;
using Gebug64.Unfloader.Protocol.Gebug.Message.MessageType;
using Gebug64.Win.Mvvm;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace Gebug64.Win.ViewModels.CategoryTabs
{
    public class ViTabViewModel : TabViewModelBase, ICategoryTabViewModel
    {
        private const string _tabName = "Vi";

        private string _framebufferGrabSavePath;

        public ICommand GetFrameBufferCommand { get; set; }

        public ICommand SetSaveFrameBufferPathCommand { get; set; }

        public ImageSource FrameBufferGrab { get; set; }

        public bool CanSendGetFrameBuffer
        {
            get
            {
                IConnectionServiceProvider? connectionServiceProvider = _connectionServiceProviderResolver.GetDeviceManager();
                if (object.ReferenceEquals(null, connectionServiceProvider))
                {
                    return false;
                }

                return !connectionServiceProvider.IsShutdown;
            }
        }

        public bool CanSetSaveFrameBufferPath => true; // always

        public string FramebufferGrabSavePath
        {
            get => _framebufferGrabSavePath;
            set
            {
                if (_framebufferGrabSavePath == value)
                {
                    return;
                }

                _framebufferGrabSavePath = value;
                OnPropertyChanged(nameof(FramebufferGrabSavePath));

                _appConfig.FramebufferGrabSavePath = value;

                SaveAppSettings();
            }
        }

        public ViTabViewModel(ILogger logger, IConnectionServiceProviderResolver connectionServiceProviderResolver)
            : base(_tabName, logger, connectionServiceProviderResolver)
        {
            GetFrameBufferCommand = new CommandHandler(GetFrameBufferCommandHandler, () => CanSendGetFrameBuffer);
            SetSaveFrameBufferPathCommand = new CommandHandler(SetSaveFrameBufferPathCommandHandler, () => CanSetSaveFrameBufferPath);

            DisplayOrder = 80;

            _framebufferGrabSavePath = _appConfig.FramebufferGrabSavePath;
            if (string.IsNullOrEmpty(_framebufferGrabSavePath))
            {
                _framebufferGrabSavePath = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
            }
        }

        public void GetFrameBufferCommandHandler()
        {
            IConnectionServiceProvider? connectionServiceProvider = _connectionServiceProviderResolver.GetDeviceManager();

            if (object.ReferenceEquals(null, connectionServiceProvider))
            {
                return;
            }

            connectionServiceProvider.Subscribe(PreviewGetFramebufferCallback, 1, PreviewGetFramebufferCallbackFilter);

            var msg = new GebugViFramebufferMessage();

            _logger.Log(LogLevel.Information, "Send: " + msg.ToString());

            connectionServiceProvider.SendMessage(msg);
        }

        private void PreviewGetFramebufferCallback(IGebugMessage msg)
        {
            // this is not null because this method should only be called after
            // PreviewGetFramebufferCallbackFilter succeeds.
            var viMessage = (GebugViFramebufferMessage)msg!;

            _dispatcher.BeginInvoke(() =>
            {
                var windowsFrameBuffer = Image.Utility.GeRgba5551ToWindowsArgb1555(viMessage.Data, viMessage.Width, viMessage.Height);

                var bmp = Image.Utility.BitmapFromRaw(windowsFrameBuffer, System.Drawing.Imaging.PixelFormat.Format16bppArgb1555, viMessage.Width, viMessage.Height);

                string filename = $"framegrab-{DateTime.Now.ToString("yyyyMMdd-HHmmss-fff")}.jpg";
                string savePath = System.IO.Path.Combine(FramebufferGrabSavePath, filename);
                bmp.Save(savePath, ImageFormat.Jpeg);

                Task.Run(() => _logger.Log(LogLevel.Information, $"Save frame buffer image: {savePath}"));

                // The following assignment has to occur on the dispatcher thread.
                // Be careful with one of the relevant streams being closed before the following completes ...
                FrameBufferGrab = Image.Utility.BitmapToImageSource(bmp);
                OnPropertyChanged(nameof(FrameBufferGrab));
            });
        }

        private bool PreviewGetFramebufferCallbackFilter(IGebugMessage msg)
        {
            if (msg.Category == GebugMessageCategory.Vi
                && msg.Command == (int)GebugCmdVi.GrabFramebuffer)
            {
                // Could also check the AckId matches the original request Id, but
                // probably safe to ignore that.
                return true;
            }

            return false;
        }

        private void SetSaveFrameBufferPathCommandHandler()
        {
            // TODO: Use the new folder open dialog in .net 8.
            CommonOpenFileDialog dialog = new CommonOpenFileDialog();

            string startDir = FramebufferGrabSavePath;

            if (string.IsNullOrEmpty(startDir) || !System.IO.Directory.Exists(startDir))
            {
                startDir = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
            }

            dialog.InitialDirectory = startDir;
            dialog.IsFolderPicker = true;

            if (dialog.ShowDialog() == CommonFileDialogResult.Ok && !string.IsNullOrEmpty(dialog.FileName))
            {
                FramebufferGrabSavePath = dialog.FileName!;
            }
        }
    }
}
