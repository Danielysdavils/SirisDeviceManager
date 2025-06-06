using SirisDeviceManager.ViewModel.Device.Resume;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace SirisDeviceManager.View.UserControls.Device.Resume
{
    /// <summary>
    /// Interação lógica para DeviceResume.xam
    /// </summary>
    public partial class DeviceResume : UserControl
    {
        private TextBox? _messagesTextBox;

        public DeviceResume()
        {
            InitializeComponent();

            var grid = this.FindName("RootGrid") as Grid;
            if (grid != null)
            {
                // Se o DataContext já estiver definido, conecta imediatamente
                if (grid.DataContext is DeviceResumeViewModel vm)
                {
                    HookVm(vm);
                }

                // Escuta alterações no DataContext do Grid
                DependencyPropertyDescriptor.FromProperty(
                    FrameworkElement.DataContextProperty, typeof(Grid))
                    .AddValueChanged(grid, (s, e) =>
                    {
                        if (grid.DataContext is DeviceResumeViewModel newVm)
                        {
                            HookVm(newVm);
                        }
                    });
            }
        }

        private DeviceResumeViewModel? _currentVm;

        private void HookVm(DeviceResumeViewModel vm)
        {
            // Remove handler anterior, se houver
            if (_currentVm != null)
                _currentVm.PropertyChanged -= Vm_PropertyChanged;

            _currentVm = vm;
            _currentVm.PropertyChanged += Vm_PropertyChanged;
        }

        private void Vm_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(DeviceResumeViewModel.AllMessages))
            {
                Dispatcher.InvokeAsync(() =>
                {
                    _messagesTextBox?.ScrollToEnd();
                }, DispatcherPriority.Background);
            }
        }

        private void MessagesTextBox_Loaded(object sender, RoutedEventArgs e)
        {
            _messagesTextBox = sender as TextBox;
        }
    }
}
