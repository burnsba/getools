﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
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
using Gebug64.Win.Event;
using Gebug64.Win.ViewModels;
using Gebug64.Win.ViewModels.Config;
using Microsoft.Extensions.DependencyInjection;

namespace Gebug64.Win.Windows.Mdi
{
    /// <summary>
    /// MDI Child window for Map.
    /// </summary>
    public partial class MapControl : UserControl, ILayoutWindow, ITransientChild
    {
        private readonly string _typeName;
        private readonly MapWindowViewModel _vm;

        /// <summary>
        /// Initializes a new instance of the <see cref="MapControl"/> class.
        /// </summary>
        /// <param name="vm">Reference to main viewmodel.</param>
        public MapControl(MapWindowViewModel vm)
        {
            _typeName = GetType().FullName!;

            InitializeComponent();

            _vm = vm;

            DataContext = _vm;

            MapRenderControl.NotifyMouseOverGameObject += MapRenderControl_NotifyMouseOverGameObject;
            MapRenderControl.NotifyMouseMoveGamePosition += MapRenderControl_NotifyMouseMoveGamePosition;
            MapRenderControl.NotifyContextMenuGameObject += MapRenderControl_NotifyContextMenuGameObject;
            MapRenderControl.NotifyContextMenuGamePosition += MapRenderControl_NotifyContextMenuGamePosition;

            _vm.BondMoveEvent += _vm_BondMoveEvent;

            var appConfig = (AppConfigViewModel)Workspace.Instance.ServiceProvider.GetService(typeof(AppConfigViewModel))!;

            CheckBg.IsChecked = appConfig.Map.ShowMapLayer[Enum.UiMapLayer.Bg];
            CheckStan.IsChecked = appConfig.Map.ShowMapLayer[Enum.UiMapLayer.Stan];
            CheckBond.IsChecked = appConfig.Map.ShowMapLayer[Enum.UiMapLayer.Bond];
            CheckSetupPad.IsChecked = appConfig.Map.ShowMapLayer[Enum.UiMapLayer.SetupPad];
            CheckSetupPathWaypoint.IsChecked = appConfig.Map.ShowMapLayer[Enum.UiMapLayer.SetupPathWaypoint];
            CheckSetupPatrolPath.IsChecked = appConfig.Map.ShowMapLayer[Enum.UiMapLayer.SetupPatrolPath];
            CheckSetupAlarm.IsChecked = appConfig.Map.ShowMapLayer[Enum.UiMapLayer.SetupAlarm];
            CheckSetupAmmo.IsChecked = appConfig.Map.ShowMapLayer[Enum.UiMapLayer.SetupAmmo];
            CheckSetupAircraft.IsChecked = appConfig.Map.ShowMapLayer[Enum.UiMapLayer.SetupAircraft];
            CheckSetupBodyArmor.IsChecked = appConfig.Map.ShowMapLayer[Enum.UiMapLayer.SetupBodyArmor];
            CheckSetupGuard.IsChecked = appConfig.Map.ShowMapLayer[Enum.UiMapLayer.SetupGuard];
            CheckSetupCctv.IsChecked = appConfig.Map.ShowMapLayer[Enum.UiMapLayer.SetupCctv];
            CheckSetupCollectable.IsChecked = appConfig.Map.ShowMapLayer[Enum.UiMapLayer.SetupCollectable];
            CheckSetupDoor.IsChecked = appConfig.Map.ShowMapLayer[Enum.UiMapLayer.SetupDoor];
            CheckSetupDrone.IsChecked = appConfig.Map.ShowMapLayer[Enum.UiMapLayer.SetupDrone];
            CheckSetupKey.IsChecked = appConfig.Map.ShowMapLayer[Enum.UiMapLayer.SetupKey];
            CheckSetupSafe.IsChecked = appConfig.Map.ShowMapLayer[Enum.UiMapLayer.SetupSafe];
            CheckSetupSingleMontior.IsChecked = appConfig.Map.ShowMapLayer[Enum.UiMapLayer.SetupSingleMontior];
            CheckSetupStandardProp.IsChecked = appConfig.Map.ShowMapLayer[Enum.UiMapLayer.SetupStandardProp];
            CheckSetupTank.IsChecked = appConfig.Map.ShowMapLayer[Enum.UiMapLayer.SetupTank];
            CheckSetupIntro.IsChecked = appConfig.Map.ShowMapLayer[Enum.UiMapLayer.SetupIntro];
        }

        private void _vm_BondMoveEvent(object sender, NotifyBondMoveEventArgs e)
        {
            Dispatcher.BeginInvoke(() =>
            {
                MapRenderControl.PanToGameCoord(e);
            });
        }

        /// <inheritdoc />
        public string TypeName => _typeName;

        private void MapRenderControl_NotifyMouseOverGameObject(object sender, NotifyMouseOverGameObjectEventArgs e)
        {
            _vm.NotifyMouseOverObjectsChanged(e);
        }

        private void MapRenderControl_NotifyMouseMoveGamePosition(object sender, NotifyMouseMoveTranslatedPositionEventArgs e)
        {
            _vm.NotifyMouseMove(e);
        }

        private void MapRenderControl_NotifyContextMenuGameObject(object sender, NotifyMouseOverGameObjectEventArgs e)
        {
            _vm.NotifyContextMenuObjectsChanged(e);
        }

        private void MapRenderControl_NotifyContextMenuGamePosition(object sender, NotifyMouseMoveTranslatedPositionEventArgs e)
        {
            _vm.NotifyContextMenuPosition(e);
        }
    }
}
