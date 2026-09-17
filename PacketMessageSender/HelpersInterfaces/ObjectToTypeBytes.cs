using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace PacketMessageSender.HelpersInterfaces
{
    public static class ObjectToTypeBytes
    {
        public static Dictionary<string, (Type Tipo, int TamanoBytes, bool EsValido)> IsThereAnyFailure (this Dictionary<string, (object, object, int)> diccionario)
        {
            var resultado = new Dictionary<string, (Type, int, bool)>();

            if (diccionario == null)
                return resultado;

            foreach (var item in diccionario)
            {
                object t1 = item.Value.Item1;
                object t2 = item.Value.Item2;

                if (t1 == null)
                    continue; 

                Type? tipoEspecificado = t1 as Type;

                if (tipoEspecificado == null && t1 != null)
                {
                    tipoEspecificado = t1.GetType();
                }

                if (tipoEspecificado == null)
                {
                    resultado.Add(item.Key, (null, 0, false));
                    continue; 
                }

                int tamanoBytes = ObtenerTamanoEnBytes(tipoEspecificado);

                bool esValido = EsTipoValido(tipoEspecificado, t2);

                resultado.Add(item.Key, (tipoEspecificado, tamanoBytes, esValido));
            }

            return resultado;
        }

        private static int ObtenerTamanoEnBytes(Type tipo)
        {
            if (tipo.IsValueType)
            {
                try
                {
                    return Marshal.SizeOf(tipo);
                }
                catch
                {
                    return IntPtr.Size;
                }
            }

            return IntPtr.Size;
        }

        private static bool EsTipoValido(Type tipoEsperado, object valor)
        {
            if (valor == null)
            {
                return !tipoEsperado.IsValueType || Nullable.GetUnderlyingType(tipoEsperado) != null;
            }

            return tipoEsperado.IsInstanceOfType(valor);
        }

    }
}
