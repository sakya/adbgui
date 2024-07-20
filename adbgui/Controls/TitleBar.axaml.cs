using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using System;
using Avalonia.Reactive;

namespace adbgui.Controls
{
    public partial class TitleBar : UserControl
    {
        private bool _canGoBack;

        public TitleBar()
        {
            InitializeComponent();

            IsVisible = Environment.OSVersion.Platform == PlatformID.Win32NT;
            CanMinimize = true;
            CanMaximize = true;
        }

        public bool CanGoBack {
            get => _canGoBack;
            set
            {
                _canGoBack = value;
                BackBtn.IsVisible = _canGoBack;
                Icon.IsVisible = !_canGoBack;
            }
        }

        public bool CanMinimize { get; set; }
        public bool CanMaximize { get; set; }

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);

            var pw = this.VisualRoot as Window;
            if (pw != null) {
                SetTitle(pw.Title);
                var title = pw.GetObservable(Window.TitleProperty);
                title.Subscribe(new AnonymousObserver<string?>(SetTitle));

                var wState = pw.GetObservable(Window.WindowStateProperty);
                wState.Subscribe(new AnonymousObserver<WindowState>(s =>
                {
                    if (s == WindowState.Maximized) {
                        pw.Padding = new Thickness(5);
                        MaximizeBtn.Content = new Projektanker.Icons.Avalonia.Icon() { Value = "fas fa-window-restore" };
                    } else {
                        pw.Padding = new Thickness(0);
                        MaximizeBtn.Content = new Projektanker.Icons.Avalonia.Icon() { Value = "fas fa-window-maximize" };
                    }
                }));
            }

            MinimizeBtn.Click += (e, a) =>
            {
                ((Window)this.VisualRoot!).WindowState = WindowState.Minimized;
            };
            MinimizeBtn.IsVisible = CanMinimize;

            MaximizeBtn.Click += (s, a) =>
            {
                if (this.VisualRoot is Window vr)
                    vr.WindowState = vr.WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
            };
            MaximizeBtn.IsVisible = CanMinimize;

            CloseBtn.Click += (s, a) =>
            {
                ((Window)this.VisualRoot!).Close();
            };

            BackBtn.Click += async (s, a) =>
            {
                await (pw as MainWindow)!.NavigateBack();
            };
        }

        private void SetTitle(string? title)
        {
            Title.Text = title;
        }
    }
}
