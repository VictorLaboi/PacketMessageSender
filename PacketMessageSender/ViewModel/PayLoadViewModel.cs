using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;

namespace PacketMessageSender.ViewModel
{
    public class PayLoadViewModel : INotifyPropertyChanged
    {
        private string _propertyName;
        private object _type_paload;
        private object _payload_value;
        private int _payload_length;

        public string PropertyName
        {
            get => _propertyName;
            set
            {
                if (value != _propertyName)
                {
                    _propertyName = value;
                    OnPropertyChanged(nameof(this.PropertyName));
                }
            }
        }
        public object Type_paload
        {
            get => _type_paload;
            set
            {
                if (value != _type_paload)
                {
                    _type_paload = value;
                    OnPropertyChanged(nameof(this.Type_paload));
                }
            }
        }
        public object Payload_value
        {
            get => _payload_value;
            set
            {
                if (value != _payload_value)
                {
                    _payload_value = value;
                    OnPropertyChanged(nameof(this.Payload_value));
                }
            }
        }
        public int Payload_length
        {
            get => _payload_length;
            set
            {
                if (value != _payload_length)
                {
                    _payload_length = value;
                    OnPropertyChanged(nameof(this.Payload_length));
                }
            }
        }


        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
