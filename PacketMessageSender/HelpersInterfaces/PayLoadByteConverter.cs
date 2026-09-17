using System;
using System.Collections.Generic;
using System.Text;

namespace PacketMessageSender.HelpersInterfaces
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Text;
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using System.Text;

    public static class PayloadByteConverter
    {
        public static Dictionary<string, byte[]> GetProcessData(this Dictionary<string, (object, object, int)> prop_type_value)
        {
            var resultado = new Dictionary<string, byte[]>();

            if (prop_type_value == null)
            {
                return resultado;
            }

            foreach (var item in prop_type_value)
            {
                string clave = item.Key;
                object t1 = item.Value.Item1; // Tipo especificado
                object t2 = item.Value.Item2; // Valor asignado
                int payloadLength = item.Value.Item3; // Longitud personalizada de bytes

                // 1. Obtener y validar el Tipo (T1)
                Type tipoEspecificado = ObtenerTipo(t1);
                if (tipoEspecificado == null)
                {
                    throw new InvalidOperationException($"El tipo especificado para '{clave}' es inválido o nulo.");
                }

                if (!EsTipoValido(tipoEspecificado, t2))
                {
                    throw new ArgumentException($"El valor entregado para '{clave}' no corresponde al tipo '{tipoEspecificado.Name}'.");
                }

                byte[] rawBytes = ConvertirABytesLittleEndian(tipoEspecificado, t2);

                // 4. Aplicar tamaño por defecto o rellenar (padding con 0x00) si se especificó Payload_length
                byte[] finalBytes = AplicarTamanoYPadding(rawBytes, payloadLength);

                resultado.Add(clave, finalBytes);
            }

            return resultado;
        }

        /// <summary>
        /// Obtiene la trama continua de bytes (buffer plano) concatenando los bytes de cada propiedad.
        /// </summary>
        public static byte[] GetBytes(this Dictionary<string, (object, object, int)> prop_type_value)
        {
            Dictionary<string, byte[]> bytesPorPropiedad = GetProcessData(prop_type_value);

            int tamanoTotal = 0;
            foreach (var arrayBytes in bytesPorPropiedad.Values)
            {
                tamanoTotal += arrayBytes.Length;
            }

            byte[] tramaCompleta = new byte[tamanoTotal];
            int posicionActual = 0;

            foreach (var arrayBytes in bytesPorPropiedad.Values)
            {
                Buffer.BlockCopy(arrayBytes, 0, tramaCompleta, posicionActual, arrayBytes.Length);
                posicionActual += arrayBytes.Length;
            }

            return tramaCompleta;
        }

        private static Type ObtenerTipo(object t1)
        {
            if (t1 is Type typeDirecto)
                return typeDirecto;

            return t1?.GetType();
        }

        private static bool EsTipoValido(Type tipoEsperado, object valor)
        {
            if (valor == null)
            {
                return !tipoEsperado.IsValueType || Nullable.GetUnderlyingType(tipoEsperado) != null;
            }

            return tipoEsperado.IsInstanceOfType(valor);
        }

        private static byte[] ConvertirABytesLittleEndian(Type tipo, object valor)
        {
            byte[] bytes;

            if (valor == null)
            {
                int defaultSize = tipo.IsValueType ? Marshal.SizeOf(tipo) : 0;
                return new byte[defaultSize];
            }

            switch (Convert.GetTypeCode(valor))
            {
                case TypeCode.Boolean:
                    bytes = BitConverter.GetBytes((bool)valor);
                    break;
                case TypeCode.Char:
                    bytes = BitConverter.GetBytes((char)valor);
                    break;
                case TypeCode.Int16:
                    bytes = BitConverter.GetBytes((short)valor);
                    break;
                case TypeCode.Int32:
                    bytes = BitConverter.GetBytes((int)valor);
                    break;
                case TypeCode.Int64:
                    bytes = BitConverter.GetBytes((long)valor);
                    break;
                case TypeCode.UInt16:
                    bytes = BitConverter.GetBytes((ushort)valor);
                    break;
                case TypeCode.UInt32:
                    bytes = BitConverter.GetBytes((uint)valor);
                    break;
                case TypeCode.UInt64:
                    bytes = BitConverter.GetBytes((ulong)valor);
                    break;
                case TypeCode.Single:
                    bytes = BitConverter.GetBytes((float)valor);
                    break;
                case TypeCode.Double:
                    bytes = BitConverter.GetBytes((double)valor);
                    break;
                case TypeCode.Byte:
                    bytes = new byte[] { (byte)valor };
                    break;
                case TypeCode.SByte:
                    bytes = new byte[] { unchecked((byte)(sbyte)valor) };
                    break;

                case TypeCode.String:
                    bytes = Encoding.UTF8.GetBytes((string)valor);
                    break;

                default:
                    if (tipo.IsValueType)
                    {
                        int size = Marshal.SizeOf(valor);
                        bytes = new byte[size];
                        IntPtr ptr = Marshal.AllocHGlobal(size);
                        try
                        {
                            Marshal.StructureToPtr(valor, ptr, true);
                            Marshal.Copy(ptr, bytes, 0, size);
                        }
                        finally
                        {
                            Marshal.FreeHGlobal(ptr);
                        }
                    }
                    else
                    {
                        throw new NotSupportedException($"El tipo '{tipo.Name}' no es soportado para conversión binaria.");
                    }
                    break;
            }

            if (!BitConverter.IsLittleEndian && bytes.Length > 1 && tipo != typeof(string))
            {
                Array.Reverse(bytes);
            }

            return bytes;
        }

        private static byte[] AplicarTamanoYPadding(byte[] rawBytes, int payloadLength)
        {
            if (payloadLength <= 0 || payloadLength <= rawBytes.Length)
            {
                return rawBytes;
            }

            byte[] paddedBytes = new byte[payloadLength];
            Array.Copy(rawBytes, 0, paddedBytes, 0, rawBytes.Length);

            return paddedBytes;
        }
    }
}

