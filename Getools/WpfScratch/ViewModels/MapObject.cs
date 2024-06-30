using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WpfScratch.Mvvm;

namespace WpfScratch.ViewModels
{
    public abstract class MapObject : ViewModelBase
    {
        public double UiX { get; private set; }
        public double UiY { get; private set; }
        public double UiWidth { get; set; }
        public double UiHeight { get; set; }

        public bool IsVisible { get; set; }

        public void SetUiX(double d)
        {
            UiX = d;
            OnPropertyChanged(nameof(UiX));
        }

        public void SetUiY(double d)
        {
            UiY = d;
            OnPropertyChanged(nameof(UiY));
        }

        public MapObject()
        {
        }

        public MapObject(double x, double y)
        {
            UiX = x;
            UiY = y;
        }
    }
}
