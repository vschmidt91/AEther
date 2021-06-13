using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace AEther.WindowsForms
{
    public class ConfigurationPanel : Panel
    {

        public event EventHandler<AnalyzerOptions>? ConfigurationChanged;

        readonly PropertyGrid ConfigurationGrid;
        public AnalyzerOptions? Configuration
        {
            get => ConfigurationGrid.SelectedObject as AnalyzerOptions;
            set => ConfigurationGrid.SelectedObject = value;
        }

        public ConfigurationPanel(AnalyzerOptions configuration)
        {

            ConfigurationGrid = new PropertyGrid
            {
                Dock = DockStyle.Fill,
                SelectedObject = configuration,
            };
            ConfigurationGrid.PropertyValueChanged += ConfigurationGrid_PropertyValueChanged;
            Controls.Add(ConfigurationGrid);

        }

        private void ConfigurationGrid_PropertyValueChanged(object? sender, PropertyValueChangedEventArgs e)
        {
            if(Configuration != default)
            {
                ConfigurationChanged?.Invoke(sender, Configuration);
            }
        }

    }
}