using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace AEther.WindowsForms
{
    public class ConfigurationPanel : Panel
    {

        public event EventHandler<Configuration>? ConfigurationChanged;

        readonly PropertyGrid ConfigurationGrid;
        public Configuration? Configuration
        {
            get => ConfigurationGrid.SelectedObject as Configuration;
            set => ConfigurationGrid.SelectedObject = value;
        }

        public ConfigurationPanel(Configuration configuration)
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