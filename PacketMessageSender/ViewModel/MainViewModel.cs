using PacketMessageSender.HelpersInterfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net.Sockets;
using System.Text;
using System.Windows.Input;

namespace PacketMessageSender.ViewModel
{
    public class MainViewModel : ModelBase
    {
        private string _ipAdress; 
        private int _port; 
        private ObservableCollection<PayLoadViewModel> _user_payloads; 
        private Dictionary<string, (object, object, int)> prop_type_value;
        private string _responseMessage;
        public string IpAdress
        {
            get => _ipAdress; set
            {
                if(_ipAdress != value) 
                {
                    _ipAdress = value;
                    OnPropertyChanged(nameof(IpAdress)); 
                }
            }
        }
        public int Port
        {
            get => _port; set
            {
                if(_port != value) 
                {
                    _port = value;
                    OnPropertyChanged(nameof(Port)); 
                }
            }
        }
        public string ResponseMessage
        {
            get => _responseMessage;
            set
            {
                if (_responseMessage != value)
                {
                    _responseMessage = value;
                    OnPropertyChanged(nameof(ResponseMessage));
                }
            }
        }
        public ObservableCollection<PayLoadViewModel> UserPayloads
        {
            get => _user_payloads;
            set
            {
                if(value != _user_payloads) 
                {
                    _user_payloads = value;
                    OnPropertyChanged(nameof(UserPayloads)); 
                }
            }
        }
        public ICommand SendCommand
        {
            get;
            set; 
        }
        public ICommand AddCommand
        {
            get;
            set; 
        }
        public ICommand DeleteCommand
        {
            get;
            set; 
        }
        public MainViewModel()
        {
            SendCommand = new RelayAsyncCommand(async () => await SendValueDataAsync());
            AddCommand = new RelayCommand(_ => AddPayload());
            DeleteCommand = new RelayCommand(x => this.DeletePayloadCommandExecute(x));
            this.prop_type_value = new Dictionary<string, (object, object, int)>();
            this._user_payloads = new ObservableCollection<PayLoadViewModel>(); 
        }
        private void AddPayload()
        {
            this.UserPayloads.Add(new PayLoadViewModel());  
        }
        private void DeletePayloadCommandExecute(object parameter)
        {
            if (parameter is not PayLoadViewModel payLoad)
                return; 

            this.UserPayloads.Remove(payLoad);  
        }
        private async Task SendValueDataAsync()
        {
            this.CastIntoKVP();

            if (this.prop_type_value == null || this.prop_type_value.Count <= 0)
                return;

            byte[] processData = PayloadByteConverter.GetBytes(this.prop_type_value);
            if (processData == null || processData.Length <= 0)
                return;

            try
            {
                if (!await this.TestConnectionAsync())
                    return;

                using (TcpClient client = new TcpClient())
                {
                    await client.ConnectAsync(this.IpAdress, this.Port);

                    using (NetworkStream stream = client.GetStream())
                    {
                        await stream.WriteAsync(processData, 0, processData.Length);
                        await stream.FlushAsync();
                    }
                }
            }
            catch (SocketException ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error de Socket al enviar TCP: {ex.Message}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error al enviar data por TCP: {ex.Message}");
            }
        }
        public async Task<bool> TestConnectionAsync(int timeoutMilliseconds = 3000)
        {
            if (this.Port == 0 || string.IsNullOrWhiteSpace(IpAdress))
                return false; 

            using (var cts = new CancellationTokenSource(timeoutMilliseconds))
            using (var client = new TcpClient())
            {
                try
                {
                    await client.ConnectAsync(this.IpAdress, this.Port).WaitAsync(cts.Token);

                    if (client.Connected)
                    {
                        return true;
                    }
                }
                catch (OperationCanceledException)
                {
                    await SetTemporaryResponseMessageAsync("Error: Tiempo de espera agotado (Timeout).");
                }
                catch (SocketException ex)
                {
                    await SetTemporaryResponseMessageAsync($"Error de red: {ex.SocketErrorCode}");
                }
                catch (Exception ex)
                {
                    await SetTemporaryResponseMessageAsync($"Error al conectar: {ex.Message}");
                }
            }

            return false;
        }
        private async Task SetTemporaryResponseMessageAsync(string message, int delayMilliseconds = 3000)
        {
            ResponseMessage = message;

            if (string.IsNullOrEmpty(message)) return;

            await Task.Delay(delayMilliseconds);

            if (ResponseMessage == message)
            {
                ResponseMessage = string.Empty;
            }
        }

        private void CastIntoKVP()
        {
            if (this._user_payloads == null)
                return;

            this.prop_type_value = this._user_payloads
                    .Where(item => !string.IsNullOrWhiteSpace(item.PropertyName))
                    .GroupBy(item => item.PropertyName)
                    .ToDictionary(
                        grupo => grupo.Key,
                        grupo =>
                        {
                            var item = grupo.First();
                            return (item.Type_paload, item.Payload_value, item.Payload_length);
                        }
                    );
        }


    }
}
