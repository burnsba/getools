using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gebug64.Win.Mvvm;

namespace Gebug64.Win.ViewModels.Map
{
    public abstract class MapObject : ViewModelBase
    {
        private bool _isVisible;
        private double _uix;
        private double _uiy;
        private double _uiWidth;
        private double _uiHeight;

        public double UiX
        {
            get => _uix;
            set
            {
                if (_uix != value)
                {
                    _uix = value;
                    OnPropertyChanged(nameof(UiX));
                }
            }
        }

        public double UiY
        {
            get => _uiy;
            set
            {
                if (_uiy != value)
                {
                    _uiy = value;
                    OnPropertyChanged(nameof(UiY));
                }
            }
        }

        public double UiWidth
        {
            get => _uiWidth;
            set
            {
                if (_uiWidth != value)
                {
                    _uiWidth = value;
                    OnPropertyChanged(nameof(UiWidth));
                }
            }
        }

        public double UiHeight
        {
            get => _uiHeight;
            set
            {
                if (_uiHeight != value)
                {
                    _uiHeight = value;
                    OnPropertyChanged(nameof(UiHeight));
                }
            }
        }

        public bool IsVisible
        {
            get => _isVisible;
            set
            {
                if (_isVisible != value)
                {
                    _isVisible = value;
                    OnPropertyChanged(nameof(IsVisible));
                }
            }
        }

        public System.Windows.Media.Brush Stroke { get; set; }

        public double StrokeThickness { get; set; }

        public System.Windows.Media.Brush Fill { get; set; }

        public MapObject()
        {
        }

        public MapObject(double x, double y)
        {
            _uix = x;
            _uiy = y;
        }
    }
}
