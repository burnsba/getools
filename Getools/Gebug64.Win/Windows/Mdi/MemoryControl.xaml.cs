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
using Gebug64.Win.ViewModels;

namespace Gebug64.Win.Windows.Mdi
{
    /// <summary>
    /// MDI Child window for memory watch.
    /// </summary>
    public partial class MemoryControl : UserControl, ILayoutWindow, ITransientChild
    {
        private readonly string _typeName;
        private readonly MemoryWindowViewModel _vm;

        /// <summary>
        /// Initializes a new instance of the <see cref="MemoryControl"/> class.
        /// </summary>
        /// <param name="vm">Reference to main viewmodel.</param>
        public MemoryControl(MemoryWindowViewModel vm)
        {
            _typeName = GetType().FullName!;

            InitializeComponent();

            _vm = vm;

            DataContext = _vm;
        }

        /// <inheritdoc />
        public string TypeName => _typeName;
    }
}
