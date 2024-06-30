﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gebug64.Win.ViewModels.Map
{
    public class MapViewModel
    {
        public ObservableCollection<MapLayerViewModel> Layers { get; set; } = new ObservableCollection<MapLayerViewModel>();

        public MapViewModel() { }
    }
}
