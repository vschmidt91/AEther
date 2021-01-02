using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace AEther.WindowsForms
{
    public class ConfigurationPanel : Panel
    {

        public event EventHandler<SessionOptions>? ConfigurationChanged;

        readonly PropertyGrid ConfigurationGrid;
        public SessionOptions? Configuration
        {
            get => ConfigurationGrid.SelectedObject as SessionOptions;
            set => ConfigurationGrid.SelectedObject = value;
        }

        public ConfigurationPanel(SessionOptions configuration)
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