using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DemosWPF
{
    // A simple view-model that has the basic Map properties

    class DataBindingViewModel : INotifyPropertyChanged
    {
        MapboxNetCore.GeoLocation _location = new MapboxNetCore.GeoLocation(47.3769, 8.5417);
        public MapboxNetCore.GeoLocation Location
        {
            get => _location;
            set
            {
                if (value == _location)
                    return;

                _location = value;
                OnPropertyRaised(nameof(Location));
            }
        }

        double _zoom = 13;
        public double Zoom
        {
            get => _zoom;
            set
            {
                if (value == _zoom)
                    return;

                _zoom = value;
                OnPropertyRaised(nameof(Zoom));
            }
        }

        double _pitch;
        public double Pitch
        {
            get => _pitch;
            set
            {
                if (value == _pitch)
                    return;

                _pitch = value;
                OnPropertyRaised(nameof(Pitch));
            }
        }

        double _bearing;
        public double Bearing
        {
            get => _bearing;
            set
            {
                if (value == _bearing)
                    return;

                _bearing = value;
                OnPropertyRaised(nameof(Bearing));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyRaised(string propertyname)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyname));
        }
    }
}
